using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI.Editor
{
    // Explicit authoring command only. The saved scene contains every uGUI object.
    public static class ConfigureTaskBackend
    {
        private const string ScreenPath = "Assets/Prefabs/Screens/Tasks.prefab";
        private const string CardPath = "Assets/Prefabs/Components/TaskCard.prefab";
        private static void Reference(UnityEngine.Object owner, string field, UnityEngine.Object value)
        { var so = new SerializedObject(owner); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static RectTransform Rect(string name, Transform parent)
        { var go = new GameObject(name, typeof(RectTransform)); var r = (RectTransform)go.transform; r.SetParent(parent,false); return r; }
        private static void Box(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        { r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=position;r.sizeDelta=size;r.localScale=Vector3.one;r.localRotation=Quaternion.identity; }
        private static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, int size)
        {
            var r=Rect(name,parent);var text=r.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=size;text.color=new Color32(38,41,42,255);text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;text.overflowMode=TextOverflowModes.Overflow;return text;
        }
        private static Button MakeButton(string name, Transform parent, GameObject source, TMP_FontAsset font, string label, Vector2 pos, float width)
        {
            var go=UnityEngine.Object.Instantiate(source,parent);go.name=name;
            foreach(var a in go.GetComponents<UIAction>())UnityEngine.Object.DestroyImmediate(a);
            var b=go.GetComponent<Button>() ?? go.AddComponent<Button>();b.onClick=new Button.ButtonClickedEvent();b.targetGraphic=go.GetComponent<Image>();
            Box((RectTransform)go.transform,new Vector2(0,0),new Vector2(0,0),pos,new Vector2(width,32));
            var text=Text("Label",go.transform,font,15);text.text=label;text.alignment=TextAlignmentOptions.Center;
            var rt=text.rectTransform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
            return b;
        }
        [MenuItem("Clash Royale/创建任务卡片与后端绑定（一次）")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("请先停止运行。");
            var root=PrefabUtility.LoadPrefabContents(ScreenPath);
            try
            {
                if(root.GetComponent<UITaskBoard>())throw new InvalidOperationException("任务卡片已配置，请编辑现有预制体。");
                var scroll=root.GetComponentInChildren<ScrollRect>(true);var content=scroll.content;
                var old=content.Cast<Transform>().ToArray();
                var collapsed=old.First(x=>x.name.Contains("[81:306]")).gameObject;
                var gradient=old.First(x=>x.name.Contains("[71:213]")).GetComponent<Image>().sprite;
                var buttonSource=old.First(x=>x.name.Contains("[130:5]")).gameObject;
                var arrowSprite=old.First(x=>x.name.Contains("[130:14]")).GetComponent<Image>().sprite;
                var font=old.First(x=>x.name.Contains("[130:6]")).GetComponent<TMP_Text>().font;
                var legacy=Rect("OriginalTaskMockup · 原始设计保留",content);legacy.gameObject.SetActive(false);
                foreach(var t in old)t.SetParent(legacy,false);
                var contentLayout=content.GetComponent<LayoutElement>();if(contentLayout)UnityEngine.Object.DestroyImmediate(contentLayout);
                content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(0.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=Vector2.zero;
                var group=content.gameObject.AddComponent<VerticalLayoutGroup>();group.spacing=7;group.padding=new RectOffset(0,0,0,3);group.childControlWidth=true;group.childControlHeight=true;group.childForceExpandWidth=true;group.childForceExpandHeight=false;
                var fitter=content.GetComponent<ContentSizeFitter>();fitter.horizontalFit=ContentSizeFitter.FitMode.Unconstrained;fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
                var go=UnityEngine.Object.Instantiate(collapsed,content);go.name="TaskCard";go.SetActive(true);var card=go.AddComponent<UITaskCard>();
                var cardRect=(RectTransform)go.transform;cardRect.anchorMin=new Vector2(0,1);cardRect.anchorMax=new Vector2(1,1);cardRect.pivot=new Vector2(.5f,1);cardRect.anchoredPosition=Vector2.zero;cardRect.sizeDelta=new Vector2(0,104);
                foreach(var layer in go.GetComponents<FigmaLayer>())UnityEngine.Object.DestroyImmediate(layer);
                var background=go.GetComponent<Image>();background.raycastTarget=false;
                var layout=go.AddComponent<LayoutElement>();layout.preferredHeight=104;layout.minHeight=104;
                var brief=Text("Brief · 任务简述",go.transform,font,17);Box(brief.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-18),new Vector2(-136,64));brief.rectTransform.anchorMax=new Vector2(1,1);brief.alignment=TextAlignmentOptions.MidlineLeft;
                var accept=MakeButton("Accept · 接取状态",go.transform,buttonSource,font,"接取任务",Vector2.zero,83);Box((RectTransform)accept.transform,new Vector2(1,1),new Vector2(1,1),new Vector2(-20,-27),new Vector2(83,40));UnityEventTools.AddPersistentListener(accept.onClick,card.Accept);
                var details=Rect("Details · 展开内容",go.transform);details.anchorMin=Vector2.zero;details.anchorMax=Vector2.one;details.offsetMin=details.offsetMax=Vector2.zero;
                var description=Text("Description · 详细描述",details,font,16);Box(description.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-87),new Vector2(-32,83));description.rectTransform.anchorMax=new Vector2(1,1);
                var progress=Text("Progress · 进度与消耗",details,font,13);Box(progress.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-174),new Vector2(-32,22));progress.rectTransform.anchorMax=new Vector2(1,1);
                var help=MakeButton("ClanHelp · 部落求助",details,buttonSource,font,"部落求助",new Vector2(16,10),96);UnityEventTools.AddPersistentListener(help.onClick,card.RequestHelp);
                var activate=MakeButton("Activate · 激活任务",details,buttonSource,font,"激活任务",new Vector2(122,10),96);activate.GetComponent<Image>().color=new Color32(230,197,231,255);UnityEventTools.AddPersistentListener(activate.onClick,card.Activate);
                var expand=Rect("ExpandToggle · 三角展开收起",go.transform);Box(expand,new Vector2(1,0),new Vector2(1,0),new Vector2(-3,0),new Vector2(44,44));var hit=expand.gameObject.AddComponent<Image>();hit.color=Color.clear;var expandButton=expand.gameObject.AddComponent<Button>();expandButton.targetGraphic=hit;expandButton.transition=Selectable.Transition.None;UnityEventTools.AddPersistentListener(expandButton.onClick,card.ToggleExpanded);
                var triangle=Rect("Triangle · 圆角三角",expand);Box(triangle,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(18.9287f,17.1888f));var triangleImage=triangle.gameObject.AddComponent<Image>();triangleImage.sprite=arrowSprite;triangleImage.raycastTarget=false;
                Reference(card,"briefLabel",brief);Reference(card,"detailLabel",description);Reference(card,"progressLabel",progress);Reference(card,"statusLabel",accept.GetComponentInChildren<TMP_Text>());Reference(card,"helpLabel",help.GetComponentInChildren<TMP_Text>());
                Reference(card,"detailGroup",details.gameObject);Reference(card,"acceptButton",accept);Reference(card,"activateButton",activate);Reference(card,"helpButton",help);Reference(card,"expandButton",expandButton);Reference(card,"triangle",triangle);Reference(card,"background",background);Reference(card,"statusBackground",accept.GetComponent<Image>());Reference(card,"collapsedSprite",background.sprite);Reference(card,"expandedSprite",gradient);Reference(card,"layout",layout);
                var config=JsonUtility.FromJson<BackendState>(File.ReadAllText("Backend/Config/design-seed.json"));var sample=config.tasks[0];sample.status="available";card.Bind(sample,null,false);
                PrefabUtility.SaveAsPrefabAsset(go,CardPath);UnityEngine.Object.DestroyImmediate(go);
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(CardPath);var cards=new UITaskCard[62];
                for(int i=0;i<62;i++)
                {
                    var row=(GameObject)PrefabUtility.InstantiatePrefab(asset,content);row.name="TaskCard "+(i+1).ToString("00");cards[i]=row.GetComponent<UITaskCard>();
                    var sampleData=config.tasks[i];sampleData.status=i==1?"accepted":i==2?"active":"available";cards[i].Bind(sampleData,null,i==0);row.SetActive(i<3);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(cards[i]);
                }
                var board=root.AddComponent<UITaskBoard>();var so=new SerializedObject(board);so.FindProperty("cards").arraySize=cards.Length;
                for(int i=0;i<cards.Length;i++)so.FindProperty("cards").GetArrayElementAtIndex(i).objectReferenceValue=cards[i];
                so.FindProperty("sort").objectReferenceValue=root.GetComponentInChildren<UIRuleDropdown>(true);so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root,ScreenPath);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
            var router=UnityEngine.Object.FindFirstObjectByType<UIScreenRouter>(FindObjectsInactive.Include);
            var client=router.GetComponent<UIBackendClient>() ?? router.gameObject.AddComponent<UIBackendClient>();
            var feedback=Text("BackendFeedback · 操作提示",router.transform,AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansSC SDF.asset"),15);
            Box(feedback.rectTransform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,82),new Vector2(350,40));feedback.alignment=TextAlignmentOptions.Center;feedback.gameObject.SetActive(false);Reference(client,"feedback",feedback);
            router.Show(ScreenId.BattleHome);EditorSceneManager.MarkSceneDirty(router.gameObject.scene);EditorSceneManager.SaveScene(router.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("已保存62个原生uGUI任务卡片及后端绑定；运行时仅更新内容与状态。");
        }
    }
}
