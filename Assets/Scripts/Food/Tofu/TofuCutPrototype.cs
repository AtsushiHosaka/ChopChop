using System.Collections.Generic;
using UnityEngine;

namespace ChopChop.Food.Tofu
{
    [AddComponentMenu("ChopChop/Food/Tofu Cut Prototype")]
    public sealed class TofuCutPrototype : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform cellRoot;
        [SerializeField] private Transform grooveRoot;
        [SerializeField] private Material tofuMaterial;
        [SerializeField] private Material grooveMaterial;
        [Tooltip("Number of latent tofu pieces along the x axis after cutting.")]
        [SerializeField] private int columns = 3;
        [Tooltip("Number of latent tofu pieces along the z axis after cutting.")]
        [SerializeField] private int rows = 3;
        [Tooltip("Visible spacing revealed after a cut reaches full depth.")]
        [SerializeField] private float grooveWidth = 0.018f;
        [Tooltip("Stroke pressure needed to reveal one complete cut line.")]
        [SerializeField] private float cutResistance = 1.4f;
        [Tooltip("Whether this prototype cuts itself for validation.")]
        [SerializeField] private bool demoMotion;

        private readonly List<Cell> cells = new List<Cell>();
        private readonly List<CutLine> cutLines = new List<CutLine>();
        private GameObject wholeBlock;

        public float Cut01
        {
            get
            {
                if (cutLines.Count == 0)
                {
                    return 0f;
                }

                var cut = 0f;
                for (var i = 0; i < cutLines.Count; i++)
                {
                    cut += cutLines[i].Progress01;
                }

                return cut / cutLines.Count;
            }
        }

        public float Damage01 => Cut01;

        public void Rebuild()
        {
            ClearChildren();
            tofuMaterial = tofuMaterial != null ? tofuMaterial : CreateMaterial("Soft Tofu Prototype", new Color(0.96f, 0.91f, 0.78f, 1f));
            grooveMaterial = grooveMaterial != null ? grooveMaterial : CreateMaterial("Tofu Cut Groove", new Color(0.72f, 0.63f, 0.46f, 1f));

            visualRoot = new GameObject("Single Tofu Body").transform;
            visualRoot.SetParent(transform, false);
            cellRoot = new GameObject("Latent Cut Pieces").transform;
            cellRoot.SetParent(transform, false);
            grooveRoot = new GameObject("Revealed Cut Grooves").transform;
            grooveRoot.SetParent(transform, false);
            cells.Clear();
            cutLines.Clear();

            CreateWholeBlock();
            CreateLatentCells();
            CreateCutLines();
            ApplyVisualState();
        }

        public void ApplyChopstickPressure(Vector3 worldPoint, float pressure, float deltaTime)
        {
            ApplyChopstickCut(worldPoint, transform.TransformDirection(Vector3.forward), pressure, deltaTime);
        }

        public void ApplyChopstickCut(Vector3 worldPoint, Vector3 worldDirection, float pressure, float deltaTime)
        {
            if (cutLines.Count == 0)
            {
                return;
            }

            var localPoint = transform.InverseTransformPoint(worldPoint);
            var localDirection = transform.InverseTransformDirection(worldDirection).normalized;
            var closestIndex = FindClosestCutLine(localPoint, localDirection);
            var line = cutLines[closestIndex];
            line.Progress01 = Mathf.Clamp01(line.Progress01 + pressure * deltaTime / Mathf.Max(0.01f, cutResistance));
            cutLines[closestIndex] = line;
            ApplyVisualState();
        }

        private void Awake()
        {
            if (wholeBlock == null && visualRoot == null)
            {
                Rebuild();
            }
        }

        private void Update()
        {
            if (!demoMotion || cutLines.Count == 0)
            {
                return;
            }

            var index = Mathf.FloorToInt(Time.time * 0.85f) % cutLines.Count;
            var line = cutLines[index];
            var point = transform.TransformPoint(line.SampleLocalPoint);
            var direction = transform.TransformDirection(line.LocalDirection);
            ApplyChopstickCut(point, direction, 0.85f, Time.deltaTime);
        }

        private void CreateWholeBlock()
        {
            wholeBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wholeBlock.name = "Uncut Tofu Block";
            wholeBlock.transform.SetParent(visualRoot, false);
            wholeBlock.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            wholeBlock.transform.localScale = new Vector3(1f, 0.16f, 0.72f);
            wholeBlock.GetComponent<Renderer>().sharedMaterial = tofuMaterial;
            DestroyGeneratedObject(wholeBlock.GetComponent<Collider>());
        }

        private void CreateLatentCells()
        {
            var safeColumns = Mathf.Max(1, columns);
            var safeRows = Mathf.Max(1, rows);
            var cellWidth = (1f - grooveWidth * (safeColumns - 1)) / safeColumns;
            var cellDepth = (0.72f - grooveWidth * (safeRows - 1)) / safeRows;
            var startX = -0.5f + cellWidth * 0.5f;
            var startZ = -0.36f + cellDepth * 0.5f;

            for (var z = 0; z < safeRows; z++)
            {
                for (var x = 0; x < safeColumns; x++)
                {
                    var cellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cellObject.name = $"Cut Tofu Piece {x + 1}-{z + 1}";
                    cellObject.transform.SetParent(cellRoot, false);
                    cellObject.transform.localPosition = new Vector3(
                        startX + x * (cellWidth + grooveWidth),
                        0.08f,
                        startZ + z * (cellDepth + grooveWidth));
                    cellObject.transform.localScale = new Vector3(cellWidth, 0.16f, cellDepth);
                    cellObject.GetComponent<Renderer>().sharedMaterial = tofuMaterial;
                    DestroyGeneratedObject(cellObject.GetComponent<Collider>());

                    cells.Add(new Cell(cellObject.transform, x, z));
                }
            }
        }

