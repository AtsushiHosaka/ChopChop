using UnityEngine;

namespace ChopChop.Input
{
    public sealed class HashiCalibration
    {
        private Quaternion neutralRotation = Quaternion.identity;
        private bool hasNeutral;

        public bool HasNeutral => hasNeutral;

        public void Clear()
        {
            neutralRotation = Quaternion.identity;
            hasNeutral = false;
        }

        public void Capture(HashiInputFrame frame)
        {
            neutralRotation = Quaternion.Inverse(frame.LocalRotation);
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
                frame.HasSample,
                frame.TimeReceived);
        }
    }
}
