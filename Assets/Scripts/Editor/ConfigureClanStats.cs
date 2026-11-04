using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClashRoyaleUI.Editor
{
    public static class ConfigureClanStats
    {
        static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
        {
            var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        static void Layer(GameObject target,string id,string name)
        {var layer=target.AddComponent<FigmaLayer>();layer.nodeId=id;layer.sourceName=name;}
        static UnityEngine.UI.Image Image(RectTransform rect,Sprite sprite,Color color)
        {var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.color=color;image.raycastTarget=false;return image;}
        static TMP_Text Find(GameObject root,string id)=>root.GetComponentsInChildren<TMP_Text>(true).Single(t=>t.name.Contains("["+id+"]"));
        static TMP_Text ResourceBar(Transform parent,string name,string id,Sprite icon,Sprite fill,Sprite border,TMP_FontAsset font)
        {
            var bar=Rect(name,parent,Vector2.zero,new Vector2(172,40));Layer(bar.gameObject,id,name);
            var layout=bar.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();layout.preferredWidth=172;layout.preferredHeight=40;
            var bg=Rect("Background",bar,new Vector2(18,-5),new Vector2(-18,30));bg.anchorMax=new Vector2(1,1);
            Image(bg,fill,new Color32(232,232,232,255)).type=UnityEngine.UI.Image.Type.Sliced;
            var outline=Rect("Outline",bg,Vector2.zero,Vector2.zero);outline.anchorMin=Vector2.zero;outline.anchorMax=Vector2.one;outline.offsetMin=outline.offsetMax=Vector2.zero;
            Image(outline,border,Color.black).type=UnityEngine.UI.Image.Type.Sliced;
            var art=Image(Rect("Icon",bar,Vector2.zero,new Vector2(40,40)),icon,Color.white);art.preserveAspect=true;
            var tr=Rect("Value",bar,new Vector2(40,-5),new Vector2(-46,30));tr.anchorMax=new Vector2(1,1);
            var text=tr.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=18;text.fontStyle=FontStyles.Bold;
            text.color=new Color32(32,33,38,255);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;text.text="0";text.textWrappingMode=TextWrappingModes.NoWrap;
            return text;
        }
        [MenuItem("Clash Royale/同步部落资源栏与数据绑定")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before authoring.");
            AssetDatabase.ImportAsset("Assets/Art/d1479.png");var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/d1479.png");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            foreach(var screen in new[]{"ClanHome","ClanShop"})
            {
                string path="Assets/Prefabs/Screens/"+screen+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var binding=root.GetComponent<UIClanStats>()??root.AddComponent<UIClanStats>();
                    binding.clanTokens=Array.Empty<TMP_Text>();binding.clanScore=Array.Empty<TMP_Text>();
                    if(screen=="ClanHome")
                    {
                        binding.personalContribution=new[]{Find(root,"145:4"),Find(root,"145:13")};
                        binding.clanScore=new[]{Find(root,"145:3")};
                    }
                    else
                    {
                        var row=root.transform.Find("ClanResourceBar");
                        if(!row)
                        {
                            var r=Rect("ClanResourceBar",root.transform,new Vector2(16,-99),new Vector2(-32,40));r.anchorMax=new Vector2(1,1);row=r;Layer(r.gameObject,"243:100","顶部资源栏 / 个人贡献与部落代币");
                            var layout=r.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();layout.spacing=26;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
                            var fill=PolishClashRoyaleUI.Shape(new[]{15f,15f,15f,15f},0,false);var border=PolishClashRoyaleUI.Shape(new[]{15f,15f,15f,15f},1,false);
                            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansSC SDF.asset");
                            ResourceBar(row,"PersonalContribution","243:101",AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/2a4d3.png"),fill,border,font);
                            ResourceBar(row,"ClanTokens","243:105",AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/d1479.png"),fill,border,font);
                        }
                        binding.personalContribution=new[]{row.Find("PersonalContribution/Value").GetComponent<TMP_Text>()};
                        binding.clanTokens=new[]{row.Find("ClanTokens/Value").GetComponent<TMP_Text>()};
                        binding.personalContribution[0].text="340";binding.clanTokens[0].text="0";
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            AssetDatabase.SaveAssets();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.isDirty)EditorSceneManager.SaveScene(scene);
        }
    }
}
