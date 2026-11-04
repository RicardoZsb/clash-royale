if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play Mode required");
var battle=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.TrainingBattleController>();
var played=new System.Collections.Generic.HashSet<string>();
var states=new System.Collections.Generic.HashSet<string>();
int skeletons=0;float next=0;int turns=0;
UnityEngine.Time.timeScale=4;
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(!UnityEngine.Application.isPlaying||!battle){UnityEditor.EditorApplication.update-=tick;return;}
 foreach(var a in battle.units.Where(a=>a.Alive)){
  if(a.card.id=="skeleton")skeletons++;
  foreach(var state in new[]{"Spawn","Idle","Move","Attack","Summon","Die"})if(a.animator.GetCurrentAnimatorStateInfo(0).IsName(state))states.Add(a.card.id+":"+state);
 }
 if(battle.Running&&battle.Elapsed>=next){
  next=battle.Elapsed+1.5f;turns++;
  var slots=System.Linq.Enumerable.Range(0,4).OrderBy(s=>played.Contains(battle.deck.cards[battle.Hand[s]].id)?1:0).ThenBy(s=>(s+turns)%4).ToArray();
  bool waiting=slots.Any(s=>!played.Contains(battle.deck.cards[battle.Hand[s]].id));
  foreach(int slot in slots){
   var card=battle.deck.cards[battle.Hand[slot]];
   if(waiting&&played.Contains(card.id))continue;
   UnityEngine.Vector2 point=new UnityEngine.Vector2(turns%2==0?-104:104,-65);
   if(card.spell){var enemy=battle.units.FirstOrDefault(a=>a.Alive&&a.team==1);point=enemy?enemy.Position:battle.towers[3+turns%2].Position;}
   if(battle.TryPlayCard(slot,point)){played.Add(card.id);break;}
  }
 }
 if(!battle.Running&&battle.resultPanel.activeSelf&&battle.continueButton.interactable){
  UnityEditor.EditorApplication.update-=tick;UnityEngine.Time.timeScale=0;
  var report=new{battle.MatchId,battle.Elapsed,result=battle.resultTitle.text,reward=battle.resultRewards.text,played=played.OrderBy(x=>x).ToArray(),states=states.OrderBy(x=>x).ToArray(),summonedSkeletonSamples=skeletons,towerHealth=battle.towers.Select(t=>t.health).ToArray(),backendError=battle.GetComponentInParent<ClashRoyaleUI.UIBackendClient>().LastError};
  System.IO.File.WriteAllText("DesignReference/Validation/TrainingBattle/playthrough.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
 }
};
UnityEditor.EditorApplication.update+=tick;
return "Playthrough running at 4x; result will pause and write playthrough.json";
