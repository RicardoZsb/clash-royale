using TMPro;
using UnityEngine;

namespace ClashRoyaleUI
{
    public sealed class UIRewardPopup : MonoBehaviour
    {
        public UIRewardArt art;
        public RectTransform panel;
        public UnityEngine.UI.Button dismiss;
        public UnityEngine.UI.Image[] icons;
        public TMP_Text[] amounts;

        public void Show(RewardData reward)
        {
            int count=0;
            foreach(var icon in icons)icon.transform.parent.gameObject.SetActive(false);
            void Add(Sprite sprite,string quantity)
            {
                icons[count].sprite=sprite;amounts[count].text=quantity;
                icons[count].transform.parent.gameObject.SetActive(true);count++;
            }
            if(reward.chest_star>0)Add(art.Chest(reward.chest_star),"");
            if(reward.gold>0)Add(art.gold,"×"+reward.gold);
            if(reward.cards>0)Add(art.common,"×"+reward.cards);
            if(reward.ap>0)Add(art.action,"×"+reward.ap);
            if(reward.amount>0)Add(art.Resource(reward.resource),"×"+reward.amount);
            panel.sizeDelta=new Vector2(count==1?210:340,count<=2?210:350);
            for(int i=0;i<count;i++)
            {
                var slot=(RectTransform)icons[i].transform.parent;
                slot.anchoredPosition=count<=2?new Vector2((i-(count-1)*.5f)*150,0):new Vector2((i%2-.5f)*150,i<2?80:-80);
            }
            transform.SetAsLastSibling();gameObject.SetActive(true);
        }
        public void Close()=>gameObject.SetActive(false);
    }
}
