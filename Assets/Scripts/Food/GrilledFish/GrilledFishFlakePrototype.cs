using System.Collections.Generic;
using UnityEngine;

namespace ChopChop.Food.GrilledFish
{
    [AddComponentMenu("ChopChop/Food/Grilled Fish Flake Prototype")]
    public sealed class GrilledFishFlakePrototype : MonoBehaviour
    {
        [SerializeField] private Transform flakeRoot;
        [SerializeField] private Material skinMaterial;
        [SerializeField] private Material fleshMaterial;
        [SerializeField] private Material boneMaterial;
        [SerializeField] private Material charMaterial;
        [Tooltip("How many soft flesh flakes are generated along the fish body.")]
        [SerializeField] private int flakeCount = 18;
        [Tooltip("Whether this prototype loosens flakes by itself for visual validation.")]
        [SerializeField] private bool demoMotion;
        [Tooltip("Stroke energy needed to fully separate one fish flake.")]
        [SerializeField] private float releaseEnergy = 1.4f;
        [Tooltip("Preferred direction for separating grilled fish flesh fibers.")]
        [SerializeField] private Vector3 fiberDirection = Vector3.right;
        [SerializeField] private float bodyLength = 1.55f;
        [SerializeField] private float bodyWidth = 0.34f;
        [SerializeField] private float bodyHeight = 0.16f;

        private readonly List<Flake> flakes = new List<Flake>();

        public float Released01
        {
            get
            {
                if (flakes.Count == 0)
                {
                    return 0f;
                }

                var released = 0f;
                for (var i = 0; i < flakes.Count; i++)
                {
                    released += flakes[i].Release01;
                }

                return released / flakes.Count;
            }
        }

        public void Rebuild()
        {
            ClearChildren();
            EnsureMaterials();

            flakeRoot = new GameObject("Flesh Flakes").transform;
            flakeRoot.SetParent(transform, false);
            flakes.Clear();

            CreateSkin();
            CreateCharMarks();
            CreateHeadAndTail();
            CreateBone();
            CreateFlakes();
        }

