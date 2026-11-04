using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI
{
    public sealed class TrainingProjectile : MonoBehaviour
    {
        public RectTransform rect, artRect;
        public Image art;
        public Animator animator;
        private TrainingBattleController battle;
        private TrainingActor target;
        private Vector2 from, to;
        private float age, flight, damage, splash, stun;
        private int team;
        private bool spell, impacted, hitApplied;
        public void Launch(TrainingBattleController owner,TrainingCard visual,int side,Vector2 start,Vector2 end,float amount,float area,float stunSeconds,bool isSpell,TrainingActor victim=null)
        {
            battle=owner;team=side;from=start;to=end;damage=amount;splash=area;stun=stunSeconds;spell=isSpell;target=victim;
            age=0;flight=isSpell?.58f:.32f;impacted=false;hitApplied=false;rect.anchoredPosition=from;
            artRect.sizeDelta=Vector2.one*(isSpell?visual.artSize:23);art.color=Color.white;
            animator.runtimeAnimatorController=visual.animator;art.sprite=visual.portrait;gameObject.SetActive(true);animator.Rebind();animator.Update(0);
        }
        public void Tick(float dt)
        {
            if(!gameObject.activeSelf||!battle.Running)return;
            age+=dt;
            if(!impacted)
            {
                if(target&&target.Alive)to=target.Position;
                float t=Mathf.Clamp01(age/flight);rect.anchoredPosition=Vector2.Lerp(from,to,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*(spell?30:7));
                if(t>=1){impacted=true;animator.SetTrigger("Impact");}
            }
            if(age>2)gameObject.SetActive(false);
        }
        public void Impact()
        {
            if(hitApplied||!battle.Running)return;hitApplied=true;
            if(spell||splash>0)battle.AreaDamage(team,to,Mathf.Max(splash,12),damage,stun,spell);
            else if(target&&target.Alive)target.TakeDamage(damage,stun);
        }
        public void EffectFinished()=>gameObject.SetActive(false);
    }
}
