using ChopChop.BLE;
using ChopChop.Input;
using System.Text;
using UnityEngine;

namespace ChopChop.Debugging
{
    [AddComponentMenu("ChopChop/Debug/Hashi IMU Tuning Overlay")]
    public sealed class HashiImuTuningOverlay : MonoBehaviour
    {
        [SerializeField] private bool visible;
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
        [SerializeField] private HashiIndependentPoseMode independentPoseMode = HashiIndependentPoseMode.AccelerationOpening;
        [SerializeField, Range(0f, 1f)] private float independentPoseWeight = 1f;
        [SerializeField] private float gyroDeadzoneDegreesPerSecond = 1.5f;
        [SerializeField] private float accelerationCorrectionWeight = 0.02f;
        [SerializeField] private float accelerationCorrectionDeadzoneDegrees = 2f;
        [SerializeField] private float accelerationCorrectionMagnitudeToleranceG = 0.45f;
        [SerializeField] private HashiImuAxis imu1OpeningAxis = HashiImuAxis.X;
        [SerializeField] private HashiImuAxis imu2OpeningAxis = HashiImuAxis.X;
        [SerializeField] private float imu1OpeningDirection = 1f;
        [SerializeField] private float imu2OpeningDirection = 1f;
        [SerializeField] private float imu1OpeningScale = 1f;
        [SerializeField] private float imu2OpeningScale = 1f;
        [SerializeField] private float openingAngleMinDegrees = -55f;
        [SerializeField] private float openingAngleMaxDegrees = 55f;
        [SerializeField] private float openingAngleStaticJumpRejectDegrees = 12f;
        [SerializeField] private float openingAngleStaticVelocityThresholdDps = 20f;
        [SerializeField] private float maxBleSampleAgeSeconds = 0.6f;
        [SerializeField] private float bleResumeSettleSeconds = 0.2f;
        [SerializeField] private float chopstickOpeningSeparationPerDegree = 0.0015f;
        [SerializeField] private float maxChopstickOpeningSeparation = 0.08f;
        [SerializeField] private Vector3 chopstickSeparationAxis = Vector3.right;
        [SerializeField] private bool useHardwarePosition = true;
        [SerializeField] private bool autoCapturePositionNeutral = true;
        [SerializeField] private HashiPositionTrackingMode positionTrackingMode = HashiPositionTrackingMode.AccelerationDirect;
        [SerializeField] private HashiImuMotionSource positionMotionSource = HashiImuMotionSource.Average;
        [SerializeField] private Vector3 positionInputEuler = Vector3.zero;
        [SerializeField] private Vector3 positionAxisSigns = Vector3.one;
        [SerializeField] private Vector3 positionAccelerationScale = new Vector3(0.55f, 0.4f, 0.4f);
        [SerializeField] private Vector3 positionMaxOffset = new Vector3(0.28f, 0.22f, 0.22f);
        [SerializeField] private float positionDeadzoneG = 0.08f;
        [SerializeField] private float positionVelocityDamping = 5f;
        [SerializeField] private float positionReturnToNeutral = 1.5f;
        [SerializeField] private float positionSampleGapResetSeconds = 0.7f;
        [SerializeField] private float chopstickVisualLengthScale = 0.88f;
        [SerializeField] private Vector3 hashiDisplayAlignmentEuler = new Vector3(0f, -90f, 135f);
        [SerializeField] private Vector3 hashiDisplayOffsetWorld = new Vector3(1f, 0f, 0f);
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
        [SerializeField] private Vector3 imu1InputEuler = Vector3.zero;
        [SerializeField] private Vector3 imu2InputEuler = Vector3.zero;
        [SerializeField] private Vector3 imu1GyroSigns = Vector3.one;
        [SerializeField] private Vector3 imu2GyroSigns = Vector3.one;
        [SerializeField] private Vector3 imu1GyroAxisScale = Vector3.one;
        [SerializeField] private Vector3 imu2GyroAxisScale = Vector3.one;
        [SerializeField] private Vector3 imu1AccelerationSigns = Vector3.one;
        [SerializeField] private Vector3 imu2AccelerationSigns = Vector3.one;
        [SerializeField] private Vector3 upperOutputEuler = Vector3.zero;
        [SerializeField] private Vector3 lowerOutputEuler = Vector3.zero;
        [SerializeField] private bool showAdvancedControls;

        private DiagnosticSnapshot imu1GravitySnapshot;
        private DiagnosticSnapshot imu2GravitySnapshot;
        private DiagnosticSnapshot imu1MotionSnapshot;
        private DiagnosticSnapshot imu2MotionSnapshot;
        private Rect windowRect = new Rect(12f, 12f, 520f, 620f);
        private Vector2 scrollPosition;
        private HashiInputProvider provider;
        private string diagnosticStatus = "No independent IMU diagnostic records yet.";
        private float nextRuntimeApplyAt;

        private void OnEnable()
        {
            if (applyProfileOnEnable)
            {
                ApplyToRuntime();
            }
        }

