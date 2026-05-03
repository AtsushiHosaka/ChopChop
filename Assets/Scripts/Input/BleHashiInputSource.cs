using ChopChop.BLE;
using UnityEngine;

namespace ChopChop.Input
{
    public sealed class BleHashiInputSource : IHashiInputSource
    {
        private static readonly object RuntimeLock = new object();
        private static BleHashiInputSource activeSource;
        private static string activeOwnerLabel = string.Empty;

        private readonly string deviceNameFilter;
        private readonly string serviceUuid;
        private readonly string characteristicUuid;
        private readonly string ownerLabel;
        private readonly MacHashiBleReceiver receiver = new MacHashiBleReceiver();
        private Quaternion rotation = Quaternion.identity;
        private bool blockedByActiveSource;
        private bool blockedLogEmitted;

        public bool IsRunning => receiver.IsRunning;
        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;

        public BleHashiInputSource(string deviceNameFilter, string serviceUuid, string characteristicUuid, string ownerLabel = "")
        {
            this.deviceNameFilter = deviceNameFilter;
            this.serviceUuid = serviceUuid;
            this.characteristicUuid = characteristicUuid;
            this.ownerLabel = string.IsNullOrWhiteSpace(ownerLabel) ? "Hashi" : ownerLabel.Trim();
        }

        public void StartInput()
        {
            blockedByActiveSource = false;
            blockedLogEmitted = false;

            lock (RuntimeLock)
            {
                if (!MacHashiBleReceiver.SupportsConcurrentReceivers && activeSource != null && !ReferenceEquals(activeSource, this))
                {
                    blockedByActiveSource = true;
                    CurrentFrame = HashiInputFrame.Empty;
                    return;
                }

                activeSource = this;
                activeOwnerLabel = ownerLabel;
            }

            receiver.Start(deviceNameFilter, serviceUuid, characteristicUuid);

            if (!receiver.IsRunning)
            {
                ReleaseRuntimeLock();
            }
        }

        public void StopInput()
        {
            receiver.Stop();
            ReleaseRuntimeLock();
        }

        public void Tick(float deltaTime)
        {
            if (blockedByActiveSource)
            {
                if (!blockedLogEmitted)
                {
                    Debug.LogError($"[Hashi Input/{ownerLabel}] BLE input was blocked because '{activeOwnerLabel}' is already using the current single-receiver macOS BLE plugin. Two-player BLE needs the native multi-receiver upgrade before production play.");
                    blockedLogEmitted = true;
                }

                return;
            }

            while (receiver.TryDequeueLog(out var message))
            {
                Debug.Log($"[Hashi Input/{ownerLabel}] {message}");
            }

            while (receiver.TryDequeueRawPayload(out var rawPayload))
            {
                if (!HashiBleSampleParser.TryParse(rawPayload, out var sample, out var error))
                {
                    Debug.LogWarning($"[Hashi Input/{ownerLabel}] Ignored BLE payload: {error}");
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

        private void ReleaseRuntimeLock()
        {
            lock (RuntimeLock)
            {
                if (!ReferenceEquals(activeSource, this))
                {
                    return;
                }

                activeSource = null;
                activeOwnerLabel = string.Empty;
            }
        }
    }
}
