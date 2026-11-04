using UnityEngine;
using UnityEngine.UI;
namespace ClashRoyaleUI
{
    public sealed class UIChestQueueRow : MonoBehaviour
    {
        public Image background,icon;
        public UIRewardArt art;
        private int index;
        public void Bind(ChestData chest,int slot){index=slot;background.color=art.Color(chest.current_star);icon.sprite=art.Chest(chest.current_star);}
        public void Open()=>GetComponentInParent<UIBackendBindings>().OpenChest(index);
    }
}