        public void ApplyChopstickStroke(Vector3 worldPoint, Vector3 worldDirection, float energy)
        {
            if (flakes.Count == 0)
            {
                return;
            }

            var closestIndex = 0;
            var closestDistance = float.MaxValue;
            for (var i = 0; i < flakes.Count; i++)
            {
                var distance = Vector3.SqrMagnitude(flakes[i].Transform.position - worldPoint);
                if (distance < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }

            var flake = flakes[closestIndex];
            var fiber = transform.TransformDirection(NormalizedOr(fiberDirection, Vector3.right)).normalized;
            var alignedEnergy = energy * Mathf.Lerp(0.25f, 1f, Mathf.Abs(Vector3.Dot(worldDirection.normalized, fiber)));
            flake.Release01 = Mathf.Clamp01(flake.Release01 + alignedEnergy / releaseEnergy);
            ApplyFlakePose(flake, fiber);
            flakes[closestIndex] = flake;
        }

        private void Awake()
        {
            if (flakes.Count == 0 && flakeRoot == null)
            {
                Rebuild();
            }
        }

        private void Update()
        {
            if (!demoMotion || flakes.Count == 0)
            {
                return;
            }

            var index = Mathf.FloorToInt(Time.time * 0.9f) % flakes.Count;
            ApplyChopstickStroke(flakes[index].Transform.position, transform.TransformDirection(fiberDirection), Time.deltaTime * 0.8f);
        }

        private void CreateSkin()
        {
            var skin = new GameObject("Crisp Burnished Skin", typeof(MeshFilter), typeof(MeshRenderer));
            skin.transform.SetParent(transform, false);
            skin.GetComponent<MeshFilter>().sharedMesh = CreateFishBodyMesh(bodyLength, bodyWidth, bodyHeight, 26, 10);
            skin.GetComponent<MeshRenderer>().sharedMaterial = skinMaterial;
        }

        private void CreateCharMarks()
        {
            const int markCount = 9;
            for (var i = 0; i < markCount; i++)
            {
                var t = markCount == 1 ? 0.5f : i / (float)(markCount - 1);
                var x = Mathf.Lerp(-bodyLength * 0.38f, bodyLength * 0.38f, t);
                var side = i % 2 == 0 ? -1f : 1f;

                var mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = $"Grill Mark {i + 1:00}";
                mark.transform.SetParent(transform, false);
                mark.transform.localPosition = new Vector3(x, bodyHeight * 0.87f, side * bodyWidth * 0.11f);
                mark.transform.localRotation = Quaternion.Euler(0f, side * 8f, 15f + side * 4f);
                mark.transform.localScale = new Vector3(0.028f, 0.012f, 0.22f);
                mark.GetComponent<Renderer>().sharedMaterial = charMaterial;
                DestroyGeneratedObject(mark.GetComponent<Collider>());
            }
        }

        private void CreateHeadAndTail()
        {
            var head = new GameObject("Roasted Head Shape", typeof(MeshFilter), typeof(MeshRenderer));
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(-bodyLength * 0.54f, bodyHeight * 0.34f, 0f);
            head.transform.localRotation = Quaternion.Euler(0f, 0f, -5f);
            head.GetComponent<MeshFilter>().sharedMesh = CreateTaperedBoxMesh(0.26f, 0.18f, 0.28f, 0.42f);
            head.GetComponent<MeshRenderer>().sharedMaterial = skinMaterial;

            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Charred Eye Spot";
            eye.transform.SetParent(transform, false);
            eye.transform.localPosition = new Vector3(-bodyLength * 0.61f, bodyHeight * 0.5f, -bodyWidth * 0.18f);
            eye.transform.localScale = new Vector3(0.018f, 0.018f, 0.018f);
            eye.GetComponent<Renderer>().sharedMaterial = charMaterial;
            DestroyGeneratedObject(eye.GetComponent<Collider>());

            var tail = new GameObject("Split Tail", typeof(MeshFilter), typeof(MeshRenderer));
            tail.transform.SetParent(transform, false);
            tail.transform.localPosition = new Vector3(bodyLength * 0.58f, bodyHeight * 0.25f, 0f);
            tail.GetComponent<MeshFilter>().sharedMesh = CreateTailMesh(0.28f, 0.28f, 0.05f);
            tail.GetComponent<MeshRenderer>().sharedMaterial = skinMaterial;
        }

        private void CreateBone()
        {
            var boneRoot = new GameObject("Realistic Bone Structure").transform;
            boneRoot.SetParent(transform, false);

            CreateBoneSegment(boneRoot, "Center Spine", new Vector3(-0.58f, 0.185f, 0f), new Vector3(0.58f, 0.185f, 0f), 0.014f);

            const int ribCount = 14;
            for (var i = 0; i < ribCount; i++)
            {
                var t = ribCount == 1 ? 0.5f : i / (float)(ribCount - 1);
                var x = Mathf.Lerp(-0.5f, 0.5f, t);
                var ribLength = Mathf.Lerp(0.13f, 0.23f, Mathf.Sin(t * Mathf.PI));
                var tilt = Mathf.Lerp(0.06f, -0.08f, t);
                var origin = new Vector3(x, 0.18f, 0f);
                var leftEnd = new Vector3(x + tilt, 0.145f, ribLength);
                var rightEnd = new Vector3(x + tilt, 0.145f, -ribLength);
                CreateBoneSegment(boneRoot, $"Left Rib {i + 1:00}", origin, leftEnd, 0.0075f);
                CreateBoneSegment(boneRoot, $"Right Rib {i + 1:00}", origin, rightEnd, 0.0075f);

                if (i % 2 == 0)
                {
                    var vertebra = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    vertebra.name = $"Vertebra {i + 1:00}";
                    vertebra.transform.SetParent(boneRoot, false);
                    vertebra.transform.localPosition = origin;
                    vertebra.transform.localScale = new Vector3(0.028f, 0.018f, 0.018f);
                    vertebra.GetComponent<Renderer>().sharedMaterial = boneMaterial;
                    DestroyGeneratedObject(vertebra.GetComponent<Collider>());
                }
            }
        }

        private void CreateFlakes()
        {
            var startX = -0.48f;
            var spacing = 0.96f / Mathf.Max(1, flakeCount - 1);
            for (var i = 0; i < flakeCount; i++)
            {
                var flakeObject = new GameObject($"Flesh Flake {i + 1:00}", typeof(MeshFilter), typeof(MeshRenderer));
                flakeObject.name = $"Flesh Flake {i + 1:00}";
                flakeObject.transform.SetParent(flakeRoot, false);
                var x = startX + spacing * i;
                var side = i % 2 == 0 ? 1f : -1f;
                var width = Mathf.Lerp(0.13f, 0.2f, Mathf.Sin(i * 1.71f) * 0.5f + 0.5f);
                var length = Mathf.Lerp(0.08f, 0.14f, Mathf.Sin(i * 2.11f + 0.4f) * 0.5f + 0.5f);
                flakeObject.transform.localPosition = new Vector3(x, 0.205f, side * Mathf.Lerp(0.055f, 0.13f, Mathf.Sin(i * 1.31f) * 0.5f + 0.5f));
                flakeObject.transform.localRotation = Quaternion.Euler(0f, side * Mathf.Lerp(7f, 16f, Mathf.Sin(i) * 0.5f + 0.5f), side * Mathf.Lerp(2f, 8f, Mathf.Sin(i * 0.73f) * 0.5f + 0.5f));
                flakeObject.GetComponent<MeshFilter>().sharedMesh = CreateFleshFlakeMesh(length, width, 0.035f, i);
                flakeObject.GetComponent<MeshRenderer>().sharedMaterial = fleshMaterial;

                flakes.Add(new Flake(flakeObject.transform));
            }
        }

        private void ApplyFlakePose(Flake flake, Vector3 fiber)
        {
            var lift = Mathf.SmoothStep(0f, 1f, flake.Release01);
            flake.Transform.position = flake.BaseWorldPosition + fiber * (0.13f * lift) + Vector3.up * (0.07f * lift);
            flake.Transform.rotation = flake.BaseWorldRotation * Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 18f, lift));
        }

