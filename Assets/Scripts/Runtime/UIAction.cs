using UnityEngine;
using TMPro;

namespace ClashRoyaleUI
{
    public enum UIActionKind { Navigate, ToggleObject, SetText, ClaimFreeRewards, UnlockPassPreview, OpenChest, ReturnFromChest, SelectShopTab, ShowChatInput, SendChat, BeginBattle }
    [DisallowMultipleComponent]
    public sealed class UIAction : MonoBehaviour
    {
        public UIActionKind action;
        public ScreenId destination;
        public GameObject target;
        public GameObject[] targets;
        public TMP_Text label;
        [TextArea] public string value;
        public int index;
        public void Execute()
        {
            var router = GetComponentInParent<UIScreenRouter>();
            var demo = GetComponentInParent<UIPrototypeState>();
            switch (action)
            {
                case UIActionKind.Navigate: router?.Show(destination); break;
                case UIActionKind.ToggleObject: if (target) target.SetActive(!target.activeSelf); break;
                case UIActionKind.SetText: if (label) label.text = value; break;
                case UIActionKind.ClaimFreeRewards: demo?.ClaimFreeRewards(); break;
                case UIActionKind.UnlockPassPreview: demo?.UnlockPass(); break;
                case UIActionKind.OpenChest: demo?.OpenChest(index); break;
                case UIActionKind.ReturnFromChest: demo?.CloseChest(); break;
                case UIActionKind.SelectShopTab: demo?.SelectShopTab(index); break;
                case UIActionKind.ShowChatInput: router?.Show(ScreenId.ClanChatInput); break;
                case UIActionKind.SendChat: demo?.SendChat(); break;
                case UIActionKind.BeginBattle: GetComponentInParent<UIBackendBindings>()?.BeginBattle(); break;
            }
        }
    }
}
