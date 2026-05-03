using System.Collections.Generic;
using ChopChop.Interaction;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class MeatContactZone : MonoBehaviour
    {
        [Tooltip("Local outward normal of the meat surface. Poking is movement against this normal.")]
        [SerializeField] private Vector3 localSurfaceNormal = Vector3.up;
        [Tooltip("Local axis around which flipping is allowed.")]
        [SerializeField] private Vector3 localAllowedFlipAxis = Vector3.right;

        private readonly Dictionary<ChopstickTip, ContactState> contacts = new Dictionary<ChopstickTip, ContactState>();
        private readonly List<ChopstickTip> staleTips = new List<ChopstickTip>();
        private Collider zoneCollider;
        private Vector3 previousPosition;
        private bool hasPreviousFrame;

        public Vector3 WorldSurfaceNormal => transform.TransformDirection(NormalizedOr(localSurfaceNormal, Vector3.up)).normalized;
        public Vector3 WorldAllowedFlipAxis => transform.TransformDirection(NormalizedOr(localAllowedFlipAxis, Vector3.right)).normalized;
        public Vector3 Velocity { get; private set; }

        public int ActiveContactCount => contacts.Count;

        public void GetActiveContacts(List<MeatTipContact> results)
        {
            results.Clear();
            RemoveInactiveTips();

            foreach (var pair in contacts)
            {
                var state = pair.Value;
                results.Add(new MeatTipContact(
                    pair.Key,
                    state.WorldPoint,
                    state.PreviousWorldPoint,
                    state.InitialLocalPoint,
                    state.CurrentLocalPoint,
                    pair.Key.Velocity,
                    pair.Key.PinchNormalized,
                    Time.time - state.StartTime));
            }
        }

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            previousPosition = transform.position;
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (hasPreviousFrame && deltaTime > 0f)
            {
                Velocity = (transform.position - previousPosition) / deltaTime;
            }
            else
            {
                Velocity = Vector3.zero;
                hasPreviousFrame = true;
            }

            previousPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            Track(other);
        }

        private void OnTriggerStay(Collider other)
        {
            Track(other);
        }

        private void OnTriggerExit(Collider other)
        {
            var tip = other.GetComponentInParent<ChopstickTip>();
            if (tip != null)
            {
                contacts.Remove(tip);
            }
        }

        private void Track(Collider other)
        {
            var tip = other.GetComponentInParent<ChopstickTip>();
            if (tip == null)
            {
                return;
            }

            var point = zoneCollider.ClosestPoint(tip.transform.position);
            if (!contacts.TryGetValue(tip, out var state))
            {
                var localPoint = transform.InverseTransformPoint(point);
                contacts[tip] = new ContactState
                {
                    StartTime = Time.time,
                    InitialLocalPoint = localPoint,
                    CurrentLocalPoint = localPoint,
                    PreviousWorldPoint = point,
                    WorldPoint = point
                };
                return;
            }

            state.PreviousWorldPoint = state.WorldPoint;
            state.WorldPoint = point;
            state.CurrentLocalPoint = transform.InverseTransformPoint(point);
            contacts[tip] = state;
        }

        private void RemoveInactiveTips()
        {
            staleTips.Clear();
            foreach (var pair in contacts)
            {
                if (pair.Key == null || !pair.Key.isActiveAndEnabled)
                {
                    staleTips.Add(pair.Key);
                }
            }

            foreach (var tip in staleTips)
            {
                contacts.Remove(tip);
            }
        }

        private static Vector3 NormalizedOr(Vector3 value, Vector3 fallback)
        {
            return value.sqrMagnitude > 0.0001f ? value.normalized : fallback;
        }

        private struct ContactState
        {
            public float StartTime;
            public Vector3 InitialLocalPoint;
            public Vector3 CurrentLocalPoint;
            public Vector3 PreviousWorldPoint;
            public Vector3 WorldPoint;
        }
    }
}
