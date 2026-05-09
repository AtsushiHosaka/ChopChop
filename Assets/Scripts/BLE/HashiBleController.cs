using UnityEngine;
using UnityEngine.Serialization;

namespace ChopChop.BLE
{
    [AddComponentMenu("ChopChop/Hashi Dual IMU BLE Controller")]
    public sealed class HashiBleController : MonoBehaviour
    {
        [Header("Chopsticks")]
        public Transform lowerChopstick;
        public Transform upperChopstick;
        public Transform lowerPivot;
        public Transform upperPivot;

        [Header("BLE")]
        public string deviceNameFilter = HashiBleDefaults.DeviceNameFilter;
        public string serviceUuid = HashiBleDefaults.ServiceUuid;
        public string characteristicUuid = HashiBleDefaults.CharacteristicUuid;
        public float staleWarningSeconds = 2f;

        [Header("Sensor Scale")]
        public float rawAccelerationToG = HashiBleDefaults.RawAccelerationToG;
        public float rawGyroToDegreesPerSecond = HashiBleDefaults.RawGyroToDegreesPerSecond;

        [Header("IMU Roles and Mounting")]
        public bool imu1IsUpper = true;
        public HashiImuAxisMapping imu1Mapping = new HashiImuAxisMapping();
        public HashiImuAxisMapping imu2Mapping = new HashiImuAxisMapping();

        [Header("Opening")]
        public HashiImuAxis sensorOpeningAxis = HashiImuAxis.X;
        public float sensorOpeningDirection = 1f;
        [FormerlySerializedAs("openingAxis")] public Vector3 visualOpeningAxis = Vector3.forward;
        public float minOpeningDegrees = 0f;
        public float maxOpeningDegrees = 35f;
        public float openingGyroDeadzoneDegreesPerSecond = 1.5f;
        public float openingAccelerationCorrectionRate = 10f;

        [Header("Whole Chopstick Rotation")]
        public HashiParentMotionSource parentMotionSource = HashiParentMotionSource.Lower;
        public bool invertParentRotation;
        public Vector3 parentOutputEuler = Vector3.zero;
        [Range(0f, 1f)] public float wholeRotationSensitivity = 0.7f;
        public float parentGyroDeadzoneDegreesPerSecond = 1f;
        public float parentTiltCorrectionRate = 4f;

        [Header("Acceleration Filtering")]
        public float accelerationMagnitudeToleranceG = 0.35f;
        public float correctionFullBelowDegreesPerSecond = 50f;
        public float correctionZeroAboveDegreesPerSecond = 260f;
        public float maxDeltaTimeSeconds = 0.12f;
        public float maxSampleGapSeconds = 0.7f;

        [Header("Calibration")]
        public bool calibrateOnStart = true;
        [Min(5)] public int calibrationSamples = 50;
        public KeyCode recalibrateKey = KeyCode.C;
        public KeyCode zeroPoseKey = KeyCode.Z;

        [Header("Presentation")]
        public float smoothing = 16f;
        public bool compensatePivot = true;
        public float separationPerDegree = 0f;
        public float maxSeparation = 0.08f;
        public Vector3 separationAxis = Vector3.right;
        public bool logParsedSamples;

        private readonly HashiDualImuFusion fusion = new HashiDualImuFusion();
        private MacHashiBleReceiver receiver;

        private Quaternion baseParentLocalRotation;
        private Quaternion displayedParentLocalRotation;
        private Quaternion lowerBaseLocalRotation;
        private Quaternion upperBaseLocalRotation;
        private Quaternion displayedLowerLocalRotation;
        private Quaternion displayedUpperLocalRotation;
        private Vector3 lowerBaseLocalPosition;
        private Vector3 upperBaseLocalPosition;
        private Vector3 lowerPivotLocal;
        private Vector3 upperPivotLocal;
        private Vector3 lowerBasePivotParent;
        private Vector3 upperBasePivotParent;
        private float displayedOpeningDegrees;
        private double lastPayloadHostTime = -1.0;
        private bool hasBaseline;
        private bool hasLoggedMissingReferences;
        private bool hasLoggedStaleSamples;
        private float nextMalformedPayloadWarningAt;
        private int suppressedMalformedPayloadWarnings;

