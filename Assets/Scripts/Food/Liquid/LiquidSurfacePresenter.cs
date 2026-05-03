using UnityEngine;

namespace ChopChop.Food.Liquid
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("ChopChop/Food/Liquid Surface Presenter")]
    public sealed class LiquidSurfacePresenter : MonoBehaviour
    {
        [SerializeField] private Vector2 radius = new Vector2(0.18f, 0.18f);
        [SerializeField] private int segments = 64;
        [SerializeField] private int rings = 4;
        [SerializeField] private float rippleAmplitude = 0.006f;
        [SerializeField] private float rippleFrequency = 10f;
        [SerializeField] private float rippleSpeed = 1.6f;
        [SerializeField] private float edgeWobble = 0.012f;
        [Range(0f, 1f)] [SerializeField] private float viscosity = 0.45f;
        [SerializeField] private Color baseColor = new Color(1f, 0.72f, 0.18f, 1f);
        [SerializeField] private Color mixedColor = new Color(1f, 0.86f, 0.28f, 1f);
        [Range(0f, 1f)] [SerializeField] private float mix01;
        [Range(0f, 1f)] [SerializeField] private float activity01;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Vector3[] baseVertices;
        private Vector3[] animatedVertices;
        private float displayedActivity01;
        private float lastAnimationTime;

        public float Mix01 => mix01;
        public float Activity01 => activity01;
        public float Viscosity => viscosity;

        public void Configure(
            Vector2 surfaceRadius,
            Color liquidColor,
            Color completeMixColor,
            float amplitude,
            float speed,
            float viscosity01 = -1f)
        {
            radius = surfaceRadius;
            baseColor = liquidColor;
            mixedColor = completeMixColor;
            rippleAmplitude = amplitude;
            rippleSpeed = speed;
            if (viscosity01 >= 0f)
            {
                viscosity = Mathf.Clamp01(viscosity01);
            }

            RebuildMesh();
            ApplyMaterialProperties();
        }

        public void SetMix(float value)
        {
            mix01 = Mathf.Clamp01(value);
            ApplyMaterialProperties();
        }

        public void SetActivity(float value)
        {
            activity01 = Mathf.Clamp01(value);
        }

        public void SetViscosity(float value)
        {
            viscosity = Mathf.Clamp01(value);
        }

        private void OnEnable()
        {
            EnsureComponents();
            RebuildMesh();
            ApplyMaterialProperties();
        }

        private void OnValidate()
        {
            segments = Mathf.Clamp(segments, 12, 160);
            rings = Mathf.Clamp(rings, 1, 16);
            radius = new Vector2(Mathf.Max(0.01f, radius.x), Mathf.Max(0.01f, radius.y));
            rippleAmplitude = Mathf.Max(0f, rippleAmplitude);
            rippleFrequency = Mathf.Max(0.1f, rippleFrequency);
            rippleSpeed = Mathf.Max(0f, rippleSpeed);
            activity01 = Mathf.Clamp01(activity01);
            viscosity = Mathf.Clamp01(viscosity);

            EnsureComponents();
            RebuildMesh();
            ApplyMaterialProperties();
        }

        private void LateUpdate()
        {
            if (mesh == null || baseVertices == null || animatedVertices == null)
            {
                return;
            }

#if UNITY_EDITOR
            var time = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
            var time = Time.time;
#endif
            var deltaTime = lastAnimationTime > 0f ? Mathf.Clamp(time - lastAnimationTime, 0f, 0.1f) : 1f / 60f;
            lastAnimationTime = time;

            var responseSpeed = Mathf.Lerp(12f, 2.2f, viscosity);
            displayedActivity01 = Mathf.Lerp(
                displayedActivity01,
                activity01,
                1f - Mathf.Exp(-responseSpeed * deltaTime));

            var effectiveSpeed = rippleSpeed * Mathf.Lerp(1f, 0.42f, viscosity);
            var effectiveFrequency = rippleFrequency * Mathf.Lerp(1f, 0.55f, viscosity);
            var amplitude = rippleAmplitude * (1f + displayedActivity01 * Mathf.Lerp(2.6f, 1.45f, viscosity));
            var swirlAmplitude = rippleAmplitude * displayedActivity01 * viscosity * 0.9f;

            for (var i = 0; i < baseVertices.Length; i++)
            {
                var vertex = baseVertices[i];
                var radial = Mathf.Sqrt(Mathf.Clamp01(
                    Mathf.Pow(vertex.x / radius.x, 2f) +
                    Mathf.Pow(vertex.z / radius.y, 2f)));
                var angle = Mathf.Atan2(vertex.z / radius.y, vertex.x / radius.x);
                var wavePhase = vertex.x * 7.1f + vertex.z * 5.3f + time * effectiveSpeed;
                var swirlPhase = angle * 2f + radial * 4f - time * effectiveSpeed * 1.8f;
                var meniscus = Mathf.SmoothStep(0f, 1f, radial);
                vertex.y =
                    Mathf.Sin(wavePhase * effectiveFrequency) * amplitude * radial +
                    Mathf.Sin(swirlPhase) * swirlAmplitude * (1f - meniscus) +
                    Mathf.Pow(radial, 4f) * rippleAmplitude * 0.35f;
                animatedVertices[i] = vertex;
            }

            mesh.vertices = animatedVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void EnsureComponents()
        {
            meshFilter = meshFilter != null ? meshFilter : GetComponent<MeshFilter>();
            meshRenderer = meshRenderer != null ? meshRenderer : GetComponent<MeshRenderer>();
            propertyBlock = propertyBlock ?? new MaterialPropertyBlock();
        }

        private void RebuildMesh()
        {
            EnsureComponents();

            if (mesh == null)
            {
                mesh = new Mesh
                {
                    name = $"{name} Liquid Surface",
                    hideFlags = HideFlags.DontSave
                };
                mesh.MarkDynamic();
                meshFilter.sharedMesh = mesh;
            }

            var vertexCount = 1 + rings * segments;
            baseVertices = new Vector3[vertexCount];
            animatedVertices = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var triangles = new int[segments * 3 + Mathf.Max(0, rings - 1) * segments * 6];

            baseVertices[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);

            var vertex = 1;
            for (var ring = 1; ring <= rings; ring++)
            {
                var t = ring / (float)rings;
                for (var segment = 0; segment < segments; segment++)
                {
                    var angle = segment / (float)segments * Mathf.PI * 2f;
                    var rimNoise = ring == rings
                        ? 1f + Mathf.Sin(angle * 5f) * edgeWobble
                        : 1f;
                    var x = Mathf.Cos(angle) * radius.x * t * rimNoise;
                    var z = Mathf.Sin(angle) * radius.y * t * rimNoise;
                    baseVertices[vertex] = new Vector3(x, 0f, z);
                    uvs[vertex] = new Vector2(x / (radius.x * 2f) + 0.5f, z / (radius.y * 2f) + 0.5f);
                    vertex++;
                }
            }

            var tri = 0;
            for (var segment = 0; segment < segments; segment++)
            {
                triangles[tri++] = 0;
                triangles[tri++] = 1 + (segment + 1) % segments;
                triangles[tri++] = 1 + segment;
            }

            for (var ring = 2; ring <= rings; ring++)
            {
                var innerStart = 1 + (ring - 2) * segments;
                var outerStart = 1 + (ring - 1) * segments;
                for (var segment = 0; segment < segments; segment++)
                {
                    var next = (segment + 1) % segments;
                    triangles[tri++] = innerStart + segment;
                    triangles[tri++] = innerStart + next;
                    triangles[tri++] = outerStart + segment;
                    triangles[tri++] = innerStart + next;
                    triangles[tri++] = outerStart + next;
                    triangles[tri++] = outerStart + segment;
                }
            }

            mesh.Clear();
            mesh.vertices = baseVertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void ApplyMaterialProperties()
        {
            if (meshRenderer == null)
            {
                return;
            }

            var color = Color.Lerp(baseColor, mixedColor, mix01);
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
