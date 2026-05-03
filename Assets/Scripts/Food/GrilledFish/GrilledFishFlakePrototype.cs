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
        [Tooltip("How many soft flesh flakes are generated along the fish body.")]
        [SerializeField] private int flakeCount = 12;
        [Tooltip("Whether this prototype loosens flakes by itself for visual validation.")]
        [SerializeField] private bool demoMotion;
        [Tooltip("Stroke energy needed to fully separate one fish flake.")]
        [SerializeField] private float releaseEnergy = 1.4f;
        [Tooltip("Preferred direction for separating grilled fish flesh fibers.")]
        [SerializeField] private Vector3 fiberDirection = Vector3.right;

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
            var skin = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            skin.name = "Crisp Skin Shell";
            skin.transform.SetParent(transform, false);
            skin.transform.localPosition = new Vector3(0f, 0.025f, 0f);
            skin.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            skin.transform.localScale = new Vector3(0.22f, 0.82f, 0.09f);
            skin.GetComponent<Renderer>().sharedMaterial = skinMaterial;
            DestroyGeneratedObject(skin.GetComponent<Collider>());
        }

        private void CreateBone()
        {
            var bone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bone.name = "Center Bone";
            bone.transform.SetParent(transform, false);
            bone.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            bone.transform.localScale = new Vector3(1.25f, 0.018f, 0.028f);
            bone.GetComponent<Renderer>().sharedMaterial = boneMaterial;
            DestroyGeneratedObject(bone.GetComponent<Collider>());
        }

        private void CreateFlakes()
        {
            var startX = -0.52f;
            var spacing = 1.04f / Mathf.Max(1, flakeCount - 1);
            for (var i = 0; i < flakeCount; i++)
            {
                var flakeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flakeObject.name = $"Flesh Flake {i + 1:00}";
                flakeObject.transform.SetParent(flakeRoot, false);
                var x = startX + spacing * i;
                flakeObject.transform.localPosition = new Vector3(x, 0.105f, i % 2 == 0 ? 0.04f : -0.04f);
                flakeObject.transform.localRotation = Quaternion.Euler(0f, i % 2 == 0 ? -8f : 8f, 0f);
                flakeObject.transform.localScale = new Vector3(0.08f, 0.026f, 0.18f);
                flakeObject.GetComponent<Renderer>().sharedMaterial = fleshMaterial;
                DestroyGeneratedObject(flakeObject.GetComponent<Collider>());

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
            skinMaterial = skinMaterial != null ? skinMaterial : CreateMaterial("Grilled Fish Skin", new Color(0.82f, 0.48f, 0.18f, 1f));
            fleshMaterial = fleshMaterial != null ? fleshMaterial : CreateMaterial("Grilled Fish Flesh", new Color(1f, 0.91f, 0.74f, 1f));
            boneMaterial = boneMaterial != null ? boneMaterial : CreateMaterial("Fish Bone", new Color(0.94f, 0.88f, 0.76f, 1f));
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
