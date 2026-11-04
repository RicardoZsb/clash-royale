using TMPro;
using UnityEngine;

namespace ClashRoyaleUI
{
    // Presentation state only. The complete uGUI hierarchy is serialized in the prefab.
    [DisallowMultipleComponent]
    public sealed class UIActionPointReserve : MonoBehaviour
    {
        [SerializeField] private GameObject popup;
        [SerializeField] private RectTransform expandArrow;
        [SerializeField] private TMP_Text actionPointLabel;
        [SerializeField] private TMP_Text reserveLabel;
        [SerializeField] private TMP_Text withdrawLabel;
        [SerializeField] private UnityEngine.UI.Button withdrawButton;
        [SerializeField, Min(0)] private int actionPoints = 16;
        [SerializeField, Min(1)] private int actionPointCapacity = 24;
        [SerializeField, Min(0)] private int reservePoints = 16;
        [SerializeField, Min(1)] private int reserveCapacity = 16;

        public int ActionPoints => actionPoints;
        public int ReservePoints => reservePoints;
        public bool IsOpen => popup && popup.activeSelf;

        private void OnEnable()
        {
            Close();
            RefreshLabels();
        }

        private void OnDisable() => Close();

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (!popup) return;
            var page = GetComponentInParent<UIScreenRouter>();
            if (page)
                foreach (var dropdown in page.GetComponentsInChildren<UIRuleDropdown>())
                    dropdown.Close();
            RefreshLabels();
            popup.SetActive(true);
            if (expandArrow) expandArrow.localRotation = Quaternion.Euler(0, 0, 180);
        }

        public void Close()
        {
            if (popup) popup.SetActive(false);
            if (expandArrow) expandArrow.localRotation = Quaternion.Euler(0, 0, 180);
        }

        public void Withdraw()
        {
            var backend = GetComponentInParent<UIBackendClient>();
            if (backend) { backend.Send("/v1/reserve/withdraw"); return; }
            // Fill available capacity; unclaimed points remain in reserve.
            int amount = Mathf.Min(reservePoints, Mathf.Max(0, actionPointCapacity - actionPoints));
            if (amount <= 0) return;
            actionPoints += amount;
            reservePoints -= amount;
            RefreshLabels();
        }

        public void ApplyServerState(PlayerData player)
        {
            actionPoints = player.normal_ap; actionPointCapacity = player.normal_cap;
            reservePoints = player.reserve_ap; reserveCapacity = player.reserve_cap;
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (actionPointLabel) actionPointLabel.text = $"{actionPoints}/{actionPointCapacity}";
            if (reserveLabel) reserveLabel.text = $"{reservePoints}/{reserveCapacity}";
            bool full = actionPoints >= actionPointCapacity;
            if (withdrawButton) withdrawButton.interactable = !full && reservePoints > 0;
            if (withdrawLabel) withdrawLabel.text = full ? "行动点已满" : reservePoints <= 0 ? "暂无结余" : "提取行动点";
        }
    }
}
