using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ClashRoyaleUI
{
    public enum TaskOrderRule { LevelAscending, LevelDescending, CompletionDescending }

    // All controls and option rows are authored in the prefab. This only changes their state.
    [DisallowMultipleComponent]
    public sealed class UIRuleDropdown : MonoBehaviour
    {
        [SerializeField] private GameObject popup;
        [SerializeField] private TMP_Text caption;
        [SerializeField] private TMP_Text expandArrow;
        [SerializeField] private string[] optionLabels;
        [SerializeField] private UnityEngine.UI.Image[] optionBackgrounds;
        [SerializeField] private Color normalColor = new Color32(248, 246, 241, 255);
        [SerializeField] private Color selectedColor = new Color32(255, 226, 125, 255);
        [SerializeField] private TaskOrderRule selectedRule;
        [SerializeField] private UnityEvent<int> onRuleSelected = new UnityEvent<int>();

        public TaskOrderRule SelectedRule => selectedRule;
        public bool IsOpen => popup && popup.activeSelf;
        public UnityEvent<int> OnRuleSelected => onRuleSelected;

        private void OnEnable() => RefreshSelection();
        private void OnDisable() => Close();

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (!popup) return;
            RefreshSelection();
            popup.SetActive(true);
            if (expandArrow) expandArrow.text = "▲";
        }

        public void Close()
        {
            if (popup) popup.SetActive(false);
            if (expandArrow) expandArrow.text = "▼";
        }

        public void SelectRule(int index)
        {
            if (optionLabels == null || index < 0 || index >= optionLabels.Length) return;
            selectedRule = (TaskOrderRule)index;
            RefreshSelection();
            Close();
            onRuleSelected.Invoke(index);
        }

        public void SetRuleWithoutNotify(int index)
        {
            if (optionLabels == null || index < 0 || index >= optionLabels.Length) return;
            selectedRule = (TaskOrderRule)index;
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            int index = (int)selectedRule;
            if (caption && optionLabels != null && index >= 0 && index < optionLabels.Length)
                caption.text = optionLabels[index];
            if (optionBackgrounds == null) return;
            for (int i = 0; i < optionBackgrounds.Length; i++)
                if (optionBackgrounds[i]) optionBackgrounds[i].color = i == index ? selectedColor : normalColor;
        }
    }
}
