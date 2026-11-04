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
    public static class ConfigureRewards
    {
        static Sprite Sprite(string path)=>AssetDatabase.LoadAssetAtPath<Sprite>(path);
        static RectTransform Rect(string name,Transform parent){var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);return r;}
        static void Box(RectTransform r,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size){r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;}
        static void Stretch(RectTransform r,float left=0,float bottom=0,float right=0,float top=0){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
        static Image Image(string name,Transform parent,Sprite sprite,Color color){var r=Rect(name,parent);var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.color=color;im.raycastTarget=false;return im;}
        static Button Button(Image image,UnityEngine.Events.UnityAction action){image.raycastTarget=true;var button=image.GetComponent<Button>()??image.gameObject.AddComponent<Button>();button.targetGraphic=image;button.transition=Selectable.Transition.None;button.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(button.onClick,action);return button;}
        static TMP_Text Text(Transform parent,TMP_FontAsset font){var r=Rect("Quantity",parent);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=24;t.alignment=TextAlignmentOptions.Center;t.color=new Color32(36,36,36,255);t.raycastTarget=false;return t;}
        static Transform Find(GameObject root,string text)=>root.GetComponentsInChildren<Transform>(true).First(t=>t.name.Contains(text));
        static UIRewardArt Art()
        {
            AssetDatabase.Refresh();
            foreach(var file in Directory.GetFiles("Assets/Art/Rewards","*.png"))
            {var importer=(TextureImporter)AssetImporter.GetAtPath(file);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=1024;importer.SaveAndReimport();}
            const string path="Assets/Tables/RewardArt.asset";var art=AssetDatabase.LoadAssetAtPath<UIRewardArt>(path);
            if(!art){art=ScriptableObject.CreateInstance<UIRewardArt>();AssetDatabase.CreateAsset(art,path);}
            art.gold=Sprite("Assets/Art/Rewards/reward_coin_chest.png");art.gems=Sprite("Assets/Art/Rewards/reward_gem_bag.png");
            art.common=Sprite("Assets/Art/Rewards/wildcard_common.png");art.rare=Sprite("Assets/Art/Rewards/wildcard_rare.png");art.epic=Sprite("Assets/Art/Rewards/wildcard_epic.png");art.legendary=Sprite("Assets/Art/Rewards/wildcard_legendary.png");
            art.chests=new[]{"1star_white","2star_blue","3star_purple","4star_gold","5star_red"}.Select(n=>Sprite("Assets/Art/Rewards/mystery_chest_"+n+".png")).ToArray();
            art.action=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Screens/BattlePass.prefab").GetComponent<UIPassDataBinding>().actionIcon;
            EditorUtility.SetDirty(art);return art;
        }
        [MenuItem("Clash Royale/更新宝箱与战令组件")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before authoring.");
            var art=Art();var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansSC SDF.asset");
            foreach(var name in new[]{"BattlePass","ChestQueue","ChestReveal"})
            {
                string path="Assets/Prefabs/Screens/"+name+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
                try{if(name=="BattlePass")Pass(root,art);else if(name=="ChestQueue")Queue(root,art);else Reveal(root,art,font);PrefabUtility.SaveAsPrefabAsset(root,path);}
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            var router=UnityEngine.Object.FindFirstObjectByType<UIScreenRouter>(FindObjectsInactive.Include);
            var feedback=router.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.StartsWith("BackendFeedback"));
            if(feedback)UnityEngine.Object.DestroyImmediate(feedback.gameObject);
            var client=router.GetComponent<UIBackendClient>();var so=new SerializedObject(client);so.FindProperty("feedback").objectReferenceValue=null;so.ApplyModifiedPropertiesWithoutUndo();
            router.Show(ScreenId.BattleHome);EditorSceneManager.MarkSceneDirty(router.gameObject.scene);EditorSceneManager.SaveScene(router.gameObject.scene);AssetDatabase.SaveAssets();
        }
        static void Pass(GameObject root,UIRewardArt art)
        {
            var binding=root.GetComponent<UIPassDataBinding>();binding.art=art;
            ConfigureFreeRewardLocks(root);
            ConfigurePassClaims(root);
            foreach(var row in binding.rows)
            {
                row.premiumLockIcon=row.premiumLock.transform.GetChild(0).gameObject;
                foreach(var image in new[]{row.freeIcon,row.premiumIcon}){Box(image.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-4),new Vector2(64,64));image.preserveAspect=true;image.raycastTarget=false;}
                foreach(var t in new[]{row.freeQuantity,row.premiumQuantity}){t.fontSize=13;t.text="";t.raycastTarget=false;}
            }
            var unlock=Find(root,"[59:95]").GetComponent<UIAction>();unlock.action=UIActionKind.UnlockPassPreview;
            binding.unlockButton=Button(unlock.GetComponent<Image>(),unlock.Execute);binding.unlockIcon=Find(root,"[167:57]").gameObject;
            binding.unlockIcon.GetComponent<Image>().raycastTarget=false;
            var claim=Find(root,"[59:118]").GetComponent<UIAction>();claim.action=UIActionKind.ClaimFreeRewards;Button(claim.GetComponent<Image>(),claim.Execute);
            foreach(var row in binding.rows)if(row.premiumLock)
            {var im=row.premiumLock.GetComponent<Image>();if(im){var action=im.GetComponent<UIAction>()??im.gameObject.AddComponent<UIAction>();action.action=UIActionKind.UnlockPassPreview;Button(im,action.Execute);}}
            var preview=JsonUtility.FromJson<BackendState>(File.ReadAllText("Backend/Config/design-seed.json"));
            preview.player=new PlayerData{pass_level=0,premium=0};preview.pass_claims=Array.Empty<PassClaimData>();binding.Apply(preview);
        }
        public static void ConfigureFreeRewardLocks(GameObject root)
        {
            foreach(var row in root.GetComponent<UIPassDataBinding>().rows)
            {
                var existing=row.freeMarker.transform.Find("Lock");
                var source=row.premiumLockIcon.GetComponent<UnityEngine.UI.Image>();
                var image=existing?existing.GetComponent<UnityEngine.UI.Image>():Image("Lock",row.freeMarker.transform,source.sprite,source.color);
                var target=image.rectTransform;var reference=source.rectTransform;
                target.anchorMin=reference.anchorMin;target.anchorMax=reference.anchorMax;target.pivot=reference.pivot;
                target.anchoredPosition=reference.anchoredPosition;target.sizeDelta=reference.sizeDelta;
                image.preserveAspect=source.preserveAspect;image.raycastTarget=false;
                row.freeLockIcon=image.gameObject;
            }
        }
        public static void ConfigurePassClaims(GameObject root)
        {
            var binding=root.GetComponent<UIPassDataBinding>();
            foreach(var row in binding.rows)
            {
                Button Claim(UnityEngine.UI.Image icon,string track)
                {
                    var action=icon.GetComponent<UIPassRewardClaim>()??icon.gameObject.AddComponent<UIPassRewardClaim>();
                    action.level=row.level;action.track=track;return Button(icon,action.Claim);
                }
                row.freeClaim=Claim(row.freeIcon,"FREE");row.premiumClaim=Claim(row.premiumIcon,"PREMIUM");
            }
            if(binding.rewardPopup)return;
            var overlay=Image("RewardPopup",root.transform,null,new Color(0,0,0,.36f));Stretch(overlay.rectTransform);
            var popup=overlay.gameObject.AddComponent<UIRewardPopup>();binding.rewardPopup=popup;popup.art=binding.art;popup.dismiss=Button(overlay,popup.Close);
            var panel=Image("Panel",overlay.transform,Sprite("Assets/Art/Generated/Panel_13_13_13_13_0_False.png"),new Color32(240,231,206,255));
            panel.type=UnityEngine.UI.Image.Type.Sliced;Box(panel.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(210,210));popup.panel=panel.rectTransform;
            var outline=Image("Outline",panel.transform,Sprite("Assets/Art/Generated/Panel_13_13_13_13_2_False.png"),Color.black);outline.type=UnityEngine.UI.Image.Type.Sliced;Stretch(outline.rectTransform);
            popup.icons=new UnityEngine.UI.Image[4];popup.amounts=new TMP_Text[4];
            for(int i=0;i<4;i++)
            {
                var slot=Rect("Item "+(i+1),panel.transform);Box(slot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(140,164));
                var icon=Image("Icon",slot,binding.art.gold,Color.white);icon.preserveAspect=true;Box(icon.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),Vector2.zero,new Vector2(128,128));
                var quantity=Text(slot,binding.rows[0].freeQuantity.font);Box(quantity.rectTransform,new Vector2(.5f,0),new Vector2(.5f,0),Vector2.zero,new Vector2(140,32));
                popup.icons[i]=icon;popup.amounts[i]=quantity;
            }
            popup.Close();
        }
        static void Queue(GameObject root,UIRewardArt art)
        {
            foreach(var n in new[]{"Merge_3_Star_Pair_To_4_Star","Open_2_Star_Chest"}){var legacy=root.transform.Find(n);if(legacy)legacy.gameObject.SetActive(false);}
            var old=root.transform.Find("ChestScrollView");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var scrollRect=Rect("ChestScrollView",root.transform);Stretch(scrollRect,20,94,20,161);
            var scroll=scrollRect.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
            var viewport=Rect("Viewport",scrollRect);Stretch(viewport);var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var content=Rect("Content",viewport);Box(content,new Vector2(0,1),new Vector2(.5f,1),Vector2.zero,Vector2.zero);content.anchorMax=new Vector2(1,1);
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=10;layout.childControlHeight=layout.childControlWidth=true;layout.childForceExpandHeight=false;
            var fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport;scroll.content=content;
            var view=root.GetComponent<UIChestDataView>();view.rows=new UIChestQueueRow[20];view.slots=Array.Empty<GameObject>();view.panels=Array.Empty<GameObject>();view.stars=Array.Empty<TMP_Text>();
            for(int i=0;i<20;i++)
            {
                var bg=Image("Chest "+(i+1).ToString("00"),content,Sprite("Assets/Art/Generated/Panel_13_13_13_13_0_False.png"),art.Color(i%5+1));bg.type=UnityEngine.UI.Image.Type.Sliced;
                var le=bg.gameObject.AddComponent<LayoutElement>();le.preferredHeight=104;le.minHeight=104;
                var border=Image("Outline",bg.transform,Sprite("Assets/Art/Generated/Panel_13_13_13_13_2_False.png"),Color.black);border.type=UnityEngine.UI.Image.Type.Sliced;Stretch(border.rectTransform);
                var icon=Image("ChestIcon",bg.transform,art.Chest(i%5+1),Color.white);icon.preserveAspect=true;Box(icon.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(100,100));
                var row=bg.gameObject.AddComponent<UIChestQueueRow>();row.art=art;row.background=bg;row.icon=icon;Button(bg,row.Open);view.rows[i]=row;bg.gameObject.SetActive(i<3);
            }
        }
        static void Reveal(GameObject root,UIRewardArt art,TMP_FontAsset font)
        {
            foreach(var child in root.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
            var old=root.GetComponent<UIChestDataView>();if(old)UnityEngine.Object.DestroyImmediate(old);
            var opening=root.GetComponent<UIChestOpening>()??root.AddComponent<UIChestOpening>();opening.art=art;
            var bg=Image("Background",root.transform,null,art.Color(1));Stretch(bg.rectTransform);opening.background=bg;opening.tap=Button(bg,opening.Tap);
            var chest=Image("Chest",root.transform,art.Chest(1),Color.white);Box(chest.rectTransform,new Vector2(.5f,.55f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(192,192));chest.preserveAspect=true;opening.chest=chest;
            opening.questionSprite=Sprite("Assets/Art/Generated/Crop_79.asset");opening.spentSprite=Sprite("Assets/Art/Generated/Panel_21_21_21_21_0_False.png");opening.questionMarks=new Image[4];
            for(int i=0;i<4;i++){var q=Image("Roll "+(i+1),root.transform,opening.questionSprite,Color.white);Box(q.rectTransform,new Vector2(.5f,.2f),new Vector2(.5f,.5f),new Vector2((i-1.5f)*54,0),new Vector2(44,44));q.preserveAspect=true;opening.questionMarks[i]=q;}
            var rewards=Rect("Rewards",root.transform);Box(rewards,new Vector2(.5f,.52f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(360,220));opening.rewardPanel=rewards;opening.rewardIcons=new Image[4];opening.rewardAmounts=new TMP_Text[4];
            for(int i=0;i<4;i++)
            {var slot=Rect("Reward "+(i+1),rewards);Box(slot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(144,200));var icon=Image("Icon",slot,art.gold,Color.white);icon.preserveAspect=true;Box(icon.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),Vector2.zero,new Vector2(144,144));var amount=Text(slot,font);Box(amount.rectTransform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,12),new Vector2(144,40));opening.rewardIcons[i]=icon;opening.rewardAmounts[i]=amount;}
            rewards.gameObject.SetActive(false);
        }
    }
}
