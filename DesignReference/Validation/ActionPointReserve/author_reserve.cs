
const string basePath = "Assets/ClashRoyaleUI/";
const string pagePath = basePath + "Prefabs/Screens/BattleHome.prefab";
const string componentPath = basePath + "Prefabs/Components/ActionPointReserveDropdown.prefab";
const string arrowPath = basePath + "Art/ReserveDropdownArrow.png";
if (EditorApplication.isPlaying) throw new Exception("Author prefabs in edit mode only.");
AssetDatabase.ImportAsset(arrowPath, ImportAssetOptions.ForceSynchronousImport);
var importer = (TextureImporter)AssetImporter.GetAtPath(arrowPath);
importer.textureType = TextureImporterType.Sprite;
importer.spriteImportMode = SpriteImportMode.Single;
importer.alphaIsTransparency = true;
importer.mipmapEnabled = false;
importer.textureCompression = TextureImporterCompression.Uncompressed;
importer.SaveAndReimport();
var page = PrefabUtility.LoadPrefabContents(pagePath);
try
{
    if (page.GetComponentInChildren<ClashRoyaleUI.UIActionPointReserve>(true)) throw new Exception("Reserve dropdown already exists.");
    var layers = page.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true);
    var trigger = layers.Single(x => x.nodeId == "71:212").GetComponent<RectTransform>();
    var label = layers.Single(x => x.nodeId == "71:208").GetComponent<TMPro.TMP_Text>();
    var icon = layers.Single(x => x.nodeId == "70:188").GetComponent<RectTransform>();
    var root = Stretch("ActionPointReserveDropdown", page.transform);
    var controller = root.gameObject.AddComponent<ClashRoyaleUI.UIActionPointReserve>();

    trigger.SetParent(root, false);
    BottomRect(trigger, 139, 581, 117, 34);
    label.transform.SetParent(root, false);
    BottomRect(label.rectTransform, 172, 587, 56, 26);
    label.fontSize = 20;
    label.enableAutoSizing = false;
    label.alignment = TMPro.TextAlignmentOptions.TopLeft;
    label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
    label.raycastTarget = false;

    // Preserve the exact Figma image asset and its crop/scale inside the 23 x 27 slot.
    var iconSlot = Rect("ActionPointIconSlot", root, 0, 0, 23, 27);
    BottomRect(iconSlot, 150, 586, 23, 27);
    iconSlot.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
    icon.SetParent(iconSlot, false);
    LocalRect(icon, 0, 0, 23 * 1.0968f, 27 * .9189f);
    icon.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;

    var arrow = Rect("ExpandArrow [214:118]", root, 0, 0, 22, 22);
    BottomRect(arrow, 225, 587, 22, 22);
    arrow.pivot = new Vector2(.5f, .5f);
    arrow.anchoredPosition += new Vector2(11, -11);
    arrow.localRotation = Quaternion.Euler(0, 0, 180);
    var arrowTag = arrow.gameObject.AddComponent<ClashRoyaleUI.FigmaLayer>();
    arrowTag.nodeId = "214:118";
    arrowTag.sourceName = "Polygon 5";
    var arrowArt = Rect("FigmaTriangle", arrow, 2.93485f, 2, 16.1303f, 14.5f);
    var arrowImage = arrowArt.gameObject.AddComponent<UnityEngine.UI.Image>();
    arrowImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(arrowPath);
    arrowImage.raycastTarget = false;
    UnityEditor.Events.UnityEventTools.AddPersistentListener(Button(trigger.gameObject).onClick, controller.Toggle);

    var popup = Stretch("Popup", root);
    var popupCanvas = popup.gameObject.AddComponent<Canvas>();
    popupCanvas.overrideSorting = false;
    popup.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    var outside = Stretch("OutsideClickToClose", popup);
    var outsideImage = outside.gameObject.AddComponent<UnityEngine.UI.Image>();
    outsideImage.color = Color.clear;
    var outsideButton = Button(outside.gameObject);
    outsideButton.transition = UnityEngine.UI.Selectable.Transition.None;
    UnityEditor.Events.UnityEventTools.AddPersistentListener(outsideButton.onClick, controller.Close);

    var panel = Rect("ReservePanel", popup, 0, 0, 164, 116);
    BottomRect(panel, 92, 619, 164, 116);
    Image(panel, "Panel_7_7_7_7_0_False", new Color32(224, 221, 211, 255), true);
    var border = Stretch("Border", panel);
    Image(border, "Panel_7_7_7_7_2_False", new Color32(40, 40, 40, 255), false);
    var balance = Rect("Balance", panel, 5, 5, 154, 66);
    Image(balance, "Panel_5_5_5_5_0_False", new Color32(248, 246, 241, 255), false);
    Label("Title", balance, 4, 3, 146, 24, "储备池结余", 16);
    var reserveLabel = Label("ReserveBalance", balance, 4, 29, 146, 30, "16 点行动点", 20);
    var withdraw = Rect("WithdrawButton", panel, 5, 77, 154, 34);
    Image(withdraw, "Panel_5_5_5_5_0_False", new Color32(255, 226, 125, 255), true);
    var withdrawLabel = Label("Label", withdraw, 4, 0, 146, 34, "提取", 16);
    var withdrawButton = Button(withdraw.gameObject);
    UnityEditor.Events.UnityEventTools.AddPersistentListener(withdrawButton.onClick, controller.Withdraw);

    var serialized = new SerializedObject(controller);
    serialized.FindProperty("popup").objectReferenceValue = popup.gameObject;
    serialized.FindProperty("expandArrow").objectReferenceValue = arrow;
    serialized.FindProperty("actionPointLabel").objectReferenceValue = label;
    serialized.FindProperty("reserveLabel").objectReferenceValue = reserveLabel;
    serialized.FindProperty("withdrawLabel").objectReferenceValue = withdrawLabel;
    serialized.FindProperty("withdrawButton").objectReferenceValue = withdrawButton;
    serialized.ApplyModifiedPropertiesWithoutUndo();
    popup.gameObject.SetActive(false);
    PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject, componentPath, InteractionMode.AutomatedAction);
    PrefabUtility.SaveAsPrefabAsset(page, pagePath);
    return new { prefab = componentPath, entryNode = "71:212", arrowNode = "214:118", reservePoints = 16, mainSceneSaved = false };
}
finally { PrefabUtility.UnloadPrefabContents(page); }

