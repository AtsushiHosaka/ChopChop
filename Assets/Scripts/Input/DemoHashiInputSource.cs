using UnityEngine;

namespace ChopChop.Input
{
    public sealed class DemoHashiInputSource : IHashiInputSource
    {
        private HashiDemoPhase phase;
        private float phaseElapsed;

        public bool IsRunning { get; private set; }
        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;

        public void SetPhase(HashiDemoPhase nextPhase)
        {
            if (phase == nextPhase)
            {
                return;
            }

            phase = nextPhase;
            phaseElapsed = 0f;
        }

        public void StartInput()
        {
            IsRunning = true;
            phaseElapsed = 0f;
            CurrentFrame = FrameForPhase(0f);
        }

        public void StopInput()
        {
            IsRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return;
            }

            phaseElapsed += Mathf.Max(0f, deltaTime);
            CurrentFrame = FrameForPhase(phaseElapsed);
        }

        private HashiInputFrame FrameForPhase(float time)
        {
            switch (phase)
            {
                case HashiDemoPhase.MixEgg:
                    return MixEggFrame(time);
                case HashiDemoPhase.PickMeat:
                    return ClosedGripFrame(time, 6.5f, 0.92f, 1.1f);
                case HashiDemoPhase.CarryToBroth:
                    return ClosedGripFrame(time, 7f, 0.95f, 1.6f);
                case HashiDemoPhase.FlipMeat:
                    return FlipFrame(time);
                case HashiDemoPhase.DipEgg:
                    return ClosedGripFrame(time, 7.5f, 0.9f, 1.4f);
                case HashiDemoPhase.DeliverToMouth:
                    return ClosedGripFrame(time, 7f, 0.88f, 1.2f);
                default:
                    return CalibrationFrame(time);
            }
        }

        private static HashiInputFrame CalibrationFrame(float time)
        {
            var rotation = Quaternion.Euler(
                Mathf.Sin(time * 1.2f) * 1.5f,
                Mathf.Cos(time * 1.1f) * 2f,
                0f);

            return CreateFrame(22f, 0.25f, Vector3.up, new Vector3(0.4f, 0.5f, 0.2f), rotation);
        }

        private static HashiInputFrame MixEggFrame(float time)
        {
            var loop = time * Mathf.PI * 2f;
            var theta = 25.5f + Mathf.Sin(loop * 0.8f) * 2.2f;
            var rotation = Quaternion.Euler(
                Mathf.Sin(loop * 0.9f) * 9f,
                Mathf.Cos(loop * 0.75f) * 16f,
                Mathf.Sin(loop * 1.15f) * 10f);

            var gyro = new Vector3(
                Mathf.Sin(loop * 0.9f) * 1.4f,
                7.4f + Mathf.Cos(loop * 0.5f) * 1.1f,
                Mathf.Sin(loop * 1.25f) * 1.8f);

            var acceleration = Vector3.up + new Vector3(
                Mathf.Sin(loop) * 0.18f,
                0f,
                Mathf.Cos(loop) * 0.18f);

            return CreateFrame(theta, 0.35f, acceleration, gyro, rotation);
        }

        private static HashiInputFrame ClosedGripFrame(float time, float theta, float pinch, float motionScale)
        {
            var loop = time * Mathf.PI * 2f;
            var rotation = Quaternion.Euler(
                6f + Mathf.Sin(loop * 0.45f) * 3f * motionScale,
                Mathf.Sin(loop * 0.3f) * 7f * motionScale,
                Mathf.Cos(loop * 0.4f) * 4f * motionScale);

            var gyro = new Vector3(
                Mathf.Sin(loop * 0.35f) * 0.8f * motionScale,
                1.8f * motionScale,
                Mathf.Cos(loop * 0.4f) * 0.6f * motionScale);

            return CreateFrame(theta, pinch, Vector3.up, gyro, rotation);
        }

        private static HashiInputFrame FlipFrame(float time)
        {
            var loop = time * Mathf.PI * 2f;
            var flip01 = Mathf.PingPong(time * 0.85f, 1f);
            var easedFlip = flip01 * flip01 * (3f - 2f * flip01);
            var rotation = Quaternion.Euler(
                Mathf.Lerp(-24f, 34f, easedFlip),
                Mathf.Sin(loop * 0.45f) * 10f,
                18f + Mathf.Sin(loop * 0.8f) * 12f);

            var gyro = new Vector3(
                2.2f + Mathf.Sin(loop * 0.6f) * 1.2f,
                9.6f + Mathf.Cos(loop * 0.5f) * 1.2f,
                2.8f + Mathf.Sin(loop * 0.9f) * 1.1f);

            return CreateFrame(7f, 0.94f, Vector3.up, gyro, rotation);
        }

        private static HashiInputFrame CreateFrame(
            float thetaDegrees,
            float pinchNormalized,
            Vector3 acceleration,
            Vector3 gyroDegreesPerSecond,
            Quaternion localRotation)
        {
            return new HashiInputFrame(
                thetaDegrees,
                pinchNormalized,
                acceleration,
                gyroDegreesPerSecond,
                localRotation,
                true,
                Time.time);
        }
    }
}
