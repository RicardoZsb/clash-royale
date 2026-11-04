using UnityEngine;

namespace ClashRoyaleUI
{
    [ExecuteAlways]
    public sealed class TrainingBattleLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform board;
        public RectTransform Board { get => board; set { board = value; Fit(); } }
        private void OnEnable() => Fit();
        private void OnRectTransformDimensionsChange() => Fit();
        private void Fit()
        {
            if (!board) return;
            var size = ((RectTransform)transform).rect.size;
            float scale = Mathf.Min(size.x / 402f, size.y / 874f);
            board.localScale = Vector3.one * Mathf.Max(.01f, scale);
        }
    }
}
