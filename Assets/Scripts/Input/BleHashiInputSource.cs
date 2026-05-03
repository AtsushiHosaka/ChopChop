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
        private readonly float accelerometerCorrection;
        private readonly MacHashiBleReceiver receiver = new MacHashiBleReceiver();
        private Quaternion rotation = Quaternion.identity;
        private bool blockedByActiveSource;
        private bool blockedLogEmitted;
        private float lastMalformedPayloadWarningAt = -999f;
        private int suppressedMalformedPayloadWarnings;
        private bool staleSampleLogEmitted;

        public bool IsRunning => receiver.IsRunning;
        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;

        public BleHashiInputSource(
            string deviceNameFilter,
            string serviceUuid,
            string characteristicUuid,
            string ownerLabel = "",
            float accelerometerCorrection = 0.015f)
        {
            this.deviceNameFilter = deviceNameFilter;
            this.serviceUuid = serviceUuid;
            this.characteristicUuid = characteristicUuid;
            this.ownerLabel = string.IsNullOrWhiteSpace(ownerLabel) ? "Hashi" : ownerLabel.Trim();
            this.accelerometerCorrection = Mathf.Clamp01(accelerometerCorrection);
        }

        public void StartInput()
        {
            blockedByActiveSource = false;
            blockedLogEmitted = false;
            staleSampleLogEmitted = false;
            suppressedMalformedPayloadWarnings = 0;

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
                    LogMalformedPayload(error);
                    continue;
                }

                var gyro = sample.HasGyroVector ? sample.GyroVector : Vector3.zero;
                if (deltaTime > 0f)
                {
                    rotation *= Quaternion.Euler(gyro * deltaTime);
                }

                if (sample.HasAccelerationVector)
                {
                    rotation = ApplyAccelerometerCorrection(rotation, sample.AccelerationVector);
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
                staleSampleLogEmitted = false;
            }

            if (CurrentFrame.HasSample && !staleSampleLogEmitted && Time.realtimeSinceStartup - CurrentFrame.TimeReceived >= 2f)
            {
                Debug.LogWarning($"[Hashi Input/{ownerLabel}] No BLE sample received for {Time.realtimeSinceStartup - CurrentFrame.TimeReceived:0.0}s. Holding the last valid chopstick pose.");
                staleSampleLogEmitted = true;
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

        private Quaternion ApplyAccelerometerCorrection(Quaternion currentRotation, Vector3 acceleration)
        {
            if (accelerometerCorrection <= 0f)
            {
                return currentRotation;
            }

            var magnitude = acceleration.magnitude;
            if (magnitude < 0.25f || magnitude > 3.5f)
            {
                return currentRotation;
            }

            var measuredUp = -acceleration.normalized;
            var currentUp = currentRotation * Vector3.up;
            if (currentUp.sqrMagnitude < 0.0001f || measuredUp.sqrMagnitude < 0.0001f)
            {
                return currentRotation;
            }

            var correctedRotation = Quaternion.FromToRotation(currentUp.normalized, measuredUp) * currentRotation;
            return Quaternion.Slerp(currentRotation, correctedRotation, accelerometerCorrection);
        }

        private void LogMalformedPayload(string error)
        {
            var now = Time.realtimeSinceStartup;
            if (now - lastMalformedPayloadWarningAt < 2f)
            {
                suppressedMalformedPayloadWarnings++;
                return;
            }

            var suffix = suppressedMalformedPayloadWarnings > 0
                ? $" Suppressed {suppressedMalformedPayloadWarnings} similar malformed payload warning(s)."
                : string.Empty;
            Debug.LogWarning($"[Hashi Input/{ownerLabel}] Ignored BLE payload: {error}.{suffix}");
            lastMalformedPayloadWarningAt = now;
            suppressedMalformedPayloadWarnings = 0;
        }
    }
}
