using UnityEngine;

namespace ChopChop.BLE
{
    [AddComponentMenu("ChopChop/Hashi BLE Controller")]
    public sealed class HashiBleController : MonoBehaviour
    {
        [Header("Chopsticks")]
        public Transform lowerChopstick;
        public Transform upperChopstick;

        [Header("BLE")]
        public string deviceNameFilter = string.Empty;
        public string serviceUuid = string.Empty;
        public string characteristicUuid = string.Empty;

        [Header("Motion")]
        [Min(0f)] public float gyroSensitivity = 1f;
        [Min(0f)] public float accelerometerCorrection = 0.35f;
        [Min(0f)] public float smoothing = 12f;

        [SerializeField] private Vector3 openingAxis = Vector3.forward;
        [SerializeField] private float staleWarningSeconds = 2f;
        [SerializeField] private bool logParsedSamples = false;

        private MacHashiBleReceiver bleReceiver;
        private Quaternion baseParentLocalRotation;
        private Quaternion displayedParentLocalRotation;
        private Quaternion sensorLocalRotation = Quaternion.identity;
        private Quaternion lowerBaseLocalRotation;
        private Quaternion upperBaseLocalRotation;
        private Vector3 normalizedOpeningAxis = Vector3.forward;
        private float currentTheta;
        private float targetTheta;
        private float lastSampleRealtime;
        private bool hasBaseline;
        private bool hasReceivedSample;
        private bool hasLoggedMissingReferences;
        private bool hasLoggedStaleSample;
        private float nextMalformedPayloadWarningTime;

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
            StartInput();
        }

        private void OnDisable()
        {
            StopInput();
        }

        private void OnValidate()
        {
            gyroSensitivity = Mathf.Max(0f, gyroSensitivity);
            accelerometerCorrection = Mathf.Max(0f, accelerometerCorrection);
            smoothing = Mathf.Max(0f, smoothing);
            staleWarningSeconds = Mathf.Max(0.25f, staleWarningSeconds);
            normalizedOpeningAxis = openingAxis.sqrMagnitude > 0.0001f ? openingAxis.normalized : Vector3.forward;
        }

        private void Update()
        {
            FlushBleLogs();

            var hasNewSample = TryGetLatestSample(out var sample);
            if (hasNewSample)
            {
                ConsumeSample(sample, Time.deltaTime);
            }

            ApplyTransforms(Time.deltaTime);
            WarnIfBleStale();
        }

        private void CaptureBaseline()
        {
            normalizedOpeningAxis = openingAxis.sqrMagnitude > 0.0001f ? openingAxis.normalized : Vector3.forward;
            baseParentLocalRotation = transform.localRotation;
            displayedParentLocalRotation = transform.localRotation;
            sensorLocalRotation = Quaternion.identity;

            if (lowerChopstick != null)
            {
                lowerBaseLocalRotation = lowerChopstick.localRotation;
            }

            if (upperChopstick != null)
            {
                upperBaseLocalRotation = upperChopstick.localRotation;
            }

            hasBaseline = true;
        }

        private void StartInput()
        {
            hasReceivedSample = false;
            hasLoggedStaleSample = false;
            lastSampleRealtime = Time.realtimeSinceStartup;

            bleReceiver = new MacHashiBleReceiver();
            bleReceiver.Start(deviceNameFilter, serviceUuid, characteristicUuid);
            FlushBleLogs();
        }

        private void StopInput()
        {
            if (bleReceiver == null)
            {
                return;
            }

            bleReceiver.Dispose();
            bleReceiver = null;
        }

        private bool TryGetLatestSample(out HashiBleSample latestSample)
        {
            latestSample = default;
            var hasParsedSample = false;

            if (bleReceiver == null)
            {
                return false;
            }

            while (bleReceiver.TryDequeueRawPayload(out var rawPayload))
            {
                if (HashiBleSampleParser.TryParse(rawPayload, out var parsedSample, out var error))
                {
                    latestSample = parsedSample;
                    hasParsedSample = true;
                }
                else
                {
                    WarnMalformedPayload(rawPayload, error);
                }
            }

            return hasParsedSample;
        }

