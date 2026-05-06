using System.Collections;
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
        private Vector2 lastParentSize = Vector2.negativeInfinity;
        private int lastChildCount = -1;
        private bool forceNextLayout = true;
        private bool isApplyingLayout;

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
            RequestLayout();
        }

        public void ApplyLayout()
        {
            if (isApplyingLayout)
            {
                return;
            }

            EnsureReferences();
            if (rectTransform == null || grid == null)
            {
                return;
            }

            var width = rectTransform.rect.width;
            if (forceNextLayout && width <= 1f)
            {
                Canvas.ForceUpdateCanvases();
                width = rectTransform.rect.width;
            }

            var parentRect = rectTransform.parent as RectTransform;
            var parentSize = parentRect == null ? Vector2.zero : parentRect.rect.size;
            if (width <= 1f)
            {
                return;
            }

            var childCount = rectTransform.childCount;
            if (!forceNextLayout &&
                Mathf.Approximately(width, lastWidth) &&
                Approximately(parentSize, lastParentSize) &&
                childCount == lastChildCount &&
                grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                return;
            }

            isApplyingLayout = true;
            try
            {
                forceNextLayout = false;
                lastWidth = width;
                lastParentSize = parentSize;
                lastChildCount = childCount;
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

                var rows = Mathf.Max(1, Mathf.CeilToInt(childCount / (float)columns));
                var contentHeight = appliedPadding.top + appliedPadding.bottom + rows * cellHeight + Mathf.Max(0, rows - 1) * spacing.y;
                if (!Mathf.Approximately(rectTransform.sizeDelta.y, contentHeight))
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                Canvas.ForceUpdateCanvases();
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        public void RequestLayout()
        {
            forceNextLayout = true;
            ApplyLayout();
        }

        private void OnEnable()
        {
            RequestLayout();
            StartCoroutine(RebuildAfterCanvasLayout());
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isApplyingLayout)
            {
                return;
            }

            RequestLayout();
        }

        private void LateUpdate()
        {
            ApplyLayout();
        }

        private IEnumerator RebuildAfterCanvasLayout()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            RequestLayout();
            Canvas.ForceUpdateCanvases();
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

        private static bool Approximately(Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }
    }
}
