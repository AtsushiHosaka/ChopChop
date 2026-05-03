using Obi;
using UnityEngine;

namespace ChopChop.Food.Liquid
{
    [AddComponentMenu("ChopChop/Food/Obi Fluid Mix Monitor")]
    public sealed class ObiFluidMixMonitor : MonoBehaviour
    {
        [SerializeField] private ObiSolver solver;
        [SerializeField] private ObiEmitter eggWhiteEmitter;
        [SerializeField] private ObiEmitter eggYolkEmitter;
        [SerializeField] private ObiEmitter brothEmitter;
        [SerializeField] private Color eggWhiteColor = new Color(1f, 0.93f, 0.72f, 1f);
        [SerializeField] private Color eggYolkColor = new Color(1f, 0.58f, 0.05f, 1f);
        [SerializeField] private Color mixedEggColor = new Color(1f, 0.78f, 0.18f, 1f);
        [SerializeField] private Color brothColor = new Color(0.45f, 0.17f, 0.06f, 1f);
        [SerializeField] private Transform[] stirrers = new Transform[0];
        [Min(0.01f)] [SerializeField] private float stirRadius = 0.16f;
        [Min(0f)] [SerializeField] private float stirredDiffusionRate = 3.5f;
        [Min(0f)] [SerializeField] private float passiveDiffusionRate = 0.015f;
        [Min(0f)] [SerializeField] private float minimumStirSpeed = 0.03f;
        [Min(0.001f)] [SerializeField] private float maximumStirSpeed = 0.75f;

        private float eggMix01;
        private int activeEggParticleCount;
        private bool hasWhiteParticles;
        private bool hasYolkParticles;
        private Vector3[] previousStirrerPositions = new Vector3[0];
        private bool[] hasPreviousStirrerPositions = new bool[0];
        private float[] stirrerSpeeds = new float[0];

        public float EggMix01 => eggMix01;
        public int ActiveEggParticleCount => activeEggParticleCount;
        public bool HasActiveEggFluid => activeEggParticleCount > 0 && hasWhiteParticles && hasYolkParticles;

        public void SetStirrers(params Transform[] activeStirrers)
        {
            stirrers = activeStirrers ?? new Transform[0];
            EnsureStirrerState();
        }

        public void Configure(
            ObiSolver obiSolver,
            ObiEmitter whiteEmitter,
            ObiEmitter yolkEmitter,
            ObiEmitter sukiyakiBrothEmitter)
        {
            Unsubscribe();
            solver = obiSolver;
            eggWhiteEmitter = whiteEmitter;
            eggYolkEmitter = yolkEmitter;
            brothEmitter = sukiyakiBrothEmitter;
            Subscribe();
        }

        private void Reset()
        {
            solver = GetComponent<ObiSolver>();
        }

