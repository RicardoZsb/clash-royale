if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Exit Play Mode before saving anchors.");
var plan=Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText("DesignReference/Validation/Anchors/anchor-plan.json"));
UnityEngine.Vector2 V(Newtonsoft.Json.Linq.JToken token) => new UnityEngine.Vector2((float)token[0],(float)token[1]);
int Apply(UnityEngine.GameObject root, Newtonsoft.Json.Linq.JArray entries) {
    int count=0;
    foreach(var entry in entries.OrderByDescending(e=>((string)e["path"]).Length)) {
        string path=(string)entry["path"];
        var tr=path==""?root.transform:root.transform.Find(path.TrimStart('/'));
        if(tr==null) throw new System.Exception("Missing RectTransform: "+root.name+path);
        var rect=(UnityEngine.RectTransform)tr;
        var lo=V(entry["min"]);var hi=V(entry["max"]);var p=V(entry["p"]);var s=V(entry["s"]);
        if(rect.anchorMin==lo && rect.anchorMax==hi && rect.anchoredPosition==p && rect.sizeDelta==s) continue;
        rect.anchorMin=lo;rect.anchorMax=hi;rect.anchoredPosition=p;rect.sizeDelta=s;
        UnityEditor.EditorUtility.SetDirty(rect);
        if(UnityEditor.PrefabUtility.IsPartOfPrefabInstance(rect)) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
        count++;
    }
    return count;
}
// Validate all paths before changing any asset.
foreach(var asset in plan) {
    string path=(string)asset["asset"];
    var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
    if(prefab==null) throw new System.Exception("Missing prefab "+path);
    foreach(var change in asset["changes"]) {
        string rp=(string)change["path"];
        if(rp!="" && prefab.transform.Find(rp.TrimStart('/'))==null) throw new System.Exception("Missing "+path+rp);
    }
}
var result=new System.Collections.Generic.List<object>();
foreach(var asset in plan) {
    string path=(string)asset["asset"];
    var root=UnityEditor.PrefabUtility.LoadPrefabContents(path);
    try {
        int count=Apply(root,(Newtonsoft.Json.Linq.JArray)asset["changes"]);
        if(count>0) UnityEditor.PrefabUtility.SaveAsPrefabAsset(root,path);
        result.Add(new {asset=path,rects=count});
    } finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
}
var router=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIScreenRouter>();
foreach(var asset in plan.Where(p=>((string)p["asset"]).Contains("/Screens/"))) {
    var name=System.IO.Path.GetFileNameWithoutExtension((string)asset["asset"]);
    var screen=router.Screens.First(s=>s.id.ToString()==name);
    Apply(screen.view,(Newtonsoft.Json.Linq.JArray)asset["changes"]);
}
var shell=(UnityEngine.RectTransform)router.Screens[0].view.transform.parent;
UnityEditor.Undo.RecordObject(shell,"Adapt UI root anchors");
shell.anchorMin=UnityEngine.Vector2.zero;shell.anchorMax=UnityEngine.Vector2.one;
shell.anchoredPosition=UnityEngine.Vector2.zero;shell.sizeDelta=UnityEngine.Vector2.zero;
router.Show(ClashRoyaleUI.ScreenId.BattleHome);
UnityEditor.EditorUtility.SetDirty(router);
foreach(var screen in router.Screens) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(screen.view);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(router.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(router.gameObject.scene);
System.IO.File.WriteAllText("DesignReference/Validation/Anchors/applied.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
return result;
