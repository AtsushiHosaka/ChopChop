using UnityEngine;

namespace ChopChop.Input
{
    public readonly struct HashiInputFrame
    {
        public float ThetaDegrees { get; }
        public float PinchNormalized { get; }
        public Vector3 Acceleration { get; }
        public Vector3 GyroDegreesPerSecond { get; }
        public Quaternion LocalRotation { get; }
        public Vector3 LocalPosition { get; }
        public bool HasSample { get; }
        public float TimeReceived { get; }

        public HashiInputFrame(
            float thetaDegrees,
            float pinchNormalized,
            Vector3 acceleration,
            Vector3 gyroDegreesPerSecond,
            Quaternion localRotation,
            bool hasSample,
            float timeReceived)
            : this(
                thetaDegrees,
                pinchNormalized,
                acceleration,
                gyroDegreesPerSecond,
                localRotation,
                Vector3.zero,
                hasSample,
                timeReceived)
        {
        }

        public HashiInputFrame(
            float thetaDegrees,
            float pinchNormalized,
            Vector3 acceleration,
            Vector3 gyroDegreesPerSecond,
            Quaternion localRotation,
            Vector3 localPosition,
            bool hasSample,
            float timeReceived)
        {
            ThetaDegrees = thetaDegrees;
            PinchNormalized = Mathf.Clamp01(pinchNormalized);
            Acceleration = acceleration;
            GyroDegreesPerSecond = gyroDegreesPerSecond;
            LocalRotation = localRotation;
            LocalPosition = localPosition;
            HasSample = hasSample;
            TimeReceived = timeReceived;
        }

        public static HashiInputFrame Empty => new HashiInputFrame(
            0f,
            0f,
            Vector3.zero,
            Vector3.zero,
            Quaternion.identity,
            false,
            0f);
    }
}
