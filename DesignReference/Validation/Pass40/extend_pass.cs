if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Exit Play Mode before editing the serialized prefab.");
const string assetPath="Assets/ClashRoyaleUI/Prefabs/Screens/BattlePass.prefab";
const int totalLevels=40;
var root=UnityEditor.PrefabUtility.LoadPrefabContents(assetPath);
try {
    var scroll=root.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
    if(scroll==null || scroll.content.childCount!=5)throw new System.Exception("Expected the existing five reward rows. No changes saved.");
    var content=scroll.content;
    var rows=content.Cast<UnityEngine.Transform>().ToArray();
    var layers=root.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true);
    var axis=(UnityEngine.RectTransform)layers.Single(x=>x.nodeId=="158:68").transform;
    var badgeTemplate=layers.Single(x=>x.nodeId=="59:113").gameObject;
    var numberTemplate=layers.Single(x=>x.nodeId=="158:66").GetComponent<TMPro.TMP_Text>();
    var badgePos=((UnityEngine.RectTransform)badgeTemplate.transform).anchoredPosition;
    var numberPos=numberTemplate.rectTransform.anchoredPosition;
    var axisOffset=(UnityEngine.Vector2)content.InverseTransformPoint(axis.position);
    // All added objects are authored here once, then saved into the prefab.
    // No runtime generation, scroll controller, or per-frame layout script is needed.
    for(int level=6;level<=totalLevels;level++) {
        var template=rows[(level-1)%5];
        var row=UnityEngine.Object.Instantiate(template.gameObject,content,false);
        row.name="Lv"+level.ToString("00")+" · 未完成";
        foreach(var marker in row.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true).Where(x=>x.nodeId.EndsWith(";157:73"))) {
            bool isFree=false;
            for(var p=marker.transform.parent;p!=null && p!=row.transform;p=p.parent)
                if(p.name.StartsWith("免费奖励"))isFree=true;
            if(isFree)marker.gameObject.SetActive(false);
        }
        foreach(var layer in row.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true)) {
            layer.nodeId="local:pass:"+level+":"+layer.nodeId;
            layer.sourceName=System.Text.RegularExpressions.Regex.Replace(layer.sourceName??"","Lv[0-9]+","Lv"+level);
        }
        foreach(var tr in row.GetComponentsInChildren<UnityEngine.Transform>(true)) {
            if(tr==row.transform)continue;
            tr.name=System.Text.RegularExpressions.Regex.Replace(
                System.Text.RegularExpressions.Regex.Replace(tr.name,@"\s*\[[^\]]+\]$",""),"Lv[0-9]+","Lv"+level);
        }
        foreach(var graphic in row.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))graphic.raycastTarget=false;
        foreach(var changed in row.GetComponentsInChildren<UnityEngine.Component>(true))
            if(UnityEditor.PrefabUtility.IsPartOfPrefabInstance(changed))UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(changed);
    }
    // Keep the entire level axis inside the masked scrolling content. It overlays
    // the rows but does not participate in the VerticalLayoutGroup's row sizing.
    axis.SetParent(content,false);
    axis.name="等级轴与进度 · Lv1-Lv40 [158:68]";
    axis.anchorMin=new UnityEngine.Vector2(0,1);axis.anchorMax=new UnityEngine.Vector2(0,1);
    axis.anchoredPosition=axisOffset;
    axis.sizeDelta=new UnityEngine.Vector2(axis.sizeDelta.x,axis.sizeDelta.y+(totalLevels-5)*99f);
    var ignored=axis.gameObject.GetComponent<UnityEngine.UI.LayoutElement>()??axis.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
    ignored.ignoreLayout=true;
    for(int level=6;level<=totalLevels;level++) {
        float offset=(level-5)*99f;
        var badge=UnityEngine.Object.Instantiate(badgeTemplate,axis,false);
        badge.name="等级 "+level+" · 未完成·灰";
        ((UnityEngine.RectTransform)badge.transform).anchoredPosition=badgePos-new UnityEngine.Vector2(0,offset);
        var badgeLayer=badge.GetComponent<ClashRoyaleUI.FigmaLayer>();badgeLayer.nodeId="local:pass:badge:"+level;badgeLayer.sourceName=badge.name;
        var number=UnityEngine.Object.Instantiate(numberTemplate.gameObject,axis,false).GetComponent<TMPro.TMP_Text>();
        number.name="等级数字 "+level;number.text=level.ToString();
        number.rectTransform.anchoredPosition=numberPos-new UnityEngine.Vector2(0,offset);
        var numberLayer=number.GetComponent<ClashRoyaleUI.FigmaLayer>();numberLayer.nodeId="local:pass:number:"+level;numberLayer.sourceName=number.name;
    }
    foreach(var graphic in axis.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))graphic.raycastTarget=false;
    axis.SetAsLastSibling();
    content.anchorMin=new UnityEngine.Vector2(0,1);content.anchorMax=new UnityEngine.Vector2(1,1);
    content.pivot=new UnityEngine.Vector2(0,1);content.anchoredPosition=UnityEngine.Vector2.zero;
    content.sizeDelta=new UnityEngine.Vector2(0,totalLevels*99f);
    var layout=content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
    layout.childControlWidth=true;layout.childControlHeight=true;
    layout.childForceExpandWidth=false;layout.childForceExpandHeight=false;
    var fit=content.GetComponent<UnityEngine.UI.ContentSizeFitter>();
    fit.horizontalFit=UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
    fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
    scroll.horizontal=false;scroll.vertical=true;scroll.inertia=true;
    scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
    scroll.viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
    scroll.StopMovement();scroll.verticalNormalizedPosition=1;
    scroll.transform.parent.name="奖励列表 · Lv1-Lv40 [158:73]";
    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    int rowCount=content.Cast<UnityEngine.Transform>().Count(t=>t.GetComponent<UnityEngine.UI.LayoutElement>()?.ignoreLayout==false);
    int levelCount=axis.GetComponentsInChildren<TMPro.TMP_Text>(true).Count(t=>int.TryParse(t.text,out _));
    if(rowCount!=40||levelCount!=40)throw new System.Exception("Invalid row or level count "+rowCount+"/"+levelCount);
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(root,assetPath);
    return new{asset=assetPath,rows=rowCount,levelMarkers=levelCount,contentHeight=content.rect.height,axisInsideViewport=axis.IsChildOf(scroll.viewport),runtimeScriptsAdded=0};
} finally {UnityEditor.PrefabUtility.UnloadPrefabContents(root);}
