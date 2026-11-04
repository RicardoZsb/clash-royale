const string rootPath = "Assets/ClashRoyaleUI/";
const string path = rootPath + "Prefabs/Components/ActionPointReserveDropdown.prefab";
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Edit prefab outside Play mode.");
var shape = typeof(ClashRoyaleUI.Editor.PolishClashRoyaleUI).GetMethod("Shape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
var bottomSprite = (UnityEngine.Sprite)shape.Invoke(null, new object[] { new float[] { 0, 1, 9, 9 }, 0f, false });
var prefab = UnityEditor.PrefabUtility.LoadPrefabContents(path);
try
{
    var panel = (UnityEngine.RectTransform)prefab.transform.Find("Popup/ReservePanel");
    panel.anchoredPosition = new UnityEngine.Vector2(70 - 201, 874 - 615);
    panel.sizeDelta = new UnityEngine.Vector2(187, 118);
    var panelImage = panel.GetComponent<UnityEngine.UI.Image>();
    panelImage.sprite = Sprite("Panel_11_11_11_11_0_False");
    panelImage.color = new UnityEngine.Color32(210, 200, 200, 255);
    Tag(panel, "216:119", "Rectangle 77");
    var border = panel.Find("Border").GetComponent<UnityEngine.UI.Image>();
    border.sprite = Sprite("Panel_11_11_11_11_2_False");
    border.color = UnityEngine.Color.black;

    var title = panel.Find("Balance/Title").GetComponent<TMPro.TMP_Text>();
    title.transform.SetParent(panel, false);
    Text(title, 54, 8, 72, 29, "储备池", 24);
    Tag(title.transform, "216:120", "储备池");
    var balance = (UnityEngine.RectTransform)panel.Find("Balance");
    Rect(balance, 33, 38, 121, 34);
    balance.GetComponent<UnityEngine.UI.Image>().sprite = Sprite("Panel_11_11_11_11_0_False");
    balance.GetComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color32(217, 217, 217, 255);
    Tag(balance, "216:122", "Rectangle 78");
    var amount = balance.Find("ReserveBalance").GetComponent<TMPro.TMP_Text>();
    Text(amount, 28, 3, 61, 29, "16/16", 24);
    Tag(amount.transform, "216:124", "16/16");

    var withdraw = (UnityEngine.RectTransform)panel.Find("WithdrawButton");
    Rect(withdraw, 2, 76, 183, 40);
    withdraw.GetComponent<UnityEngine.UI.Image>().sprite = bottomSprite;
    withdraw.GetComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color32(233, 193, 168, 255);
    Tag(withdraw, "216:123", "Rectangle 79");
    var label = withdraw.Find("Label").GetComponent<TMPro.TMP_Text>();
    Text(label, 42, 8, 100, 24, "提取行动点", 20);
    Tag(label.transform, "216:125", "提取行动点");
    var button = withdraw.GetComponent<UnityEngine.UI.Button>();
    var colors = button.colors;
    colors.disabledColor = UnityEngine.Color.white;
    button.colors = colors;
    button.interactable = true;
    border.transform.SetAsLastSibling();
    var state = new UnityEditor.SerializedObject(prefab.GetComponent<ClashRoyaleUI.UIActionPointReserve>());
    state.FindProperty("reserveCapacity").intValue = 16;
    state.ApplyModifiedPropertiesWithoutUndo();
    prefab.transform.Find("Popup").gameObject.SetActive(false);
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefab, path);
    return new { path, figmaNodes = new[] { "216:119", "216:120", "216:122", "216:123", "216:124", "216:125" } };
}
finally { UnityEditor.PrefabUtility.UnloadPrefabContents(prefab); }

UnityEngine.Sprite Sprite(string name) => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(rootPath + "Art/Generated/" + name + ".png");
void Rect(UnityEngine.RectTransform r, float x, float y, float width, float height)
{
    r.anchorMin = r.anchorMax = r.pivot = new UnityEngine.Vector2(0, 1);
    r.anchoredPosition = new UnityEngine.Vector2(x, -y);
    r.sizeDelta = new UnityEngine.Vector2(width, height);
}
void Text(TMPro.TMP_Text t, float x, float y, float width, float height, string text, float size)
{
    Rect(t.rectTransform, x, y, width, height);
    t.text = text;
    t.font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(rootPath + "Fonts/Inter SDF.asset");
    t.fontSize = size;
    t.color = UnityEngine.Color.black;
    t.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
    t.enableAutoSizing = false;
    t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
    t.margin = UnityEngine.Vector4.zero;
    t.raycastTarget = false;
}
void Tag(UnityEngine.Transform target, string id, string name)
{
    var tag = target.GetComponent<ClashRoyaleUI.FigmaLayer>() ?? target.gameObject.AddComponent<ClashRoyaleUI.FigmaLayer>();
    tag.nodeId = id;
    tag.sourceName = name;
}
