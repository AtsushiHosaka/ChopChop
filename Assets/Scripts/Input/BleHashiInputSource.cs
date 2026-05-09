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
        private const float ParentGyroIgnoreAboveDegreesPerSecond = 720f;
        private const float ParentAccelerationCorrectionFloor = 0.18f;
        private const float ParentAccelerationFullCorrectionBelowDps = 60f;
        private const float ParentAccelerationZeroCorrectionAboveDps = 260f;

        private readonly string deviceNameFilter;
        private readonly string serviceUuid;
        private readonly string characteristicUuid;
        private readonly string ownerLabel;
        private readonly float accelerometerCorrection;
        private readonly MacHashiBleReceiver receiver = new MacHashiBleReceiver();
        private Quaternion rotation = Quaternion.identity;
        private float lastRotationSampleRealtime;
        private bool hasParentNeutralAcceleration;
        private Vector3 parentNeutralAcceleration;
        private HashiImuMotionSource parentNeutralMotionSource;
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
            float accelerometerCorrection = 0.18f)
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
            rotation = Quaternion.identity;
            lastRotationSampleRealtime = 0f;
            hasParentNeutralAcceleration = false;
            parentNeutralAcceleration = Vector3.zero;
            parentNeutralMotionSource = HashiImuMappingRuntime.MotionSource;
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

                var hasParentMotion = HashiImuMappingRuntime.TrySelectMotion(sample, out var acceleration, out var gyro);
                if (!hasParentMotion)
                {
                    acceleration = sample.HasAccelerationVector ? sample.AccelerationVector : Vector3.zero;
                    gyro = sample.HasGyroVector ? sample.GyroVector : Vector3.zero;
                    hasParentMotion = sample.HasAccelerationVector || sample.HasGyroVector;
                }

                if (hasParentMotion)
                {
                    HashiImuMappingRuntime.ApplyParentRotationInputBasis(ref acceleration, ref gyro);
                    gyro = HashiImuMappingRuntime.FilterParentRotationGyro(gyro);
                }

                var rotationDeltaTime = ParentRotationDeltaTime(sample, deltaTime);
                var parentGyroSaturated = HashiImuMappingRuntime.IsGyroSaturated(
                    sample,
                    HashiImuMappingRuntime.MotionSource);
                if (hasParentMotion && rotationDeltaTime > 0f)
                {
                    rotation = IntegrateParentRotation(rotation, gyro, rotationDeltaTime, parentGyroSaturated);
                }

                if (hasParentMotion && acceleration.sqrMagnitude > 0.000001f)
                {
                    rotation = ApplyAccelerometerCorrection(rotation, acceleration, gyro);
                }

                var theta = ResolveTheta(sample, CurrentFrame.ThetaDegrees);
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
                    acceleration,
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

        private static float ResolveTheta(HashiBleSample sample, float fallbackTheta)
        {
            if (HashiImuMappingRuntime.TryReadIndependentOpeningDegrees(sample, out var independentOpening))
            {
                return independentOpening;
            }

            if (HashiImuMappingRuntime.UseTunedTheta)
            {
                return HashiImuMappingRuntime.MapTheta(sample, fallbackTheta);
            }

            return sample.HasTheta ? sample.ThetaValue : fallbackTheta;
        }

        private float ParentRotationDeltaTime(HashiBleSample sample, float fallbackDeltaTime)
        {
            var deltaTime = lastRotationSampleRealtime > 0f
                ? sample.RealtimeReceived - lastRotationSampleRealtime
                : Mathf.Max(0f, fallbackDeltaTime);
            lastRotationSampleRealtime = sample.RealtimeReceived;

            if (deltaTime < 0.001f)
            {
                deltaTime = Mathf.Max(0f, fallbackDeltaTime);
            }

            var maxGap = Mathf.Max(0f, HashiImuMappingRuntime.MaxBleSampleAgeSeconds);
            if (maxGap > 0f && deltaTime > maxGap)
            {
                rotation = Quaternion.identity;
                hasParentNeutralAcceleration = false;
                return 0f;
            }

            return Mathf.Clamp(deltaTime, 0f, 0.2f);
        }

        private static Quaternion IntegrateParentRotation(
            Quaternion currentRotation,
            Vector3 gyroDegreesPerSecond,
            float deltaTime,
            bool gyroSaturated)
        {
            if (gyroSaturated)
            {
                return currentRotation;
            }

            gyroDegreesPerSecond = ApplyParentGyroDeadzone(gyroDegreesPerSecond);
            var angularSpeed = gyroDegreesPerSecond.magnitude;
            if (angularSpeed < 0.0001f || angularSpeed > ParentGyroIgnoreAboveDegreesPerSecond)
            {
                return currentRotation;
            }

            var deltaRotation = Quaternion.AngleAxis(
                angularSpeed * Mathf.Max(0f, deltaTime),
                gyroDegreesPerSecond / angularSpeed);
            return Normalize(currentRotation * deltaRotation);
        }

        private Quaternion ApplyAccelerometerCorrection(
            Quaternion currentRotation,
            Vector3 acceleration,
            Vector3 gyroDegreesPerSecond)
        {
            if (!TryReadStableGravity(acceleration, out var measuredGravity))
            {
                return currentRotation;
            }

            var motionSource = HashiImuMappingRuntime.MotionSource;
            if (!hasParentNeutralAcceleration || parentNeutralMotionSource != motionSource)
            {
                hasParentNeutralAcceleration = true;
                parentNeutralMotionSource = motionSource;
                parentNeutralAcceleration = measuredGravity;
                return currentRotation;
            }

            var correctionWeight = ParentAccelerationCorrectionWeight();
            if (correctionWeight <= 0f)
            {
                return currentRotation;
            }

            var motionWeight = 1f - Mathf.InverseLerp(
                ParentAccelerationFullCorrectionBelowDps,
                ParentAccelerationZeroCorrectionAboveDps,
                gyroDegreesPerSecond.magnitude);
            correctionWeight *= Mathf.Clamp01(motionWeight);
            if (correctionWeight <= 0f)
            {
                return currentRotation;
            }

            var neutralGravity = parentNeutralAcceleration.normalized;
            if (neutralGravity.sqrMagnitude < 0.0001f)
            {
                return currentRotation;
            }

            var predictedGravity = Quaternion.Inverse(currentRotation) * neutralGravity;
            if (predictedGravity.sqrMagnitude < 0.0001f)
            {
                return currentRotation;
            }

            var correctionAngle = Vector3.Angle(predictedGravity.normalized, measuredGravity);
            if (float.IsNaN(correctionAngle)
                || correctionAngle < Mathf.Max(0f, HashiImuMappingRuntime.AccelerationCorrectionDeadzoneDegrees))
            {
                return currentRotation;
            }

            var localCorrection = Quaternion.FromToRotation(measuredGravity, predictedGravity.normalized);
            var correctedRotation = currentRotation * Quaternion.Slerp(
                Quaternion.identity,
                localCorrection,
                correctionWeight);
            // A 6-axis IMU cannot observe yaw from acceleration. Keep yaw gyro-only and use
            // acceleration solely to correct gravity tilt, otherwise horizontal pointing
            // motions get pulled back toward the neutral direction.
            return Normalize(correctedRotation);
        }

        private float ParentAccelerationCorrectionWeight()
        {
            var configuredWeight = Mathf.Max(accelerometerCorrection, HashiImuMappingRuntime.AccelerationCorrectionWeight);
            return Mathf.Clamp01(Mathf.Max(configuredWeight, ParentAccelerationCorrectionFloor));
        }

        private static Vector3 ApplyParentGyroDeadzone(Vector3 gyroDegreesPerSecond)
        {
            var threshold = Mathf.Max(0f, HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond);
            if (threshold <= 0f)
            {
                return gyroDegreesPerSecond;
            }

            return new Vector3(
                Mathf.Abs(gyroDegreesPerSecond.x) < threshold ? 0f : gyroDegreesPerSecond.x,
                Mathf.Abs(gyroDegreesPerSecond.y) < threshold ? 0f : gyroDegreesPerSecond.y,
                Mathf.Abs(gyroDegreesPerSecond.z) < threshold ? 0f : gyroDegreesPerSecond.z);
        }

        private static Quaternion Normalize(Quaternion rotation)
        {
            var magnitude = Mathf.Sqrt(
                rotation.x * rotation.x
                + rotation.y * rotation.y
                + rotation.z * rotation.z
                + rotation.w * rotation.w);
            if (magnitude < 0.000001f)
            {
                return Quaternion.identity;
            }

            return new Quaternion(
                rotation.x / magnitude,
                rotation.y / magnitude,
                rotation.z / magnitude,
                rotation.w / magnitude);
        }

        private static bool TryReadStableGravity(Vector3 acceleration, out Vector3 gravity)
        {
            gravity = Vector3.zero;
            var magnitude = acceleration.magnitude;
            if (magnitude < 0.0001f)
            {
                return false;
            }

            var toleranceG = Mathf.Max(0f, HashiImuMappingRuntime.AccelerationCorrectionMagnitudeToleranceG);
            if (toleranceG > 0f && Mathf.Abs(magnitude - 1f) > toleranceG)
            {
                return false;
            }

            gravity = acceleration / magnitude;
            return true;
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
