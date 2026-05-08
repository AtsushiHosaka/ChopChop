using ChopChop.BLE;
using ChopChop.Input;
using UnityEngine;

namespace ChopChop.Debugging
{
    [AddComponentMenu("ChopChop/Debug/Hashi IMU Tuning Overlay")]
    public sealed class HashiImuTuningOverlay : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private bool applyProfileOnEnable = true;
        [SerializeField] private bool useTunedTheta = true;
        [SerializeField] private HashiImuThetaMode thetaMode = HashiImuThetaMode.AccelerationAngle;
        [SerializeField] private HashiImuMotionSource motionSource = HashiImuMotionSource.Imu1;
        [SerializeField] private HashiImuAxis imu1Axis = HashiImuAxis.Z;
        [SerializeField] private HashiImuAxis imu2Axis = HashiImuAxis.Z;
        [SerializeField] private bool invertTheta;
        [SerializeField] private float inputClosed = 95f;
        [SerializeField] private float inputOpen = 130f;
        [SerializeField] private float outputClosed = 5f;
        [SerializeField] private float outputOpen = 35f;
        [SerializeField] private float rawAccelerationToG = HashiImuMappingRuntime.Esp32Lsm6RawAccelerationToG;
        [SerializeField] private float rawGyroToDegreesPerSecond = HashiBleDefaults.RawGyroToDegreesPerSecond;
        [SerializeField, Range(0f, 1f)] private float parentRotationWeight;
        [SerializeField] private bool useIndependentImuPose = true;
        [SerializeField] private bool autoCaptureIndependentNeutral = true;
        [SerializeField, Range(0f, 1f)] private float independentPoseWeight = 1f;
        [SerializeField] private bool invertImu1Pose;
        [SerializeField] private bool invertImu2Pose;
        [SerializeField] private HashiImuAxis imu1MapX = HashiImuAxis.X;
        [SerializeField] private HashiImuAxis imu1MapY = HashiImuAxis.Y;
        [SerializeField] private HashiImuAxis imu1MapZ = HashiImuAxis.Z;
        [SerializeField] private HashiImuAxis imu2MapX = HashiImuAxis.X;
        [SerializeField] private HashiImuAxis imu2MapY = HashiImuAxis.Y;
        [SerializeField] private HashiImuAxis imu2MapZ = HashiImuAxis.Z;
        [SerializeField] private Vector3 imu1MapSigns = Vector3.one;
        [SerializeField] private Vector3 imu2MapSigns = Vector3.one;

        private Rect windowRect = new Rect(12f, 12f, 440f, 720f);
        private HashiInputProvider provider;

        private void OnEnable()
        {
            if (applyProfileOnEnable)
            {
                ApplyToRuntime();
            }
        }

        private void Update()
        {
            ApplyToRuntime();
        }

        private void OnGUI()
        {
            var currentEvent = Event.current;
            if (currentEvent != null
                && currentEvent.type == EventType.KeyDown
                && currentEvent.keyCode == KeyCode.F8)
            {
                visible = !visible;
                currentEvent.Use();
            }

            if (!visible)
            {
                return;
            }

            windowRect = GUILayout.Window(24051, windowRect, DrawWindow, "Hashi IMU Tuning");
        }