        private void Update()
        {
            if (!visible || Time.unscaledTime < nextRuntimeApplyAt)
            {
                return;
            }

            nextRuntimeApplyAt = Time.unscaledTime + 0.1f;
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

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            var frame = provider.CurrentFrame;
            GUILayout.Label($"Provider: {provider.name}");
            GUILayout.Label($"BLE: {provider.DebugSourceStatus}");
            GUILayout.Label($"Frame: sample={frame.HasSample} theta={frame.ThetaDegrees:0.0} acc={FormatVector(frame.Acceleration)} gyro={FormatVector(frame.GyroDegreesPerSecond)}");

            var hasDualSample = provider.TryGetLatestBleSample(out var sample) && sample.HasDualImu;
            if (hasDualSample)
            {
                DrawSample(sample);
            }
            else
            {
                GUILayout.Label("Latest dual IMU sample: none");
            }

            DrawIndependentDiagnostics(hasDualSample, sample);

            GUILayout.Space(8f);
            showAdvancedControls = GUILayout.Toggle(showAdvancedControls, "Show advanced sliders");
            if (!showAdvancedControls)
            {
                GUILayout.EndScrollView();
                ApplyToRuntime();
                GUI.DragWindow();
                return;
            }

            GUILayout.Space(8f);
            GUILayout.Label("Independent IMU pose");
            useIndependentImuPose = GUILayout.Toggle(useIndependentImuPose, "Use IMU1 upper / IMU2 lower");
            autoCaptureIndependentNeutral = GUILayout.Toggle(autoCaptureIndependentNeutral, "Auto neutral from first dual sample");
            independentPoseMode = DrawEnumCycle("Pose mode", independentPoseMode);
            independentPoseWeight = DrawSlider("Independent pose weight", independentPoseWeight, 0f, 1f);
            gyroDeadzoneDegreesPerSecond = DrawSlider("Gyro deadzone dps", gyroDeadzoneDegreesPerSecond, 0f, 20f);
            GUILayout.Label("Opening angle extraction");
            imu1OpeningAxis = DrawEnumCycle("IMU1 opening sensor axis", imu1OpeningAxis);
            imu2OpeningAxis = DrawEnumCycle("IMU2 opening sensor axis", imu2OpeningAxis);
            GUILayout.BeginHorizontal();
            imu1OpeningDirection = DrawSignToggle("IMU1 dir", imu1OpeningDirection);
            imu2OpeningDirection = DrawSignToggle("IMU2 dir", imu2OpeningDirection);
            GUILayout.EndHorizontal();
            imu1OpeningScale = DrawSlider("IMU1 opening scale", imu1OpeningScale, 0f, 3f);
            imu2OpeningScale = DrawSlider("IMU2 opening scale", imu2OpeningScale, 0f, 3f);
            openingAngleMinDegrees = DrawSlider("Opening min deg", openingAngleMinDegrees, -90f, 0f);
            openingAngleMaxDegrees = DrawSlider("Opening max deg", openingAngleMaxDegrees, 0f, 90f);
            openingAngleStaticJumpRejectDegrees = DrawSlider("Static jump reject deg", openingAngleStaticJumpRejectDegrees, 0f, 45f);
            openingAngleStaticVelocityThresholdDps = DrawSlider("Static jump max dps", openingAngleStaticVelocityThresholdDps, 0f, 80f);
            maxBleSampleAgeSeconds = DrawSlider("Max BLE sample age sec", maxBleSampleAgeSeconds, 0f, 2f);
            bleResumeSettleSeconds = DrawSlider("BLE resume settle sec", bleResumeSettleSeconds, 0f, 1f);
            chopstickOpeningSeparationPerDegree = DrawSlider("Opening separation per deg", chopstickOpeningSeparationPerDegree, 0f, 0.006f);
            maxChopstickOpeningSeparation = DrawSlider("Max opening separation", maxChopstickOpeningSeparation, 0f, 0.2f);
            chopstickSeparationAxis = DrawVectorSlider("Separation axis", chopstickSeparationAxis, -1f, 1f);
            GUILayout.Space(4f);
            GUILayout.Label("Hardware position");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Safe bounded movement"))
            {
                ApplySafeMovementProfile();
            }

            if (GUILayout.Button("Disable movement"))
            {
                useHardwarePosition = false;
                HashiImuMappingRuntime.ClearHardwarePosition();
            }
            GUILayout.EndHorizontal();
            useHardwarePosition = GUILayout.Toggle(useHardwarePosition, "Use BLE IMU position offset");
            autoCapturePositionNeutral = GUILayout.Toggle(autoCapturePositionNeutral, "Auto position neutral from first dual sample");
            positionTrackingMode = DrawEnumCycle("Position mode", positionTrackingMode);
            positionMotionSource = DrawEnumCycle("Position source", positionMotionSource);
            positionInputEuler = DrawEulerControls("Position input basis", positionInputEuler);
            GUILayout.Label("Position axis signs");
            GUILayout.BeginHorizontal();
            positionAxisSigns.x = DrawSignToggle("PX", positionAxisSigns.x);
            positionAxisSigns.y = DrawSignToggle("PY", positionAxisSigns.y);
            positionAxisSigns.z = DrawSignToggle("PZ", positionAxisSigns.z);
            GUILayout.EndHorizontal();
            positionAccelerationScale = DrawVectorSlider("Position accel scale", positionAccelerationScale, 0f, 20f);
            positionMaxOffset = DrawVectorSlider("Position max offset", positionMaxOffset, 0f, 4f);
            positionDeadzoneG = DrawSlider("Position deadzone G", positionDeadzoneG, 0f, 0.5f);
            positionVelocityDamping = DrawSlider("Position velocity damping", positionVelocityDamping, 0f, 20f);
            positionReturnToNeutral = DrawSlider("Position return to neutral", positionReturnToNeutral, 0f, 3f);
            positionSampleGapResetSeconds = DrawSlider("Position gap reset sec", positionSampleGapResetSeconds, 0f, 3f);
            chopstickVisualLengthScale = DrawSlider("Chopstick visual length", chopstickVisualLengthScale, 0.55f, 1f);
            hashiDisplayAlignmentEuler = DrawEulerControls("Hashi screen alignment", hashiDisplayAlignmentEuler);
            hashiDisplayOffsetWorld = DrawVectorSlider("Hashi display world offset", hashiDisplayOffsetWorld, -2f, 2f);
            if (GUILayout.Button("Set display: tips left / upper above"))
            {
                hashiDisplayAlignmentEuler = new Vector3(0f, -90f, 135f);
                hashiDisplayOffsetWorld = new Vector3(1f, 0f, 0f);
            }

            accelerationCorrectionWeight = DrawSlider("Acc correction weight", accelerationCorrectionWeight, 0f, 0.2f);
            accelerationCorrectionDeadzoneDegrees = DrawSlider("Acc correction deadzone deg", accelerationCorrectionDeadzoneDegrees, 0f, 20f);
            accelerationCorrectionMagnitudeToleranceG = DrawSlider("Acc 1G tolerance", accelerationCorrectionMagnitudeToleranceG, 0f, 1.5f);
            parentRotationWeight = DrawSlider("Whole Hashi rotation", parentRotationWeight, 0f, 1f);
            invertImu1Pose = GUILayout.Toggle(invertImu1Pose, "Invert IMU1 upper pose");
            invertImu2Pose = GUILayout.Toggle(invertImu2Pose, "Invert IMU2 lower pose");
            DrawAxisMap("IMU1 upper", ref imu1MapX, ref imu1MapY, ref imu1MapZ, ref imu1MapSigns);
            DrawAxisMap("IMU2 lower", ref imu2MapX, ref imu2MapY, ref imu2MapZ, ref imu2MapSigns);
            DrawDirectionControls(
                "IMU1 upper",
                ref imu1InputEuler,
                ref upperOutputEuler,
                ref imu1GyroSigns,
                ref imu1GyroAxisScale,
                ref imu1AccelerationSigns);
            DrawDirectionControls(
                "IMU2 lower",
                ref imu2InputEuler,
                ref lowerOutputEuler,
                ref imu2GyroSigns,
                ref imu2GyroAxisScale,
                ref imu2AccelerationSigns);

            GUILayout.BeginHorizontal();
            GUI.enabled = provider.TryGetLatestBleSample(out sample) && sample.HasDualImu;
            if (GUILayout.Button("Capture Both Neutrals"))
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

            GUILayout.EndScrollView();
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
            GUILayout.Label($"IMU1 gyro dps {FormatVector(HashiImuMappingRuntime.MapImu1Gyro(sample.Imu1GyroRaw))} acc G {HashiImuMappingRuntime.MapImu1Acceleration(sample.Imu1AccelerationRaw).magnitude * HashiImuMappingRuntime.RawAccelerationToG:0.00}");
            GUILayout.Label($"IMU2 gyro dps {FormatVector(HashiImuMappingRuntime.MapImu2Gyro(sample.Imu2GyroRaw))} acc G {HashiImuMappingRuntime.MapImu2Acceleration(sample.Imu2AccelerationRaw).magnitude * HashiImuMappingRuntime.RawAccelerationToG:0.00}");
            HashiImuMappingRuntime.TryReadOpeningVelocity(sample, HashiImuId.Imu1, out var imu1OpeningVelocity);
            HashiImuMappingRuntime.TryReadOpeningVelocity(sample, HashiImuId.Imu2, out var imu2OpeningVelocity);
            GUILayout.Label($"Opening velocity dps IMU1 {imu1OpeningVelocity:0.0} / IMU2 {imu2OpeningVelocity:0.0}");
        }

