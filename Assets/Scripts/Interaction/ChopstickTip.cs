using UnityEngine;

namespace ChopChop.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ChopstickTip : MonoBehaviour
    {
        [Tooltip("0 for lower chopstick, 1 for upper chopstick.")]
        [SerializeField] private int tipIndex = 0;
        [Tooltip("Estimated pressure from BLE pinch or a local fallback value.")]
        [Range(0f, 1f)] [SerializeField] private float pinchNormalized;
        [Tooltip("Current theta in degrees, used as a fallback pressure hint.")]
        [SerializeField] private float thetaDegrees;

        private Vector3 previousPosition;
        private Quaternion previousRotation;
        private bool hasPreviousFrame;

        public int TipIndex => tipIndex;
        public float PinchNormalized => pinchNormalized;
        public float ThetaDegrees => thetaDegrees;
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocityDegrees { get; private set; }

        public void SetInputHints(float pinchNormalized, float thetaDegrees)
        {
            this.pinchNormalized = Mathf.Clamp01(pinchNormalized);
            this.thetaDegrees = thetaDegrees;
        }

        private void OnEnable()
        {
            previousPosition = transform.position;
            previousRotation = transform.rotation;
            hasPreviousFrame = false;
            Velocity = Vector3.zero;
            AngularVelocityDegrees = Vector3.zero;
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (hasPreviousFrame && deltaTime > 0f)
            {
                Velocity = (transform.position - previousPosition) / deltaTime;
                AngularVelocityDegrees = EstimateAngularVelocity(previousRotation, transform.rotation, deltaTime);
            }
            else
            {
                Velocity = Vector3.zero;
                AngularVelocityDegrees = Vector3.zero;
                hasPreviousFrame = true;
            }

            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }

        private static Vector3 EstimateAngularVelocity(Quaternion previous, Quaternion current, float deltaTime)
        {
            var delta = current * Quaternion.Inverse(previous);
            delta.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (float.IsNaN(axis.x) || axis.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            return axis.normalized * (angle / deltaTime);
        }
    }
}