        public HashiDualImuPose CurrentPose
        {
            get { return fusion.CurrentPose; }
        }

        private void Reset()
        {
            if (transform.childCount > 0)
            {
                lowerChopstick = transform.GetChild(0);
            }

            if (transform.childCount > 1)
            {
                upperChopstick = transform.GetChild(1);
            }
        }

        private void OnEnable()
        {
            CaptureBaseline();
            ConfigureFusion();
            fusion.Reset();
            if (calibrateOnStart)
            {
                fusion.BeginCalibration(calibrationSamples);
            }

            StartInput();
        }

        private void OnDisable()
        {
            StopInput();
        }

        private void OnValidate()
        {
            rawAccelerationToG = Mathf.Max(0.0000001f, rawAccelerationToG);
            rawGyroToDegreesPerSecond = Mathf.Max(0.0000001f, rawGyroToDegreesPerSecond);
            staleWarningSeconds = Mathf.Max(0.25f, staleWarningSeconds);
            minOpeningDegrees = Mathf.Clamp(minOpeningDegrees, -180f, 180f);
            maxOpeningDegrees = Mathf.Clamp(maxOpeningDegrees, -180f, 180f);
            openingGyroDeadzoneDegreesPerSecond = Mathf.Max(0f, openingGyroDeadzoneDegreesPerSecond);
            openingAccelerationCorrectionRate = Mathf.Max(0f, openingAccelerationCorrectionRate);
            parentGyroDeadzoneDegreesPerSecond = Mathf.Max(0f, parentGyroDeadzoneDegreesPerSecond);
            parentTiltCorrectionRate = Mathf.Max(0f, parentTiltCorrectionRate);
            accelerationMagnitudeToleranceG = Mathf.Max(0f, accelerationMagnitudeToleranceG);
            correctionFullBelowDegreesPerSecond = Mathf.Max(0f, correctionFullBelowDegreesPerSecond);
            correctionZeroAboveDegreesPerSecond = Mathf.Max(correctionFullBelowDegreesPerSecond + 1f, correctionZeroAboveDegreesPerSecond);
            maxDeltaTimeSeconds = Mathf.Max(0.001f, maxDeltaTimeSeconds);
            maxSampleGapSeconds = Mathf.Max(0.05f, maxSampleGapSeconds);
            calibrationSamples = Mathf.Max(5, calibrationSamples);
            smoothing = Mathf.Max(0f, smoothing);
            separationPerDegree = Mathf.Max(0f, separationPerDegree);
            maxSeparation = Mathf.Max(0f, maxSeparation);
            wholeRotationSensitivity = Mathf.Clamp01(wholeRotationSensitivity);
        }

        private void Update()
        {
            FlushBleLogs();
            HandleHotkeys();
            ConfigureFusion();
            DrainBlePayloads();
            ApplyTransforms(Time.deltaTime);
            WarnIfStale();
        }

        public void Recalibrate()
        {
            fusion.BeginCalibration(calibrationSamples);
            displayedOpeningDegrees = 0f;
            displayedParentLocalRotation = baseParentLocalRotation;
            displayedLowerLocalRotation = lowerBaseLocalRotation;
            displayedUpperLocalRotation = upperBaseLocalRotation;
            hasLoggedStaleSamples = false;
            Debug.Log("[Hashi BLE] Calibration started. Hold both chopsticks still in the closed neutral pose.", this);
        }

        public void ZeroPose()
        {
            fusion.ZeroPose();
            displayedOpeningDegrees = 0f;
            displayedParentLocalRotation = baseParentLocalRotation;
            displayedLowerLocalRotation = lowerBaseLocalRotation;
            displayedUpperLocalRotation = upperBaseLocalRotation;
            Debug.Log("[Hashi BLE] Pose zeroed.", this);
        }

