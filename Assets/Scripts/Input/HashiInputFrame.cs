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
        {
            ThetaDegrees = thetaDegrees;
            PinchNormalized = Mathf.Clamp01(pinchNormalized);
            Acceleration = acceleration;
            GyroDegreesPerSecond = gyroDegreesPerSecond;
            LocalRotation = localRotation;
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