        private void OnEnable()
        {
            if (solver == null)
            {
                solver = GetComponent<ObiSolver>();
            }

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void LateUpdate()
        {
            ApplyStirring(Time.deltaTime);
            RefreshEggMix();
            ApplyEggColors();
            ApplyBrothColor();
        }

        private void ApplyStirring(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            EnsureStirrerState();
            MeasureStirrerSpeeds(deltaTime);
            ApplyDiffusion(eggWhiteEmitter, deltaTime);
            ApplyDiffusion(eggYolkEmitter, deltaTime);
        }

        private void EnsureStirrerState()
        {
            var length = stirrers != null ? stirrers.Length : 0;
            if (previousStirrerPositions.Length == length && hasPreviousStirrerPositions.Length == length)
            {
                return;
            }

            previousStirrerPositions = new Vector3[length];
            hasPreviousStirrerPositions = new bool[length];
            stirrerSpeeds = new float[length];
        }

        private void MeasureStirrerSpeeds(float deltaTime)
        {
            for (var i = 0; i < stirrerSpeeds.Length; i++)
            {
                stirrerSpeeds[i] = 0f;
                var stirrer = stirrers[i];
                if (stirrer == null)
                {
                    continue;
                }

                if (hasPreviousStirrerPositions[i])
                {
                    stirrerSpeeds[i] = (stirrer.position - previousStirrerPositions[i]).magnitude / deltaTime;
                }

                previousStirrerPositions[i] = stirrer.position;
                hasPreviousStirrerPositions[i] = true;
            }
        }

        private void ApplyDiffusion(ObiEmitter emitter, float deltaTime)
        {
            if (emitter == null || !emitter.isLoaded || emitter.solver == null)
            {
                return;
            }

            for (var i = 0; i < emitter.activeParticleCount && i < emitter.solverIndices.count; i++)
            {
                var solverIndex = emitter.solverIndices[i];
                if (emitter.solver.life[solverIndex] <= 0f)
                {
                    continue;
                }

                var current = ReadYolk01(emitter, solverIndex);
                var intensity = StirIntensityAt(emitter.solver, solverIndex);
                var rate = passiveDiffusionRate + stirredDiffusionRate * intensity;
                var amount = 1f - Mathf.Exp(-rate * deltaTime);
                if (amount <= 0f)
                {
                    continue;
                }

                emitter.solver.userData[solverIndex] = new Vector4(Mathf.Lerp(current, 0.5f, amount), 0f, 0f, 0f);
            }
        }

        private float StirIntensityAt(ObiSolver particleSolver, int solverIndex)
        {
            if (stirrers == null || stirrers.Length == 0 || solverIndex < 0 || solverIndex >= particleSolver.positions.count)
            {
                return 0f;
            }

            var particlePosition = particleSolver.positions[solverIndex];
            var worldPosition = particleSolver.transform.TransformPoint(new Vector3(particlePosition.x, particlePosition.y, particlePosition.z));
            var intensity = 0f;
            for (var i = 0; i < stirrers.Length; i++)
            {
                var stirrer = stirrers[i];
                if (stirrer == null)
                {
                    continue;
                }

                var distance01 = 1f - Mathf.Clamp01(Vector3.Distance(worldPosition, stirrer.position) / stirRadius);
                var speed01 = Mathf.InverseLerp(minimumStirSpeed, maximumStirSpeed, stirrerSpeeds[i]);
                intensity = Mathf.Max(intensity, distance01 * speed01);
            }

            return intensity;
        }

        private void Subscribe()
        {
            if (eggWhiteEmitter != null)
            {
                eggWhiteEmitter.OnEmitParticle += HandleEmitParticle;
            }

            if (eggYolkEmitter != null)
            {
                eggYolkEmitter.OnEmitParticle += HandleEmitParticle;
            }

            if (brothEmitter != null)
            {
                brothEmitter.OnEmitParticle += HandleEmitParticle;
            }
        }

        private void Unsubscribe()
        {
            if (eggWhiteEmitter != null)
            {
                eggWhiteEmitter.OnEmitParticle -= HandleEmitParticle;
            }

            if (eggYolkEmitter != null)
            {
                eggYolkEmitter.OnEmitParticle -= HandleEmitParticle;
            }

            if (brothEmitter != null)
            {
                brothEmitter.OnEmitParticle -= HandleEmitParticle;
            }
        }

        private void HandleEmitParticle(ObiEmitter emitter, int particleIndex)
        {
            if (emitter == null || emitter.solver == null || particleIndex < 0 || particleIndex >= emitter.solverIndices.count)
            {
                return;
            }

            var solverIndex = emitter.solverIndices[particleIndex];
            if (emitter == eggYolkEmitter)
            {
                SetParticleData(emitter.solver, solverIndex, 1f, eggYolkColor);
            }
            else if (emitter == eggWhiteEmitter)
            {
                SetParticleData(emitter.solver, solverIndex, 0f, eggWhiteColor);
            }
            else if (emitter == brothEmitter)
            {
                SetParticleData(emitter.solver, solverIndex, 0f, brothColor);
            }
        }

        private static void SetParticleData(ObiSolver particleSolver, int solverIndex, float yolk01, Color color)
        {
            particleSolver.userData[solverIndex] = new Vector4(yolk01, 0f, 0f, 0f);
            particleSolver.colors[solverIndex] = color;
        }

        private void RefreshEggMix()
        {
            activeEggParticleCount = 0;
            hasWhiteParticles = false;
            hasYolkParticles = false;

            var whiteCount = CountActiveParticles(eggWhiteEmitter);
            var yolkCount = CountActiveParticles(eggYolkEmitter);
            activeEggParticleCount = whiteCount + yolkCount;
            hasWhiteParticles = whiteCount > 0;
            hasYolkParticles = yolkCount > 0;

            if (!HasActiveEggFluid)
            {
                eggMix01 = 0f;
                return;
            }

            var mean = SumYolk(eggWhiteEmitter) + SumYolk(eggYolkEmitter);
            mean /= activeEggParticleCount;

            var variance = SumVariance(eggWhiteEmitter, mean) + SumVariance(eggYolkEmitter, mean);
            variance /= activeEggParticleCount;

            var maxVariance = Mathf.Max(0.0001f, mean * (1f - mean));
            eggMix01 = Mathf.Clamp01(1f - variance / maxVariance);
            if (!IsFinite(eggMix01))
            {
                eggMix01 = 0f;
            }
        }

        private int CountActiveParticles(ObiEmitter emitter)
        {
            if (emitter == null || !emitter.isLoaded || emitter.solver == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < emitter.activeParticleCount && i < emitter.solverIndices.count; i++)
            {
                if (emitter.solver.life[emitter.solverIndices[i]] > 0f)
                {
                    count++;
                }
            }

            return count;
        }

        private float SumYolk(ObiEmitter emitter)
        {
            if (emitter == null || !emitter.isLoaded || emitter.solver == null)
            {
                return 0f;
            }

            var sum = 0f;
            for (var i = 0; i < emitter.activeParticleCount && i < emitter.solverIndices.count; i++)
            {
                var solverIndex = emitter.solverIndices[i];
                if (emitter.solver.life[solverIndex] > 0f)
                {
                    sum += ReadYolk01(emitter, solverIndex);
                }
            }

            return sum;
        }

        private float SumVariance(ObiEmitter emitter, float mean)
        {
            if (emitter == null || !emitter.isLoaded || emitter.solver == null)
            {
                return 0f;
            }

            var sum = 0f;
            for (var i = 0; i < emitter.activeParticleCount && i < emitter.solverIndices.count; i++)
            {
                var solverIndex = emitter.solverIndices[i];
                if (emitter.solver.life[solverIndex] > 0f)
                {
                    var delta = ReadYolk01(emitter, solverIndex) - mean;
                    sum += delta * delta;
                }
            }

            return sum;
        }

        private void ApplyEggColors()
        {
            ApplyEggColors(eggWhiteEmitter);
            ApplyEggColors(eggYolkEmitter);
        }

        private void ApplyEggColors(ObiEmitter emitter)
        {
            if (emitter == null || !emitter.isLoaded || emitter.solver == null)
            {
                return;
            }

            for (var i = 0; i < emitter.activeParticleCount && i < emitter.solverIndices.count; i++)
            {
                var solverIndex = emitter.solverIndices[i];
                if (emitter.solver.life[solverIndex] <= 0f)
                {
                    continue;
                }

                var yolk01 = ReadYolk01(emitter, solverIndex);
                var particleColor = Color.Lerp(eggWhiteColor, eggYolkColor, yolk01);
                emitter.solver.colors[solverIndex] = Color.Lerp(particleColor, mixedEggColor, eggMix01 * 0.45f);
            }

            emitter.solver.colors.Upload(true);
        }

        private void ApplyBrothColor()
        {
            if (brothEmitter == null || !brothEmitter.isLoaded || brothEmitter.solver == null)
            {
                return;
            }

            for (var i = 0; i < brothEmitter.activeParticleCount && i < brothEmitter.solverIndices.count; i++)
            {
                var solverIndex = brothEmitter.solverIndices[i];
                if (brothEmitter.solver.life[solverIndex] > 0f)
                {
                    brothEmitter.solver.colors[solverIndex] = brothColor;
                }
            }

            brothEmitter.solver.colors.Upload(true);
        }

        private float ReadYolk01(ObiEmitter emitter, int solverIndex)
        {
            var fallback = emitter == eggYolkEmitter ? 1f : 0f;
            if (emitter == null || emitter.solver == null || solverIndex < 0 || solverIndex >= emitter.solver.userData.count)
            {
                return fallback;
            }

            var value = emitter.solver.userData[solverIndex].x;
            if (!IsFinite(value))
            {
                emitter.solver.userData[solverIndex] = new Vector4(fallback, 0f, 0f, 0f);
                return fallback;
            }

            return Mathf.Clamp01(value);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
