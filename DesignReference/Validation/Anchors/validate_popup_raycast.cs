if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Validation requires Play Mode");
var a=typeof(UnityEditor.Editor).Assembly;
var flags=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static;
var sizesType=a.GetType("UnityEditor.GameViewSizes");
var sizes=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{UnityEditor.GameViewSizeGroupType.Standalone});
var groupType=group.GetType();
var sizeType=a.GetType("UnityEditor.GameViewSize");
var ctor=sizeType.GetConstructors(flags).First(c=>c.GetParameters().Length==4);
int addedIndex=(int)groupType.GetMethod("GetTotalCount").Invoke(group,null);
var custom=ctor.Invoke(new object[]{System.Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1080,2640,"Anchor validation 22x9"});
groupType.GetMethod("AddCustomSize").Invoke(group,new object[]{custom});
var game=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>().First(w=>w.GetType().FullName=="UnityEditor.GameView");
var selected=game.GetType().GetProperty("selectedSizeIndex",flags);
game.Focus();
int[] indices={7,8,9,addedIndex};
string[] labels={"402x874","1080x1920","1080x2400","1080x2640"};
var router=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIScreenRouter>();
var initial=router.CurrentScreen;
var screens=new[]{ClashRoyaleUI.ScreenId.BattleHome};
string folder=System.IO.Path.GetFullPath("DesignReference/Validation/Anchors");
int ratio=0,page=-1,stage=0,frame=UnityEngine.Time.frameCount;
double due=UnityEditor.EditorApplication.timeSinceStartup;
var records=new System.Collections.Generic.List<object>();
var errors=new System.Collections.Generic.List<string>();
string PathOf(UnityEngine.Transform t,UnityEngine.Transform root){return t==root?"":PathOf(t.parent,root)+"/"+t.name;}
object RectData(UnityEngine.RectTransform rect,UnityEngine.RectTransform root){
    var p=root.InverseTransformPoint(rect.TransformPoint(new UnityEngine.Vector3(rect.rect.xMin,rect.rect.yMax,0)));
    return new{path=PathOf(rect,root),x=p.x-root.rect.xMin,y=root.rect.yMax-p.y,w=rect.rect.width,h=rect.rect.height,active=rect.gameObject.activeInHierarchy};
}
void Finish(){
    selected.SetValue(game,7); router.Show(ClashRoyaleUI.ScreenId.BattleHome);
    groupType.GetMethod("RemoveCustomSize").Invoke(group,new object[]{addedIndex});
    System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"closed-home-layouts.json"),Newtonsoft.Json.JsonConvert.SerializeObject(records,Newtonsoft.Json.Formatting.Indented));
    System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"interaction-check-complete.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{captures=records.Count,errors}));
}
UnityEditor.EditorApplication.CallbackFunction job=null;
job=()=>{
    if(!UnityEditor.EditorApplication.isPlaying){UnityEditor.EditorApplication.update-=job;errors.Add("Play Mode interrupted");Finish();return;}
    if(UnityEditor.EditorApplication.timeSinceStartup<due || UnityEngine.Time.frameCount<frame+8) {game.Repaint();return;}
    try{
        if(stage==0){
            page++;
            if(page==screens.Length){page=0;ratio++;}
            if(ratio==indices.Length){UnityEditor.EditorApplication.update-=job;Finish();return;}
            selected.SetValue(game,indices[ratio]);
            if(page<screens.Length)router.Show(screens[page]);
            else{
                router.Show(page==screens.Length?ClashRoyaleUI.ScreenId.Tasks:ClashRoyaleUI.ScreenId.BattleHome);
                router.Screens.First(s=>s.id==router.CurrentScreen).view.GetComponentInChildren<ClashRoyaleUI.UIRuleDropdown>(true).Open();
            }
            UnityEngine.Canvas.ForceUpdateCanvases(); game.Repaint(); stage=1;
        }else{
            foreach(var id in new[]{ClashRoyaleUI.ScreenId.BattleHome,ClashRoyaleUI.ScreenId.Tasks}) {
    router.Show(id); UnityEngine.Canvas.ForceUpdateCanvases();
    var drop=router.Screens.First(s=>s.id==id).view.GetComponentInChildren<ClashRoyaleUI.UIRuleDropdown>(true);
    foreach(var corner in new[]{new UnityEngine.Vector2(2,2),new UnityEngine.Vector2(UnityEngine.Screen.width-2,2),new UnityEngine.Vector2(2,UnityEngine.Screen.height-2),new UnityEngine.Vector2(UnityEngine.Screen.width-2,UnityEngine.Screen.height-2)}) {
        drop.Open(); UnityEngine.Canvas.ForceUpdateCanvases();
        var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=corner,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
        var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);
        if(hits.Count==0 || hits[0].gameObject.name!="OutsideClickToClose") errors.Add(labels[ratio]+" "+id+" corner raycast missed blocker "+corner);
        else UnityEngine.EventSystems.ExecuteEvents.Execute(hits[0].gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        if(drop.IsOpen) errors.Add(labels[ratio]+" "+id+" failed to dismiss");
        drop.Close();
    }
}
router.Show(ClashRoyaleUI.ScreenId.BattleHome);UnityEngine.Canvas.ForceUpdateCanvases();
var entry=router.Screens.First(s=>s.id==router.CurrentScreen);var root=(UnityEngine.RectTransform)entry.view.transform;
            string name=page<screens.Length?entry.id.ToString():(page==screens.Length?"TaskSortDropdown-Open":"TaskActivationDropdown-Open");
            string dir=System.IO.Path.Combine(folder,"After-"+labels[ratio]);System.IO.Directory.CreateDirectory(dir);
            UnityEngine.ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir,name+".png"));
            var canvas=root.GetComponentInParent<UnityEngine.Canvas>().GetComponent<UnityEngine.RectTransform>();
            if(UnityEngine.Mathf.Abs(canvas.rect.width-root.rect.width)>0.1f || UnityEngine.Mathf.Abs(canvas.rect.height-root.rect.height)>0.1f)errors.Add(labels[ratio]+" "+name+" root does not fill canvas");
            records.Add(new{resolution=labels[ratio],screen=name,screenWidth=UnityEngine.Screen.width,screenHeight=UnityEngine.Screen.height,rootWidth=root.rect.width,rootHeight=root.rect.height,rects=root.GetComponentsInChildren<UnityEngine.RectTransform>(true).Select(r=>RectData(r,root)).ToArray()});
            stage=0;
        }
        frame=UnityEngine.Time.frameCount;due=UnityEditor.EditorApplication.timeSinceStartup+0.45;
    }catch(System.Exception e){errors.Add(e.ToString());UnityEditor.EditorApplication.update-=job;Finish();}
};
UnityEditor.EditorApplication.update+=job;
return "Capturing 10 pages and 2 dropdown states at 4 aspect ratios; restoring BattleHome afterward.";
