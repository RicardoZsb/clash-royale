using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI
{
    public sealed class TrainingActor : MonoBehaviour
    {
        public RectTransform rect, artRect;
        public Image art, healthFill, teamRing;
        public TMP_Text healthText;
        public Animator animator;
        public bool tower, king;
        public int team;
        public float maxHealth, health;
        public TrainingCard card;
        public bool Alive => gameObject.activeSelf && health > 0;
        public Vector2 Position => rect.anchoredPosition;
        public float Radius => tower ? king ? 30 : 24 : card.radius;
        public bool Flying => !tower && card.flying;
        private TrainingBattleController battle;
        private TrainingActor target, attackTarget;
        private float cooldown, think, spawnWait, actionWait, stunWait, summonWait, flash;
        private bool dying;

        public void Initialize(TrainingBattleController owner, TrainingCard definition, int side, Vector2 position, bool isTower = false, bool isKing = false)
        {
            battle=owner;card=definition;team=side;tower=isTower;king=isKing;rect.anchoredPosition=position;
            maxHealth=tower?(king?owner.deck.kingHealth:owner.deck.princessHealth):card.health;health=maxHealth;
            cooldown=.7f;think=0;spawnWait=tower?0:.62f;actionWait=0;stunWait=0;summonWait=3;flash=0;dying=false;target=null;attackTarget=null;
            artRect.sizeDelta=Vector2.one*(tower?(king?85:73):card.artSize);
            animator.runtimeAnimatorController=tower?(king?owner.deck.kingAnimator:owner.deck.princessAnimator):card.animator;
            art.sprite=tower?(king?owner.deck.kingPortrait:owner.deck.princessPortrait):card.portrait;
            art.color=Color.white;artRect.localScale=Vector3.one;
            teamRing.color=side==0?new Color32(89,174,222,220):new Color32(237,118,117,220);
            healthFill.color=side==0?new Color32(83,167,221,255):new Color32(225,99,103,255);
            gameObject.SetActive(true);animator.Rebind();animator.Update(0);UpdateHealth();
        }
        public void Tick(float dt)
        {
            if(!Alive||!battle.Running)return;
            if(flash>0){flash-=dt;if(flash<=0)art.color=Color.white;}
            if(spawnWait>0){spawnWait-=dt;return;}
            if(stunWait>0){stunWait-=dt;animator.SetBool("Moving",false);return;}
            cooldown-=dt;actionWait-=dt;summonWait-=dt;think-=dt;
            if(tower&&king&&!battle.KingAwake(team))return;
            if(actionWait>0)return;
            if(!tower&&card.summons&&summonWait<=0)
            {
                summonWait=7;actionWait=.68f;animator.SetBool("Moving",false);animator.SetTrigger("Summon");return;
            }
            if(think<=0||!target||!target.Alive){target=battle.FindTarget(this);think=.22f;}
            if(!target){animator.SetBool("Moving",false);return;}
            float distance=Vector2.Distance(Position,target.Position);
            float reach=(tower?145:card.range)+target.Radius;
            if(distance<=reach)
            {
                animator.SetBool("Moving",false);
                if(cooldown<=0){cooldown=tower?1.1f:card.attackInterval;actionWait=.62f;attackTarget=target;animator.SetTrigger("Attack");}
            }
            else if(!tower)
            {
                animator.SetBool("Moving",true);
                Vector2 direction=battle.Waypoint(this,target.Position)-Position;
                battle.MoveActor(this,direction.normalized*card.speed*dt);
                if(Mathf.Abs(direction.x)>8)artRect.localScale=new Vector3(direction.x<0?-1:1,1,1);
            }
        }
        // Animation events place damage, projectiles and summoned units on the drawn action frame.
        public void Strike()
        {
            if(!Alive||!battle.Running||!attackTarget||!attackTarget.Alive)return;
            if(tower||card.range>50)battle.FireProjectile(this,attackTarget);
            else if(Vector2.Distance(Position,attackTarget.Position)<=card.range+attackTarget.Radius+15)attackTarget.TakeDamage(card.damage);
        }
        public void Summon()
        {
            if(!Alive||!battle.Running||tower||!card.summons)return;
            for(int i=-1;i<=1;i++)battle.Spawn(battle.deck.skeleton,team,Position+new Vector2(i*18,team==0?20:-20));
        }
        public void TakeDamage(float amount, float stun = 0)
        {
            if(!Alive||!battle.Running)return;
            health=Mathf.Max(0,health-amount);stunWait=Mathf.Max(stunWait,stun);flash=.12f;art.color=new Color32(255,170,170,255);UpdateHealth();
            if(health<=0&&!dying)
            {
                dying=true;animator.SetBool("Moving",false);animator.ResetTrigger("Attack");animator.ResetTrigger("Summon");animator.SetTrigger("Die");
                teamRing.gameObject.SetActive(false);if(tower)battle.TowerFell(this);
            }
        }
        public void DeathFinished(){if(!tower)gameObject.SetActive(false);}
        private void UpdateHealth()
        {
            healthFill.fillAmount=health/maxHealth;
            if(healthText)healthText.text=tower?Mathf.CeilToInt(health).ToString():"";
            teamRing.gameObject.SetActive(true);
        }
    }
}
