using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI
{
    public sealed class TrainingBattleController : MonoBehaviour
    {
        public TrainingDeck deck;
        public RectTransform arena, ghostRect;
        public Image ghostArt, ghostRing, enemyPlacementShade, elixirFill, nextPortrait;
        public TMP_Text timerLabel, elixirLabel, playerCrownsLabel, enemyCrownsLabel;
        public TrainingHandCard[] handViews;
        public TrainingActor[] towers, units;
        public TrainingProjectile[] projectiles;
        public GameObject resultPanel;
        public TMP_Text resultTitle, resultScore, resultRewards;
        public Button continueButton;
        public TMP_Text continueLabel;
        public bool Running { get; private set; }
        public string MatchId { get; private set; }
        public float PlayerElixir => playerElixir;
        public float EnemyElixir => enemyElixir;
        public float Elapsed => elapsed;
        public int[] Hand => hand;
        private readonly Queue<int> cycle = new Queue<int>();
        private readonly Queue<int> enemyCycle = new Queue<int>();
        private readonly int[] hand = new int[4], enemyHand = new int[4];
        private UIBackendClient client;
        private UIScreenRouter router;
        private int selected = -1, playerCrowns, enemyCrowns, winner;
        private float playerElixir, enemyElixir, elapsed, botWait;
        private bool overtime, submitted, settled;

        public void StartMatch(string id)
        {
            StopAllCoroutines();client=GetComponentInParent<UIBackendClient>();router=GetComponentInParent<UIScreenRouter>();MatchId=id;
            foreach(var actor in units)actor.gameObject.SetActive(false);
            foreach(var p in projectiles)p.gameObject.SetActive(false);
            for(int side=0;side<2;side++)
            {
                float sign=side==0?-1:1;
                towers[side*3].Initialize(this,null,side,new Vector2(-104,sign*174),true);
                towers[side*3+1].Initialize(this,null,side,new Vector2(104,sign*174),true);
                towers[side*3+2].Initialize(this,null,side,new Vector2(0,sign*250),true,true);
            }
            int[] initial={0,1,5,2};for(int i=0;i<4;i++){hand[i]=initial[i];enemyHand[i]=initial[i];}
            cycle.Clear();enemyCycle.Clear();foreach(int i in new[]{3,4,6,7}){cycle.Enqueue(i);enemyCycle.Enqueue(i);}
            playerElixir=enemyElixir=5;elapsed=0;botWait=2.8f;selected=-1;playerCrowns=enemyCrowns=0;overtime=false;submitted=false;settled=false;
            resultPanel.SetActive(false);ghostRect.gameObject.SetActive(false);enemyPlacementShade.gameObject.SetActive(false);Running=true;RefreshHud();
        }
        private void Update()
        {
            if(!Running)return;
            float dt=Time.deltaTime;elapsed+=dt;
            float rate=(elapsed>=deck.regulationSeconds-60?2:1)/deck.elixirSeconds;
            playerElixir=Mathf.Min(10,playerElixir+dt*rate);enemyElixir=Mathf.Min(10,enemyElixir+dt*rate);
            foreach(var actor in towers)actor.Tick(dt);
            foreach(var actor in units)if(actor.gameObject.activeSelf)actor.Tick(dt);
            foreach(var p in projectiles)if(p.gameObject.activeSelf)p.Tick(dt);
            if(!Running){RefreshHud();return;}
            botWait-=dt;if(botWait<=0){botWait=Random.Range(1.4f,2.4f);BotTurn();}
            if(!overtime&&elapsed>=deck.regulationSeconds)
            {
                if(playerCrowns!=enemyCrowns)Finish(playerCrowns>enemyCrowns?0:1);
                else overtime=true;
            }
            if(overtime&&elapsed>=deck.regulationSeconds+deck.overtimeSeconds)
            {
                float ours=towers.Where(t=>t.team==0&&t.Alive).Min(t=>t.health);
                float theirs=towers.Where(t=>t.team==1&&t.Alive).Min(t=>t.health);
                Finish(Mathf.Abs(ours-theirs)<.1f?-1:ours>theirs?0:1);
            }
            RefreshHud();
        }
        public void SelectCard(int slot, bool toggle = true)
        {
            if(!Running||slot<0||slot>3)return;
            selected=toggle&&selected==slot?-1:slot;ghostRect.gameObject.SetActive(false);
            enemyPlacementShade.gameObject.SetActive(selected>=0&&!deck.cards[hand[selected]].spell);RefreshHud();
        }
        public void PreviewPlacement(Vector2 screen,Camera camera)
        {
            if(!Running||selected<0||!RectTransformUtility.RectangleContainsScreenPoint(arena,screen,camera)){ghostRect.gameObject.SetActive(false);return;}
            RectTransformUtility.ScreenPointToLocalPointInRectangle(arena,screen,camera,out Vector2 point);
            var card=deck.cards[hand[selected]];ghostRect.anchoredPosition=ClampPosition(point);ghostArt.sprite=card.portrait;
            ghostRect.sizeDelta=Vector2.one*(card.spell?card.splash*2:64);
            bool valid=card.cost<=playerElixir&&(card.spell||point.y<-30);
            ghostRing.color=valid?new Color32(92,189,158,150):new Color32(235,98,109,160);ghostRect.gameObject.SetActive(true);
        }
        public void PlayAtScreen(Vector2 screen,Camera camera)
        {
            if(!Running||selected<0)return;
            if(RectTransformUtility.RectangleContainsScreenPoint(arena,screen,camera))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(arena,screen,camera,out Vector2 point);TryPlayCard(selected,point);
            }
            ghostRect.gameObject.SetActive(false);
        }
        public bool TryPlayCard(int slot,Vector2 point)
        {
            if(!Running||slot<0||slot>3)return false;
            var card=deck.cards[hand[slot]];
            if(card.cost>playerElixir||!card.spell&&point.y>=-30)return false;
            if(!Deploy(card,0,ClampPosition(point)))return false;
            playerElixir-=card.cost;cycle.Enqueue(hand[slot]);hand[slot]=cycle.Dequeue();selected=-1;
            enemyPlacementShade.gameObject.SetActive(false);RefreshHud();return true;
        }
        private bool Deploy(TrainingCard card,int side,Vector2 point)
        {
            if(card.spell)
            {
                var p=projectiles.FirstOrDefault(x=>!x.gameObject.activeSelf);if(!p)return false;
                p.Launch(this,card,side,new Vector2(0,side==0?-255:255),point,card.damage,card.splash,card.stun,true);return true;
            }
            if(units.Count(x=>!x.gameObject.activeSelf)<card.count)return false;
            for(int i=0;i<card.count;i++)Spawn(card,side,point+new Vector2((i-(card.count-1)*.5f)*24,0));
            return true;
        }
        public void Spawn(TrainingCard card,int side,Vector2 point)
        {
            var actor=units.FirstOrDefault(x=>!x.gameObject.activeSelf);if(!actor)return;
            foreach(var tower in towers.Where(t=>t.team==side&&t.Alive))
                if(Vector2.Distance(point,tower.Position)<tower.Radius+8)point.y+=side==0?42:-42;
            actor.Initialize(this,card,side,ClampPosition(point));
        }
        private void BotTurn()
        {
            var threat=units.Where(u=>u.Alive&&u.team==0&&u.Position.y>0).OrderByDescending(u=>u.Position.y).FirstOrDefault();
            int start=Random.Range(0,4);
            for(int offset=0;offset<4;offset++)
            {
                int slot=(start+offset)%4;var card=deck.cards[enemyHand[slot]];if(card.cost>enemyElixir)continue;
                Vector2 point;
                if(card.spell)
                {
                    if(threat)point=threat.Position;
                    else if(enemyElixir>8)point=towers.Where(t=>t.team==0&&t.Alive).OrderBy(t=>t.health).First().Position;
                    else continue;
                }
                else point=threat?new Vector2(Mathf.Clamp(threat.Position.x,-145,145),Mathf.Clamp(threat.Position.y+42,40,210)):new Vector2(Random.value<.5f?-104:104,Random.Range(105,185));
                if(!Deploy(card,1,point))continue;
                enemyElixir-=card.cost;enemyCycle.Enqueue(enemyHand[slot]);enemyHand[slot]=enemyCycle.Dequeue();break;
            }
        }
        public TrainingActor FindTarget(TrainingActor actor)
        {
            TrainingActor nearest=null;float distance=145;
            if(actor.tower||!actor.card.buildingsOnly)
            {
                foreach(var candidate in units)
                {
                    if(!candidate.Alive||candidate.team==actor.team||!actor.tower&&!actor.card.hitsAir&&candidate.Flying)continue;
                    float d=Vector2.Distance(actor.Position,candidate.Position)-candidate.Radius;
                    if(d<distance){nearest=candidate;distance=d;}
                }
            }
            if(nearest||actor.tower)return nearest;
            int enemy=1-actor.team;int lane=actor.Position.x<0?0:1;
            var tower=towers[enemy*3+lane];if(tower.Alive)return tower;
            tower=towers[enemy*3+2];if(tower.Alive)return tower;
            return towers[enemy*3+1-lane].Alive?towers[enemy*3+1-lane]:null;
        }
        public Vector2 Waypoint(TrainingActor actor,Vector2 destination)
        {
            var p=actor.Position;if(actor.Flying)return destination;
            bool crossing=p.y<-24&&destination.y>-24||p.y>24&&destination.y<24||Mathf.Abs(p.y)<=24;
            if(!crossing)return destination;
            float bridge=p.x<0?-104:104;float direction=destination.y>p.y?1:-1;
            if(Mathf.Abs(p.y)>24&&Mathf.Abs(p.x-bridge)>8)return new Vector2(bridge,p.y<0?-29:29);
            return new Vector2(bridge,direction*30);
        }
        public void MoveActor(TrainingActor actor,Vector2 delta)
        {
            Vector2 avoidance=Vector2.zero;
            foreach(var other in units)
            {
                if(other==actor||!other.Alive||other.Flying!=actor.Flying)continue;
                Vector2 away=actor.Position-other.Position;float minimum=actor.Radius+other.Radius-4;
                if(away.sqrMagnitude>1&&away.magnitude<minimum)avoidance+=away.normalized*(minimum-away.magnitude)*.04f;
            }
            Vector2 next=ClampPosition(actor.Position+delta+Vector2.ClampMagnitude(avoidance,delta.magnitude*.7f));
            if(!actor.Flying&&Mathf.Abs(next.y)<24)next.x=Mathf.Clamp(next.x,(next.x<0?-104:104)-23,(next.x<0?-104:104)+23);
            actor.rect.anchoredPosition=next;
        }
        private static Vector2 ClampPosition(Vector2 p)=>new Vector2(Mathf.Clamp(p.x,-171,171),Mathf.Clamp(p.y,-278,278));
        public bool KingAwake(int side)=>towers[side*3+2].health<towers[side*3+2].maxHealth||!towers[side*3].Alive||!towers[side*3+1].Alive;
        public void FireProjectile(TrainingActor source,TrainingActor target)
        {
            var p=projectiles.FirstOrDefault(x=>!x.gameObject.activeSelf);if(!p)return;
            var visual=source.tower?(source.king?deck.cards[5]:deck.cards[6]):source.card.id=="archer"?deck.cards[6]:source.card.id=="witch"?deck.cards[7]:deck.cards[5];
            p.Launch(this,visual,source.team,source.Position,target.Position,source.tower?(source.king?80:65):source.card.damage,source.tower?0:source.card.splash,0,false,target);
        }
        public void AreaDamage(int side,Vector2 point,float radius,float damage,float stun,bool spell)
        {
            foreach(var a in units)if(a.Alive&&a.team!=side&&Vector2.Distance(a.Position,point)<=radius+a.Radius)a.TakeDamage(damage,stun);
            foreach(var a in towers)if(a.Alive&&a.team!=side&&Vector2.Distance(a.Position,point)<=radius+a.Radius)a.TakeDamage(damage*(spell?.4f:1),stun);
        }
        public void TowerFell(TrainingActor tower)
        {
            if(tower.team==1)playerCrowns=tower.king?3:playerCrowns+1;else enemyCrowns=tower.king?3:enemyCrowns+1;
            if(tower.king||overtime)Finish(1-tower.team);
        }
        private void Finish(int victoriousSide)
        {
            if(!Running)return;Running=false;winner=victoriousSide;selected=-1;ghostRect.gameObject.SetActive(false);enemyPlacementShade.gameObject.SetActive(false);
            RefreshHud();
            StartCoroutine(ShowResult());
        }
        private IEnumerator ShowResult()
        {
            yield return new WaitForSeconds(.8f);
            resultTitle.text=winner==0?"胜利":winner==1?"惜败":"平局";resultScore.text=$"{playerCrowns}  :  {enemyCrowns}";resultRewards.text="";
            continueLabel.text="继续";continueButton.interactable=false;resultPanel.SetActive(true);
            while(client.Busy)yield return null;
            SubmitResult();
        }
        private void SubmitResult()
        {
            if(submitted||client.Busy)return;submitted=true;
            string outcome=winner==0?"win":winner==1?"loss":"draw";
            client.Send("/v1/training/"+MatchId+"/finish",$"{{\"outcome\":\"{outcome}\",\"elapsed_seconds\":{Mathf.FloorToInt(elapsed)}}}",reply=>
            {
                settled=true;resultRewards.text=reply.result.completed_count>0?$"任务宝箱 ×{reply.result.completed_count}":"";continueButton.interactable=true;
            });
            StartCoroutine(CheckSettlement());
        }
        private IEnumerator CheckSettlement()
        {
            while(client.Busy)yield return null;
            if(!settled){submitted=false;continueLabel.text="重试";continueButton.interactable=true;}
        }
        public void Continue()
        {
            if(!settled){continueButton.interactable=false;SubmitResult();return;}
            router.Show(ScreenId.BattleHome);
        }
        private void RefreshHud()
        {
            float remaining=Mathf.Max(0,(overtime?deck.regulationSeconds+deck.overtimeSeconds:deck.regulationSeconds)-elapsed);
            int seconds=Mathf.CeilToInt(remaining);timerLabel.text=(overtime?"+":"")+$"{seconds/60}:{seconds%60:00}";
            timerLabel.color=overtime||remaining<30?new Color32(174,63,67,255):new Color32(44,42,48,255);
            elixirFill.fillAmount=playerElixir/10;elixirLabel.text=Mathf.FloorToInt(playerElixir).ToString();
            playerCrownsLabel.text=playerCrowns.ToString();enemyCrownsLabel.text=enemyCrowns.ToString();
            for(int i=0;i<4;i++)handViews[i].Display(deck.cards[hand[i]],deck.cards[hand[i]].cost<=playerElixir,i==selected);
            if(cycle.Count>0)nextPortrait.sprite=deck.cards[cycle.Peek()].portrait;
        }
    }
}