        private void DrawWindow(int id)
        {
            provider = provider != null
                ? provider
                : FindAnyObjectByType<HashiInputProvider>(FindObjectsInactive.Include);

            GUILayout.Label("F8: show/hide");
            if (provider == null)
            {
                GUILayout.Label("HashiInputProvider: none");
                GUI.DragWindow();
                return;
            }

            var frame = provider.CurrentFrame;
            GUILayout.Label($"Provider: {provider.name}");
            GUILayout.Label($"BLE: {provider.DebugSourceStatus}");
            GUILayout.Label($"Frame: sample={frame.HasSample} theta={frame.ThetaDegrees:0.0} acc={FormatVector(frame.Acceleration)} gyro={FormatVector(frame.GyroDegreesPerSecond)}");

            if (provider.TryGetLatestBleSample(out var sample) && sample.HasDualImu)
            {
                DrawSample(sample);
            }
            else
            {
                GUILayout.Label("Latest dual IMU sample: none");
            }

            GUILayout.Space(8f);
            GUILayout.Label("Independent IMU pose");
            useIndependentImuPose = GUILayout.Toggle(useIndependentImuPose, "Use IMU1 upper / IMU2 lower");
            autoCaptureIndependentNeutral = GUILayout.Toggle(autoCaptureIndependentNeutral, "Auto neutral from first dual sample");
            independentPoseWeight = DrawSlider("Independent pose weight", independentPoseWeight, 0f, 1f);
            parentRotationWeight = DrawSlider("Whole Hashi rotation", parentRotationWeight, 0f, 1f);
            invertImu1Pose = GUILayout.Toggle(invertImu1Pose, "Invert IMU1 upper pose");
            invertImu2Pose = GUILayout.Toggle(invertImu2Pose, "Invert IMU2 lower pose");
            DrawAxisMap("IMU1 upper", ref imu1MapX, ref imu1MapY, ref imu1MapZ, ref imu1MapSigns);
            DrawAxisMap("IMU2 lower", ref imu2MapX, ref imu2MapY, ref imu2MapZ, ref imu2MapSigns);

            GUILayout.BeginHorizontal();
            GUI.enabled = provider.TryGetLatestBleSample(out sample) && sample.HasDualImu;
            if (GUILayout.Button("Capture Neutral"))
            {
                ApplyToRuntime();
                HashiImuMappingRuntime.CaptureIndependentNeutral(sample);
            }

            GUI.enabled = true;
            if (GUILayout.Button("Clear Neutral"))
            {
                HashiImuMappingRuntime.ClearIndependentNeutral();
            }

            GUILayout.EndHorizontal();
            GUILayout.Label($"Neutral: {(HashiImuMappingRuntime.HasIndependentNeutral ? "captured" : "none")}");

            GUILayout.Space(8f);
            GUILayout.Label("Theta fallback/gameplay");
            useTunedTheta = GUILayout.Toggle(useTunedTheta, "Use tuned theta");
            invertTheta = GUILayout.Toggle(invertTheta, "Invert theta");
            thetaMode = DrawEnumCycle("Theta mode", thetaMode);
            motionSource = DrawEnumCycle("Motion source", motionSource);
            imu1Axis = DrawEnumCycle("IMU1 axis", imu1Axis);
            imu2Axis = DrawEnumCycle("IMU2 axis", imu2Axis);

            inputClosed = DrawSlider("Input closed", inputClosed, -32768f, 32768f);
            inputOpen = DrawSlider("Input open", inputOpen, -32768f, 32768f);
            outputClosed = DrawSlider("Output closed deg", outputClosed, -20f, 80f);
            outputOpen = DrawSlider("Output open deg", outputOpen, -20f, 80f);
            rawAccelerationToG = DrawSlider("Raw acc to G", rawAccelerationToG, 0.00001f, 0.0005f);
            rawGyroToDegreesPerSecond = DrawSlider("Raw gyro to dps", rawGyroToDegreesPerSecond, 0.001f, 0.05f);

            if (provider.TryGetLatestBleSample(out sample)
                && HashiImuMappingRuntime.TryMeasureThetaInput(sample, out var input))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Closed"))
                {
                    inputClosed = input;
                }

                if (GUILayout.Button("Set Open"))
                {
                    inputOpen = input;
                }

                GUILayout.EndHorizontal();
            }

