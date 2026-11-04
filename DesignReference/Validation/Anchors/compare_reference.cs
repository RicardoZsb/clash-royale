if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play Mode required");
var folder=System.IO.Path.GetFullPath("DesignReference/Validation/Anchors");
var baseline=Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(folder,"before-structures.json")));
var router=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIScreenRouter>();
foreach(var d in UnityEngine.Object.FindObjectsByType<ClashRoyaleUI.UIRuleDropdown>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None))d.Close();
var saved=new System.Collections.Generic.Dictionary<UnityEngine.RectTransform,UnityEngine.Vector2[]>();
foreach(var entry in router.Screens)foreach(var r in entry.view.GetComponentsInChildren<UnityEngine.RectTransform>(true))saved[r]=new[]{r.anchorMin,r.anchorMax,r.anchoredPosition,r.sizeDelta};
UnityEngine.Vector2 V(Newtonsoft.Json.Linq.JToken t)=>new UnityEngine.Vector2((float)t[0],(float)t[1]);
void Set(UnityEngine.RectTransform r,UnityEngine.Vector2[] vals){r.anchorMin=vals[0];r.anchorMax=vals[1];r.anchoredPosition=vals[2];r.sizeDelta=vals[3];}
void Restore(){foreach(var pair in saved)if(pair.Key)Set(pair.Key,pair.Value);UnityEngine.Canvas.ForceUpdateCanvases();}
string beforeDir=System.IO.Path.Combine(folder,"ReferenceReplay-402x874");System.IO.Directory.CreateDirectory(beforeDir);
int page=0,stage=0,frame=UnityEngine.Time.frameCount;double due=0;
UnityEditor.EditorApplication.CallbackFunction job=null;
job=()=>{
    if(UnityEditor.EditorApplication.timeSinceStartup<due||UnityEngine.Time.frameCount<frame+8)return;
    try{
        if(!UnityEditor.EditorApplication.isPlaying){Restore();UnityEditor.EditorApplication.update-=job;return;}
        if(page==baseline.Count){Restore();router.Show(ClashRoyaleUI.ScreenId.BattleHome);UnityEditor.EditorApplication.update-=job;System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"reference-replay-complete.txt"),"10 before/after pairs, temporary Play Mode rect changes restored");return;}
        string name=(string)baseline[page]["screen"];
        var entry=router.Screens.First(s=>s.id.ToString()==name);
        if(stage==0){
            router.Show(entry.id);
            foreach(var b in baseline[page]["rects"]){string path=(string)b["path"];var r=(UnityEngine.RectTransform)(path==""?entry.view.transform:entry.view.transform.Find(path.TrimStart('/')));Set(r,new[]{V(b["min"]),V(b["max"]),V(b["p"]),V(b["s"])});}
            UnityEngine.Canvas.ForceUpdateCanvases();stage=1;
        }else if(stage==1){UnityEngine.ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(beforeDir,name+".png"));stage=2;}
        else if(stage==2){Restore();stage=3;}
        else{UnityEngine.ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"After-402x874",name+".png"));page++;stage=0;}
        frame=UnityEngine.Time.frameCount;due=UnityEditor.EditorApplication.timeSinceStartup+0.35;
    }catch(System.Exception e){Restore();UnityEditor.EditorApplication.update-=job;System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"reference-replay-error.txt"),e.ToString());}
};UnityEditor.EditorApplication.update+=job;
return "Comparing original and adapted RectTransform geometry at 402x874 without saving runtime changes";
