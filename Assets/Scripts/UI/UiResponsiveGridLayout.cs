using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class UiResponsiveGridLayout : MonoBehaviour
    {
        [SerializeField, Min(1)] private int preferredColumnCount = 2;
        [SerializeField, Min(80f)] private float minCellWidth = 360f;
        [SerializeField, Min(0.2f)] private float cellAspectRatio = 2.15f;
        [SerializeField] private Vector2 spacing = new Vector2(20f, 18f);
        [SerializeField] private RectOffset padding;

        private RectTransform rectTransform;
        private GridLayoutGroup grid;
        private float lastWidth = -1f;

        public void Configure(
            int preferredColumns,
            float minimumCellWidth,
            float aspectRatio,
            Vector2 gridSpacing,
            RectOffset gridPadding)
        {
            preferredColumnCount = Mathf.Max(1, preferredColumns);
            minCellWidth = Mathf.Max(80f, minimumCellWidth);
            cellAspectRatio = Mathf.Max(0.2f, aspectRatio);
            spacing = gridSpacing;
            padding = gridPadding ?? new RectOffset();
            ApplyLayout();
        }

        public void ApplyLayout()
        {
            EnsureReferences();
            if (rectTransform == null || grid == null)
            {
                return;
            }

            var width = rectTransform.rect.width;
            if (width <= 1f)
            {
                return;
            }

            if (Mathf.Approximately(width, lastWidth) && grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                return;
            }

            lastWidth = width;
            grid.spacing = spacing;
            var appliedPadding = padding ?? new RectOffset();
            grid.padding = appliedPadding;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.childAlignment = TextAnchor.UpperLeft;

            var availableWidth = Mathf.Max(1f, width - appliedPadding.left - appliedPadding.right);
            var columnsByWidth = Mathf.FloorToInt((availableWidth + spacing.x) / (minCellWidth + spacing.x));
            var columns = Mathf.Clamp(columnsByWidth, 1, preferredColumnCount);
            var cellWidth = (availableWidth - spacing.x * (columns - 1)) / columns;
            var cellHeight = cellWidth / cellAspectRatio;

            grid.constraintCount = columns;
            grid.cellSize = new Vector2(cellWidth, cellHeight);
        }

        private void OnEnable()
        {
            ApplyLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyLayout();
        }

        private void LateUpdate()
        {
            ApplyLayout();
        }

        private void EnsureReferences()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (grid == null)
            {
                grid = GetComponent<GridLayoutGroup>();
            }
        }
    }
}
