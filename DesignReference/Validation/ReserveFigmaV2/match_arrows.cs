const string basePath = "Assets/ClashRoyaleUI/Prefabs/Components/";
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Edit outside Play mode.");
var source = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(basePath + "ActionPointReserveDropdown.prefab");
var sourceArrow = source.transform.Find("ExpandArrow [214:118]");
var page = UnityEditor.PrefabUtility.LoadPrefabContents(basePath + "TaskActivationDropdown.prefab");
try
{
    var arrow = (UnityEngine.RectTransform)page.transform.Find("ExpandArrow");
    var controller = new UnityEditor.SerializedObject(page.GetComponent<ClashRoyaleUI.UIRuleDropdown>());
    controller.FindProperty("expandArrow").objectReferenceValue = null;
    controller.ApplyModifiedPropertiesWithoutUndo();
    var oldText = arrow.GetComponent<TMPro.TMP_Text>();
    if (oldText) UnityEngine.Object.DestroyImmediate(oldText);
    arrow.anchorMin = arrow.anchorMax = new UnityEngine.Vector2(.5f, 0);
    arrow.pivot = new UnityEngine.Vector2(.5f, .5f);
    arrow.anchoredPosition = new UnityEngine.Vector2(36, 200);
    arrow.sizeDelta = new UnityEngine.Vector2(22, 22);
    arrow.localRotation = UnityEngine.Quaternion.Euler(0, 0, 180);
    if (!arrow.Find("FigmaTriangle"))
    {
        var art = UnityEngine.Object.Instantiate(sourceArrow.Find("FigmaTriangle").gameObject, arrow, false);
        art.name = "FigmaTriangle";
    }
    var tag = arrow.GetComponent<ClashRoyaleUI.FigmaLayer>() ?? arrow.gameObject.AddComponent<ClashRoyaleUI.FigmaLayer>();
    tag.nodeId = "181:49";
    tag.sourceName = "Polygon 5";
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(page, basePath + "TaskActivationDropdown.prefab");
    return new { size = "22x22", sameGraphic = arrow.Find("FigmaTriangle").GetComponent<UnityEngine.UI.Image>().sprite == sourceArrow.Find("FigmaTriangle").GetComponent<UnityEngine.UI.Image>().sprite };
}
finally { UnityEditor.PrefabUtility.UnloadPrefabContents(page); }
