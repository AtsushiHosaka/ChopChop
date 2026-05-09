using UnityEngine;

namespace ChopChop.Input
{
    public sealed class HashiCalibration
    {
        private Quaternion neutralRotation = Quaternion.identity;
        private Vector3 neutralPosition;
        private bool hasNeutral;

        public bool HasNeutral => hasNeutral;

        public void Clear()
        {
            neutralRotation = Quaternion.identity;
            neutralPosition = Vector3.zero;
            hasNeutral = false;
        }

        public void Capture(HashiInputFrame frame)
        {
            neutralRotation = Quaternion.Inverse(frame.LocalRotation);
            neutralPosition = frame.LocalPosition;
            hasNeutral = true;
        }

        public HashiInputFrame Apply(HashiInputFrame frame)
        {
            if (!hasNeutral)
            {
                return frame;
            }

            return new HashiInputFrame(
                frame.ThetaDegrees,
                frame.PinchNormalized,
                frame.Acceleration,
                frame.GyroDegreesPerSecond,
                neutralRotation * frame.LocalRotation,
                frame.LocalPosition - neutralPosition,
                frame.HasSample,
                frame.TimeReceived);
        }
    }
}
