using System.Collections.Generic;
using ChopChop.Interaction;
using UnityEngine;

namespace ChopChop.Levels.Sukiyaki
{
    public enum SukiyakiTargetZoneRequirement
    {
        AnyTip,
        BothTips
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("ChopChop/Levels/Sukiyaki Step Target Zone")]
    public sealed class SukiyakiStepTargetZone : MonoBehaviour
    {
        [Tooltip("Sukiyaki step that this target zone validates.")]
        [SerializeField] private SukiyakiStep step = SukiyakiStep.MixEgg;
        [Tooltip("How many chopstick tips must be inside this zone.")]
        [SerializeField] private SukiyakiTargetZoneRequirement requirement = SukiyakiTargetZoneRequirement.AnyTip;
        [Tooltip("Tip transforms checked against this zone. Trigger contacts are used as a fallback when this is empty.")]
        [SerializeField] private ChopstickTip[] trackedTips = new ChopstickTip[0];
        [Tooltip("World-space tolerance around the collider boundary.")]
        [Min(0f)] [SerializeField] private float containmentTolerance = 0.01f;

        private readonly HashSet<ChopstickTip> triggerContacts = new HashSet<ChopstickTip>();
        private Collider zoneCollider;

        public SukiyakiStep Step => step;
        public SukiyakiTargetZoneRequirement Requirement => requirement;
        public int RequiredTipCount => requirement == SukiyakiTargetZoneRequirement.BothTips ? 2 : 1;
        public int ActiveTipCount => CountContainedTrackedTips() + CountTriggerFallbackTips();
        public bool IsSatisfied => ActiveTipCount >= RequiredTipCount;

        public bool ContainsPoint(Vector3 worldPoint)
        {
            EnsureCollider();
            if (zoneCollider == null || !zoneCollider.enabled)
            {
                return false;
            }

            var closest = zoneCollider.ClosestPoint(worldPoint);
            var tolerance = Mathf.Max(0.0001f, containmentTolerance);
            return (closest - worldPoint).sqrMagnitude <= tolerance * tolerance;
        }

        private void Awake()
        {
            EnsureCollider();
        }

        private void OnValidate()
        {
            EnsureCollider();
        }

        private void OnTriggerEnter(Collider other)
        {
            TrackTriggerContact(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TrackTriggerContact(other);
        }

        private void OnTriggerExit(Collider other)
        {
            var tip = other.GetComponentInParent<ChopstickTip>();
            if (tip != null)
            {
                triggerContacts.Remove(tip);
            }
        }

        private void TrackTriggerContact(Collider other)
        {
            var tip = other.GetComponentInParent<ChopstickTip>();
            if (tip != null && tip.isActiveAndEnabled)
            {
                triggerContacts.Add(tip);
            }
        }

        private int CountContainedTrackedTips()
        {
            if (trackedTips == null || trackedTips.Length == 0)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < trackedTips.Length; i++)
            {
                var tip = trackedTips[i];
                if (tip != null && tip.isActiveAndEnabled && ContainsPoint(tip.transform.position))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountTriggerFallbackTips()
        {
            if (trackedTips != null && trackedTips.Length > 0)
            {
                return 0;
            }

            triggerContacts.RemoveWhere(tip => tip == null || !tip.isActiveAndEnabled);
            return triggerContacts.Count;
        }

        private void EnsureCollider()
        {
            if (zoneCollider == null)
            {
                zoneCollider = GetComponent<Collider>();
            }

            if (zoneCollider != null)
            {
                zoneCollider.isTrigger = true;
            }
        }
    }
}
