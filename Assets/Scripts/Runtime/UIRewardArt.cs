using UnityEngine;
namespace ClashRoyaleUI
{
    [CreateAssetMenu(menuName="Clash Royale/Reward Art")]
    public sealed class UIRewardArt : ScriptableObject
    {
        public Sprite gold,gems,common,rare,epic,legendary,action;
        public Sprite[] chests;
        public Color[] starColors={new Color32(244,242,238,255),new Color32(174,218,239,255),new Color32(209,183,235,255),new Color32(249,221,130,255),new Color32(239,151,156,255)};
        public Sprite Chest(int star)=>chests[Mathf.Clamp(star-1,0,4)];
        public Color Color(int star)=>starColors[Mathf.Clamp(star-1,0,4)];
        public Sprite Resource(string kind)
        {
            switch(kind){case "gold":return gold;case "gems":return gems;case "rare":return rare;case "epic":return epic;case "legendary":return legendary;case "ap":return action;default:return common;}
        }
        public Sprite Icon(RewardData reward)=>reward.chest_star>0?Chest(reward.chest_star):Resource(reward.gold>0?"gold":reward.ap>0?"ap":reward.cards>0?"common":reward.resource);
        public string Quantity(RewardData reward)=>reward.chest_star>0?"":"×"+(reward.gold>0?reward.gold:reward.ap>0?reward.ap:reward.cards>0?reward.cards:reward.amount);
    }
}
