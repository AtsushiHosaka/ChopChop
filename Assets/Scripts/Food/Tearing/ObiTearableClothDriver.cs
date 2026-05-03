using ChopChop.Food.Meat;
using Obi;
using UnityEngine;

namespace ChopChop.Food.Tearing
{
    [AddComponentMenu("ChopChop/Food/Obi Tearable Cloth Driver")]
    public sealed class ObiTearableClothDriver : MeatTearDriverBehaviour
    {
        [SerializeField] private ObiTearableCloth cloth;

        public override void Configure(MeatTearProfile profile)
        {
            if (cloth == null || profile == null)
            {
                return;
            }

            cloth.tearResistanceMultiplier = profile.tearResistanceMultiplier;
            cloth.tearRate = profile.tearRate;
            cloth.tearDebilitation = profile.tearDebilitation;
        }

        public override bool RequestTearAt(Vector3 worldPoint, MeatTearReason reason)
        {
            if (cloth == null || !cloth.isLoaded || cloth.solver == null)
            {
                return false;
            }

            return TryFindClosestConstraint(worldPoint, out var constraint) && cloth.Tear(constraint);
        }

        private void Reset()
        {
            cloth = GetComponent<ObiTearableCloth>();
        }

        private void OnEnable()
        {
            if (cloth != null)
            {
                cloth.OnClothTorn += HandleClothTorn;
            }
        }

        private void OnDisable()
        {
            if (cloth != null)
            {
                cloth.OnClothTorn -= HandleClothTorn;
            }
        }

        private void HandleClothTorn(ObiTearableCloth sender, ObiTearableCloth.ObiClothTornEventArgs args)
        {
            var worldPoint = TryGetParticleWorldPoint(args.particleIndex, out var point)
                ? point
                : transform.position;
            RaisePhysicalTorn(worldPoint);
        }

        private bool TryFindClosestConstraint(Vector3 worldPoint, out StructuralConstraint constraint)
        {
            constraint = default;
            var constraints = cloth.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiConstraints<ObiDistanceConstraintsBatch>;
            if (constraints == null)
            {
                return false;
            }

            var bestDistance = float.PositiveInfinity;
            ObiDistanceConstraintsBatch bestBatch = null;
            var bestIndex = -1;

            for (var batchIndex = 0; batchIndex < constraints.batchCount; batchIndex++)
            {
                var batch = constraints.GetBatch(batchIndex) as ObiDistanceConstraintsBatch;
                if (batch == null)
                {
                    continue;
                }

                for (var i = 0; i < batch.activeConstraintCount; i++)
                {
                    var firstActorIndex = batch.particleIndices[i * 2];
                    var secondActorIndex = batch.particleIndices[i * 2 + 1];
                    if (!TryGetParticleWorldPoint(firstActorIndex, out var first) ||
                        !TryGetParticleWorldPoint(secondActorIndex, out var second))
                    {
                        continue;
                    }

                    var midpoint = (first + second) * 0.5f;
                    var distance = (midpoint - worldPoint).sqrMagnitude;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestBatch = batch;
                        bestIndex = i;
                    }
                }
            }

            if (bestBatch == null || bestIndex < 0)
            {
                return false;
            }

            constraint = new StructuralConstraint(bestBatch, bestIndex, -1f);
            return true;
        }

        private bool TryGetParticleWorldPoint(int actorIndex, out Vector3 point)
        {
            point = Vector3.zero;
            if (cloth == null ||
                !cloth.isLoaded ||
                actorIndex < 0 ||
                actorIndex >= cloth.solverIndices.count)
            {
                return false;
            }

            point = cloth.GetParticlePosition(cloth.solverIndices[actorIndex]);
            return true;
        }
    }
}
