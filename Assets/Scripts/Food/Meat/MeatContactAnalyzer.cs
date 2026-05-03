using System.Collections.Generic;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    public sealed class MeatContactAnalyzer
    {
        public MeatStressFrame Analyze(
            IReadOnlyList<MeatTipContact> contacts,
            MeatContactZone zone,
            MeatTearProfile profile,
            float deltaTime)
        {
            if (contacts == null || contacts.Count == 0 || zone == null || profile == null || deltaTime <= 0f)
            {
                return MeatStressFrame.None;
            }

            var normal = zone.WorldSurfaceNormal;
            var poke = 0f;
            var shear = 0f;
            var tension = 0f;
            var compression = 0f;
            var pinchSum = 0f;
            var contactPoint = Vector3.zero;

            for (var i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                var relativeVelocity = contact.TipVelocity - zone.Velocity;
                var intoMeatVelocity = Mathf.Max(0f, Vector3.Dot(relativeVelocity, -normal));
                var surfaceVelocity = Vector3.ProjectOnPlane(relativeVelocity, normal).magnitude;
                var localStretch = Vector3.Distance(contact.CurrentLocalPoint, contact.InitialLocalPoint);

                poke = Mathf.Max(poke, intoMeatVelocity);
                shear = Mathf.Max(shear, surfaceVelocity);
                tension = Mathf.Max(tension, localStretch);
                pinchSum += contact.PinchNormalized;
                contactPoint += contact.WorldPoint;
            }

            contactPoint /= contacts.Count;

            var twist = EstimateTwist(contacts, zone.WorldAllowedFlipAxis, deltaTime);
            if (contacts.Count >= 2)
            {
                var tipDistance = Vector3.Distance(contacts[0].Tip.transform.position, contacts[1].Tip.transform.position);
                compression = Mathf.Clamp01((profile.idealTipSeparation - tipDistance) / profile.compressionDistanceRange);
            }

            var averagePinch = pinchSum / contacts.Count;
            var pinchCompression = Mathf.InverseLerp(profile.pinchComfortLimit, 1f, averagePinch);
            compression = Mathf.Max(compression, pinchCompression);

            var damage = profile.EvaluateDamagePerSecond(poke, shear, twist, tension, compression);
            var reason = TearReasonClassifier.ClassifyFrame(profile, poke, shear, twist, tension, compression);

            return new MeatStressFrame(
                poke,
                shear,
                twist,
                tension,
                compression,
                damage,
                reason,
                contactPoint,
                true);
        }

        private static float EstimateTwist(IReadOnlyList<MeatTipContact> contacts, Vector3 allowedAxis, float deltaTime)
        {
            if (contacts.Count < 2)
            {
                return 0f;
            }

            var previous = contacts[1].PreviousWorldPoint - contacts[0].PreviousWorldPoint;
            var current = contacts[1].WorldPoint - contacts[0].WorldPoint;
            if (previous.sqrMagnitude < 0.0001f || current.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            var angle = Vector3.Angle(previous, current);
            var axis = Vector3.Cross(previous, current);
            if (axis.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            var normalizedAxis = axis.normalized;
            var offAxisRatio = 1f - Mathf.Abs(Vector3.Dot(normalizedAxis, allowedAxis.normalized));
            return angle / deltaTime * offAxisRatio;
        }
    }
}
