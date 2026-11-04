var rows = new System.Collections.Generic.Dictionary<string,int[]> {
 {"knight",new[]{0,235,440,638,842,1052,1254}},
 {"archer",new[]{0,240,456,665,873,1077,1254}},
 {"giant",new[]{0,205,410,618,822,1050,1254}},
 {"dragon",new[]{0,244,440,651,842,1056,1254}},
 {"witch",new[]{0,220,427,631,826,1054,1254}},
 {"skeleton",new[]{0,229,452,676,974,1254}},
 {"effects",new[]{0,213,430,661,839,1034,1254}},
 {"towers",new[]{0,300,604,887}}
};
var report=new System.Collections.Generic.List<string>();
foreach(var item in rows) {
 var path="Assets/Art/TrainingBattle/"+item.Key+".png";
 var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
 importer.textureType=UnityEditor.TextureImporterType.Sprite;
 importer.spriteImportMode=UnityEditor.SpriteImportMode.Multiple;
 importer.maxTextureSize=2048;importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
 importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.filterMode=UnityEngine.FilterMode.Bilinear;
 importer.SaveAndReimport();
 var factory=new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();factory.Init();
 var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
 var capability=provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteFrameEditCapability>();
 if(capability==null||!capability.GetEditCapability().HasCapability(UnityEditor.U2D.Sprites.EEditCapability.CreateAndDeleteSprite))throw new System.Exception("Slicing unsupported: "+path);
 importer.GetSourceTextureWidthAndHeight(out int width,out int height);
 var rects=new System.Collections.Generic.List<UnityEditor.SpriteRect>();
 var old=provider.GetSpriteRects();
 for(int r=0;r<item.Value.Length-1;r++)for(int c=0;c<6;c++) {
  string name=item.Key+"_"+(r*6+c).ToString("00");
  var prior=System.Linq.Enumerable.FirstOrDefault(old,x=>x.name==name);
  float x=UnityEngine.Mathf.Round(width*c/6f),right=UnityEngine.Mathf.Round(width*(c+1)/6f);
  rects.Add(new UnityEditor.SpriteRect{name=name,rect=new UnityEngine.Rect(x,height-item.Value[r+1],right-x,item.Value[r+1]-item.Value[r]),pivot=new UnityEngine.Vector2(.5f,.5f),alignment=UnityEngine.SpriteAlignment.Custom,spriteID=prior!=null?prior.spriteID:UnityEditor.GUID.Generate()});
 }
 provider.SetSpriteRects(rects.ToArray());
 var ids=provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteNameFileIdDataProvider>();
 if(ids!=null)ids.SetNameFileIdPairs(rects.Select(x=>new UnityEditor.SpriteNameFileIdPair(x.name,x.spriteID)));
 provider.Apply();importer.SaveAndReimport();report.Add(item.Key+": "+rects.Count);
}
return report;