        private void CreateCutLines()
        {
            var safeColumns = Mathf.Max(1, columns);
            var safeRows = Mathf.Max(1, rows);
            var cellWidth = (1f - grooveWidth * (safeColumns - 1)) / safeColumns;
            var cellDepth = (0.72f - grooveWidth * (safeRows - 1)) / safeRows;

            for (var x = 1; x < safeColumns; x++)
            {
                var localX = -0.5f + cellWidth * x + grooveWidth * (x - 0.5f);
                var groove = CreateGroove($"Vertical Cut {x}", new Vector3(localX, 0.164f, 0f), new Vector3(grooveWidth, 0.012f, 0.72f));
                cutLines.Add(new CutLine(groove, true, localX, Vector3.forward));
            }

            for (var z = 1; z < safeRows; z++)
            {
                var localZ = -0.36f + cellDepth * z + grooveWidth * (z - 0.5f);
                var groove = CreateGroove($"Horizontal Cut {z}", new Vector3(0f, 0.166f, localZ), new Vector3(1f, 0.012f, grooveWidth));
                cutLines.Add(new CutLine(groove, false, localZ, Vector3.right));
            }
        }

        private Transform CreateGroove(string name, Vector3 localPosition, Vector3 localScale)
        {
            var groove = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groove.name = name;
            groove.transform.SetParent(grooveRoot, false);
            groove.transform.localPosition = localPosition;
            groove.transform.localScale = localScale;
            groove.GetComponent<Renderer>().sharedMaterial = grooveMaterial;
            DestroyGeneratedObject(groove.GetComponent<Collider>());
            return groove.transform;
        }

        private int FindClosestCutLine(Vector3 localPoint, Vector3 localDirection)
        {
            var closestIndex = 0;
            var closestScore = float.MaxValue;
            for (var i = 0; i < cutLines.Count; i++)
            {
                var line = cutLines[i];
                var distance = line.IsVertical ? Mathf.Abs(localPoint.x - line.LocalCoordinate) : Mathf.Abs(localPoint.z - line.LocalCoordinate);
                var alignment = Mathf.Abs(Vector3.Dot(localDirection, line.LocalDirection));
                var score = distance / Mathf.Lerp(0.55f, 1f, alignment);
                if (score < closestScore)
                {
                    closestScore = score;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void ApplyVisualState()
        {
            var fullyCut = Cut01 >= 0.98f;
            if (wholeBlock != null)
            {
                wholeBlock.SetActive(!fullyCut);
            }

            if (cellRoot != null)
            {
                cellRoot.gameObject.SetActive(fullyCut);
            }

            for (var i = 0; i < cutLines.Count; i++)
            {
                var line = cutLines[i];
                var reveal = Mathf.SmoothStep(0f, 1f, line.Progress01);
                line.Transform.gameObject.SetActive(reveal > 0.02f);
                var scale = line.BaseLocalScale;
                if (line.IsVertical)
                {
                    scale.x = Mathf.Lerp(grooveWidth * 0.2f, grooveWidth, reveal);
                }
                else
                {
                    scale.z = Mathf.Lerp(grooveWidth * 0.2f, grooveWidth, reveal);
                }
                line.Transform.localScale = scale;
                cutLines[i] = line;
            }

            ApplyCellSeparation();
        }

        private void ApplyCellSeparation()
        {
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var offset = Vector3.zero;
                for (var lineIndex = 0; lineIndex < cutLines.Count; lineIndex++)
                {
                    var line = cutLines[lineIndex];
                    var reveal = Mathf.SmoothStep(0f, 1f, line.Progress01);
                    if (line.IsVertical)
                    {
                        var side = cell.BaseLocalPosition.x < line.LocalCoordinate ? -1f : 1f;
                        offset.x += side * grooveWidth * 0.45f * reveal;
                    }
                    else
                    {
                        var side = cell.BaseLocalPosition.z < line.LocalCoordinate ? -1f : 1f;
                        offset.z += side * grooveWidth * 0.45f * reveal;
                    }
                }

                cell.Transform.localPosition = cell.BaseLocalPosition + offset;
                cells[i] = cell;
            }
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = name;
            material.color = color;
            return material;
        }

        private void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyGeneratedObject(transform.GetChild(i).gameObject);
            }
        }

        private static void DestroyGeneratedObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private struct Cell
        {
            public readonly Transform Transform;
            public readonly Vector3 BaseLocalPosition;
            public readonly int Column;
            public readonly int Row;

            public Cell(Transform transform, int column, int row)
            {
                Transform = transform;
                BaseLocalPosition = transform.localPosition;
                Column = column;
                Row = row;
            }
        }

        private struct CutLine
        {
            public readonly Transform Transform;
            public readonly bool IsVertical;
            public readonly float LocalCoordinate;
            public readonly Vector3 LocalDirection;
            public readonly Vector3 BaseLocalScale;
            public readonly Vector3 SampleLocalPoint;
            public float Progress01;

            public CutLine(Transform transform, bool isVertical, float localCoordinate, Vector3 localDirection)
            {
                Transform = transform;
                IsVertical = isVertical;
                LocalCoordinate = localCoordinate;
                LocalDirection = localDirection.normalized;
                BaseLocalScale = transform.localScale;
                SampleLocalPoint = transform.localPosition;
                Progress01 = 0f;
            }
        }
    }
}
