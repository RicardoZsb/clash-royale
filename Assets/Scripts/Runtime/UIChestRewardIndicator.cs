using UnityEngine;

namespace ClashRoyaleUI
{
    // The circular gift button stays in place; only its authored extension and red dot toggle.
    public sealed class UIChestRewardIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject extension;
        [SerializeField] private GameObject redDot;
        private UIBackendClient client;
        private void OnEnable()
        {
            client = GetComponentInParent<UIBackendClient>();
            if (client) client.Changed += Apply;
            if (client && client.State != null) Apply(client.State);
            else SetPendingCount(0);
        }
        private void OnDisable() { if (client) client.Changed -= Apply; }
        private void Apply(BackendState state) => SetPendingCount(state.player.pending_chests);
        public void SetPendingCount(int count)
        {
            if (extension) extension.SetActive(count > 0);
            if (redDot) redDot.SetActive(count > 0);
        }
    }
}
