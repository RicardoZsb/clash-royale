using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

namespace ClashRoyaleUI.Editor
{
    // Explicit editor operation only: the resulting hierarchy is saved into the screen prefabs.
    public static class AddTaskRuleDropdowns
    {
        private const string Root = "Assets/";

        public static void Apply()
        {
            Add("Tasks", "148:2", "148:6", "TaskSortDropdown", 269, 103, 111, 45, true);
            Add("BattleHome", "148:3", "148:4", "TaskActivationDropdown", 139, 657, 117, 34, false);
            AssetDatabase.SaveAssets();
        }

        private static void Add(string screen, string triggerId, string labelId, string name,
            float x, float y, float width, float height, bool replaceCaption)
        {
            string path = Root + "Prefabs/Screens/" + screen + ".prefab";
            var page = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (page.GetComponentsInChildren<UIRuleDropdown>(true).Length > 0) return;
                var tags = page.GetComponentsInChildren<FigmaLayer>(true);
                var trigger = tags.Single(t => t.nodeId == triggerId).GetComponent<RectTransform>();
                var caption = tags.Single(t => t.nodeId == labelId).GetComponent<TMP_Text>();
                var root = Rect(name, page.transform, x, y, width, height);
                var menu = root.gameObject.AddComponent<UIRuleDropdown>();
                trigger.SetParent(root, false);
                trigger.anchoredPosition = Vector2.zero;
                caption.transform.SetParent(root, false);
                var captionRect = caption.GetComponent<RectTransform>();
                captionRect.anchoredPosition = replaceCaption ? new Vector2(8, -10) : new Vector2(49, -4);
                captionRect.sizeDelta = replaceCaption ? new Vector2(width - 29, 25) : new Vector2(64, 26);
                caption.raycastTarget = false;
                if (replaceCaption)
                {
                    caption.font = Font();
                    caption.fontSize = 16;
                    caption.enableAutoSizing = true;
                    caption.fontSizeMin = 12;
                    caption.fontSizeMax = 16;
                    caption.alignment = TextAlignmentOptions.MidlineLeft;
                    caption.textWrappingMode = TextWrappingModes.NoWrap;
                    caption.text = "按等级 ↑";
                }
                else
                {
                    // Keep the original icon and quantity visible above the newly interactive background.
                    var icon = tags.Single(t => t.nodeId == "148:5").GetComponent<RectTransform>();
                    icon.SetParent(root, false);
                    icon.anchoredPosition = new Vector2(11, -4);
                }
                var arrow = Label("ExpandArrow", root, width - 22, (height - 20) / 2, 14, 20, "▼", 14);
                var triggerButton = Button(trigger.gameObject);
                UnityEventTools.AddPersistentListener(triggerButton.onClick, menu.Toggle);

                var overlay = Rect("Popup", root, 0, 0, width, height);
                var canvas = overlay.gameObject.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100;
                overlay.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                var outside = Rect("OutsideClickToClose", overlay, -x, -y, 402, 874);
                var outsideImage = outside.gameObject.AddComponent<UnityEngine.UI.Image>();
                outsideImage.color = Color.clear;
                var outsideButton = Button(outside.gameObject);
                outsideButton.transition = UnityEngine.UI.Selectable.Transition.None;
                UnityEventTools.AddPersistentListener(outsideButton.onClick, menu.Close);

                const float panelWidth = 164, panelHeight = 106;
                var panel = Rect("OptionsPanel", overlay, width - panelWidth, height + 4, panelWidth, panelHeight);
                Image(panel.gameObject, "Panel_7_7_7_7_0_False", new Color32(224, 221, 211, 255), true);
                var border = Rect("Border", panel, 0, 0, panelWidth, panelHeight);
                Image(border.gameObject, "Panel_7_7_7_7_2_False", new Color32(40, 40, 40, 255), false);
                var layout = Rect("Options", panel, 5, 5, panelWidth - 10, panelHeight - 10);
                var group = layout.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                group.spacing = 2;
                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = true;
                group.childForceExpandHeight = false;
                string[] labels = { "按等级 ↑", "按等级 ↓", replaceCaption ? "按完成度分类" : "按完成度" };
                var backgrounds = new UnityEngine.UI.Image[3];
                for (int i = 0; i < labels.Length; i++)
                {
                    var option = Rect(((TaskOrderRule)i).ToString(), layout, 0, i * 32, panelWidth - 10, 30);
                    backgrounds[i] = Image(option.gameObject, "Panel_5_5_5_5_0_False",
                        i == 0 ? new Color32(255, 226, 125, 255) : new Color32(248, 246, 241, 255), true);
                    var element = option.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                    element.preferredHeight = 30;
                    var text = Label("Label", option, 12, 0, panelWidth - 34, 30, labels[i], 16);
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    UnityEventTools.AddIntPersistentListener(Button(option.gameObject).onClick, menu.SelectRule, i);
                }

                var serialized = new SerializedObject(menu);
                serialized.FindProperty("popup").objectReferenceValue = overlay.gameObject;
                serialized.FindProperty("caption").objectReferenceValue = replaceCaption ? caption : null;
                serialized.FindProperty("expandArrow").objectReferenceValue = arrow;
                var labelValues = serialized.FindProperty("optionLabels");
                var imageValues = serialized.FindProperty("optionBackgrounds");
                labelValues.arraySize = imageValues.arraySize = labels.Length;
                for (int i = 0; i < labels.Length; i++)
                {
                    labelValues.GetArrayElementAtIndex(i).stringValue = labels[i];
                    imageValues.GetArrayElementAtIndex(i).objectReferenceValue = backgrounds[i];
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                overlay.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject,
                    Root + "Prefabs/Components/" + name + ".prefab", InteractionMode.AutomatedAction);
                PrefabUtility.SaveAsPrefabAsset(page, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(page); }
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static UnityEngine.UI.Image Image(GameObject target, string spriteName, Color color, bool raycast)
        {
            var image = target.AddComponent<UnityEngine.UI.Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Generated/" + spriteName + ".png");
            if (!image.sprite) throw new FileNotFoundException("Missing UI sprite: " + spriteName);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static TMP_FontAsset Font() => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/NotoSansSC SDF.asset");

        private static TextMeshProUGUI Label(string name, Transform parent, float x, float y, float width, float height,
            string value, float size)
        {
            var text = Rect(name, parent, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Font();
            text.fontSize = size;
            text.text = value;
            text.color = new Color32(48, 48, 48, 255);
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static UnityEngine.UI.Button Button(GameObject target)
        {
            var image = target.GetComponent<UnityEngine.UI.Image>();
            image.raycastTarget = true;
            var button = target.GetComponent<UnityEngine.UI.Button>() ?? target.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(.88f, .86f, .81f, 1);
            button.colors = colors;
            return button;
        }
    }
}
