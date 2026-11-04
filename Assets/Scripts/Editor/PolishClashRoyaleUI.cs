using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ClashRoyaleUI.Editor
{
    public static class PolishClashRoyaleUI
    {
        const string Root="Assets";
        static Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static readonly Dictionary<string,float[]> Corners=new Dictionary<string,float[]>{{"79:288",new[]{18f,0,0,21}},{"79:289",new[]{21f,0,0,22}},{"79:292",new[]{19f,0,0,18}},{"79:290",new[]{17f,0,0,16}},{"79:293",new[]{14f,14,0,0}}};
        public static void Run()
        {
            // High-resolution source icons need mip filtering at their 18–78 px display sizes.
            foreach(var file in Directory.GetFiles(Root+"/Art","*.png")){var p=file.Replace('\\','/');var imp=(TextureImporter)AssetImporter.GetAtPath(p);imp.mipmapEnabled=true;imp.filterMode=FilterMode.Trilinear;imp.SaveAndReimport();}
            var data=JsonUtility.FromJson<DesignData>(File.ReadAllText(Root+"/Tables/DesignData.json"));var lookup=data.screens.SelectMany(s=>s.nodes).ToDictionary(n=>n.id);
            foreach(var path in Directory.GetFiles(Root+"/Prefabs","*.prefab",SearchOption.AllDirectories))
            {var p=path.Replace('\\','/');var root=PrefabUtility.LoadPrefabContents(p);Fix(root,lookup,data);PrefabUtility.SaveAsPrefabAsset(root,p);PrefabUtility.UnloadPrefabContents(root);}
            var router=UnityEngine.Object.FindFirstObjectByType<UIScreenRouter>();foreach(var entry in router.Screens)Fix(entry.view,lookup,data);
            router.Show(ScreenId.BattleHome);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        }
        static void Fix(GameObject root,Dictionary<string,DesignNode> lookup,DesignData data)
        {
            var tags=root.GetComponentsInChildren<FigmaLayer>(true).ToDictionary(t=>t.gameObject,t=>t.nodeId);foreach(var s in data.screens)if(root.name.StartsWith(s.key+" ·"))tags[root]=s.id;
            foreach(var pair in tags)
            {
                if(!lookup.TryGetValue(pair.Value,out var n))continue;
                if(string.IsNullOrEmpty(n.asset))
                {
                    var im=pair.Key.GetComponent<UnityEngine.UI.Image>();float rad=Mathf.Min(n.radius,Mathf.Min(n.w,n.h)/2);float[] r=Corners.TryGetValue(n.id,out var c)?c:new[]{rad,rad,rad,rad};
                    if(im&&n.fills!=null&&n.fills.Length>0&&n.fills[0].type=="SOLID"){im.sprite=Shape(r,0,n.type=="ELLIPSE");EditorUtility.SetDirty(im);PrefabUtility.RecordPrefabInstancePropertyModifications(im);}
                    var border=pair.Key.transform.Find("Border");if(border&&n.stroke>0){var bi=border.GetComponent<UnityEngine.UI.Image>();bi.sprite=Shape(r,n.stroke,n.type=="ELLIPSE");EditorUtility.SetDirty(bi);PrefabUtility.RecordPrefabInstancePropertyModifications(bi);}
                }
                if(n.type=="POLYGON"&&n.asset!=null)
                {
                    // The SVG exports only its painted bounds, not the full 26.918 px Figma box.
                    var rect=pair.Key.GetComponent<RectTransform>();float a=n.rotation*Mathf.Deg2Rad,dx=n.w*.1484f,dy=n.h*.1114f;var delta=new Vector2(Mathf.Cos(a)*dx+Mathf.Sin(a)*dy,Mathf.Sin(a)*dx-Mathf.Cos(a)*dy);var parentNode=pair.Key.transform.parent.GetComponent<FigmaLayer>();float parentX=0,parentY=0;if(parentNode){var pn=lookup[parentNode.nodeId];parentX=pn.x;parentY=pn.y;}else if(pair.Key.transform.parent.name=="Content"&&pair.Key.transform.parent.parent.name=="Viewport"){parentX=21;parentY=169;}
                    rect.anchoredPosition=new Vector2(n.x-parentX,-n.y+parentY)+delta;rect.sizeDelta=new Vector2(18.9287f,17.1888f);EditorUtility.SetDirty(rect);PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
                }
            }
            if(root.name.StartsWith("ClanHome") && !PrefabUtility.IsPartOfPrefabInstance(root))
            {
                var header=root.transform.Find("Header");foreach(var pair in tags.Where(t=>lookup[t.Value].type=="TEXT"&&lookup[t.Value].y<118).ToArray()){var n=lookup[pair.Value];var r=pair.Key.GetComponent<RectTransform>();r.SetParent(header,false);r.anchoredPosition=new Vector2(n.x,-n.y);}
            }
        }
        public static Sprite Shape(float[] radii,float stroke,bool circle)
        {
            string key="Panel_"+string.Join("_",radii)+"_"+stroke+"_"+circle;if(sprites.TryGetValue(key,out var existing))return existing;
            float max=circle?32:Mathf.Max(radii.Max(),stroke);float side=Mathf.Ceil(max*2+6);int size=Mathf.CeilToInt(side*4);var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {float px=(x+.5f)/4,py=(y+.5f)/4;float r=circle?side/2:radii[py>side/2?(px<side/2?0:1):(px<side/2?3:2)];var q=new Vector2(Mathf.Abs(px-side/2)-side/2+r,Mathf.Abs(py-side/2)-side/2+r);float d=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-r;float alpha=Mathf.Clamp01(.5f-d*4)-(stroke>0?Mathf.Clamp01(.5f-(d+stroke)*4):0);colors[y*size+x]=new Color(1,1,1,alpha);}
            tex.SetPixels(colors);tex.Apply();string path=Root+"/Art/Generated/"+key+".png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=400;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;float b=circle?size/2-1:max*4+2;imp.spriteBorder=new Vector4(b,b,b,b);imp.SaveAndReimport();var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);sprites[key]=sprite;return sprite;
        }
    }
}