RectTransform Stretch(string name, Transform parent)
{
    var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
    r.SetParent(parent, false);
    r.anchorMin = Vector2.zero;
    r.anchorMax = Vector2.one;
    r.offsetMin = r.offsetMax = Vector2.zero;
    return r;
}
RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
{
    var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
    r.SetParent(parent, false);
    LocalRect(r, x, y, w, h);
    return r;
}
void LocalRect(RectTransform r, float x, float y, float w, float h)
{
    r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
    r.anchoredPosition = new Vector2(x, -y);
    r.sizeDelta = new Vector2(w, h);
}
void BottomRect(RectTransform r, float x, float y, float w, float h)
{
    r.anchorMin = r.anchorMax = new Vector2(.5f, 0);
    r.pivot = new Vector2(0, 1);
    r.anchoredPosition = new Vector2(x - 201, 874 - y);
    r.sizeDelta = new Vector2(w, h);
}
UnityEngine.UI.Image Image(RectTransform r, string sprite, Color color, bool raycast)
{
    var i = r.gameObject.AddComponent<UnityEngine.UI.Image>();
    i.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Art/Generated/" + sprite + ".png");
    i.type = UnityEngine.UI.Image.Type.Sliced;
    i.color = color;
    i.raycastTarget = raycast;
    return i;
}
TMPro.TextMeshProUGUI Label(string name, Transform parent, float x, float y, float w, float h, string value, float size)
{
    var label = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TMPro.TextMeshProUGUI>();
    label.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(basePath + "Fonts/NotoSansSC SDF.asset");
    label.fontSize = size;
    label.text = value;
    label.color = new Color32(48, 48, 48, 255);
    label.alignment = TMPro.TextAlignmentOptions.Center;
    label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
    label.raycastTarget = false;
    return label;
}
UnityEngine.UI.Button Button(GameObject go)
{
    var b = go.AddComponent<UnityEngine.UI.Button>();
    b.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();
    b.targetGraphic.raycastTarget = true;
    b.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
    var c = b.colors;
    c.highlightedColor = Color.white;
    c.pressedColor = new Color(.88f, .86f, .81f, 1);
    b.colors = c;
    return b;
}