        private void DrawIndependentDiagnostics(bool hasDualSample, HashiBleSample sample)
        {
            GUILayout.Space(8f);
            GUILayout.Label("Independent IMU diagnostics");
            GUILayout.Label(diagnosticStatus);
            GUILayout.Label("AccelerationOpening uses gravity tilt from neutral; it should stay valid after the motion stops.");
            GUILayout.Label("Current logs suggest both IMUs see gravity near +Z while closed, and upper opening tilts around IMU +X.");
            if (!hasDualSample)
            {
                GUILayout.Label("Waiting for dual IMU sample; diagnostic buttons are disabled.");
            }
            else
            {
                DrawImuDiagnosticReadout("IMU1 upper", sample.Imu1AccelerationRaw, sample.Imu1GyroRaw, true);
                DrawImuDiagnosticReadout("IMU2 lower", sample.Imu2AccelerationRaw, sample.Imu2GyroRaw, false);
                DrawOpeningAxisReadout(sample);
            }
            DrawHardwarePositionReadout();

            GUI.enabled = hasDualSample;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture IMU1 neutral"))
            {
                ApplyToRuntime();
                diagnosticStatus = HashiImuMappingRuntime.CaptureImu1Neutral(sample)
                    ? $"IMU1 neutral captured seq {sample.Sequence}."
                    : "IMU1 neutral capture failed.";
            }

            if (GUILayout.Button("Capture IMU2 neutral"))
            {
                ApplyToRuntime();
                diagnosticStatus = HashiImuMappingRuntime.CaptureImu2Neutral(sample)
                    ? $"IMU2 neutral captured seq {sample.Sequence}."
                    : "IMU2 neutral capture failed.";
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Zero IMU1 opening"))
            {
                ApplyToRuntime();
                diagnosticStatus = HashiImuMappingRuntime.ZeroImu1IntegratedPose(sample)
                    ? $"IMU1 opening zeroed and neutral captured seq {sample.Sequence}."
                    : "IMU1 gyro zero failed.";
            }

            if (GUILayout.Button("Zero IMU2 opening"))
            {
                ApplyToRuntime();
                diagnosticStatus = HashiImuMappingRuntime.ZeroImu2IntegratedPose(sample)
                    ? $"IMU2 opening zeroed and neutral captured seq {sample.Sequence}."
                    : "IMU2 gyro zero failed.";
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Zero Both Openings"))
            {
                ApplyToRuntime();
                var imu1Ok = HashiImuMappingRuntime.ZeroImu1IntegratedPose(sample);
                var imu2Ok = HashiImuMappingRuntime.ZeroImu2IntegratedPose(sample);
                diagnosticStatus = imu1Ok && imu2Ok
                    ? $"Both openings zeroed and neutrals captured seq {sample.Sequence}."
                    : "Opening zero failed.";
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = hasDualSample;
            if (GUILayout.Button("Zero Hardware Position"))
            {
                ApplyToRuntime();
                diagnosticStatus = HashiImuMappingRuntime.ZeroHardwarePosition(sample)
                    ? $"Hardware position zeroed seq {sample.Sequence}."
                    : "Hardware position zero failed.";
            }

            GUI.enabled = true;
            if (GUILayout.Button("Clear Hardware Position"))
            {
                HashiImuMappingRuntime.ClearHardwarePosition();
                diagnosticStatus = "Hardware position cleared.";
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Copy/Print movement report"))
            {
                LogMovementReport(hasDualSample, sample);
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = hasDualSample;
            if (GUILayout.Button("Record IMU1 gravity"))
            {
                imu1GravitySnapshot = DiagnosticSnapshot.FromSample("IMU1 gravity", HashiImuId.Imu1, sample);
                diagnosticStatus = $"IMU1 gravity recorded seq {sample.Sequence}.";
                Debug.Log(BuildDiagnosticRecordLog(imu1GravitySnapshot));
            }

            if (GUILayout.Button("Record IMU2 gravity"))
            {
                imu2GravitySnapshot = DiagnosticSnapshot.FromSample("IMU2 gravity", HashiImuId.Imu2, sample);
                diagnosticStatus = $"IMU2 gravity recorded seq {sample.Sequence}.";
                Debug.Log(BuildDiagnosticRecordLog(imu2GravitySnapshot));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Record IMU1 motion"))
            {
                imu1MotionSnapshot = DiagnosticSnapshot.FromSample("IMU1 motion", HashiImuId.Imu1, sample);
                diagnosticStatus = $"IMU1 motion recorded seq {sample.Sequence}.";
                Debug.Log(BuildDiagnosticRecordLog(imu1MotionSnapshot));
            }

            if (GUILayout.Button("Record IMU2 motion"))
            {
                imu2MotionSnapshot = DiagnosticSnapshot.FromSample("IMU2 motion", HashiImuId.Imu2, sample);
                diagnosticStatus = $"IMU2 motion recorded seq {sample.Sequence}.";
                Debug.Log(BuildDiagnosticRecordLog(imu2MotionSnapshot));
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUI.enabled = hasDualSample
                || HasAnyDiagnosticSnapshot()
                || HashiImuMappingRuntime.HasImu1Neutral
                || HashiImuMappingRuntime.HasImu2Neutral;
            if (GUILayout.Button("Copy/Print independent IMU report"))
            {
                LogIndependentImuReport(hasDualSample, sample);
            }

            GUI.enabled = true;
            if (GUILayout.Button("Clear diagnostic records"))
            {
                ClearDiagnosticRecords();
            }

            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void DrawImuDiagnosticReadout(string label, Vector3 rawAcceleration, Vector3 rawGyro, bool isImu1)
        {
            var mappedAcceleration = isImu1
                ? HashiImuMappingRuntime.MapImu1Acceleration(rawAcceleration)
                : HashiImuMappingRuntime.MapImu2Acceleration(rawAcceleration);
            var mappedGyro = isImu1
                ? HashiImuMappingRuntime.MapImu1Gyro(rawGyro)
                : HashiImuMappingRuntime.MapImu2Gyro(rawGyro);
            var state = isImu1
                ? HashiImuMappingRuntime.Imu1PoseDebugState
                : HashiImuMappingRuntime.Imu2PoseDebugState;

            GUILayout.Label(
                $"{label}: acc {FormatVectorPrecise(mappedAcceleration.normalized)} dominant={DominantAxis(mappedAcceleration)} "
                + $"gyro {FormatVectorPrecise(mappedGyro)} neutral={(state.HasNeutral ? "yes" : "no")} zeroSeq={state.ZeroSequence}");
        }

        private void DrawOpeningAxisReadout(HashiBleSample sample)
        {
            HashiImuMappingRuntime.TryReadOpeningVelocity(sample, HashiImuId.Imu1, out var imu1Velocity);
            HashiImuMappingRuntime.TryReadOpeningVelocity(sample, HashiImuId.Imu2, out var imu2Velocity);
            var imu1State = HashiImuMappingRuntime.Imu1PoseDebugState;
            var imu2State = HashiImuMappingRuntime.Imu2PoseDebugState;
            GUILayout.Label(
                $"Opening angles: IMU1 upper={imu1State.OpeningAngleDegrees:0.0}deg ({imu1Velocity:0.0}dps), "
                + $"IMU2 lower={imu2State.OpeningAngleDegrees:0.0}deg ({imu2Velocity:0.0}dps)");
        }

        private static void DrawHardwarePositionReadout()
        {
            var state = HashiImuMappingRuntime.PositionDebugState;
            GUILayout.Label(
                $"Hardware position: offset={FormatVectorPrecise(state.Offset)} velocity={FormatVectorPrecise(state.Velocity)} "
                + $"linearG={FormatVectorPrecise(state.LinearAccelerationG)} neutral={(state.HasNeutral ? "yes" : "no")}");
        }

        private void ClearDiagnosticRecords()
        {
            imu1GravitySnapshot = default(DiagnosticSnapshot);
            imu2GravitySnapshot = default(DiagnosticSnapshot);
            imu1MotionSnapshot = default(DiagnosticSnapshot);
            imu2MotionSnapshot = default(DiagnosticSnapshot);
            diagnosticStatus = "No independent IMU diagnostic records yet.";
            Debug.Log("[HashiImuDiagnostic] cleared independent IMU records.");
        }

        private bool HasAnyDiagnosticSnapshot()
        {
            return imu1GravitySnapshot.HasSample
                || imu2GravitySnapshot.HasSample
                || imu1MotionSnapshot.HasSample
                || imu2MotionSnapshot.HasSample;
        }

        private void ApplySafeMovementProfile()
        {
            useHardwarePosition = true;
            autoCapturePositionNeutral = true;
            positionTrackingMode = HashiPositionTrackingMode.AccelerationDirect;
            positionMotionSource = HashiImuMotionSource.Average;
            positionInputEuler = Vector3.zero;
            positionAxisSigns = Vector3.one;
            positionAccelerationScale = new Vector3(0.55f, 0.4f, 0.4f);
            positionMaxOffset = new Vector3(0.28f, 0.22f, 0.22f);
            positionDeadzoneG = 0.08f;
            positionVelocityDamping = 5f;
            positionReturnToNeutral = 1.5f;
            positionSampleGapResetSeconds = 0.7f;
            HashiImuMappingRuntime.ClearHardwarePosition();
            diagnosticStatus = "Safe bounded movement profile applied. Zero Hardware Position before testing.";
        }

        private void LogIndependentImuReport(bool hasDualSample, HashiBleSample sample)
        {
            var builder = new StringBuilder(4096);
            builder.AppendLine("[HashiIndependentImu BEGIN]");
            builder.AppendLine("AccelerationOpening mode ignores free 3D pose and maps gravity tilt from the neutral pose into the chopstick opening axis.");
            builder.AppendLine("Current expected mounting: both IMUs see gravity near +Z while closed; recent logs show upper opening mainly rotates around IMU +X.");
            builder.AppendLine("Gyro is logged as velocity only; the displayed opening angle comes from acceleration, so it remains available after motion stops.");
            builder.AppendLine();
            AppendRuntimeState(builder);

            if (hasDualSample)
            {
                builder.AppendLine();
                builder.AppendLine("currentSample:");
                AppendLiveImuState(builder, "IMU1 upper", HashiImuId.Imu1, sample);
                AppendLiveImuState(builder, "IMU2 lower", HashiImuId.Imu2, sample);
            }

            builder.AppendLine();
            AppendDiagnosticSnapshot(builder, imu1GravitySnapshot);
            AppendDiagnosticSnapshot(builder, imu2GravitySnapshot);
            AppendDiagnosticSnapshot(builder, imu1MotionSnapshot);
            AppendDiagnosticSnapshot(builder, imu2MotionSnapshot);
            builder.AppendLine("[HashiIndependentImu END]");

            var report = builder.ToString();
            GUIUtility.systemCopyBuffer = report;
            diagnosticStatus = "Independent IMU report copied and printed.";
            Debug.Log(report);
        }

        private void LogMovementReport(bool hasDualSample, HashiBleSample sample)
        {
            var builder = new StringBuilder(2048);
            builder.AppendLine("[HashiMovement BEGIN]");
            builder.AppendLine("6-axis IMU movement is bounded. Direct mode maps acceleration relative to neutral into a clamped offset; integrated mode is drift-prone and only for experiments.");
            builder.AppendLine();
            AppendRuntimeState(builder);

            if (provider != null)
            {
                var frame = provider.CurrentFrame;
                builder.AppendLine();
                builder.AppendLine("frame:");
                builder.AppendLine($"  localPosition={FormatVectorPrecise(frame.LocalPosition)} localRotationEuler={FormatVectorPrecise(frame.LocalRotation.eulerAngles)} hasSample={frame.HasSample} timeReceived={frame.TimeReceived:0.###}");
            }

            if (hasDualSample)
            {
                builder.AppendLine();
                builder.AppendLine("sample:");
                AppendLiveImuState(builder, "IMU1 upper", HashiImuId.Imu1, sample);
                AppendLiveImuState(builder, "IMU2 lower", HashiImuId.Imu2, sample);
            }

            builder.AppendLine("[HashiMovement END]");
            var report = builder.ToString();
            GUIUtility.systemCopyBuffer = report;
            diagnosticStatus = "Movement report copied and printed.";
            Debug.Log(report);
        }

        private void AppendRuntimeState(StringBuilder builder)
        {
            var imu1State = HashiImuMappingRuntime.Imu1PoseDebugState;
            var imu2State = HashiImuMappingRuntime.Imu2PoseDebugState;
            var positionState = HashiImuMappingRuntime.PositionDebugState;
            builder.AppendLine("runtimeState:");
            builder.AppendLine($"  IMU1 neutral={imu1State.HasNeutral} neutralAcc={FormatVectorPrecise(imu1State.NeutralAcceleration)} integrated={imu1State.HasIntegratedPose} zeroSeq={imu1State.ZeroSequence}");
            builder.AppendLine($"  IMU2 neutral={imu2State.HasNeutral} neutralAcc={FormatVectorPrecise(imu2State.NeutralAcceleration)} integrated={imu2State.HasIntegratedPose} zeroSeq={imu2State.ZeroSequence}");
            builder.AppendLine($"  IMU1 openingAngle={imu1State.OpeningAngleDegrees:0.###}deg openingVelocity={imu1State.OpeningVelocityDegreesPerSecond:0.###}dps");
            builder.AppendLine($"  IMU2 openingAngle={imu2State.OpeningAngleDegrees:0.###}deg openingVelocity={imu2State.OpeningVelocityDegreesPerSecond:0.###}dps");
            builder.AppendLine($"  HardwarePosition neutral={positionState.HasNeutral} offset={FormatVectorPrecise(positionState.Offset)} velocity={FormatVectorPrecise(positionState.Velocity)} linearG={FormatVectorPrecise(positionState.LinearAccelerationG)}");
            builder.AppendLine($"  AutoCaptureIndependentNeutral={autoCaptureIndependentNeutral}");
            builder.AppendLine($"  IndependentPoseMode={independentPoseMode}");
            builder.AppendLine($"  IndependentPoseWeight={independentPoseWeight:0.###}");
            builder.AppendLine($"  GyroDeadzoneDegreesPerSecond={gyroDeadzoneDegreesPerSecond:0.###}");
            builder.AppendLine($"  OpeningSensorAxis imu1={imu1OpeningAxis} dir={imu1OpeningDirection:0.#} scale={imu1OpeningScale:0.###} imu2={imu2OpeningAxis} dir={imu2OpeningDirection:0.#} scale={imu2OpeningScale:0.###} clamp=({openingAngleMinDegrees:0.###},{openingAngleMaxDegrees:0.###})");
            builder.AppendLine($"  OpeningNoiseReject jumpDeg={openingAngleStaticJumpRejectDegrees:0.###} staticVelocityDps={openingAngleStaticVelocityThresholdDps:0.###} maxSampleAge={maxBleSampleAgeSeconds:0.###} resumeSettle={bleResumeSettleSeconds:0.###}");
            builder.AppendLine($"  ChopstickSeparation perDeg={chopstickOpeningSeparationPerDegree:0.#####} max={maxChopstickOpeningSeparation:0.###} axis={FormatVectorPrecise(chopstickSeparationAxis)}");
            builder.AppendLine($"  PositionTracking use={useHardwarePosition} autoNeutral={autoCapturePositionNeutral} mode={positionTrackingMode} source={positionMotionSource} inputEuler={FormatVectorPrecise(positionInputEuler)} signs={FormatVectorPrecise(positionAxisSigns)} scale={FormatVectorPrecise(positionAccelerationScale)} maxOffset={FormatVectorPrecise(positionMaxOffset)} deadzoneG={positionDeadzoneG:0.###} damping={positionVelocityDamping:0.###} return={positionReturnToNeutral:0.###} gapReset={positionSampleGapResetSeconds:0.###}");
            builder.AppendLine($"  ChopstickVisualLengthScale={chopstickVisualLengthScale:0.###}");
            builder.AppendLine($"  HashiDisplayAlignmentEuler={FormatVectorPrecise(hashiDisplayAlignmentEuler)}");
            builder.AppendLine($"  HashiDisplayOffsetWorld={FormatVectorPrecise(hashiDisplayOffsetWorld)}");
            builder.AppendLine($"  AccelerationCorrectionWeight={accelerationCorrectionWeight:0.###}");
            builder.AppendLine($"  IMU1 map=({imu1MapX},{imu1MapY},{imu1MapZ}) signs={FormatVectorPrecise(imu1MapSigns)} inputEuler={FormatVectorPrecise(imu1InputEuler)} gyroSigns={FormatVectorPrecise(imu1GyroSigns)} gyroScale={FormatVectorPrecise(imu1GyroAxisScale)} accSigns={FormatVectorPrecise(imu1AccelerationSigns)} outputEuler={FormatVectorPrecise(upperOutputEuler)} invert={invertImu1Pose}");
            builder.AppendLine($"  IMU2 map=({imu2MapX},{imu2MapY},{imu2MapZ}) signs={FormatVectorPrecise(imu2MapSigns)} inputEuler={FormatVectorPrecise(imu2InputEuler)} gyroSigns={FormatVectorPrecise(imu2GyroSigns)} gyroScale={FormatVectorPrecise(imu2GyroAxisScale)} accSigns={FormatVectorPrecise(imu2AccelerationSigns)} outputEuler={FormatVectorPrecise(lowerOutputEuler)} invert={invertImu2Pose}");
        }

        private void AppendLiveImuState(StringBuilder builder, string label, HashiImuId imuId, HashiBleSample sample)
        {
            var rawAcceleration = imuId == HashiImuId.Imu1 ? sample.Imu1AccelerationRaw : sample.Imu2AccelerationRaw;
            var rawGyro = imuId == HashiImuId.Imu1 ? sample.Imu1GyroRaw : sample.Imu2GyroRaw;
            var mappedAcceleration = imuId == HashiImuId.Imu1
                ? HashiImuMappingRuntime.MapImu1Acceleration(rawAcceleration)
                : HashiImuMappingRuntime.MapImu2Acceleration(rawAcceleration);
            var mappedGyro = imuId == HashiImuId.Imu1
                ? HashiImuMappingRuntime.MapImu1Gyro(rawGyro)
                : HashiImuMappingRuntime.MapImu2Gyro(rawGyro);
            HashiImuMappingRuntime.TryReadOpeningVelocity(sample, imuId, out var openingVelocity);

            builder.AppendLine($"  {label}: seq={sample.Sequence}");
            builder.AppendLine($"    rawAcc={FormatVectorPrecise(rawAcceleration)} mappedAcc={FormatVectorPrecise(mappedAcceleration)} norm={FormatVectorPrecise(mappedAcceleration.normalized)} dominant={DominantAxis(mappedAcceleration)} magnitudeG={mappedAcceleration.magnitude * HashiImuMappingRuntime.RawAccelerationToG:0.###}");
            builder.AppendLine($"    rawGyro={FormatVectorPrecise(rawGyro)} mappedGyroDps={FormatVectorPrecise(mappedGyro)} dominantGyro={DominantAxis(mappedGyro)}");
            builder.AppendLine($"    openingVelocityDps={openingVelocity:0.###}");
        }

        private void AppendDiagnosticSnapshot(StringBuilder builder, DiagnosticSnapshot snapshot)
        {
            if (!snapshot.HasSample)
            {
                return;
            }

            builder.AppendLine($"{snapshot.Label}: seq={snapshot.Sequence}");
            builder.AppendLine($"  imu={snapshot.ImuId}");
            builder.AppendLine($"  rawAcc={FormatVectorPrecise(snapshot.RawAcceleration)} mappedAcc={FormatVectorPrecise(snapshot.MappedAcceleration)} norm={FormatVectorPrecise(snapshot.MappedAcceleration.normalized)} dominant={DominantAxis(snapshot.MappedAcceleration)} magnitudeG={snapshot.MappedAcceleration.magnitude * HashiImuMappingRuntime.RawAccelerationToG:0.###}");
            builder.AppendLine($"  rawGyro={FormatVectorPrecise(snapshot.RawGyro)} mappedGyroDps={FormatVectorPrecise(snapshot.MappedGyro)} dominantGyro={DominantAxis(snapshot.MappedGyro)}");
            builder.AppendLine($"  openingVelocityDps={snapshot.OpeningVelocityDegreesPerSecond:0.###}");
        }

        private string BuildDiagnosticRecordLog(DiagnosticSnapshot snapshot)
        {
            var builder = new StringBuilder(512);
            builder.AppendLine($"[HashiImuDiagnostic] {snapshot.Label} seq={snapshot.Sequence} imu={snapshot.ImuId}");
            builder.AppendLine($"mappedAcc={FormatVectorPrecise(snapshot.MappedAcceleration)} norm={FormatVectorPrecise(snapshot.MappedAcceleration.normalized)} dominant={DominantAxis(snapshot.MappedAcceleration)}");
            builder.AppendLine($"mappedGyroDps={FormatVectorPrecise(snapshot.MappedGyro)} dominantGyro={DominantAxis(snapshot.MappedGyro)}");
            builder.AppendLine($"openingVelocityDps={snapshot.OpeningVelocityDegreesPerSecond:0.###}");
            return builder.ToString();
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
            HashiImuMappingRuntime.IndependentPoseMode = independentPoseMode;
            HashiImuMappingRuntime.IndependentPoseWeight = independentPoseWeight;
            HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond = gyroDeadzoneDegreesPerSecond;
            HashiImuMappingRuntime.AccelerationCorrectionWeight = accelerationCorrectionWeight;
            HashiImuMappingRuntime.AccelerationCorrectionDeadzoneDegrees = accelerationCorrectionDeadzoneDegrees;
            HashiImuMappingRuntime.AccelerationCorrectionMagnitudeToleranceG = accelerationCorrectionMagnitudeToleranceG;
            HashiImuMappingRuntime.Imu1OpeningAxis = imu1OpeningAxis;
            HashiImuMappingRuntime.Imu2OpeningAxis = imu2OpeningAxis;
            HashiImuMappingRuntime.Imu1OpeningDirection = imu1OpeningDirection;
            HashiImuMappingRuntime.Imu2OpeningDirection = imu2OpeningDirection;
            HashiImuMappingRuntime.Imu1OpeningScale = imu1OpeningScale;
            HashiImuMappingRuntime.Imu2OpeningScale = imu2OpeningScale;
            HashiImuMappingRuntime.OpeningAngleMinDegrees = openingAngleMinDegrees;
            HashiImuMappingRuntime.OpeningAngleMaxDegrees = openingAngleMaxDegrees;
            HashiImuMappingRuntime.OpeningAngleStaticJumpRejectDegrees = openingAngleStaticJumpRejectDegrees;
            HashiImuMappingRuntime.OpeningAngleStaticVelocityThresholdDps = openingAngleStaticVelocityThresholdDps;
            HashiImuMappingRuntime.MaxBleSampleAgeSeconds = maxBleSampleAgeSeconds;
            HashiImuMappingRuntime.BleResumeSettleSeconds = bleResumeSettleSeconds;
            HashiImuMappingRuntime.ChopstickOpeningSeparationPerDegree = chopstickOpeningSeparationPerDegree;
            HashiImuMappingRuntime.MaxChopstickOpeningSeparation = maxChopstickOpeningSeparation;
            HashiImuMappingRuntime.ChopstickSeparationAxis = chopstickSeparationAxis;
            HashiImuMappingRuntime.UseHardwarePosition = useHardwarePosition;
            HashiImuMappingRuntime.AutoCapturePositionNeutral = autoCapturePositionNeutral;
            HashiImuMappingRuntime.PositionTrackingMode = positionTrackingMode;
            HashiImuMappingRuntime.PositionMotionSource = positionMotionSource;
            HashiImuMappingRuntime.PositionInputEuler = positionInputEuler;
            HashiImuMappingRuntime.PositionAxisSigns = positionAxisSigns;
            HashiImuMappingRuntime.PositionAccelerationScale = positionAccelerationScale;
            HashiImuMappingRuntime.PositionMaxOffset = positionMaxOffset;
            HashiImuMappingRuntime.PositionDeadzoneG = positionDeadzoneG;
            HashiImuMappingRuntime.PositionVelocityDamping = positionVelocityDamping;
            HashiImuMappingRuntime.PositionReturnToNeutral = positionReturnToNeutral;
            HashiImuMappingRuntime.PositionSampleGapResetSeconds = positionSampleGapResetSeconds;
            HashiImuMappingRuntime.ChopstickVisualLengthScale = chopstickVisualLengthScale;
            HashiImuMappingRuntime.HashiDisplayAlignmentEuler = hashiDisplayAlignmentEuler;
            HashiImuMappingRuntime.HashiDisplayOffsetWorld = hashiDisplayOffsetWorld;
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
            HashiImuMappingRuntime.Imu1InputEuler = imu1InputEuler;
            HashiImuMappingRuntime.Imu2InputEuler = imu2InputEuler;
            HashiImuMappingRuntime.Imu1GyroSigns = imu1GyroSigns;
            HashiImuMappingRuntime.Imu2GyroSigns = imu2GyroSigns;
            HashiImuMappingRuntime.Imu1GyroAxisScale = imu1GyroAxisScale;
            HashiImuMappingRuntime.Imu2GyroAxisScale = imu2GyroAxisScale;
            HashiImuMappingRuntime.Imu1AccelerationSigns = imu1AccelerationSigns;
            HashiImuMappingRuntime.Imu2AccelerationSigns = imu2AccelerationSigns;
            HashiImuMappingRuntime.UpperOutputEuler = upperOutputEuler;
            HashiImuMappingRuntime.LowerOutputEuler = lowerOutputEuler;
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

        private static void DrawDirectionControls(
            string label,
            ref Vector3 inputEuler,
            ref Vector3 outputEuler,
            ref Vector3 gyroSigns,
            ref Vector3 gyroScale,
            ref Vector3 accelerationSigns)
        {
            GUILayout.Space(4f);
            GUILayout.Label($"{label} coordinate tuning");
            inputEuler = DrawEulerControls("Sensor input basis", inputEuler);
            outputEuler = DrawEulerControls("Chopstick output basis", outputEuler);

            GUILayout.Label("Gyro direction signs");
            GUILayout.BeginHorizontal();
            gyroSigns.x = DrawSignToggle("GX", gyroSigns.x);
            gyroSigns.y = DrawSignToggle("GY", gyroSigns.y);
            gyroSigns.z = DrawSignToggle("GZ", gyroSigns.z);
            GUILayout.EndHorizontal();

            gyroScale = DrawVectorSlider("Gyro axis scale", gyroScale, 0f, 3f);

            GUILayout.Label("Acceleration correction signs");
            GUILayout.BeginHorizontal();
            accelerationSigns.x = DrawSignToggle("AX", accelerationSigns.x);
            accelerationSigns.y = DrawSignToggle("AY", accelerationSigns.y);
            accelerationSigns.z = DrawSignToggle("AZ", accelerationSigns.z);
            GUILayout.EndHorizontal();

            if (GUILayout.Button($"Reset {label} direction"))
            {
                inputEuler = Vector3.zero;
                outputEuler = Vector3.zero;
                gyroSigns = Vector3.one;
                gyroScale = Vector3.one;
                accelerationSigns = Vector3.one;
            }
        }

        private static Vector3 DrawEulerControls(string label, Vector3 value)
        {
            GUILayout.Label($"{label}: ({value.x:0.#}, {value.y:0.#}, {value.z:0.#})");
            value = DrawVectorSlider("XYZ deg", value, -180f, 180f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X-90")) value.x -= 90f;
            if (GUILayout.Button("X+90")) value.x += 90f;
            if (GUILayout.Button("Y-90")) value.y -= 90f;
            if (GUILayout.Button("Y+90")) value.y += 90f;
            if (GUILayout.Button("Z-90")) value.z -= 90f;
            if (GUILayout.Button("Z+90")) value.z += 90f;
            if (GUILayout.Button("Reset")) value = Vector3.zero;
            GUILayout.EndHorizontal();

            return NormalizeEuler(value);
        }

        private static Vector3 DrawVectorSlider(string label, Vector3 value, float min, float max)
        {
            GUILayout.Label($"{label} X: {value.x:0.###}");
            value.x = GUILayout.HorizontalSlider(value.x, min, max);
            GUILayout.Label($"{label} Y: {value.y:0.###}");
            value.y = GUILayout.HorizontalSlider(value.y, min, max);
            GUILayout.Label($"{label} Z: {value.z:0.###}");
            value.z = GUILayout.HorizontalSlider(value.z, min, max);
            return value;
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

        private static Vector3 NormalizeEuler(Vector3 value)
        {
            value.x = NormalizeAngle(value.x);
            value.y = NormalizeAngle(value.y);
            value.z = NormalizeAngle(value.z);
            return value;
        }

        private static float NormalizeAngle(float value)
        {
            while (value > 180f)
            {
                value -= 360f;
            }

            while (value < -180f)
            {
                value += 360f;
            }

            return value;
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.##}, {value.y:0.##}, {value.z:0.##})";
        }

        private static string FormatVectorPrecise(Vector3 value)
        {
            return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
        }

        private static string DominantAxis(Vector3 value)
        {
            var absoluteX = Mathf.Abs(value.x);
            var absoluteY = Mathf.Abs(value.y);
            var absoluteZ = Mathf.Abs(value.z);

            if (absoluteX < 0.000001f && absoluteY < 0.000001f && absoluteZ < 0.000001f)
            {
                return "none";
            }

            if (absoluteX >= absoluteY && absoluteX >= absoluteZ)
            {
                return value.x >= 0f ? "+X" : "-X";
            }

            if (absoluteY >= absoluteX && absoluteY >= absoluteZ)
            {
                return value.y >= 0f ? "+Y" : "-Y";
            }

            return value.z >= 0f ? "+Z" : "-Z";
        }

        private struct DiagnosticSnapshot
        {
            public bool HasSample;
            public string Label;
            public HashiImuId ImuId;
            public int Sequence;
            public Vector3 RawAcceleration;
            public Vector3 RawGyro;
            public Vector3 MappedAcceleration;
            public Vector3 MappedGyro;
            public float OpeningVelocityDegreesPerSecond;

            public static DiagnosticSnapshot FromSample(string label, HashiImuId imuId, HashiBleSample sample)
            {
                var rawAcceleration = imuId == HashiImuId.Imu1
                    ? sample.Imu1AccelerationRaw
                    : sample.Imu2AccelerationRaw;
                var rawGyro = imuId == HashiImuId.Imu1
                    ? sample.Imu1GyroRaw
                    : sample.Imu2GyroRaw;
                var mappedAcceleration = imuId == HashiImuId.Imu1
                    ? HashiImuMappingRuntime.MapImu1Acceleration(rawAcceleration)
                    : HashiImuMappingRuntime.MapImu2Acceleration(rawAcceleration);
                var mappedGyro = imuId == HashiImuId.Imu1
                    ? HashiImuMappingRuntime.MapImu1Gyro(rawGyro)
                    : HashiImuMappingRuntime.MapImu2Gyro(rawGyro);
                HashiImuMappingRuntime.TryReadOpeningVelocity(sample, imuId, out var openingVelocity);

                return new DiagnosticSnapshot
                {
                    HasSample = true,
                    Label = label,
                    ImuId = imuId,
                    Sequence = sample.Sequence,
                    RawAcceleration = rawAcceleration,
                    RawGyro = rawGyro,
                    MappedAcceleration = mappedAcceleration,
                    MappedGyro = mappedGyro,
                    OpeningVelocityDegreesPerSecond = openingVelocity
                };
            }
        }
    }
}
