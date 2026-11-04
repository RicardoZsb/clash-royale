using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ClashRoyaleUI.Editor
{
    public static class BuildClashRoyaleUI
    {
        const string Root="Assets";
        static readonly Dictionary<string, Sprite> spriteCache=new Dictionary<string, Sprite>();
        static readonly Dictionary<string, TMP_FontAsset> fonts=new Dictionary<string, TMP_FontAsset>();
        static readonly Dictionary<string, GameObject> objects=new Dictionary<string, GameObject>();
        static readonly Dictionary<string, DesignNode> nodes=new Dictionary<string, DesignNode>();
        static Dictionary<string, GameObject> pages;
        static UIPrototypeState demo;

        [MenuItem("Clash Royale UI/打开还原场景")]
        public static void OpenScene() { EditorSceneManager.OpenScene(Root+"/Scenes/ClashRoyaleUI.unity"); }

        // Explicit editor-only command. Never runs on scene load, Awake, Start or entering Play mode.
        [MenuItem("Clash Royale UI/从设计快照创建场景（仅首次）")]
        public static void Build()
        {
            if(File.Exists(Root+"/Scenes/ClashRoyaleUI.unity")) throw new InvalidOperationException("场景已存在，请直接编辑场景或预制体；导入器不会覆盖你的调整。");
            foreach(var dir in new[]{"Scenes","Prefabs/Screens","Prefabs/Components","Art/Generated","Fonts"}) Directory.CreateDirectory(Root+"/"+dir);
            AssetDatabase.Refresh();
            var data=JsonUtility.FromJson<DesignData>(File.ReadAllText(Root+"/Tables/DesignData.json"));
            PrepareImages();PrepareFonts(data);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraGO=new GameObject("UI Camera",typeof(Camera));var camera=cameraGO.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(44,47,50,255);camera.orthographic=true;cameraGO.tag="MainCamera";
            var canvasGO=new GameObject("ClashRoyaleCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGO.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(402,874);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var shell=Rect("Interface · 402 × 874",canvasGO.transform,0,0,402,874);shell.anchorMin=shell.anchorMax=shell.pivot=new Vector2(.5f,.5f);shell.anchoredPosition=Vector2.zero;
            var router=shell.gameObject.AddComponent<UIScreenRouter>();demo=shell.gameObject.AddComponent<UIPrototypeState>();demo.router=router;
            var eventGO=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));eventGO.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            pages=new Dictionary<string,GameObject>();
            foreach(var screen in data.screens)
            {
                var root=Rect(screen.key+" · "+screen.name,shell,0,0,402,874);root.gameObject.AddComponent<RectMask2D>();pages[screen.key]=root.gameObject;
                var header=Rect("Header",root,0,0,402,874);var content=Rect("Content",root,0,0,402,874);var footer=Rect("BottomNavigation",root,0,0,402,874);
                foreach(var node in screen.nodes)
                {
                    nodes[node.id]=node;
                    if(node.id==screen.id){objects[node.id]=root.gameObject;Draw(root.gameObject,node);continue;}
                    Transform parent;float px=0,py=0;
                    if(node.parent!=screen.id&&objects.ContainsKey(node.parent)){parent=objects[node.parent].transform;var pn=nodes[node.parent];px=pn.x;py=pn.y;}
                    else {parent=node.y>=804||node.name=="底部导航"?footer:node.y<85&&node.name!="战令概览 / 等级、进度与开通入口"&&node.h<100?header:content;}
                    var rect=Rect(SafeName(node.name)+" ["+node.id+"]",parent,node.x-px,node.y-py,node.w,node.h);rect.localRotation=Quaternion.Euler(0,0,node.rotation);
                    objects[node.id]=rect.gameObject;var tag=rect.gameObject.AddComponent<FigmaLayer>();tag.nodeId=node.id;tag.sourceName=node.name;Draw(rect.gameObject,node);
                }
                // Empty visual containers stay editable and don't intercept clicks.
                header.SetAsLastSibling();footer.SetAsLastSibling();
                FooterActions(screen);
            }
            ConfigurePass();ConfigureClanList();ConfigureTasks();ConfigureChests();ConfigureClanNavigation();ConfigureChat();
            Navigate("7:23",ScreenId.BattlePass);Navigate("7:19",ScreenId.Tasks);Navigate("16:44",ScreenId.ChestQueue);
            Navigate("30:105",ScreenId.ClanShop);Navigate("30:104",ScreenId.ClanCompetition);Navigate("30:96",ScreenId.ClanChat);
            foreach(var key in new[]{"Tasks","BattlePass","ChestQueue"})
            { var screen=data.screens.First(s=>s.key==key);var tabs=screen.nodes.Where(n=>n.type=="RECTANGLE"&&Mathf.Abs(n.y-39)<1&&Mathf.Abs(n.h-44)<1).OrderBy(n=>n.x).ToArray();if(tabs.Length==3){Navigate(tabs[0].id,ScreenId.Tasks);Navigate(tabs[1].id,ScreenId.BattlePass);Navigate(tabs[2].id,ScreenId.ChestQueue);} }
            var entries=new List<UIScreenRouter.ScreenEntry>();
            foreach(var s in data.screens)
            {
                var page=pages[s.key];page.SetActive(true);
                string path=Root+"/Prefabs/Screens/"+s.key+".prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(page,path,InteractionMode.AutomatedAction);
                entries.Add(new UIScreenRouter.ScreenEntry{id=(ScreenId)Enum.Parse(typeof(ScreenId),s.key),view=page});
            }
            router.Screens=entries.ToArray();router.Show(ScreenId.BattleHome);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,Root+"/Scenes/ClashRoyaleUI.unity");
            var builds=EditorBuildSettings.scenes.ToList();if(!builds.Any(s=>s.path==scene.path))builds.Insert(0,new EditorBuildSettingsScene(scene.path,true));EditorBuildSettings.scenes=builds.ToArray();
            AssetDatabase.SaveAssets();Selection.activeGameObject=shell.gameObject;
            Debug.Log("Clash Royale UI: 10 editable screen prefabs authored; no runtime UI construction.");
        }

        static void PrepareImages()
        {
            foreach(var file in Directory.GetFiles(Root+"/Art","*.png",SearchOption.TopDirectoryOnly))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
            }
        }
        static void PrepareFonts(DesignData data)
        {
            string chars=string.Concat(data.screens.SelectMany(s=>s.nodes).Where(n=>n.text!=null).Select(n=>n.text))+"已领取已接取已激活输入消息发送取消开通未完成返回任务宝箱部落免费奖励战令0123456789× / Mafty";
            foreach(var name in new[]{"NotoSansSC","NotoSansSC-Bold","Inter","Inder"})
            {
                string path=Root+"/Fonts/"+name+" SDF.asset";var asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if(!asset){var font=AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/"+name+".ttf");if(!font)continue;asset=TMP_FontAsset.CreateFontAsset(font,64,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);asset.name=name+" SDF";AssetDatabase.CreateAsset(asset,path);AssetDatabase.AddObjectToAsset(asset.material,asset);foreach(var tex in asset.atlasTextures){tex.name=name+" Atlas";AssetDatabase.AddObjectToAsset(tex,asset);}asset.TryAddCharacters(chars,out string missing);EditorUtility.SetDirty(asset);}
                fonts[name]=asset;
            }
            foreach(var font in fonts.Values)if(font!=fonts["NotoSansSC"])font.fallbackFontAssetTable=new List<TMP_FontAsset>{fonts["NotoSansSC"]};
            AssetDatabase.SaveAssets();
        }
        static void Draw(GameObject go,DesignNode n)
        {
            n.radius=Mathf.Min(n.radius,Mathf.Min(n.w,n.h)/2);
            if(n.type=="TEXT")
            {
                var t=go.AddComponent<TextMeshProUGUI>();string key=n.font!=null&&n.font.family=="Inter"?"Inter":n.font!=null&&n.font.family=="Inder"?"Inder":n.font!=null&&n.font.style=="Bold"&&fonts.ContainsKey("NotoSansSC-Bold")?"NotoSansSC-Bold":"NotoSansSC";t.font=fonts[key];t.text=n.text??"";t.fontSize=n.size;t.enableAutoSizing=false;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Overflow;t.margin=Vector4.zero;t.raycastTarget=false;
                t.alignment=n.align=="CENTER"?TextAlignmentOptions.Midline:n.align=="RIGHT"?TextAlignmentOptions.MidlineRight:TextAlignmentOptions.MidlineLeft;
                t.color=ColorOf(n.fills!=null&&n.fills.Length>0?n.fills[0].color:null);t.color=new Color(t.color.r,t.color.g,t.color.b,t.color.a*n.opacity);return;
            }
            if(!string.IsNullOrEmpty(n.asset))
            {
                var im=go.AddComponent<UnityEngine.UI.Image>();var fill=n.fills?.FirstOrDefault(f=>f.type=="IMAGE");im.sprite=ImageSprite(n.asset,fill);im.preserveAspect=fill!=null&&fill.scaleMode=="FIT";im.raycastTarget=false;im.color=new Color(1,1,1,n.opacity*(fill?.opacity??1));return;
            }
            var paint=n.fills?.FirstOrDefault();
            if(paint!=null)
            {
                var im=go.AddComponent<UnityEngine.UI.Image>();im.raycastTarget=false;
                if(paint.type.StartsWith("GRADIENT")){im.sprite=GradientSprite(n,paint);im.color=new Color(1,1,1,n.opacity);}
                else {im.sprite=ShapeSprite(n.radius,n.type=="ELLIPSE",0);im.type=UnityEngine.UI.Image.Type.Sliced;im.color=ColorOf(paint.color)*new Color(1,1,1,n.opacity*paint.opacity);}
            }
            if(n.strokes!=null&&n.strokes.Length>0&&n.stroke>0)
            {
                var rect=Rect("Border",go.transform,0,0,n.w,n.h);Stretch(rect);var im=rect.gameObject.AddComponent<UnityEngine.UI.Image>();im.sprite=ShapeSprite(n.radius,n.type=="ELLIPSE",n.stroke);im.type=UnityEngine.UI.Image.Type.Sliced;im.color=ColorOf(n.strokes[0].color);im.raycastTarget=false;
            }
        }
        static Sprite ImageSprite(string asset,DesignPaint fill=null)
        {
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/"+asset);if(!tex)throw new InvalidOperationException("Missing source sprite "+asset);
            if(fill==null||fill.scaleMode=="FIT"||fill.imageTransform==null)return AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/"+asset);
            var m=fill.imageTransform;float x=Mathf.Clamp01(m[2]),y=Mathf.Clamp01(1-m[5]-m[4]),w=Mathf.Min(m[0],1-x),h=Mathf.Min(m[4],1-y);if(w<=0||h<=0)return AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/"+asset);
            string key=asset+"_"+x+"_"+y+"_"+w+"_"+h;if(spriteCache.TryGetValue(key,out var saved))return saved;
            var sprite=Sprite.Create(tex,new Rect(x*tex.width,y*tex.height,w*tex.width,h*tex.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=Path.GetFileNameWithoutExtension(asset)+" Crop";AssetDatabase.CreateAsset(sprite,Root+"/Art/Generated/Crop_"+spriteCache.Count+".asset");spriteCache[key]=sprite;return sprite;
        }
        static Color ColorOf(float[] c)=>c!=null&&c.Length>=3?new Color(c[0],c[1],c[2],c.Length>3?c[3]:1):Color.black;
        static Sprite ShapeSprite(float radius,bool circle,float stroke)
        {
            string key="shape_"+radius+"_"+circle+"_"+stroke;if(spriteCache.TryGetValue(key,out var saved))return saved;
            float r=circle?32:Mathf.Max(radius,0),side=Mathf.Ceil(Mathf.Max(r,stroke)*2+6);int pixels=Mathf.CeilToInt(side*4);var tex=new Texture2D(pixels,pixels,TextureFormat.RGBA32,false);var colors=new Color[pixels*pixels];
            for(int y=0;y<pixels;y++)for(int x=0;x<pixels;x++){float px=(x+.5f)/4,py=(y+.5f)/4;float d=RoundedDistance(px,py,side,side,circle?side/2:r);float outer=Mathf.Clamp01(.5f-d*4);float inner=stroke>0?Mathf.Clamp01(.5f-(d+stroke)*4):0;colors[y*pixels+x]=new Color(1,1,1,outer-inner);}
            tex.SetPixels(colors);tex.Apply();string path=Root+"/Art/Generated/"+key+".png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.spritePixelsPerUnit=400;imp.textureCompression=TextureImporterCompression.Uncompressed;float border=circle?pixels/2-1:Mathf.Ceil(Mathf.Max(r,stroke)*4)+2;imp.spriteBorder=new Vector4(border,border,border,border);imp.SaveAndReimport();var s=AssetDatabase.LoadAssetAtPath<Sprite>(path);spriteCache[key]=s;return s;
        }
        static float RoundedDistance(float x,float y,float w,float h,float r){Vector2 q=new Vector2(Mathf.Abs(x-w/2)-w/2+r,Mathf.Abs(y-h/2)-h/2+r);return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-r;}
        static Sprite GradientSprite(DesignNode n,DesignPaint paint)
        {
            string key="gradient_"+n.id.Replace(':','_');if(spriteCache.TryGetValue(key,out var saved))return saved;int w=Mathf.Max(2,Mathf.CeilToInt(n.w*2)),h=Mathf.Max(2,Mathf.CeilToInt(n.h*2));var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var colors=new Color[w*h];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {float u=(x+.5f)/w,v=1-(y+.5f)/h;float t=v;if(paint.type=="GRADIENT_RADIAL")t=new Vector2((u-.5f)*2,(v-.5f)*2).magnitude;else if(paint.matrix!=null){var m=paint.matrix;t=Mathf.Abs(m[1])>Mathf.Abs(m[0])?(v-m[2])/Mathf.Max(.0001f,m[1]):(u-m[2])/Mathf.Max(.0001f,m[0]);}
                var c=ColorOf(paint.stops[0].c);for(int i=1;i<paint.stops.Length;i++){var a=paint.stops[i-1];var b=paint.stops[i];if(t>=a.p)c=Color.Lerp(ColorOf(a.c),ColorOf(b.c),Mathf.InverseLerp(a.p,b.p,t));}c.a*=Mathf.Clamp01(.5f-RoundedDistance(u*n.w,v*n.h,n.w,n.h,n.radius)*2);colors[y*w+x]=c;}
            tex.SetPixels(colors);tex.Apply();string path=Root+"/Art/Generated/"+key+".png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=200;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.alphaIsTransparency=true;imp.SaveAndReimport();var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);spriteCache[key]=sprite;return sprite;
        }
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {var go=new GameObject(name,typeof(RectTransform));go.layer=5;var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static string SafeName(string s)=>s.Replace('/','·').Replace('\n',' ');
        static UIAction Action(string id,UIActionKind kind)
        {
            var go=objects[id];var im=go.GetComponent<UnityEngine.UI.Image>();if(!im){im=go.AddComponent<UnityEngine.UI.Image>();im.color=Color.clear;}im.raycastTarget=true;
            var button=go.GetComponent<UnityEngine.UI.Button>()??go.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=im;button.transition=Selectable.Transition.ColorTint;var colors=button.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.82f,.82f,.82f,1);button.colors=colors;button.navigation=new Navigation{mode=Navigation.Mode.None};
            var action=go.GetComponent<UIAction>()??go.AddComponent<UIAction>();action.action=kind;UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,action.Execute);return action;
        }
        static void Navigate(string id,ScreenId destination){var action=Action(id,UIActionKind.Navigate);action.destination=destination;}
        static void FooterActions(DesignScreen screen)
        {foreach(var node in screen.nodes.Where(n=>n.y>800&&n.name=="主页底板"))Navigate(node.id,ScreenId.BattleHome);foreach(var node in screen.nodes.Where(n=>n.y>800&&n.name=="部落底板"))Navigate(node.id,ScreenId.ClanHome);}
        static GameObject Group(string name,Transform parent,float x,float y,float w,float h,IEnumerable<string> ids)
        {var r=Rect(name,parent,x,y,w,h);foreach(var id in ids){if(!objects.ContainsKey(id))continue;var go=objects[id];go.transform.SetParent(r,false);var n=nodes[id];var t=go.GetComponent<RectTransform>();t.anchoredPosition=new Vector2(n.x-x,-(n.y-y));}return r.gameObject;}
        static RectTransform Scroll(string name,Transform parent,float x,float y,float w,float h)
        {var r=Rect(name,parent,x,y,w,h);var scroll=r.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=22;var viewport=Rect("Viewport",r,0,0,w,h);Stretch(viewport);var image=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=Color.white;var mask=viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;var content=Rect("Content",viewport,0,0,w,h);scroll.viewport=viewport;scroll.content=content;return content;}
        static void ConfigurePass()
        {
            var group=objects["158:73"].transform;var content=Scroll("RewardScrollView",group,9,25,361,495);var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=false;var fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            foreach(var id in new[]{"158:13","158:25","158:37","158:49","158:61"}){var go=objects[id];go.transform.SetParent(content,false);go.GetComponent<RectTransform>().localRotation=Quaternion.identity;var el=go.AddComponent<LayoutElement>();el.preferredWidth=361;el.preferredHeight=99;}
            // Axis and claim button remain outside the clipped row list.
            objects["158:68"].transform.SetAsLastSibling();objects["158:72"].transform.SetAsLastSibling();
            demo.claimLabel=objects["158:71"].GetComponent<TMP_Text>();Action("59:118",UIActionKind.ClaimFreeRewards);Action("59:95",UIActionKind.UnlockPassPreview);
            demo.freeMarkers=new[]{"I158:2;157:73","I158:14;157:73","I158:26;157:73"}.Select(id=>objects[id]).ToArray();demo.completedPremiumLocks=new[]{"I158:7;157:73;157:64","I158:19;157:73;157:64","I158:31;157:73;157:64"}.Select(id=>objects[id]).ToArray();
            SaveComponent(objects["158:13"],"PassRewardRow");SaveComponent(objects["158:79"],"PassOverview");SaveComponent(objects["158:80"],"RewardTabBar");SaveComponent(objects["158:81"],"BottomNavigation");
        }
        static void ConfigureClanList()
        {
            var content=Scroll("MemberScrollView",pages["ClanHome"].transform,21,258,359,492);var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=false;var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            for(int i=0;i<6;i++)
            {float y=258+82*i;var ids=nodes.Values.Where(n=>n.y>=y&&n.y<y+82&&n.w<360&&n.type!="FRAME"&&n.type!="GROUP"&&objects[n.id].transform.IsChildOf(pages["ClanHome"].transform)).Select(n=>n.id).ToArray();var row=Group("MemberRow_"+(i+1),content,21,y,359,82,ids);var el=row.AddComponent<LayoutElement>();el.preferredWidth=359;el.preferredHeight=82;if(i==0)SaveComponent(row,"ClanMemberRow");}
        }
        static void ConfigureTasks()
        {
            var content=Scroll("TaskScrollView",pages["Tasks"].transform,21,169,361,555);
            var ids=nodes.Values.Where(n=>n.y>=169&&n.y<724&&n.w<=361&&n.id!="70:196"&&objects[n.id].transform.IsChildOf(pages["Tasks"].transform)).Select(n=>n.id).ToArray();foreach(var id in ids){var r=objects[id].GetComponent<RectTransform>();r.SetParent(content,false);r.anchoredPosition=new Vector2(nodes[id].x-21,-(nodes[id].y-169));}
            var el=content.gameObject.AddComponent<LayoutElement>();el.preferredHeight=555;var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var accept=Action("130:5",UIActionKind.SetText);accept.label=objects["130:6"].GetComponent<TMP_Text>();accept.value="已接取";
            var activate=Action("130:7",UIActionKind.SetText);activate.label=objects["130:8"].GetComponent<TMP_Text>();activate.value="已激活";
        }
        static void ConfigureChests()
        {
            demo.chestCount=objects["71:222"].GetComponent<TMP_Text>();demo.revealImage=objects["63:132"].GetComponent<UnityEngine.UI.Image>();demo.fourStarChest=demo.revealImage.sprite;demo.twoStarChest=objects["71:227"].GetComponent<UnityEngine.UI.Image>().sprite;
            var parent=pages["ChestQueue"].transform;var pair=Group("Merge_3_Star_Pair_To_4_Star",parent,19,168,364,102,new[]{"71:223","71:224","71:225"});var single=Group("Open_2_Star_Chest",parent,19,280,364,102,new[]{"71:226","71:227"});demo.chestGroups=new[]{pair,single};Action("71:223",UIActionKind.OpenChest).index=0;Action("71:226",UIActionKind.OpenChest).index=1;
            // Tap the reveal background to return; no added artwork over the source design.
            Action("62:123",UIActionKind.ReturnFromChest);SaveComponent(pair,"ChestMergePair");
        }
        static void ConfigureClanNavigation()
        {
            Navigate("76:230",ScreenId.ClanCompetition);Navigate("76:229",ScreenId.ClanShop);Navigate("76:263",ScreenId.ClanCompetition);Navigate("76:261",ScreenId.ClanShop);
            Navigate("76:273",ScreenId.ClanChat);Navigate("76:271",ScreenId.ClanCompetition);Navigate("76:281",ScreenId.ClanShop);Navigate("76:279",ScreenId.ClanChat);
            string[] tabs={"79:288","79:289","79:292","79:290"};demo.shopTabs=tabs.Select(id=>objects[id].GetComponent<UnityEngine.UI.Image>()).ToArray();for(int i=0;i<tabs.Length;i++)Action(tabs[i],UIActionKind.SelectShopTab).index=i;
            Scroll("ShopCategoryContent",pages["ClanShop"].transform,46,146,352,655);
        }
        static void ConfigureChat()
        {
            Action("81:297",UIActionKind.ShowChatInput);
            var inputGO=objects["76:247"];var input=inputGO.AddComponent<TMP_InputField>();input.targetGraphic=inputGO.GetComponent<UnityEngine.UI.Image>();inputGO.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var area=Rect("TextArea",inputGO.transform,12,5,274,32);area.gameObject.AddComponent<RectMask2D>();var tr=Rect("Text",area,0,0,274,32);var text=tr.gameObject.AddComponent<TextMeshProUGUI>();text.font=fonts["NotoSansSC"];text.fontSize=18;text.color=Color.black;text.alignment=TextAlignmentOptions.MidlineLeft;text.raycastTarget=false;
            input.textViewport=area;input.textComponent=text;input.characterLimit=120;input.lineType=TMP_InputField.LineType.SingleLine;demo.chatInput=input;Action("76:269",UIActionKind.SendChat);
            var bubble=Rect("LocalMessage",pages["ClanChat"].transform,18,110,366,90);var bg=bubble.gameObject.AddComponent<UnityEngine.UI.Image>();bg.sprite=ShapeSprite(10,false,0);bg.type=UnityEngine.UI.Image.Type.Sliced;bg.color=new Color32(210,228,202,255);bg.raycastTarget=false;var label=Rect("Message",bubble,14,8,338,74).gameObject.AddComponent<TextMeshProUGUI>();label.font=fonts["NotoSansSC"];label.fontSize=18;label.color=Color.black;label.textWrappingMode=TextWrappingModes.Normal;label.raycastTarget=false;demo.chatMessage=label;demo.chatBubble=bubble.gameObject;bubble.gameObject.SetActive(false);
            SaveComponent(inputGO,"ClanChatInputField");
        }
        static void SaveComponent(GameObject go,string name){PrefabUtility.SaveAsPrefabAssetAndConnect(go,Root+"/Prefabs/Components/"+name+".prefab",InteractionMode.AutomatedAction);}

        public static void RepairGeneratedSpriteImports()
        {
            foreach(var file in Directory.GetFiles(Root+"/Art/Generated","*.png"))
            {var p=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(p);if(importer.spriteImportMode!=SpriteImportMode.Single){importer.spriteImportMode=SpriteImportMode.Single;importer.SaveAndReimport();}}
            var data=JsonUtility.FromJson<DesignData>(File.ReadAllText(Root+"/Tables/DesignData.json"));var lookup=data.screens.SelectMany(s=>s.nodes).ToDictionary(n=>n.id);
            Action<GameObject> fix=root=>
            {
                var tags=root.GetComponentsInChildren<FigmaLayer>(true).ToDictionary(t=>t.gameObject,t=>t.nodeId);
                foreach(var screen in data.screens)if(root.name.StartsWith(screen.key+" ·"))tags[root]=screen.id;
                foreach(var pair in tags)
                {
                    if(!lookup.TryGetValue(pair.Value,out var n))continue;var im=pair.Key.GetComponent<UnityEngine.UI.Image>();var fill=n.fills?.FirstOrDefault();
                    if(im&&string.IsNullOrEmpty(n.asset)&&fill!=null){string file=fill.type.StartsWith("GRADIENT")?"gradient_"+n.id.Replace(':','_'):"shape_"+n.radius+"_"+(n.type=="ELLIPSE")+"_0";im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Generated/"+file+".png");EditorUtility.SetDirty(im);}
                    var border=pair.Key.transform.Find("Border");if(border&&n.strokes!=null&&n.strokes.Length>0){var bi=border.GetComponent<UnityEngine.UI.Image>();bi.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Generated/shape_"+n.radius+"_"+(n.type=="ELLIPSE")+"_"+n.stroke+".png");EditorUtility.SetDirty(bi);}
                }
                foreach(var im in root.GetComponentsInChildren<UnityEngine.UI.Image>(true).Where(i=>i.name=="LocalMessage")){im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Generated/shape_10_False_0.png");EditorUtility.SetDirty(im);}
            };
            // Patch prefab contents in place, preserving existing objects and references.
            foreach(var path in Directory.GetFiles(Root+"/Prefabs","*.prefab",SearchOption.AllDirectories)){var p=path.Replace('\\','/');var prefab=PrefabUtility.LoadPrefabContents(p);fix(prefab);PrefabUtility.SaveAsPrefabAsset(prefab,p);PrefabUtility.UnloadPrefabContents(prefab);}
            foreach(var entry in UnityEngine.Object.FindFirstObjectByType<UIScreenRouter>().Screens){fix(entry.view);PrefabUtility.RecordPrefabInstancePropertyModifications(entry.view);}
            var state=UnityEngine.Object.FindFirstObjectByType<UIPrototypeState>();if(state.chatBubble)fix(state.chatBubble);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        }
    }
}

