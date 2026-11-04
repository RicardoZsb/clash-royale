if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play Mode required");
var client=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIBackendClient>();
var battle=client.GetComponentInChildren<ClashRoyaleUI.TrainingBattleController>(true);
var router=client.GetComponent<ClashRoyaleUI.UIScreenRouter>();
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
int stage=0;bool starting=false,waiting=false;
var checks=new System.Collections.Generic.List<object>();
UnityEngine.Time.timeScale=2;
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{
 if(!UnityEngine.Application.isPlaying){UnityEditor.EditorApplication.update-=tick;return;}
 if(waiting){
  if(!battle.resultPanel.activeSelf||!battle.continueButton.interactable||client.Busy)return;
  checks.Add(new{scenario=new[]{"overtime-first-tower","tower-health-tiebreak","draw","king-tower","deployment-and-cost"}[stage],title=battle.resultTitle.text,score=battle.resultScore.text,error=client.LastError});
  battle.Continue();stage++;waiting=false;starting=false;
  if(stage==5){UnityEditor.EditorApplication.update-=tick;UnityEngine.Time.timeScale=0;System.IO.File.WriteAllText("DesignReference/Validation/TrainingBattle/boundaries.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));return;}
 }
 if(starting||client.Busy)return;starting=true;
 client.Send("/v1/training/start","{}",reply=>{
  router.Show(ClashRoyaleUI.ScreenId.TrainingBattle);battle.StartMatch(reply.result.battle_id);
  if(stage==0){
   typeof(ClashRoyaleUI.TrainingBattleController).GetField("elapsed",flags).SetValue(battle,181f);
   typeof(ClashRoyaleUI.TrainingBattleController).GetMethod("Update",flags).Invoke(battle,null);
   battle.towers[3].TakeDamage(99999);
  }else if(stage==1||stage==2){
   typeof(ClashRoyaleUI.TrainingBattleController).GetField("elapsed",flags).SetValue(battle,241f);
   typeof(ClashRoyaleUI.TrainingBattleController).GetField("overtime",flags).SetValue(battle,true);
   if(stage==1)battle.towers[3].TakeDamage(500);
   typeof(ClashRoyaleUI.TrainingBattleController).GetMethod("Update",flags).Invoke(battle,null);
  }else{
   if(stage==4){bool enemySideRejected=!battle.TryPlayCard(3,new UnityEngine.Vector2(104,100));bool giantPlayed=battle.TryPlayCard(3,new UnityEngine.Vector2(104,-100));bool insufficientRejected=!battle.TryPlayCard(0,new UnityEngine.Vector2(104,-100));checks.Add(new{enemySideRejected,giantPlayed,insufficientRejected,elixir=battle.PlayerElixir});}
   battle.towers[5].TakeDamage(99999);
  }
  waiting=true;
 });
};
UnityEditor.EditorApplication.update+=tick;
return "Boundary scenarios running with isolated QA backend";