        private void EnsureMaterials()
        {
            skinMaterial = skinMaterial != null ? skinMaterial : CreateMaterial("Grilled Fish Skin", new Color(0.78f, 0.38f, 0.13f, 1f), 0.46f, 0f);
            fleshMaterial = fleshMaterial != null ? fleshMaterial : CreateMaterial("Grilled Fish Flesh", new Color(1f, 0.88f, 0.63f, 1f), 0.18f, 0f);
            boneMaterial = boneMaterial != null ? boneMaterial : CreateMaterial("Fish Bone", new Color(0.96f, 0.9f, 0.74f, 1f), 0.2f, 0f);
            charMaterial = charMaterial != null ? charMaterial : CreateMaterial("Fish Char", new Color(0.11f, 0.065f, 0.035f, 1f), 0.65f, 0f);
        }

        private static Material CreateMaterial(string name, Color color, float smoothness, float metallic)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = name;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            return material;
        }

        private void CreateBoneSegment(Transform parent, string segmentName, Vector3 start, Vector3 end, float radius)
        {
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = segmentName;
            segment.transform.SetParent(parent, false);
            var midpoint = (start + end) * 0.5f;
            var direction = end - start;
            segment.transform.localPosition = midpoint;
            segment.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            segment.transform.localScale = new Vector3(radius, direction.magnitude * 0.5f, radius);
            segment.GetComponent<Renderer>().sharedMaterial = boneMaterial;
            DestroyGeneratedObject(segment.GetComponent<Collider>());
        }

        private static Mesh CreateFishBodyMesh(float length, float width, float height, int lengthSegments, int radialSegments)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            for (var xIndex = 0; xIndex <= lengthSegments; xIndex++)
            {
                var u = xIndex / (float)lengthSegments;
                var x = Mathf.Lerp(-length * 0.5f, length * 0.5f, u);
                var taper = Mathf.Sin(u * Mathf.PI);
                var headBias = Mathf.Lerp(1.12f, 0.82f, u);
                var localWidth = width * (0.08f + taper * 0.92f) * headBias;
                var localHeight = height * (0.16f + taper * 0.84f);

                for (var radialIndex = 0; radialIndex <= radialSegments; radialIndex++)
                {
                    var v = radialIndex / (float)radialSegments;
                    var angle = Mathf.Lerp(0f, Mathf.PI, v);
                    var z = Mathf.Cos(angle) * localWidth * 0.5f;
                    var y = Mathf.Sin(angle) * localHeight;
                    vertices.Add(new Vector3(x, y, z));
                    normals.Add(new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle)).normalized);
                }
            }

            var row = radialSegments + 1;
            for (var xIndex = 0; xIndex < lengthSegments; xIndex++)
            {
                for (var radialIndex = 0; radialIndex < radialSegments; radialIndex++)
                {
                    var a = xIndex * row + radialIndex;
                    var b = a + row;
                    var c = b + 1;
                    var d = a + 1;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            var mesh = new Mesh { name = "Procedural Grilled Fish Body" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateFleshFlakeMesh(float length, float width, float thickness, int seed)
        {
            var random = new System.Random(seed * 7919 + 17);
            var halfLength = length * 0.5f;
            var halfWidth = width * 0.5f;
            var top = thickness * 0.5f;
            var bottom = -thickness * 0.5f;

            var points = new[]
            {
                new Vector3(-halfLength * RandomRange(random, 0.85f, 1.12f), bottom, -halfWidth * RandomRange(random, 0.78f, 1.08f)),
                new Vector3(halfLength * RandomRange(random, 0.82f, 1.08f), bottom, -halfWidth * RandomRange(random, 0.86f, 1.14f)),
                new Vector3(halfLength * RandomRange(random, 0.76f, 1.04f), bottom, halfWidth * RandomRange(random, 0.82f, 1.12f)),
                new Vector3(-halfLength * RandomRange(random, 0.82f, 1.1f), bottom, halfWidth * RandomRange(random, 0.78f, 1.08f)),
                new Vector3(-halfLength * RandomRange(random, 0.8f, 1.06f), top, -halfWidth * RandomRange(random, 0.8f, 1.08f)),
                new Vector3(halfLength * RandomRange(random, 0.78f, 1.08f), top, -halfWidth * RandomRange(random, 0.82f, 1.12f)),
                new Vector3(halfLength * RandomRange(random, 0.78f, 1.06f), top, halfWidth * RandomRange(random, 0.84f, 1.14f)),
                new Vector3(-halfLength * RandomRange(random, 0.8f, 1.08f), top, halfWidth * RandomRange(random, 0.78f, 1.08f))
            };

            var mesh = new Mesh { name = "Procedural Fish Flesh Flake" };
            mesh.vertices = points;
            mesh.triangles = new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTaperedBoxMesh(float length, float height, float width, float frontScale)
        {
            var hx = length * 0.5f;
            var hy = height * 0.5f;
            var hz = width * 0.5f;
            var mesh = new Mesh { name = "Procedural Roasted Fish Head" };
            mesh.vertices = new[]
            {
                new Vector3(-hx, -hy * frontScale, -hz * frontScale),
                new Vector3(-hx, -hy * frontScale, hz * frontScale),
                new Vector3(-hx, hy * frontScale, -hz * frontScale),
                new Vector3(-hx, hy * frontScale, hz * frontScale),
                new Vector3(hx, -hy, -hz),
                new Vector3(hx, -hy, hz),
                new Vector3(hx, hy, -hz),
                new Vector3(hx, hy, hz)
            };
            mesh.triangles = new[]
            {
                0, 2, 3, 0, 3, 1,
                4, 5, 7, 4, 7, 6,
                0, 4, 6, 0, 6, 2,
                1, 3, 7, 1, 7, 5,
                2, 6, 7, 2, 7, 3,
                0, 1, 5, 0, 5, 4
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTailMesh(float length, float width, float thickness)
        {
            var mesh = new Mesh { name = "Procedural Split Fish Tail" };
            mesh.vertices = new[]
            {
                new Vector3(-length * 0.5f, 0f, 0f),
                new Vector3(length * 0.5f, thickness, -width * 0.5f),
                new Vector3(length * 0.28f, 0f, 0f),
                new Vector3(length * 0.5f, thickness, width * 0.5f),
                new Vector3(length * 0.5f, -thickness, -width * 0.5f),
                new Vector3(length * 0.5f, -thickness, width * 0.5f)
            };
            mesh.triangles = new[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 2, 4,
                0, 5, 2
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return Mathf.Lerp(min, max, (float)random.NextDouble());
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

        private static Vector3 NormalizedOr(Vector3 value, Vector3 fallback)
        {
            return value.sqrMagnitude > 0.0001f ? value.normalized : fallback;
        }

        private struct Flake
        {
            public readonly Transform Transform;
            public readonly Vector3 BaseWorldPosition;
            public readonly Quaternion BaseWorldRotation;
            public float Release01;

            public Flake(Transform transform)
            {
                Transform = transform;
                BaseWorldPosition = transform.position;
                BaseWorldRotation = transform.rotation;
                Release01 = 0f;
            }
        }
    }
}
