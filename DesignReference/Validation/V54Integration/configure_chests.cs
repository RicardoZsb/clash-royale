var router=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIScreenRouter>(UnityEngine.FindObjectsInactive.Include);
var font=UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Fonts/NotoSansSC SDF.asset");
TMPro.TMP_Text AddStars(UnityEngine.UI.Image image){
 var parent=image.rectTransform;float cut=parent.rect.height*.24f;
 var mask=new GameObject("ChestBodyClip",typeof(RectTransform),typeof(UnityEngine.UI.RectMask2D));var mr=(RectTransform)mask.transform;mr.SetParent(parent,false);mr.anchorMin=Vector2.zero;mr.anchorMax=Vector2.one;mr.offsetMin=new Vector2(0,cut);mr.offsetMax=Vector2.zero;
 var body=new GameObject("ChestBody",typeof(RectTransform),typeof(UnityEngine.UI.Image));var br=(RectTransform)body.transform;br.SetParent(mr,false);br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=new Vector2(0,-cut);br.offsetMax=Vector2.zero;var bi=body.GetComponent<UnityEngine.UI.Image>();bi.sprite=image.sprite;bi.raycastTarget=false;
 var label=new GameObject("StarCount",typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));var tr=(RectTransform)label.transform;tr.SetParent(parent,false);tr.anchorMin=new Vector2(0,0);tr.anchorMax=new Vector2(1,0);tr.pivot=new Vector2(.5f,0);tr.anchoredPosition=Vector2.zero;tr.sizeDelta=new Vector2(8,cut+2);
 var text=label.GetComponent<TMPro.TMP_Text>();text.font=font;text.fontSize=Mathf.Clamp(cut,12,32);text.fontStyle=TMPro.FontStyles.Bold;text.alignment=TMPro.TextAlignmentOptions.Center;text.color=new Color32(255,209,0,255);text.raycastTarget=false;text.text="★★";image.color=Color.clear;return text;
}
foreach(var name in new[]{"ChestQueue","ChestReveal"}){
 var path="Assets/Prefabs/Screens/"+name+".prefab";var root=UnityEditor.PrefabUtility.LoadPrefabContents(path);
 try{
  if(root.GetComponent<ClashRoyaleUI.UIChestDataView>())throw new Exception("Chest data view already configured");
  var view=root.AddComponent<ClashRoyaleUI.UIChestDataView>();view.reveal=name=="ChestReveal";
  if(!view.reveal){
   var layers=root.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true);var images=new[]{"71:224","71:225","71:227"}.Select(id=>layers.First(x=>x.nodeId==id).GetComponent<UnityEngine.UI.Image>()).ToArray();view.slots=images.Select(x=>x.gameObject).ToArray();view.stars=images.Select(AddStars).ToArray();view.panels=new[]{images[0].transform.parent.gameObject,images[2].transform.parent.gameObject};
   foreach(var old in root.GetComponentsInChildren<ClashRoyaleUI.UIAction>(true).Where(a=>a.action==ClashRoyaleUI.UIActionKind.OpenChest).ToArray()){var oldButton=old.GetComponent<UnityEngine.UI.Button>();if(oldButton)UnityEngine.Object.DestroyImmediate(oldButton);UnityEngine.Object.DestroyImmediate(old);}
   for(int i=0;i<images.Length;i++){var b=images[i].gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=images[i];var a=images[i].gameObject.AddComponent<ClashRoyaleUI.UIAction>();a.action=ClashRoyaleUI.UIActionKind.OpenChest;a.index=i;UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick,a.Execute);}
  }else{
   var prototype=router.GetComponent<ClashRoyaleUI.UIPrototypeState>();var existing=prototype.revealImage;var id=existing.GetComponent<ClashRoyaleUI.FigmaLayer>().nodeId;var image=root.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true).First(x=>x.nodeId==id).GetComponent<UnityEngine.UI.Image>();view.stars=new[]{AddStars(image)};view.slots=new[]{image.gameObject};view.panels=new GameObject[0];
  }
  UnityEditor.PrefabUtility.SaveAsPrefabAsset(root,path);
 }finally{UnityEditor.PrefabUtility.UnloadPrefabContents(root);}
}
var cardRoot=UnityEditor.PrefabUtility.LoadPrefabContents("Assets/Prefabs/Components/TaskCard.prefab");var card=cardRoot.GetComponent<ClashRoyaleUI.UITaskCard>();var colors=card.AcceptButton.colors;colors.disabledColor=Color.white;card.AcceptButton.colors=colors;card.Refresh();UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardRoot,"Assets/Prefabs/Components/TaskCard.prefab");UnityEditor.PrefabUtility.UnloadPrefabContents(cardRoot);
return "Queue icons use individual clicks and server star counts; task state colors preserved";