        private void HandleHotkeys()
        {
            if (recalibrateKey != KeyCode.None && UnityEngine.Input.GetKeyDown(recalibrateKey))
            {
                Recalibrate();
            }

            if (zeroPoseKey != KeyCode.None && UnityEngine.Input.GetKeyDown(zeroPoseKey))
            {
                ZeroPose();
            }
        }

        private void CaptureBaseline()
        {
            baseParentLocalRotation = transform.localRotation;
            displayedParentLocalRotation = baseParentLocalRotation;

            lowerBaseLocalRotation = lowerChopstick != null ? lowerChopstick.localRotation : Quaternion.identity;
            upperBaseLocalRotation = upperChopstick != null ? upperChopstick.localRotation : Quaternion.identity;
            displayedLowerLocalRotation = lowerBaseLocalRotation;
            displayedUpperLocalRotation = upperBaseLocalRotation;

            lowerBaseLocalPosition = lowerChopstick != null ? lowerChopstick.localPosition : Vector3.zero;
            upperBaseLocalPosition = upperChopstick != null ? upperChopstick.localPosition : Vector3.zero;

            lowerPivotLocal = ResolvePivotLocal(lowerChopstick, lowerPivot);
            upperPivotLocal = ResolvePivotLocal(upperChopstick, upperPivot);
            lowerBasePivotParent = ResolvePivotParentPosition(
                lowerChopstick,
                lowerPivot,
                lowerBaseLocalPosition,
                lowerBaseLocalRotation,
                lowerPivotLocal);
            upperBasePivotParent = ResolvePivotParentPosition(
                upperChopstick,
                upperPivot,
                upperBaseLocalPosition,
                upperBaseLocalRotation,
                upperPivotLocal);

            displayedOpeningDegrees = 0f;
            hasBaseline = true;
        }

        private void StartInput()
        {
            StopInput();
            lastPayloadHostTime = -1.0;
            hasLoggedStaleSamples = false;

            receiver = new MacHashiBleReceiver();
            receiver.Start(
                DefaultIfEmpty(deviceNameFilter, HashiBleDefaults.DeviceNameFilter),
                DefaultIfEmpty(serviceUuid, HashiBleDefaults.ServiceUuid),
                DefaultIfEmpty(characteristicUuid, HashiBleDefaults.CharacteristicUuid));
            FlushBleLogs();
        }

        private void StopInput()
        {
            if (receiver == null)
            {
                return;
            }

            receiver.Dispose();
            receiver = null;
        }

        private void ConfigureFusion()
        {
            fusion.RawAccelerationToG = rawAccelerationToG;
            fusion.RawGyroToDegreesPerSecond = rawGyroToDegreesPerSecond;
            fusion.Imu1IsUpper = imu1IsUpper;
            fusion.Imu1Mapping = imu1Mapping ?? new HashiImuAxisMapping();
            fusion.Imu2Mapping = imu2Mapping ?? new HashiImuAxisMapping();
            fusion.OpeningAxis = sensorOpeningAxis;
            fusion.OpeningDirection = sensorOpeningDirection;
            fusion.OpeningMinDegrees = minOpeningDegrees;
            fusion.OpeningMaxDegrees = maxOpeningDegrees;
            fusion.OpeningGyroDeadzoneDegreesPerSecond = openingGyroDeadzoneDegreesPerSecond;
            fusion.OpeningAccelerationCorrectionRate = openingAccelerationCorrectionRate;
            fusion.ParentMotionSource = parentMotionSource;
            fusion.ParentGyroDeadzoneDegreesPerSecond = parentGyroDeadzoneDegreesPerSecond;
            fusion.ParentTiltCorrectionRate = parentTiltCorrectionRate;
            fusion.AccelerationMagnitudeToleranceG = accelerationMagnitudeToleranceG;
            fusion.CorrectionFullBelowDegreesPerSecond = correctionFullBelowDegreesPerSecond;
            fusion.CorrectionZeroAboveDegreesPerSecond = correctionZeroAboveDegreesPerSecond;
            fusion.MaxDeltaTimeSeconds = maxDeltaTimeSeconds;
            fusion.MaxSampleGapSeconds = maxSampleGapSeconds;
        }

