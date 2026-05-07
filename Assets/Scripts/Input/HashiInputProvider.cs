using ChopChop.Core;
using UnityEngine;

namespace ChopChop.Input
{
    [AddComponentMenu("ChopChop/Input/Hashi Input Provider")]
    public sealed class HashiInputProvider : MonoBehaviour
    {
        [SerializeField] private PlayerId playerId = new PlayerId(0);
        [SerializeField] private string playerLabel = "P1";
        [SerializeField] private HashiInputMode inputMode = HashiInputMode.Ble;
        [SerializeField] private string deviceNameFilter = string.Empty;
        [SerializeField] private string serviceUuid = string.Empty;
        [SerializeField] private string characteristicUuid = string.Empty;
        [SerializeField] private int udpPort = 5005;
        [SerializeField] private string udpBindAddress = string.Empty;
        [SerializeField] private bool captureNeutralOnStart;
        [SerializeField, Range(0f, 1f)] private float accelerometerCorrection = 0.015f;
        [SerializeField] private bool logImuSamples = true;
        [SerializeField, Min(0.02f)] private float imuLogIntervalSeconds = 0.1f;
        [SerializeField] private bool logRawBlePayloads = true;
        [SerializeField, Min(0.02f)] private float rawBleLogIntervalSeconds = 0.1f;

        private IHashiInputSource source;
        private readonly HashiCalibration calibration = new HashiCalibration();
        private float lastImuLogAt = -999f;

        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;
        public bool HasNeutral => calibration.HasNeutral;
        public HashiInputMode InputMode => inputMode;
        public PlayerId PlayerId => playerId;
        public string PlayerLabel => string.IsNullOrWhiteSpace(playerLabel) ? playerId.ToString() : playerLabel.Trim();

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

        public void ConfigureUdp(int port, string bindAddress = "", bool captureNeutral = false)
        {
            udpPort = Mathf.Max(1, port);
            udpBindAddress = bindAddress ?? string.Empty;
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
            calibration.Capture(CurrentFrame);
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

            switch (inputMode)
            {
                case HashiInputMode.Demo:
                    source = new DemoHashiInputSource();
                    break;
                case HashiInputMode.Udp:
                    source = new UdpHashiInputSource(udpPort, udpBindAddress, PlayerLabel, accelerometerCorrection);
                    break;
                default:
                    source = new BleHashiInputSource(
                        deviceNameFilter,
                        serviceUuid,
                        characteristicUuid,
                        PlayerLabel,
                        accelerometerCorrection,
                        logRawBlePayloads,
                        rawBleLogIntervalSeconds);
                    break;
            }

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
            if (captureNeutralOnStart && inputMode != HashiInputMode.Demo && frame.HasSample && !calibration.HasNeutral)
            {
                calibration.Capture(frame);
            }

            LogImuFrame(frame);
            CurrentFrame = calibration.Apply(frame);
        }

        private void LogImuFrame(HashiInputFrame frame)
        {
            if (!logImuSamples || !frame.HasSample)
            {
                return;
            }

            var interval = Mathf.Max(0.02f, imuLogIntervalSeconds);
            if (Time.realtimeSinceStartup - lastImuLogAt < interval)
            {
                return;
            }

            lastImuLogAt = Time.realtimeSinceStartup;
            var acceleration = frame.Acceleration;
            var gyro = frame.GyroDegreesPerSecond;
            Debug.Log(
                $"[Hashi IMU/{PlayerLabel}] acc=({acceleration.x:F2}, {acceleration.y:F2}, {acceleration.z:F2}) " +
                $"gyro=({gyro.x:F2}, {gyro.y:F2}, {gyro.z:F2}) " +
                $"theta={frame.ThetaDegrees:F2} pinch={frame.PinchNormalized:F2}");
        }
    }
}
