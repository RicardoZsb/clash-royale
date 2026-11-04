using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClashRoyaleUI
{
    [DisallowMultipleComponent]
    public sealed class UITaskBoard : MonoBehaviour
    {
        [SerializeField] private UIBackendClient client;
        [SerializeField] private UITaskCard[] cards;
        [SerializeField] private UIRuleDropdown sort;
        private readonly Dictionary<string,bool> expansion = new Dictionary<string,bool>();
        public UITaskCard[] Cards => cards;
        private void OnEnable()
        {
            if (!client) client = GetComponentInParent<UIBackendClient>();
            if (!client) return;
            client.Changed += Refresh; client.BusyChanged += BusyChanged;
            if (sort) sort.OnRuleSelected.AddListener(ChangeSort);
            if (client.State != null) Refresh(client.State);
        }
        private void OnDisable()
        {
            Remember();
            if (client) { client.Changed -= Refresh; client.BusyChanged -= BusyChanged; }
            if (sort) sort.OnRuleSelected.RemoveListener(ChangeSort);
        }
        private void Remember() { if (cards != null) foreach (var card in cards) if (card && card.Data != null && !string.IsNullOrEmpty(card.Data.task_id)) expansion[card.Data.task_id] = card.IsExpanded; }
        private void ChangeSort(int _) { if (client?.State != null) Refresh(client.State); }
        private void BusyChanged(bool _) { if (cards != null) foreach (var card in cards) if (card && card.gameObject.activeSelf) card.Refresh(); }
        public void Refresh(BackendState state)
        {
            Remember();
            IEnumerable<TaskData> data = state.tasks ?? new TaskData[0];
            switch (sort ? sort.SelectedRule : TaskOrderRule.LevelAscending)
            {
                case TaskOrderRule.LevelDescending: data = data.OrderByDescending(t => t.tier).ThenBy(t => t.task_id); break;
                case TaskOrderRule.CompletionDescending: data = data.OrderByDescending(t => (float)t.progress / Mathf.Max(1,t.target_count)).ThenBy(t => t.task_id); break;
                default: data = data.OrderBy(t => t.tier).ThenBy(t => t.task_id); break;
            }
            var rows = data.ToArray();
            for (int i = 0; i < cards.Length; i++)
            {
                bool visible = i < rows.Length; cards[i].gameObject.SetActive(visible);
                if (visible) cards[i].Bind(rows[i], client, expansion.TryGetValue(rows[i].task_id,out bool open) && open);
            }
            if (rows.Length > cards.Length) Debug.LogError("任务行数量不足，请在编辑器增加已序列化的 TaskCard。", this);
        }
    }
}