        private void DrainBlePayloads()
        {
            if (receiver == null)
            {
                return;
            }

            while (receiver.TryDequeuePayload(out var payload))
            {
                lastPayloadHostTime = payload.HostTimeSeconds;
                if (!HashiBleSampleParser.TryParse(payload, out var sample, out var error))
                {
                    WarnMalformedPayload(payload.Text, error);
                    continue;
                }

                hasLoggedStaleSamples = false;
                fusion.Consume(sample, Time.deltaTime);

                if (logParsedSamples)
                {
                    var pose = fusion.CurrentPose;
                    Debug.Log(
                        $"[Hashi BLE] seq={(sample.HasSequence ? sample.Sequence.ToString() : "-")} open={pose.OpeningDegrees:0.0}deg "
                        + $"upperAcc={pose.UpperAccelerationG} lowerAcc={pose.LowerAccelerationG} "
                        + $"upperGyro={pose.UpperGyroDps} lowerGyro={pose.LowerGyroDps}",
                        this);
                }
            }
        }

        private void ApplyTransforms(float deltaTime)
        {
            if (!hasBaseline)
            {
                CaptureBaseline();
            }

            if (lowerChopstick == null || upperChopstick == null)
            {
                if (!hasLoggedMissingReferences)
                {
                    Debug.LogWarning("[Hashi BLE] Assign lowerChopstick and upperChopstick.", this);
                    hasLoggedMissingReferences = true;
                }

                return;
            }

            var pose = fusion.CurrentPose;
            if (!pose.HasPose)
            {
                ApplyBasePose();
                return;
            }

            var blend = smoothing <= 0f || deltaTime <= 0f ? 1f : 1f - Mathf.Exp(-smoothing * deltaTime);
            displayedOpeningDegrees = Mathf.Lerp(displayedOpeningDegrees, pose.OpeningDegrees, blend);

            var targetParentRotation = baseParentLocalRotation * BuildParentOutputRotation(pose.ParentRotation);
            displayedParentLocalRotation = Quaternion.Slerp(displayedParentLocalRotation, targetParentRotation, blend);
            displayedLowerLocalRotation = Quaternion.Slerp(displayedLowerLocalRotation, lowerBaseLocalRotation, blend);
            displayedUpperLocalRotation = Quaternion.Slerp(
                displayedUpperLocalRotation,
                upperBaseLocalRotation * Quaternion.AngleAxis(displayedOpeningDegrees, NormalizedVisualOpeningAxis()),
                blend);

            transform.localRotation = displayedParentLocalRotation;
            ApplyChopstickLocalPoses(displayedOpeningDegrees);
        }

        private void ApplyBasePose()
        {
            transform.localRotation = baseParentLocalRotation;
            if (lowerChopstick != null)
            {
                lowerChopstick.localPosition = lowerBaseLocalPosition;
                lowerChopstick.localRotation = lowerBaseLocalRotation;
            }

            if (upperChopstick != null)
            {
                upperChopstick.localPosition = upperBaseLocalPosition;
                upperChopstick.localRotation = upperBaseLocalRotation;
            }
        }

        private Quaternion BuildParentOutputRotation(Quaternion sensorRotation)
        {
            var rotation = invertParentRotation ? Quaternion.Inverse(sensorRotation) : sensorRotation;
            var basis = Quaternion.Euler(parentOutputEuler);
            rotation = basis * rotation * Quaternion.Inverse(basis);
            return Quaternion.Slerp(Quaternion.identity, rotation, Mathf.Clamp01(wholeRotationSensitivity));
        }

