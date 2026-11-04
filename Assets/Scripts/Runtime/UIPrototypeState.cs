using UnityEngine;
using TMPro;

namespace ClashRoyaleUI
{
    // Local presentation state only. All views, labels, slots and buttons are serialized in the scene.
    public sealed class UIPrototypeState : MonoBehaviour
    {
        public UIScreenRouter router;
        public TMP_Text claimLabel, chestCount, chatMessage;
        public TMP_InputField chatInput;
        public GameObject[] freeMarkers, completedPremiumLocks, chestGroups;
        public UnityEngine.UI.Image[] shopTabs;
        public UnityEngine.UI.Image revealImage;
        public Sprite twoStarChest, fourStarChest;
        public GameObject chatBubble;
        public int pendingChests = 4;
        public bool passUnlocked, rewardsClaimed;
        public void ClaimFreeRewards()
        {
            var backend = GetComponent<UIBackendBindings>();
            if (backend) { backend.ClaimFreeRewards(); return; }
            if (rewardsClaimed) return;
            rewardsClaimed = true;
            foreach (var item in freeMarkers) if (item) item.SetActive(false);
            if (claimLabel) claimLabel.text = "已领取";
        }
        public void UnlockPass()
        {
            var backend = GetComponent<UIBackendClient>();
            if (backend) { if(!backend.Busy)backend.Send("/v1/pass/unlock"); return; }
            passUnlocked = !passUnlocked;
            foreach (var item in completedPremiumLocks) if (item) item.SetActive(!passUnlocked);
        }
        public void OpenChest(int group)
        {
            var backend = GetComponent<UIBackendBindings>();
            if (backend) { backend.OpenChest(group); return; }
            if (group < 0 || group >= chestGroups.Length || !chestGroups[group].activeSelf) return;
            int cost = group == 0 ? 2 : 1;
            if (pendingChests < cost) return;
            pendingChests -= cost;
            chestGroups[group].SetActive(false);
            if (chestCount) chestCount.text = "×" + pendingChests;
            if (revealImage) revealImage.sprite = group == 0 ? fourStarChest : twoStarChest;
            router.Show(ScreenId.ChestReveal);
        }
        public void CloseChest() => router.Show(ScreenId.ChestQueue);
        public void SelectShopTab(int index)
        {
            for (int i = 0; i < shopTabs.Length; i++) if (shopTabs[i])
                shopTabs[i].color = i == index ? new Color32(255, 218, 74, 255) : new Color32(217, 215, 206, 255);
        }
        public void SendChat()
        {
            if (!chatInput || string.IsNullOrWhiteSpace(chatInput.text)) return;
            if (chatMessage) chatMessage.text = chatInput.text;
            if (chatBubble) chatBubble.SetActive(true);
            chatInput.text = "";
            router.Show(ScreenId.ClanChat);
        }
    }
}