        private void ConsumeSample(HashiBleSample sample, float deltaTime)
        {
            hasReceivedSample = true;
            hasLoggedStaleSample = false;
            lastSampleRealtime = Time.realtimeSinceStartup;

            if (sample.HasTheta)
            {
                targetTheta = sample.ThetaValue;
            }

            IntegrateGyro(sample, deltaTime);
            CorrectWithAccelerometer(sample, deltaTime);

            if (logParsedSamples)
            {
                Debug.Log($"[Hashi BLE] theta={sample.ThetaValue:F2}, gyro={sample.GyroVector}, acc={sample.AccelerationVector}", this);
            }
        }

        private void IntegrateGyro(HashiBleSample sample, float deltaTime)
        {
            if (!sample.HasGyroVector || deltaTime <= 0f || gyroSensitivity <= 0f)
            {
                return;
            }

            var gyroDegrees = sample.GyroVector * gyroSensitivity * deltaTime;
            sensorLocalRotation *= Quaternion.Euler(gyroDegrees);
            sensorLocalRotation = Normalize(sensorLocalRotation);
        }

        private void CorrectWithAccelerometer(HashiBleSample sample, float deltaTime)
        {
            if (!sample.HasAccelerationVector || deltaTime <= 0f || accelerometerCorrection <= 0f)
            {
                return;
            }

            var acceleration = sample.AccelerationVector;
            if (acceleration.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var measuredDownInParent = sensorLocalRotation * acceleration.normalized;
            var correction = Quaternion.FromToRotation(measuredDownInParent, Vector3.down);
            var blend = 1f - Mathf.Exp(-accelerometerCorrection * deltaTime);
            sensorLocalRotation = Quaternion.Slerp(sensorLocalRotation, correction * sensorLocalRotation, blend);
            sensorLocalRotation = Normalize(sensorLocalRotation);
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
                    Debug.LogWarning("[Hashi BLE] Assign lowerChopstick and upperChopstick to animate the Hashi cubes.", this);
                    hasLoggedMissingReferences = true;
                }

                return;
            }

            var blend = smoothing <= 0f || deltaTime <= 0f
                ? 1f
                : 1f - Mathf.Exp(-smoothing * deltaTime);

            currentTheta = Mathf.Lerp(currentTheta, targetTheta, blend);
            var targetParentRotation = baseParentLocalRotation * sensorLocalRotation;
            displayedParentLocalRotation = Quaternion.Slerp(displayedParentLocalRotation, targetParentRotation, blend);

            transform.localRotation = displayedParentLocalRotation;
            lowerChopstick.localRotation = lowerBaseLocalRotation;
            upperChopstick.localRotation = upperBaseLocalRotation * Quaternion.AngleAxis(currentTheta, normalizedOpeningAxis);
        }

        private void FlushBleLogs()
        {
            if (bleReceiver == null)
            {
                return;
            }

            while (bleReceiver.TryDequeueLog(out var message))
            {
                Debug.Log($"[Hashi BLE] {message}", this);
            }
        }

        private void WarnMalformedPayload(string rawPayload, string error)
        {
            if (Time.realtimeSinceStartup < nextMalformedPayloadWarningTime)
            {
                return;
            }

            nextMalformedPayloadWarningTime = Time.realtimeSinceStartup + 1f;
            Debug.LogWarning($"[Hashi BLE] Ignored malformed BLE payload: {error} Payload: {rawPayload}", this);
        }

        private void WarnIfBleStale()
        {
            if (!hasReceivedSample || hasLoggedStaleSample)
            {
                return;
            }

            if (Time.realtimeSinceStartup - lastSampleRealtime < staleWarningSeconds)
            {
                return;
            }

            hasLoggedStaleSample = true;
            Debug.LogWarning("[Hashi BLE] BLE samples stopped arriving. Holding the last Hashi pose.", this);
        }

        private static Quaternion Normalize(Quaternion value)
        {
            var magnitude = Mathf.Sqrt(
                value.x * value.x +
                value.y * value.y +
                value.z * value.z +
                value.w * value.w);

            if (magnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            return new Quaternion(
                value.x / magnitude,
                value.y / magnitude,
                value.z / magnitude,
                value.w / magnitude);
        }
    }
}