        private void ApplyChopstickLocalPoses(float openingDegrees)
        {
            var separation = Mathf.Min(
                Mathf.Abs(openingDegrees) * Mathf.Max(0f, separationPerDegree),
                Mathf.Max(0f, maxSeparation));
            var axis = separationAxis.sqrMagnitude > 0.0001f ? separationAxis.normalized : Vector3.right;
            var halfOffset = axis * (separation * 0.5f);

            if (compensatePivot)
            {
                ApplyLocalPoseAroundPivot(
                    lowerChopstick,
                    displayedLowerLocalRotation,
                    lowerPivotLocal,
                    lowerBasePivotParent - halfOffset,
                    lowerBaseLocalPosition);
                ApplyLocalPoseAroundPivot(
                    upperChopstick,
                    displayedUpperLocalRotation,
                    upperPivotLocal,
                    upperBasePivotParent + halfOffset,
                    upperBaseLocalPosition);
                return;
            }

            lowerChopstick.localRotation = displayedLowerLocalRotation;
            upperChopstick.localRotation = displayedUpperLocalRotation;
            lowerChopstick.localPosition = lowerBaseLocalPosition - halfOffset;
            upperChopstick.localPosition = upperBaseLocalPosition + halfOffset;
        }

        private static void ApplyLocalPoseAroundPivot(
            Transform target,
            Quaternion localRotation,
            Vector3 pivotLocal,
            Vector3 pivotParentPosition,
            Vector3 fallbackBaseLocalPosition)
        {
            if (target == null)
            {
                return;
            }

            target.localRotation = localRotation;
            if (pivotLocal.sqrMagnitude < 0.000001f)
            {
                target.localPosition = fallbackBaseLocalPosition;
                return;
            }

            target.localPosition = pivotParentPosition - localRotation * Vector3.Scale(pivotLocal, target.localScale);
        }

        private static Vector3 ResolvePivotLocal(Transform chopstick, Transform pivot)
        {
            if (chopstick == null || pivot == null)
            {
                return Vector3.zero;
            }

            return chopstick.InverseTransformPoint(pivot.position);
        }

        private static Vector3 ResolvePivotParentPosition(
            Transform chopstick,
            Transform pivot,
            Vector3 baseLocalPosition,
            Quaternion baseLocalRotation,
            Vector3 pivotLocal)
        {
            if (chopstick == null)
            {
                return Vector3.zero;
            }

            if (pivot != null && chopstick.parent != null)
            {
                return chopstick.parent.InverseTransformPoint(pivot.position);
            }

            return baseLocalPosition + baseLocalRotation * Vector3.Scale(pivotLocal, chopstick.localScale);
        }

        private Vector3 NormalizedVisualOpeningAxis()
        {
            return visualOpeningAxis.sqrMagnitude > 0.0001f ? visualOpeningAxis.normalized : Vector3.forward;
        }

        private void FlushBleLogs()
        {
            if (receiver == null)
            {
                return;
            }

            while (receiver.TryDequeueLog(out var message))
            {
                Debug.Log("[Hashi BLE] " + message, this);
            }
        }

        private void WarnMalformedPayload(string rawPayload, string error)
        {
            var now = Time.realtimeSinceStartup;
            if (now < nextMalformedPayloadWarningAt)
            {
                suppressedMalformedPayloadWarnings++;
                return;
            }

            var suffix = suppressedMalformedPayloadWarnings > 0
                ? $" Suppressed {suppressedMalformedPayloadWarnings} similar payload warning(s)."
                : string.Empty;
            nextMalformedPayloadWarningAt = now + 1.5f;
            suppressedMalformedPayloadWarnings = 0;
            Debug.LogWarning($"[Hashi BLE] Ignored malformed payload: {error}. Payload: {rawPayload}{suffix}", this);
        }

        private void WarnIfStale()
        {
            if (lastPayloadHostTime <= 0.0 || hasLoggedStaleSamples)
            {
                return;
            }

            var age = HashiBleClock.NowSeconds - lastPayloadHostTime;
            if (age < staleWarningSeconds)
            {
                return;
            }

            hasLoggedStaleSamples = true;
            Debug.LogWarning($"[Hashi BLE] No BLE sample received for {age:0.0}s. Holding the last valid pose.", this);
        }

        private static string DefaultIfEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
