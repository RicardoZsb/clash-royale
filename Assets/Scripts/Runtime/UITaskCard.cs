using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI
{
    [DisallowMultipleComponent]
    public sealed class UITaskCard : MonoBehaviour
    {
        [Header("内容（运行时读取配表）")]
        [SerializeField] private TaskData task;
        [SerializeField] private bool expanded;
        [Header("已有 uGUI 组件")]
        [SerializeField] private TMP_Text briefLabel, detailLabel, progressLabel, statusLabel, helpLabel;
        [SerializeField] private GameObject detailGroup;
        [SerializeField] private Button acceptButton, activateButton, helpButton, expandButton;
        [SerializeField] private RectTransform triangle;
        [SerializeField] private Image background, statusBackground;
        [SerializeField] private Sprite collapsedSprite, expandedSprite;
        [SerializeField] private LayoutElement layout;
        [SerializeField] private float collapsedHeight = 104, expandedHeight = 242;
        [Header("任务等级底板颜色")]
        [SerializeField] private Color levelOneColor = new Color32(216,230,240,255);
        [SerializeField] private Color levelTwoColor = new Color32(220,235,205,255);
        [SerializeField] private Color levelThreeColor = new Color32(233,216,241,255);
        [SerializeField] private Color specialColor = new Color32(247,227,181,255);
        private UIBackendClient client;
        public TaskData Data => task;
        public bool IsExpanded => expanded;
        public bool HelpVisible => helpButton && helpButton.gameObject.activeInHierarchy;
        public Button ExpandButton => expandButton;
        public Button AcceptButton => acceptButton;
        public Button HelpButton => helpButton;
        public Button ActivateButton => activateButton;
        public void Bind(TaskData data, UIBackendClient backend, bool expand)
        { task = data; client = backend; expanded = expand; Refresh(); }
        private void OnEnable() => Refresh();
        public void ToggleExpanded() { expanded = !expanded; Refresh(); }
        public void Accept() { if (client && task?.status == "available") client.Send("/v1/tasks/" + task.task_id + "/accept"); }
        public void Activate()
        {
            if (client && task != null && (task.status == "accepted" || task.status == "active"))
                client.Send("/v1/instances/" + task.instance_id + (task.forced ? "/unforce" : "/force-activate"));
        }
        public void RequestHelp() { if (client && task != null && (task.status == "accepted" || task.status == "active")) client.Send("/v1/instances/" + task.instance_id + "/help"); }
        public void Refresh()
        {
            if (task == null) return;
            bool accepted = task.status == "accepted" || task.status == "active";
            bool active = task.status == "active";
            bool busy = client && client.Busy;
            if (briefLabel) briefLabel.text = task.brief;
            if (detailLabel) detailLabel.text = task.description;
            if (progressLabel) progressLabel.text = $"进度 {task.progress}/{task.target_count}　行动点 {task.action_cost}　贡献 {task.base_contribution}";
            if (detailGroup) detailGroup.SetActive(expanded);
            if (statusLabel) statusLabel.text = active ? task.forced ? "强制激活" : "已激活" : accepted ? "已接取" : "接取任务";
            if (acceptButton) acceptButton.interactable = !accepted && !busy && client;
            if (activateButton)
            {
                activateButton.gameObject.SetActive(expanded && accepted && !active);
                activateButton.interactable = !busy && client && string.IsNullOrEmpty(client.State?.battle_id);
                var label = activateButton.GetComponentInChildren<TMP_Text>(true);
                if(label)label.text = task.forced ? "取消强制" : "强制激活";
            }
            if (helpButton) { helpButton.gameObject.SetActive(expanded && accepted); helpButton.interactable = !busy && !task.help_requested; }
            if (helpLabel) helpLabel.text = task.help_requested ? "已求助" : "部落求助";
            if (statusBackground) statusBackground.color = active ? new Color32(229, 175, 230, 255) : accepted ? new Color32(187, 225, 181, 255) : new Color32(171, 212, 218, 255);
            if (background)
            {
                var tierColor = task.tier == 1 ? levelOneColor : task.tier == 2 ? levelTwoColor : task.tier == 3 ? levelThreeColor : specialColor;
                background.sprite = expanded ? expandedSprite : collapsedSprite;
                background.color = tierColor;
                background.type = expanded ? Image.Type.Simple : Image.Type.Sliced;
            }
            if (triangle) triangle.localRotation = Quaternion.Euler(0, 0, expanded ? 0 : 180);
            if (layout) layout.preferredHeight = expanded ? expandedHeight : collapsedHeight;
            if (transform.parent is RectTransform parent) LayoutRebuilder.MarkLayoutForRebuild(parent);
        }
    }
}
