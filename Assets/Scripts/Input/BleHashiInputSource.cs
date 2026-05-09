using ChopChop.BLE;
using UnityEngine;

namespace ChopChop.Input
{
    public sealed class BleHashiInputSource : IHashiInputSource
    {
        private static readonly object RuntimeLock = new object();
        private static BleHashiInputSource activeSource;
        private static string activeOwnerLabel = string.Empty;
        private const float ReconnectDelaySeconds = 3f;

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
        private float nextReconnectAttemptAt;
        private int reconnectAttemptCount;

        public bool IsRunning => receiver.IsRunning;
        public string Status => blockedByActiveSource
            ? $"Blocked by active source '{activeOwnerLabel}'"
            : receiver.Status;
        public bool HasLatestSample { get; private set; }
        public HashiBleSample LatestSample { get; private set; }
        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;

        public BleHashiInputSource(
            string deviceNameFilter,
            string serviceUuid,
            string characteristicUuid,
            string ownerLabel = "",
            float accelerometerCorrection = 0.015f)
        {
            this.deviceNameFilter = DefaultIfEmpty(deviceNameFilter, HashiBleDefaults.DeviceNameFilter);
            this.serviceUuid = DefaultIfEmpty(serviceUuid, HashiBleDefaults.ServiceUuid);
            this.characteristicUuid = DefaultIfEmpty(characteristicUuid, HashiBleDefaults.CharacteristicUuid);
            this.ownerLabel = string.IsNullOrWhiteSpace(ownerLabel) ? "Hashi" : ownerLabel.Trim();
            this.accelerometerCorrection = Mathf.Clamp01(accelerometerCorrection);
        }

        public void StartInput()
        {
            blockedByActiveSource = false;
            blockedLogEmitted = false;
            staleSampleLogEmitted = false;
            suppressedMalformedPayloadWarnings = 0;
            HasLatestSample = false;
            nextReconnectAttemptAt = 0f;
            reconnectAttemptCount = 0;

            if (!TryClaimRuntimeLock())
            {
                blockedByActiveSource = true;
                CurrentFrame = HashiInputFrame.Empty;
                return;
            }

            Debug.Log($"[Hashi Input/{ownerLabel}] Starting BLE input. deviceName='{deviceNameFilter}' serviceUuid='{serviceUuid}' characteristicUuid='{characteristicUuid}'");
            receiver.Start(deviceNameFilter, serviceUuid, characteristicUuid);

            if (!receiver.IsRunning)
            {
                Debug.LogWarning($"[Hashi Input/{ownerLabel}] BLE receiver did not start. Status: {receiver.Status}");
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

            if (!receiver.IsRunning)
            {
                ReleaseRuntimeLock();
                TryReconnect();
                return;
            }

            while (receiver.TryDequeueRawPayload(out var rawPayload))
            {
                if (!HashiBleSampleParser.TryParse(rawPayload, out var sample, out var error))
                {
                    LogMalformedPayload(error);
                    continue;
                }

                LatestSample = sample;
                HasLatestSample = true;

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
                var localPosition = CurrentFrame.LocalPosition;
                if (HashiImuMappingRuntime.UpdateHardwarePosition(sample, deltaTime, out var hardwarePosition, out _))
                {
                    localPosition = hardwarePosition;
                }

                CurrentFrame = new HashiInputFrame(
                    theta,
                    pinch,
                    sample.HasAccelerationVector ? sample.AccelerationVector : Vector3.zero,
                    gyro,
                    HashiImuMappingRuntime.ApplyParentRotationWeight(rotation),
                    localPosition,
                    true,
                    sample.RealtimeReceived);
                staleSampleLogEmitted = false;
                reconnectAttemptCount = 0;
            }

            if (CurrentFrame.HasSample && !staleSampleLogEmitted && Time.realtimeSinceStartup - CurrentFrame.TimeReceived >= 2f)
            {
                Debug.LogWarning($"[Hashi Input/{ownerLabel}] No BLE sample received for {Time.realtimeSinceStartup - CurrentFrame.TimeReceived:0.0}s. Holding the last valid chopstick pose.");
                staleSampleLogEmitted = true;
            }
        }

        public static bool ShouldAutoReconnect(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return true;
            }

            return !status.StartsWith("Native plugin", System.StringComparison.OrdinalIgnoreCase)
                && !status.StartsWith("Unsupported", System.StringComparison.OrdinalIgnoreCase)
                && !status.StartsWith("Unauthorized", System.StringComparison.OrdinalIgnoreCase);
        }

        private bool TryClaimRuntimeLock()
        {
            lock (RuntimeLock)
            {
                if (!MacHashiBleReceiver.SupportsConcurrentReceivers && activeSource != null && !ReferenceEquals(activeSource, this))
                {
                    return false;
                }

                activeSource = this;
                activeOwnerLabel = ownerLabel;
                return true;
            }
        }

        private void TryReconnect()
        {
            if (!ShouldAutoReconnect(receiver.Status) || Time.realtimeSinceStartup < nextReconnectAttemptAt)
            {
                return;
            }

            if (!TryClaimRuntimeLock())
            {
                blockedByActiveSource = true;
                return;
            }

            reconnectAttemptCount++;
            Debug.LogWarning($"[Hashi Input/{ownerLabel}] BLE receiver is not running ({receiver.Status}). Reconnect attempt {reconnectAttemptCount}.");
            receiver.Start(deviceNameFilter, serviceUuid, characteristicUuid);

            if (receiver.IsRunning)
            {
                blockedByActiveSource = false;
                staleSampleLogEmitted = false;
                nextReconnectAttemptAt = 0f;
                return;
            }

            ReleaseRuntimeLock();
            nextReconnectAttemptAt = Time.realtimeSinceStartup + ReconnectDelaySeconds;
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

        private static string DefaultIfEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
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