            ApplyToRuntime();
            GUI.DragWindow();
        }

        private void DrawSample(HashiBleSample sample)
        {
            if (HashiImuMappingRuntime.TryMeasureThetaInput(sample, out var input))
            {
                GUILayout.Label($"Input theta: {input:0.0}");
            }

            GUILayout.Label($"IMU1 acc raw {FormatVector(sample.Imu1AccelerationRaw)} gyro raw {FormatVector(sample.Imu1GyroRaw)}");
            GUILayout.Label($"IMU2 acc raw {FormatVector(sample.Imu2AccelerationRaw)} gyro raw {FormatVector(sample.Imu2GyroRaw)}");
        }

        private void ApplyToRuntime()
        {
            HashiImuMappingRuntime.UseTunedTheta = useTunedTheta;
            HashiImuMappingRuntime.ThetaMode = thetaMode;
            HashiImuMappingRuntime.MotionSource = motionSource;
            HashiImuMappingRuntime.Imu1Axis = imu1Axis;
            HashiImuMappingRuntime.Imu2Axis = imu2Axis;
            HashiImuMappingRuntime.InvertTheta = invertTheta;
            HashiImuMappingRuntime.InputClosed = inputClosed;
            HashiImuMappingRuntime.InputOpen = inputOpen;
            HashiImuMappingRuntime.OutputClosed = outputClosed;
            HashiImuMappingRuntime.OutputOpen = outputOpen;
            HashiImuMappingRuntime.RawAccelerationToG = rawAccelerationToG;
            HashiImuMappingRuntime.RawGyroToDegreesPerSecond = rawGyroToDegreesPerSecond;
            HashiImuMappingRuntime.ParentRotationWeight = parentRotationWeight;
            HashiImuMappingRuntime.UseIndependentImuPose = useIndependentImuPose;
            HashiImuMappingRuntime.AutoCaptureIndependentNeutral = autoCaptureIndependentNeutral;
            HashiImuMappingRuntime.IndependentPoseWeight = independentPoseWeight;
            HashiImuMappingRuntime.InvertImu1Pose = invertImu1Pose;
            HashiImuMappingRuntime.InvertImu2Pose = invertImu2Pose;
            HashiImuMappingRuntime.Imu1MapX = imu1MapX;
            HashiImuMappingRuntime.Imu1MapY = imu1MapY;
            HashiImuMappingRuntime.Imu1MapZ = imu1MapZ;
            HashiImuMappingRuntime.Imu2MapX = imu2MapX;
            HashiImuMappingRuntime.Imu2MapY = imu2MapY;
            HashiImuMappingRuntime.Imu2MapZ = imu2MapZ;
            HashiImuMappingRuntime.Imu1MapSigns = imu1MapSigns;
            HashiImuMappingRuntime.Imu2MapSigns = imu2MapSigns;
        }

        private static void DrawAxisMap(
            string label,
            ref HashiImuAxis mapX,
            ref HashiImuAxis mapY,
            ref HashiImuAxis mapZ,
            ref Vector3 signs)
        {
            GUILayout.Label(label);
            mapX = DrawEnumCycle("Local X from", mapX);
            mapY = DrawEnumCycle("Local Y from", mapY);
            mapZ = DrawEnumCycle("Local Z from", mapZ);

            GUILayout.BeginHorizontal();
            signs.x = DrawSignToggle("X", signs.x);
            signs.y = DrawSignToggle("Y", signs.y);
            signs.z = DrawSignToggle("Z", signs.z);
            GUILayout.EndHorizontal();
        }

        private static T DrawEnumCycle<T>(string label, T value) where T : System.Enum
        {
            if (!GUILayout.Button($"{label}: {value}"))
            {
                return value;
            }

            var values = (T[])System.Enum.GetValues(typeof(T));
            var index = System.Array.IndexOf(values, value);
            return values[(index + 1) % values.Length];
        }

        private static float DrawSlider(string label, float value, float min, float max)
        {
            GUILayout.Label($"{label}: {value:0.#####}");
            return GUILayout.HorizontalSlider(value, min, max);
        }

        private static float DrawSignToggle(string label, float value)
        {
            var negative = value < 0f;
            negative = GUILayout.Toggle(negative, $"{label}-");
            return negative ? -1f : 1f;
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.##}, {value.y:0.##}, {value.z:0.##})";
        }
    }
}
