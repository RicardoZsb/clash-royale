var router=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIScreenRouter>(UnityEngine.FindObjectsInactive.Include);
var path="Assets/Prefabs/Screens/BattlePass.prefab";
var root=UnityEditor.PrefabUtility.LoadPrefabContents(path);
try{
var binding=root.GetComponent<ClashRoyaleUI.UIPassDataBinding>()??root.AddComponent<ClashRoyaleUI.UIPassDataBinding>();
var content=root.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true).content;
var rows=content.Cast<Transform>().Where(t=>t.name.StartsWith("Lv")).ToArray();
var sample=rows[0].GetComponentsInChildren<TMPro.TMP_Text>(true).Where(t=>t.name.StartsWith("奖励数量")).ToArray();
var axis=content.Cast<Transform>().First(t=>t.name.StartsWith("等级轴"));
binding.rows=new ClashRoyaleUI.UIPassDataBinding.Row[40];
for(int i=0;i<40;i++){
 var row=rows[i];var free=row.Cast<Transform>().First(t=>t.name.StartsWith("免费奖励")&&!t.name.Contains("底板"));var premium=row.Cast<Transform>().First(t=>t.name.StartsWith("战令奖励")&&!t.name.Contains("底板"));
 var old=row.GetComponentsInChildren<TMPro.TMP_Text>(true).Where(t=>t.name.StartsWith("奖励数量")).ToArray();foreach(var label in old)label.gameObject.SetActive(false);
 TMPro.TMP_Text Quantity(int side){var name=side==0?"FreeRewardQuantity":"PremiumRewardQuantity";var existing=row.Find(name);if(existing)return existing.GetComponent<TMPro.TMP_Text>();var obj=UnityEngine.Object.Instantiate(sample[side].gameObject,row);obj.name=name;obj.SetActive(true);var text=obj.GetComponent<TMPro.TMP_Text>();text.fontSize=13;text.text=side==0?"奖励":"战令奖励";return text;}
 var freeImage=free.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true).First(x=>x.nodeId.EndsWith("157:72")).GetComponent<UnityEngine.UI.Image>();
 var premiumImage=premium.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true).First(x=>x.nodeId.EndsWith("157:72")).GetComponent<UnityEngine.UI.Image>();
 var diamond=axis.Cast<Transform>().First(t=>t.name.StartsWith("等级 "+(i+1)+" ·")).GetComponent<UnityEngine.UI.Image>();
 var number=axis.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t=>t.name.StartsWith("等级数字 "+(i+1)+" ")||t.name=="等级数字 "+(i+1));
 binding.rows[i]=new ClashRoyaleUI.UIPassDataBinding.Row{level=i+1,freeQuantity=Quantity(0),premiumQuantity=Quantity(1),freeIcon=freeImage,premiumIcon=premiumImage,diamond=diamond,levelLabel=number,freeMarker=free.GetComponentsInChildren<UnityEngine.UI.Image>(true).FirstOrDefault(x=>x.name.Contains("领取标记"))?.gameObject,premiumLock=premium.GetComponentsInChildren<UnityEngine.UI.Image>(true).FirstOrDefault(x=>x.name.Contains("领取标记"))?.gameObject};
}
var layers=root.GetComponentsInChildren<ClashRoyaleUI.FigmaLayer>(true);Transform Find(string id)=>layers.First(x=>x.nodeId==id).transform;
binding.headerLevel=Find("158:75").GetComponent<TMPro.TMP_Text>();binding.headerProgress=Find("158:77").GetComponent<TMPro.TMP_Text>();binding.axisProgress=Find("158:67").GetComponent<TMPro.TMP_Text>();binding.axisBadge=(RectTransform)Find("59:114");binding.axisValue=binding.axisProgress.rectTransform;binding.axisLine=(RectTransform)Find("59:116");
binding.actionIcon=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Crop_11.png");
binding.rewardIcon=router.GetComponent<ClashRoyaleUI.UIPrototypeState>().twoStarChest;
UnityEditor.PrefabUtility.SaveAsPrefabAsset(root,path);
return new{rows=binding.rows.Length,actionIcon=binding.actionIcon?.name,rewardIcon=binding.rewardIcon?.name};
}finally{UnityEditor.PrefabUtility.UnloadPrefabContents(root);}

