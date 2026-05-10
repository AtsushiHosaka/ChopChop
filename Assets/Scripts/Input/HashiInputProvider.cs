using ChopChop.BLE;
using ChopChop.Core;
using UnityEngine;

namespace ChopChop.Input
{
    [AddComponentMenu("ChopChop/Input/Hashi Input Provider")]
    public sealed class HashiInputProvider : MonoBehaviour
    {
        private const float BleConnectionPromptStaleSeconds = 1.5f;
        private const float ResetPoseTipDirectionRollDegrees = 360f;

        [SerializeField] private PlayerId playerId = new PlayerId(0);
        [SerializeField] private string playerLabel = "P1";
        [SerializeField] private HashiInputMode inputMode = HashiInputMode.Ble;
        [SerializeField] private string deviceNameFilter = HashiBleDefaults.DeviceNameFilter;
        [SerializeField] private string serviceUuid = HashiBleDefaults.ServiceUuid;
        [SerializeField] private string characteristicUuid = HashiBleDefaults.CharacteristicUuid;
        [SerializeField] private bool captureNeutralOnStart;
        [SerializeField, Range(0f, 1f)] private float accelerometerCorrection = 0.18f;

        private IHashiInputSource source;
        private readonly HashiCalibration calibration = new HashiCalibration();

        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;
        public bool HasNeutral => calibration.HasNeutral;
        public HashiInputMode InputMode => inputMode;
        public PlayerId PlayerId => playerId;
        public string PlayerLabel => string.IsNullOrWhiteSpace(playerLabel) ? playerId.ToString() : playerLabel.Trim();
        public bool IsSourceRunning => source?.IsRunning ?? false;
        public string DebugSourceStatus => source is BleHashiInputSource bleSource
            ? bleSource.Status
            : source?.GetType().Name ?? "None";
        public bool NeedsBleConnectionPrompt
        {
            get
            {
                if (inputMode != HashiInputMode.Ble)
                {
                    return false;
                }

                var bleSource = source as BleHashiInputSource;
                if (bleSource == null)
                {
                    return true;
                }

                if (!bleSource.IsRunning || !bleSource.HasLatestSample)
                {
                    return true;
                }

                return Time.realtimeSinceStartup - bleSource.LatestSample.RealtimeReceived
                    > BleConnectionPromptStaleSeconds;
            }
        }

        public bool TryGetLatestBleSample(out HashiBleSample sample)
        {
            if (source is BleHashiInputSource bleSource && bleSource.HasLatestSample)
            {
                sample = bleSource.LatestSample;
                return true;
            }

            sample = default;
            return false;
        }

        public void ConfigurePlayer(PlayerId id, string label = "")
        {
            playerId = id;
            playerLabel = string.IsNullOrWhiteSpace(label) ? id.ToString() : label.Trim();
        }

        public void ConfigureConnection(
            string deviceName = "",
            string service = "",
            string characteristic = "",
            bool captureNeutral = false)
        {
            deviceNameFilter = deviceName ?? string.Empty;
            serviceUuid = service ?? string.Empty;
            characteristicUuid = characteristic ?? string.Empty;
            captureNeutralOnStart = captureNeutral;

            if (isActiveAndEnabled)
            {
                StartSource();
            }
        }

        public void SetInputMode(HashiInputMode mode)
        {
            if (inputMode == mode)
            {
                return;
            }

            inputMode = mode;
            calibration.Clear();
            CurrentFrame = HashiInputFrame.Empty;

            if (isActiveAndEnabled)
            {
                StartSource();
            }
        }

        public void SetDemoPhase(HashiDemoPhase phase)
        {
            if (source is DemoHashiInputSource demoSource)
            {
                demoSource.SetPhase(phase);
            }
        }

        public void CaptureNeutral()
        {
            ResetPose();
        }

        public bool ResetPose()
        {
            var frame = source?.CurrentFrame ?? CurrentFrame;
            if (!frame.HasSample)
            {
                return false;
            }

            if (source is BleHashiInputSource bleSource && bleSource.HasLatestSample)
            {
                var sample = bleSource.LatestSample;
                HashiImuMappingRuntime.CaptureIndependentNeutral(sample);
                HashiImuMappingRuntime.ZeroImu1IntegratedPose(sample);
                HashiImuMappingRuntime.ZeroImu2IntegratedPose(sample);
            }

            calibration.Capture(frame, ResetPosePostNeutralRotation());
            CurrentFrame = calibration.Apply(frame);
            return true;
        }

        private static Quaternion ResetPosePostNeutralRotation()
        {
            var displayAlignment = Quaternion.Euler(HashiImuMappingRuntime.HashiDisplayAlignmentEuler);
            var tipDirectionRoll = Quaternion.AngleAxis(ResetPoseTipDirectionRollDegrees, Vector3.forward);
            return displayAlignment * tipDirectionRoll * Quaternion.Inverse(displayAlignment);
        }

        private void OnEnable()
        {
            StartSource();
        }

        private void OnDisable()
        {
            source?.StopInput();
            source = null;
        }

        private void StartSource()
        {
            source?.StopInput();
            source = inputMode == HashiInputMode.Demo
                ? new DemoHashiInputSource()
                : new BleHashiInputSource(deviceNameFilter, serviceUuid, characteristicUuid, PlayerLabel, accelerometerCorrection);
            source.StartInput();
        }

        private void Update()
        {
            source?.Tick(Time.deltaTime);

            if (source == null)
            {
                return;
            }

            var frame = source.CurrentFrame;
            if (captureNeutralOnStart && inputMode == HashiInputMode.Ble && frame.HasSample && !calibration.HasNeutral)
            {
                ResetPose();
            }

            CurrentFrame = calibration.Apply(frame);
        }
    }
}
