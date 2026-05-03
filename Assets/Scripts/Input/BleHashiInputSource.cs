using ChopChop.BLE;
using UnityEngine;

namespace ChopChop.Input
{
    public sealed class BleHashiInputSource : IHashiInputSource
    {
        private readonly string deviceNameFilter;
        private readonly string serviceUuid;
        private readonly string characteristicUuid;
        private readonly MacHashiBleReceiver receiver = new MacHashiBleReceiver();
        private Quaternion rotation = Quaternion.identity;

        public bool IsRunning => receiver.IsRunning;
        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;

        public BleHashiInputSource(string deviceNameFilter, string serviceUuid, string characteristicUuid)
        {
            this.deviceNameFilter = deviceNameFilter;
            this.serviceUuid = serviceUuid;
            this.characteristicUuid = characteristicUuid;
        }

        public void StartInput()
        {
            receiver.Start(deviceNameFilter, serviceUuid, characteristicUuid);
        }

        public void StopInput()
        {
            receiver.Stop();
        }

        public void Tick(float deltaTime)
        {
            while (receiver.TryDequeueLog(out var message))
            {
                Debug.Log($"[Hashi Input] {message}");
            }

            while (receiver.TryDequeueRawPayload(out var rawPayload))
            {
                if (!HashiBleSampleParser.TryParse(rawPayload, out var sample, out var error))
                {
                    Debug.LogWarning($"[Hashi Input] Ignored BLE payload: {error}");
                    continue;
                }

                var gyro = sample.HasGyroVector ? sample.GyroVector : Vector3.zero;
                if (deltaTime > 0f)
                {
                    rotation *= Quaternion.Euler(gyro * deltaTime);
                }

                var theta = sample.HasTheta ? sample.ThetaValue : CurrentFrame.ThetaDegrees;
                var pinch = sample.HasPinch
                    ? Mathf.InverseLerp(0f, 200f, sample.PinchValue)
                    : Mathf.InverseLerp(40f, 5f, theta);

                CurrentFrame = new HashiInputFrame(
                    theta,
                    pinch,
                    sample.HasAccelerationVector ? sample.AccelerationVector : Vector3.zero,
                    gyro,
                    rotation,
                    true,
                    sample.RealtimeReceived);
            }
        }
    }
}
