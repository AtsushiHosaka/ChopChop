using UnityEngine;

namespace ChopChop.BLE
{
    public enum HashiImuThetaMode
    {
        AccelerationAngle,
        Imu1Axis,
        Imu2Axis,
        AxisDifference
    }

    public enum HashiImuMotionSource
    {
        Imu1,
        Imu2,
        Average,
        Difference
    }

    public enum HashiImuAxis
    {
        X,
        Y,
        Z
    }

    public enum HashiIndependentPoseMode
    {
        AccelerationOpening,
        OpeningAxisIntegrated,
        GyroIntegrated,
        AccelerationRelative,
        RelativeOpeningIntegrated
    }

    public enum HashiPositionTrackingMode
    {
        AccelerationIntegrated,
        AccelerationDirect
    }

    public enum HashiImuId
    {
        Imu1,
        Imu2
    }

    public struct HashiImuPoseDebugState
    {
        public bool HasNeutral;
        public Vector3 NeutralAcceleration;
        public bool HasIntegratedPose;
        public Quaternion IntegratedRotation;
        public float OpeningAngleDegrees;
        public float OpeningVelocityDegreesPerSecond;
        public bool HasRelativeOpeningNeutral;
        public float RelativeOpeningNeutralAngleDegrees;
        public Quaternion RelativeOpeningLowerToUpperRotation;
        public bool HasRelativeOpeningAcceleration;
        public float RelativeOpeningAccelerationDegrees;
        public float RelativeOpeningGyroWeight;
        public float RelativeOpeningGyroMotionDegreesPerSecond;
        public float RelativeOpeningVelocityDominance;
        public bool RelativeOpeningAccelerationCorrectionAllowed;
        public int LastSampleSequence;
        public float LastSampleRealtime;
        public int ZeroSequence;
    }

    public struct HashiPositionDebugState
    {
        public bool HasNeutral;
        public Vector3 NeutralAccelerationG;
        public Vector3 LinearAccelerationG;
        public Vector3 Velocity;
        public Vector3 Offset;
        public int LastSampleSequence;
        public float LastSampleRealtime;
    }

    public static class HashiImuMappingRuntime
    {
        public const float Esp32Lsm6RawAccelerationToG = HashiBleDefaults.RawAccelerationToG;

        public static bool UseTunedTheta { get; set; }
        public static HashiImuThetaMode ThetaMode { get; set; } = HashiImuThetaMode.AccelerationAngle;
        public static HashiImuMotionSource MotionSource { get; set; } = HashiImuMotionSource.Imu2;
        public static HashiImuAxis Imu1Axis { get; set; } = HashiImuAxis.Z;
        public static HashiImuAxis Imu2Axis { get; set; } = HashiImuAxis.Z;
        public static bool InvertTheta { get; set; }
        public static float InputClosed { get; set; } = 0f;
        public static float InputOpen { get; set; } = 180f;
        public static float OutputClosed { get; set; } = 0f;
        public static float OutputOpen { get; set; } = 180f;
        public static float RawAccelerationToG { get; set; } = HashiBleDefaults.RawAccelerationToG;
        public static float RawGyroToDegreesPerSecond { get; set; } = HashiBleDefaults.RawGyroToDegreesPerSecond;
        public static float ParentRotationWeight { get; set; } = 0.6f;
        public static Vector3 ParentRotationInputEuler { get; set; } = new Vector3(-90f, -90f, 0f);
        public static bool ParentPlanarYawOnly { get; set; } = true;
        public static float ParentPlanarYawDirection { get; set; } = -1f;
        public static bool ParentVerticalPitchWhenDominant { get; set; } = true;
        public static float ParentVerticalPitchScale { get; set; } = 0.55f;
        public static float ParentLongAxisTwistWeight { get; set; } = 0f;
        public static bool UseIndependentImuPose { get; set; } = true;
        public static bool AutoCaptureIndependentNeutral { get; set; } = true;
        public static HashiIndependentPoseMode IndependentPoseMode { get; set; } = HashiIndependentPoseMode.RelativeOpeningIntegrated;
        public static float IndependentPoseWeight { get; set; } = 1f;
        public static float GyroDeadzoneDegreesPerSecond { get; set; } = 1.5f;
        public static float AccelerationCorrectionWeight { get; set; } = 0.18f;
        public static float AccelerationCorrectionDeadzoneDegrees { get; set; } = 2f;
        public static float AccelerationCorrectionMagnitudeToleranceG { get; set; } = 0.45f;
        public static HashiImuAxis Imu1OpeningAxis { get; set; } = HashiImuAxis.X;
        public static HashiImuAxis Imu2OpeningAxis { get; set; } = HashiImuAxis.X;
        public static float Imu1OpeningDirection { get; set; } = 1f;
        public static float Imu2OpeningDirection { get; set; } = 1f;
        public static float Imu1OpeningScale { get; set; } = 1f;
        public static float Imu2OpeningScale { get; set; } = 1f;
        public static float OpeningAngleMinDegrees { get; set; } = 0f;
        public static float OpeningAngleMaxDegrees { get; set; } = 35f;
        public static float OpeningAngleStaticJumpRejectDegrees { get; set; } = 12f;
        public static float OpeningAngleStaticVelocityThresholdDps { get; set; } = 20f;
        public static float RelativeOpeningAccelerationWeight { get; set; } = 0.35f;
        public static float RelativeOpeningGyroFullWeightBelowDps { get; set; } = 140f;
        public static float RelativeOpeningGyroZeroWeightAboveDps { get; set; } = 900f;
        public static float MaxBleSampleAgeSeconds { get; set; } = 0.6f;
        public static float BleResumeSettleSeconds { get; set; } = 0.2f;
        public static float ChopstickOpeningSeparationPerDegree { get; set; } = 0.0015f;
        public static float MaxChopstickOpeningSeparation { get; set; } = 0.08f;
        public static Vector3 ChopstickSeparationAxis { get; set; } = Vector3.right;
        public static bool UseHardwarePosition { get; set; } = true;
        public static bool AutoCapturePositionNeutral { get; set; } = true;
        public static HashiPositionTrackingMode PositionTrackingMode { get; set; } = HashiPositionTrackingMode.AccelerationDirect;
        public static HashiImuMotionSource PositionMotionSource { get; set; } = HashiImuMotionSource.Average;
        public static Vector3 PositionInputEuler { get; set; } = Vector3.zero;
        public static Vector3 PositionAxisSigns { get; set; } = Vector3.one;
        public static Vector3 PositionAccelerationScale { get; set; } = new Vector3(0.55f, 0.4f, 0.4f);
        public static Vector3 PositionMaxOffset { get; set; } = new Vector3(0.28f, 0.22f, 0.22f);
        public static float PositionDeadzoneG { get; set; } = 0.08f;
        public static float PositionVelocityDamping { get; set; } = 5f;
        public static float PositionReturnToNeutral { get; set; } = 1.5f;
        public static float PositionSampleGapResetSeconds { get; set; } = 0.7f;
        public static float ChopstickVisualLengthScale { get; set; } = 0.88f;
        public static Vector3 HashiDisplayAlignmentEuler { get; set; } = new Vector3(0f, -90f, 135f);
        public static Vector3 HashiDisplayOffsetWorld { get; set; } = new Vector3(1f, 0f, 0f);
        public static bool InvertImu1Pose { get; set; }
        public static bool InvertImu2Pose { get; set; }
        public static HashiImuAxis Imu1MapX { get; set; } = HashiImuAxis.X;
        public static HashiImuAxis Imu1MapY { get; set; } = HashiImuAxis.Y;
        public static HashiImuAxis Imu1MapZ { get; set; } = HashiImuAxis.Z;
        public static HashiImuAxis Imu2MapX { get; set; } = HashiImuAxis.X;
        public static HashiImuAxis Imu2MapY { get; set; } = HashiImuAxis.Y;
        public static HashiImuAxis Imu2MapZ { get; set; } = HashiImuAxis.Z;
        public static Vector3 Imu1MapSigns { get; set; } = Vector3.one;
        public static Vector3 Imu2MapSigns { get; set; } = Vector3.one;
        public static Vector3 Imu1InputEuler { get; set; } = Vector3.zero;
        public static Vector3 Imu2InputEuler { get; set; } = Vector3.zero;
        public static Vector3 Imu1GyroSigns { get; set; } = Vector3.one;
        public static Vector3 Imu2GyroSigns { get; set; } = Vector3.one;
        public static Vector3 Imu1GyroAxisScale { get; set; } = Vector3.one;
        public static Vector3 Imu2GyroAxisScale { get; set; } = Vector3.one;
        public static Vector3 Imu1AccelerationSigns { get; set; } = Vector3.one;
        public static Vector3 Imu2AccelerationSigns { get; set; } = Vector3.one;
        public static Vector3 UpperOutputEuler { get; set; } = Vector3.zero;
        public static Vector3 LowerOutputEuler { get; set; } = Vector3.zero;
        public static bool HasIndependentNeutral => HasImu1Neutral && HasImu2Neutral;
        public static bool HasImu1Neutral => imu1Pose.HasNeutral;
        public static bool HasImu2Neutral => imu2Pose.HasNeutral;
        public static HashiImuPoseDebugState Imu1PoseDebugState => BuildDebugState(imu1Pose);
        public static HashiImuPoseDebugState Imu2PoseDebugState => BuildDebugState(imu2Pose);
        public static HashiPositionDebugState PositionDebugState => BuildPositionDebugState(positionTracker);

        private static ImuPoseTracker imu1Pose = ImuPoseTracker.Create();
        private static ImuPoseTracker imu2Pose = ImuPoseTracker.Create();
        private static PositionTracker positionTracker = PositionTracker.Create();

        public static void ResetDefaults()
        {
            UseTunedTheta = false;
            ThetaMode = HashiImuThetaMode.AccelerationAngle;
            MotionSource = HashiImuMotionSource.Imu2;
            Imu1Axis = HashiImuAxis.Z;
            Imu2Axis = HashiImuAxis.Z;
            InvertTheta = false;
            InputClosed = 0f;
            InputOpen = 180f;
            OutputClosed = 0f;
            OutputOpen = 180f;
            RawAccelerationToG = HashiBleDefaults.RawAccelerationToG;
            RawGyroToDegreesPerSecond = HashiBleDefaults.RawGyroToDegreesPerSecond;
            ParentRotationWeight = 0.6f;
            ParentRotationInputEuler = new Vector3(-90f, -90f, 0f);
            ParentPlanarYawOnly = true;
            ParentPlanarYawDirection = -1f;
            ParentVerticalPitchWhenDominant = true;
            ParentVerticalPitchScale = 0.55f;
            ParentLongAxisTwistWeight = 0f;
            UseIndependentImuPose = true;
            AutoCaptureIndependentNeutral = true;
            IndependentPoseMode = HashiIndependentPoseMode.RelativeOpeningIntegrated;
            IndependentPoseWeight = 1f;
            GyroDeadzoneDegreesPerSecond = 1.5f;
            AccelerationCorrectionWeight = 0.18f;
            AccelerationCorrectionDeadzoneDegrees = 2f;
            AccelerationCorrectionMagnitudeToleranceG = 0.45f;
            Imu1OpeningAxis = HashiImuAxis.X;
            Imu2OpeningAxis = HashiImuAxis.X;
            Imu1OpeningDirection = 1f;
            Imu2OpeningDirection = 1f;
            Imu1OpeningScale = 1f;
            Imu2OpeningScale = 1f;
            OpeningAngleMinDegrees = 0f;
            OpeningAngleMaxDegrees = 35f;
            OpeningAngleStaticJumpRejectDegrees = 12f;
            OpeningAngleStaticVelocityThresholdDps = 20f;
            RelativeOpeningAccelerationWeight = 0.35f;
            RelativeOpeningGyroFullWeightBelowDps = 140f;
            RelativeOpeningGyroZeroWeightAboveDps = 900f;
            MaxBleSampleAgeSeconds = 0.6f;
            BleResumeSettleSeconds = 0.2f;
            ChopstickOpeningSeparationPerDegree = 0.0015f;
            MaxChopstickOpeningSeparation = 0.08f;
            ChopstickSeparationAxis = Vector3.right;
            UseHardwarePosition = true;
            AutoCapturePositionNeutral = true;
            PositionTrackingMode = HashiPositionTrackingMode.AccelerationDirect;
            PositionMotionSource = HashiImuMotionSource.Average;
            PositionInputEuler = Vector3.zero;
            PositionAxisSigns = Vector3.one;
            PositionAccelerationScale = new Vector3(0.55f, 0.4f, 0.4f);
            PositionMaxOffset = new Vector3(0.28f, 0.22f, 0.22f);
            PositionDeadzoneG = 0.08f;
            PositionVelocityDamping = 5f;
            PositionReturnToNeutral = 1.5f;
            PositionSampleGapResetSeconds = 0.7f;
            ChopstickVisualLengthScale = 0.88f;
            HashiDisplayAlignmentEuler = new Vector3(0f, -90f, 135f);
            HashiDisplayOffsetWorld = new Vector3(1f, 0f, 0f);
            InvertImu1Pose = false;
            InvertImu2Pose = false;
            Imu1MapX = HashiImuAxis.X;
            Imu1MapY = HashiImuAxis.Y;
            Imu1MapZ = HashiImuAxis.Z;
            Imu2MapX = HashiImuAxis.X;
            Imu2MapY = HashiImuAxis.Y;
            Imu2MapZ = HashiImuAxis.Z;
            Imu1MapSigns = Vector3.one;
            Imu2MapSigns = Vector3.one;
            Imu1InputEuler = Vector3.zero;
            Imu2InputEuler = Vector3.zero;
            Imu1GyroSigns = Vector3.one;
            Imu2GyroSigns = Vector3.one;
            Imu1GyroAxisScale = Vector3.one;
            Imu2GyroAxisScale = Vector3.one;
            Imu1AccelerationSigns = Vector3.one;
            Imu2AccelerationSigns = Vector3.one;
            UpperOutputEuler = Vector3.zero;
            LowerOutputEuler = Vector3.zero;
            ClearIndependentNeutral();
            ClearHardwarePosition();
        }

        public static float MapTheta(HashiBleSample sample, float fallbackTheta)
        {
            if (!UseTunedTheta || !TryMeasureThetaInput(sample, out var input))
            {
                return fallbackTheta;
            }

            var denominator = InputOpen - InputClosed;
            var t = Mathf.Abs(denominator) < 0.0001f
                ? 0f
                : Mathf.InverseLerp(InputClosed, InputOpen, input);

            if (InvertTheta)
            {
                t = 1f - t;
            }

            return Mathf.Lerp(OutputClosed, OutputOpen, Mathf.Clamp01(t));
        }

        public static bool TryMeasureThetaInput(HashiBleSample sample, out float input)
        {
            input = 0f;
            if (!sample.HasDualImu)
            {
                return false;
            }

            switch (ThetaMode)
            {
                case HashiImuThetaMode.AccelerationAngle:
                    var first = sample.Imu1AccelerationRaw;
                    var second = sample.Imu2AccelerationRaw;
                    if (first.sqrMagnitude < 0.000001f || second.sqrMagnitude < 0.000001f)
                    {
                        return false;
                    }

                    input = Vector3.Angle(first, second);
                    return !float.IsNaN(input);
                case HashiImuThetaMode.Imu1Axis:
                    input = ReadAxis(sample.Imu1AccelerationRaw, Imu1Axis);
                    return true;
                case HashiImuThetaMode.Imu2Axis:
                    input = ReadAxis(sample.Imu2AccelerationRaw, Imu2Axis);
                    return true;
                case HashiImuThetaMode.AxisDifference:
                    input = ReadAxis(sample.Imu1AccelerationRaw, Imu1Axis)
                        - ReadAxis(sample.Imu2AccelerationRaw, Imu2Axis);
                    return true;
                default:
                    return false;
            }
        }

        public static bool TrySelectMotion(HashiBleSample sample, out Vector3 acceleration, out Vector3 gyro)
        {
            acceleration = Vector3.zero;
            gyro = Vector3.zero;

            if (!sample.HasDualImu)
            {
                return false;
            }

            switch (MotionSource)
            {
                case HashiImuMotionSource.Imu1:
                    acceleration = MapImu1Acceleration(sample.Imu1AccelerationRaw) * RawAccelerationToG;
                    gyro = MapImu1Gyro(sample.Imu1GyroRaw);
                    return true;
                case HashiImuMotionSource.Imu2:
                    acceleration = MapImu2Acceleration(sample.Imu2AccelerationRaw) * RawAccelerationToG;
                    gyro = MapImu2Gyro(sample.Imu2GyroRaw);
                    return true;
                case HashiImuMotionSource.Average:
                    acceleration = (MapImu1Acceleration(sample.Imu1AccelerationRaw)
                        + MapImu2Acceleration(sample.Imu2AccelerationRaw)) * (RawAccelerationToG * 0.5f);
                    gyro = (MapImu1Gyro(sample.Imu1GyroRaw) + MapImu2Gyro(sample.Imu2GyroRaw)) * 0.5f;
                    return true;
                case HashiImuMotionSource.Difference:
                    acceleration = (MapImu1Acceleration(sample.Imu1AccelerationRaw)
                        - MapImu2Acceleration(sample.Imu2AccelerationRaw)) * RawAccelerationToG;
                    gyro = MapImu1Gyro(sample.Imu1GyroRaw) - MapImu2Gyro(sample.Imu2GyroRaw);
                    return true;
                default:
                    return false;
            }
        }

        public static Quaternion ApplyParentRotationWeight(Quaternion rotation)
        {
            return ApplyParentLongAxisTwistWeight(rotation);
        }

        public static Vector3 ApplyParentRotationRateWeight(Vector3 gyro)
        {
            return gyro * Mathf.Clamp01(ParentRotationWeight);
        }

        public static Quaternion ApplyParentLongAxisTwistWeight(Quaternion rotation)
        {
            var twistWeight = Mathf.Clamp01(ParentLongAxisTwistWeight);
            if (twistWeight >= 0.999f)
            {
                return rotation;
            }

            if (!TryExtractTwist(rotation, Vector3.right, out var twist))
            {
                return rotation;
            }

            var swing = rotation * Quaternion.Inverse(twist);
            var weightedTwist = Quaternion.Slerp(Quaternion.identity, twist, twistWeight);
            return Normalize(swing * weightedTwist);
        }

        public static void ApplyParentRotationInputBasis(ref Vector3 acceleration, ref Vector3 gyro)
        {
            var inputBasis = Quaternion.Euler(ParentRotationInputEuler);
            acceleration = inputBasis * acceleration;
            gyro = inputBasis * gyro;
        }

        public static Vector3 FilterParentRotationGyro(Vector3 gyro)
        {
            if (!ParentPlanarYawOnly)
            {
                return gyro;
            }

            var yawDirection = ParentPlanarYawDirection < 0f ? -1f : 1f;
            var yaw = yawDirection * gyro.y;
            var pitch = ParentVerticalPitchWhenDominant
                ? gyro.z * Mathf.Clamp01(ParentVerticalPitchScale)
                : 0f;
            return new Vector3(0f, yaw, pitch);
        }

        public static void CaptureIndependentNeutral(HashiBleSample sample)
        {
            CaptureImu1Neutral(sample);
            CaptureImu2Neutral(sample);
        }

        public static bool CaptureImu1Neutral(HashiBleSample sample)
        {
            return CaptureImuNeutral(ref imu1Pose, sample, HashiImuId.Imu1);
        }

        public static bool CaptureImu2Neutral(HashiBleSample sample)
        {
            return CaptureImuNeutral(ref imu2Pose, sample, HashiImuId.Imu2);
        }

        public static bool ZeroImu1IntegratedPose(HashiBleSample sample)
        {
            return ZeroIntegratedPose(ref imu1Pose, sample, HashiImuId.Imu1);
        }

        public static bool ZeroImu2IntegratedPose(HashiBleSample sample)
        {
            return ZeroIntegratedPose(ref imu2Pose, sample, HashiImuId.Imu2);
        }

        public static void ResetImu1IntegratedPose()
        {
            imu1Pose.ResetIntegratedPose();
        }

        public static void ResetImu2IntegratedPose()
        {
            imu2Pose.ResetIntegratedPose();
        }

        public static void ClearIndependentNeutral()
        {
            imu1Pose.Clear();
            imu2Pose.Clear();
        }

        public static void ClearHardwarePosition()
        {
            positionTracker.Clear();
        }

        public static bool CaptureHardwarePositionNeutral(HashiBleSample sample)
        {
            if (!TrySelectPositionAccelerationG(sample, out var accelerationG))
            {
                return false;
            }

            positionTracker.CaptureNeutral(accelerationG, sample);
            return positionTracker.HasNeutral;
        }

        public static bool ZeroHardwarePosition(HashiBleSample sample)
        {
            if (!CaptureHardwarePositionNeutral(sample))
            {
                positionTracker.ResetMotion();
                return false;
            }

            positionTracker.ResetMotion();
            positionTracker.LastSampleRealtime = sample.RealtimeReceived;
            positionTracker.LastSampleSequence = sample.Sequence;
            return true;
        }

        public static bool UpdateHardwarePosition(
            HashiBleSample sample,
            float fallbackDeltaTime,
            out Vector3 positionOffset,
            out Vector3 velocity)
        {
            positionOffset = positionTracker.Offset;
            velocity = positionTracker.Velocity;

            if (!UseHardwarePosition || !TrySelectPositionAccelerationG(sample, out var accelerationG))
            {
                return false;
            }

            if (AutoCapturePositionNeutral && !positionTracker.HasNeutral)
            {
                positionTracker.CaptureNeutral(accelerationG, sample);
            }

            if (!positionTracker.HasNeutral)
            {
                return false;
            }

            var deltaTime = positionTracker.LastSampleRealtime > 0f
                ? sample.RealtimeReceived - positionTracker.LastSampleRealtime
                : Mathf.Max(0f, fallbackDeltaTime);
            if (deltaTime < 0.001f)
            {
                deltaTime = Mathf.Max(0f, fallbackDeltaTime);
            }

            var maxGap = Mathf.Max(0f, PositionSampleGapResetSeconds);
            if (maxGap > 0f && deltaTime > maxGap)
            {
                positionTracker.Velocity = Vector3.zero;
                positionTracker.LastSampleRealtime = sample.RealtimeReceived;
                positionTracker.LastSampleSequence = sample.Sequence;
                positionOffset = positionTracker.Offset;
                velocity = positionTracker.Velocity;
                return true;
            }

            deltaTime = Mathf.Clamp(deltaTime, 0f, 0.12f);
            var linearAccelerationG = accelerationG - positionTracker.NeutralAccelerationG;
            linearAccelerationG = Quaternion.Euler(PositionInputEuler)
                * MultiplyComponents(linearAccelerationG, PositionAxisSigns);
            linearAccelerationG = ApplyMagnitudeDeadzone(linearAccelerationG, Mathf.Max(0f, PositionDeadzoneG));

            positionTracker.LinearAccelerationG = linearAccelerationG;
            if (PositionTrackingMode == HashiPositionTrackingMode.AccelerationDirect)
            {
                positionTracker.Offset = ClampComponents(
                    MultiplyComponents(linearAccelerationG, PositionAccelerationScale),
                    PositionMaxOffset);
                positionTracker.Velocity = Vector3.zero;
            }
            else if (deltaTime > 0f)
            {
                var accelerationUnits = MultiplyComponents(linearAccelerationG, PositionAccelerationScale);
                positionTracker.Velocity += accelerationUnits * deltaTime;
                positionTracker.Velocity = DampVector(positionTracker.Velocity, PositionVelocityDamping, deltaTime);
                positionTracker.Offset += positionTracker.Velocity * deltaTime;

                if (linearAccelerationG.sqrMagnitude < 0.000001f)
                {
                    positionTracker.Offset = DampVector(positionTracker.Offset, PositionReturnToNeutral, deltaTime);
                }

                positionTracker.Offset = ClampComponents(positionTracker.Offset, PositionMaxOffset);
            }

            positionTracker.LastSampleRealtime = sample.RealtimeReceived;
            positionTracker.LastSampleSequence = sample.Sequence;
            positionOffset = positionTracker.Offset;
            velocity = positionTracker.Velocity;
            return true;
        }

        public static bool UpdateIndependentRotations(
            HashiBleSample sample,
            float fallbackDeltaTime,
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            return UpdateIndependentRotations(
                sample,
                fallbackDeltaTime,
                Vector3.forward,
                out upperRotation,
                out lowerRotation);
        }

        public static bool UpdateIndependentRotations(
            HashiBleSample sample,
            float fallbackDeltaTime,
            Vector3 openingAxis,
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            upperRotation = Quaternion.identity;
            lowerRotation = Quaternion.identity;

            if (IndependentPoseMode == HashiIndependentPoseMode.AccelerationOpening)
            {
                var upperOk = TryGetImuAccelerationOpeningRotation(
                    ref imu1Pose,
                    sample,
                    HashiImuId.Imu1,
                    openingAxis,
                    out upperRotation);
                var lowerOk = TryGetImuAccelerationOpeningRotation(
                    ref imu2Pose,
                    sample,
                    HashiImuId.Imu2,
                    openingAxis,
                    out lowerRotation);
                return upperOk & lowerOk;
            }

            if (IndependentPoseMode == HashiIndependentPoseMode.OpeningAxisIntegrated)
            {
                var upperOk = UpdateImuOpeningAxisRotation(
                    ref imu1Pose,
                    sample,
                    fallbackDeltaTime,
                    HashiImuId.Imu1,
                    openingAxis,
                    out upperRotation);
                var lowerOk = UpdateImuOpeningAxisRotation(
                    ref imu2Pose,
                    sample,
                    fallbackDeltaTime,
                    HashiImuId.Imu2,
                    openingAxis,
                    out lowerRotation);
                return upperOk & lowerOk;
            }

            if (IndependentPoseMode == HashiIndependentPoseMode.RelativeOpeningIntegrated)
            {
                return UpdateRelativeOpeningAxisRotation(
                    sample,
                    fallbackDeltaTime,
                    openingAxis,
                    out upperRotation,
                    out lowerRotation);
            }

            if (IndependentPoseMode == HashiIndependentPoseMode.AccelerationRelative)
            {
                var upperOk = TryGetImuAccelerationRelativeRotation(ref imu1Pose, sample, HashiImuId.Imu1, out upperRotation);
                var lowerOk = TryGetImuAccelerationRelativeRotation(ref imu2Pose, sample, HashiImuId.Imu2, out lowerRotation);
                return upperOk & lowerOk;
            }

            var upperIntegratedOk = UpdateImuIntegratedRotation(
                ref imu1Pose,
                sample,
                fallbackDeltaTime,
                HashiImuId.Imu1,
                out upperRotation);
            var lowerIntegratedOk = UpdateImuIntegratedRotation(
                ref imu2Pose,
                sample,
                fallbackDeltaTime,
                HashiImuId.Imu2,
                out lowerRotation);
            return upperIntegratedOk & lowerIntegratedOk;
        }

        public static bool TryGetIndependentRotations(
            HashiBleSample sample,
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            upperRotation = Quaternion.identity;
            lowerRotation = Quaternion.identity;

            var upperOk = TryGetImuAccelerationRelativeRotation(ref imu1Pose, sample, HashiImuId.Imu1, out upperRotation);
            var lowerOk = TryGetImuAccelerationRelativeRotation(ref imu2Pose, sample, HashiImuId.Imu2, out lowerRotation);
            return upperOk & lowerOk;
        }

        public static bool TryGetCurrentIndependentRotations(
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            return TryGetCurrentIndependentRotations(Vector3.forward, out upperRotation, out lowerRotation);
        }

        public static bool TryGetCurrentIndependentRotations(
            Vector3 openingAxis,
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            upperRotation = Quaternion.identity;
            lowerRotation = Quaternion.identity;

            if (!UseIndependentImuPose || (!imu1Pose.HasIntegratedPose && !imu2Pose.HasIntegratedPose))
            {
                return false;
            }

            switch (IndependentPoseMode)
            {
                case HashiIndependentPoseMode.GyroIntegrated:
                case HashiIndependentPoseMode.AccelerationRelative:
                    upperRotation = BuildImuOutput(imu1Pose.IntegratedRotation, HashiImuId.Imu1);
                    lowerRotation = BuildImuOutput(imu2Pose.IntegratedRotation, HashiImuId.Imu2);
                    return true;
                case HashiIndependentPoseMode.AccelerationOpening:
                case HashiIndependentPoseMode.OpeningAxisIntegrated:
                case HashiIndependentPoseMode.RelativeOpeningIntegrated:
                default:
                    upperRotation = BuildOpeningAxisOutput(imu1Pose.OpeningAngleDegrees, HashiImuId.Imu1, openingAxis);
                    lowerRotation = BuildOpeningAxisOutput(imu2Pose.OpeningAngleDegrees, HashiImuId.Imu2, openingAxis);
                    return true;
            }
        }

        public static bool TryReadOpeningVelocity(
            HashiBleSample sample,
            HashiImuId imuId,
            out float velocityDegreesPerSecond)
        {
            velocityDegreesPerSecond = 0f;
            if (!HasImuSample(sample, imuId))
            {
                return false;
            }

            velocityDegreesPerSecond = ReadOpeningAxisVelocity(MapImuGyro(sample, imuId), imuId);
            return true;
        }

        public static bool TryReadIndependentOpeningDegrees(HashiBleSample sample, out float openingDegrees)
        {
            openingDegrees = 0f;
            if (!UseIndependentImuPose || !sample.HasDualImu)
            {
                return false;
            }

            if (!imu1Pose.HasIntegratedPose && !imu2Pose.HasIntegratedPose)
            {
                if (IndependentPoseMode == HashiIndependentPoseMode.RelativeOpeningIntegrated
                    && TryReadRelativeAccelerationOpening(sample, out var relativeOpening))
                {
                    openingDegrees = Mathf.Abs(relativeOpening);
                    return true;
                }

                return false;
            }

            openingDegrees = Mathf.Max(
                Mathf.Abs(imu1Pose.OpeningAngleDegrees),
                Mathf.Abs(imu2Pose.OpeningAngleDegrees));
            return true;
        }

        public static Vector3 MapImu1Acceleration(Vector3 raw)
        {
            return Quaternion.Euler(Imu1InputEuler)
                * MultiplyComponents(MapVector(raw, Imu1MapX, Imu1MapY, Imu1MapZ, Imu1MapSigns), Imu1AccelerationSigns);
        }

        public static Vector3 MapImu2Acceleration(Vector3 raw)
        {
            return Quaternion.Euler(Imu2InputEuler)
                * MultiplyComponents(MapVector(raw, Imu2MapX, Imu2MapY, Imu2MapZ, Imu2MapSigns), Imu2AccelerationSigns);
        }

        public static Vector3 MapImu1Gyro(Vector3 raw)
        {
            var mapped = MapVector(raw, Imu1MapX, Imu1MapY, Imu1MapZ, Imu1MapSigns);
            mapped = MultiplyComponents(mapped, Imu1GyroSigns);
            mapped = MultiplyComponents(mapped, Imu1GyroAxisScale);
            return (Quaternion.Euler(Imu1InputEuler) * mapped) * RawGyroToDegreesPerSecond;
        }

        public static Vector3 MapImu2Gyro(Vector3 raw)
        {
            var mapped = MapVector(raw, Imu2MapX, Imu2MapY, Imu2MapZ, Imu2MapSigns);
            mapped = MultiplyComponents(mapped, Imu2GyroSigns);
            mapped = MultiplyComponents(mapped, Imu2GyroAxisScale);
            return (Quaternion.Euler(Imu2InputEuler) * mapped) * RawGyroToDegreesPerSecond;
        }

        public static float ReadAxis(Vector3 value, HashiImuAxis axis)
        {
            switch (axis)
            {
                case HashiImuAxis.X:
                    return value.x;
                case HashiImuAxis.Y:
                    return value.y;
                case HashiImuAxis.Z:
                    return value.z;
                default:
                    return 0f;
            }
        }

        private static bool TrySelectPositionAccelerationG(HashiBleSample sample, out Vector3 accelerationG)
        {
            accelerationG = Vector3.zero;
            if (!sample.HasDualImu)
            {
                return false;
            }

            var imu1 = MapImu1Acceleration(sample.Imu1AccelerationRaw) * RawAccelerationToG;
            var imu2 = MapImu2Acceleration(sample.Imu2AccelerationRaw) * RawAccelerationToG;
            switch (PositionMotionSource)
            {
                case HashiImuMotionSource.Imu1:
                    accelerationG = imu1;
                    return true;
                case HashiImuMotionSource.Imu2:
                    accelerationG = imu2;
                    return true;
                case HashiImuMotionSource.Average:
                    accelerationG = (imu1 + imu2) * 0.5f;
                    return true;
                case HashiImuMotionSource.Difference:
                    accelerationG = imu1 - imu2;
                    return true;
                default:
                    return false;
            }
        }

        private static Vector3 ApplyMagnitudeDeadzone(Vector3 value, float deadzone)
        {
            var magnitude = value.magnitude;
            if (magnitude <= deadzone || magnitude < 0.000001f)
            {
                return Vector3.zero;
            }

            return value.normalized * (magnitude - deadzone);
        }

        private static Vector3 DampVector(Vector3 value, float damping, float deltaTime)
        {
            if (damping <= 0f || deltaTime <= 0f)
            {
                return value;
            }

            return Vector3.Lerp(value, Vector3.zero, 1f - Mathf.Exp(-damping * deltaTime));
        }

        private static Vector3 ClampComponents(Vector3 value, Vector3 maxAbs)
        {
            var maxX = Mathf.Max(0f, Mathf.Abs(maxAbs.x));
            var maxY = Mathf.Max(0f, Mathf.Abs(maxAbs.y));
            var maxZ = Mathf.Max(0f, Mathf.Abs(maxAbs.z));
            return new Vector3(
                Mathf.Clamp(value.x, -maxX, maxX),
                Mathf.Clamp(value.y, -maxY, maxY),
                Mathf.Clamp(value.z, -maxZ, maxZ));
        }

        private static bool CaptureImuNeutral(ref ImuPoseTracker tracker, HashiBleSample sample, HashiImuId imuId)
        {
            if (!HasImuSample(sample, imuId))
            {
                return false;
            }

            var acceleration = MapImuAcceleration(sample, imuId);
            tracker.CaptureNeutral(acceleration.normalized, sample);
            return tracker.HasNeutral;
        }

        private static bool ZeroIntegratedPose(ref ImuPoseTracker tracker, HashiBleSample sample, HashiImuId imuId)
        {
            if (!HasImuSample(sample, imuId))
            {
                return false;
            }

            if (IndependentPoseMode == HashiIndependentPoseMode.AccelerationOpening)
            {
                CaptureImuNeutral(ref tracker, sample, imuId);
            }
            else if (IndependentPoseMode == HashiIndependentPoseMode.RelativeOpeningIntegrated)
            {
                CaptureRelativeOpeningNeutral(sample);
            }

            tracker.ZeroIntegratedPose(sample);
            return true;
        }

        private static bool TryGetImuAccelerationOpeningRotation(
            ref ImuPoseTracker tracker,
            HashiBleSample sample,
            HashiImuId imuId,
            Vector3 openingAxis,
            out Quaternion outputRotation)
        {
            outputRotation = Quaternion.identity;
            if (!TryEnsureImuNeutral(ref tracker, sample, imuId))
            {
                return false;
            }

            var currentAcceleration = MapImuAcceleration(sample, imuId);
            if (currentAcceleration.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            var sensorOpeningAxis = GetImuOpeningAxisVector(imuId);
            var signedAngle = Vector3.SignedAngle(
                tracker.NeutralAcceleration.normalized,
                currentAcceleration.normalized,
                sensorOpeningAxis);
            if (float.IsNaN(signedAngle))
            {
                return false;
            }

            tracker.OpeningVelocityDegreesPerSecond = ReadOpeningAxisVelocity(MapImuGyro(sample, imuId), imuId);
            var candidateAngle = ClampOpeningAngle(
                signedAngle * GetImuOpeningDirection(imuId) * GetImuOpeningScale(imuId));
            if (!ShouldRejectOpeningAngleJump(tracker, candidateAngle))
            {
                tracker.OpeningAngleDegrees = candidateAngle;
            }

            tracker.HasIntegratedPose = true;
            tracker.LastSampleRealtime = sample.RealtimeReceived;
            tracker.LastSampleSequence = sample.Sequence;

            outputRotation = BuildOpeningAxisOutput(tracker.OpeningAngleDegrees, imuId, openingAxis);
            return true;
        }

        private static bool ShouldRejectOpeningAngleJump(ImuPoseTracker tracker, float candidateAngle)
        {
            if (!tracker.HasIntegratedPose)
            {
                return false;
            }

            var rejectDegrees = Mathf.Max(0f, OpeningAngleStaticJumpRejectDegrees);
            var jump = Mathf.Abs(candidateAngle - tracker.OpeningAngleDegrees);
            if (jump <= rejectDegrees)
            {
                return false;
            }

            var candidateLooksClosed = Mathf.Abs(candidateAngle) <= rejectDegrees;
            var currentLooksOpen = Mathf.Abs(tracker.OpeningAngleDegrees) > rejectDegrees;
            var looksStatic = Mathf.Abs(tracker.OpeningVelocityDegreesPerSecond) < Mathf.Max(0f, OpeningAngleStaticVelocityThresholdDps);
            return candidateLooksClosed && currentLooksOpen && looksStatic;
        }

        private static bool UpdateImuOpeningAxisRotation(
            ref ImuPoseTracker tracker,
            HashiBleSample sample,
            float fallbackDeltaTime,
            HashiImuId imuId,
            Vector3 openingAxis,
            out Quaternion outputRotation)
        {
            outputRotation = Quaternion.identity;
            if (!HasImuSample(sample, imuId))
            {
                return false;
            }

            if (!tracker.HasIntegratedPose)
            {
                tracker.StartIntegratedPose(sample);
                outputRotation = BuildOpeningAxisOutput(tracker.OpeningAngleDegrees, imuId, openingAxis);
                return true;
            }

            if (sample.Sequence == tracker.LastSampleSequence)
            {
                outputRotation = BuildOpeningAxisOutput(tracker.OpeningAngleDegrees, imuId, openingAxis);
                return true;
            }

            var deltaTime = sample.RealtimeReceived - tracker.LastSampleRealtime;
            if (deltaTime < 0.001f)
            {
                deltaTime = Mathf.Max(0f, fallbackDeltaTime);
            }

            deltaTime = Mathf.Clamp(deltaTime, 0f, 0.25f);
            tracker.OpeningVelocityDegreesPerSecond = ReadOpeningAxisVelocity(MapImuGyro(sample, imuId), imuId);
            if (deltaTime > 0f)
            {
                tracker.OpeningAngleDegrees = ClampOpeningAngle(
                    tracker.OpeningAngleDegrees + tracker.OpeningVelocityDegreesPerSecond * deltaTime);
            }

            tracker.LastSampleRealtime = sample.RealtimeReceived;
            tracker.LastSampleSequence = sample.Sequence;
            outputRotation = BuildOpeningAxisOutput(tracker.OpeningAngleDegrees, imuId, openingAxis);
            return true;
        }

        private static bool UpdateRelativeOpeningAxisRotation(
            HashiBleSample sample,
            float fallbackDeltaTime,
            Vector3 openingAxis,
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            upperRotation = Quaternion.identity;
            lowerRotation = Quaternion.identity;
            if (!sample.HasDualImu)
            {
                return false;
            }

            if (AutoCaptureIndependentNeutral && !imu1Pose.HasRelativeOpeningNeutral)
            {
                CaptureRelativeOpeningNeutral(sample);
            }

            if (!imu1Pose.HasIntegratedPose || !imu2Pose.HasIntegratedPose)
            {
                imu1Pose.StartIntegratedPose(sample);
                imu2Pose.StartIntegratedPose(sample);
                upperRotation = BuildOpeningAxisOutput(imu1Pose.OpeningAngleDegrees, HashiImuId.Imu1, openingAxis);
                lowerRotation = Quaternion.identity;
                return true;
            }

            if (sample.Sequence == imu1Pose.LastSampleSequence)
            {
                upperRotation = BuildOpeningAxisOutput(imu1Pose.OpeningAngleDegrees, HashiImuId.Imu1, openingAxis);
                lowerRotation = Quaternion.identity;
                return true;
            }

            var deltaTime = sample.RealtimeReceived - imu1Pose.LastSampleRealtime;
            if (deltaTime < 0.001f)
            {
                deltaTime = Mathf.Max(0f, fallbackDeltaTime);
            }

            var maxGap = Mathf.Max(0f, MaxBleSampleAgeSeconds);
            if (maxGap > 0f && deltaTime > maxGap)
            {
                imu1Pose.LastSampleRealtime = sample.RealtimeReceived;
                imu1Pose.LastSampleSequence = sample.Sequence;
                imu2Pose.LastSampleRealtime = sample.RealtimeReceived;
                imu2Pose.LastSampleSequence = sample.Sequence;
                upperRotation = BuildOpeningAxisOutput(imu1Pose.OpeningAngleDegrees, HashiImuId.Imu1, openingAxis);
                lowerRotation = Quaternion.identity;
                return true;
            }

            deltaTime = Mathf.Clamp(deltaTime, 0f, 0.2f);
            var imu1Gyro = MapImuGyro(sample, HashiImuId.Imu1);
            var imu2Gyro = MapImuGyro(sample, HashiImuId.Imu2);
            var imu1Velocity = ReadOpeningAxisVelocity(imu1Gyro, HashiImuId.Imu1);
            var imu2Velocity = ReadOpeningAxisVelocity(imu2Gyro, HashiImuId.Imu2);
            var relativeVelocity = imu1Velocity - imu2Velocity;
            if (Mathf.Abs(relativeVelocity) < Mathf.Max(0f, GyroDeadzoneDegreesPerSecond))
            {
                relativeVelocity = 0f;
            }

            var gyroWeight = RelativeOpeningGyroIntegrationWeight(
                imu1Gyro,
                imu2Gyro,
                relativeVelocity,
                out var gyroMotion,
                out var velocityDominance);
            if (IsGyroSaturated(sample, HashiImuMotionSource.Difference))
            {
                gyroWeight = Mathf.Abs(relativeVelocity) >= Mathf.Max(OpeningAngleStaticVelocityThresholdDps, 1f)
                    ? Mathf.Max(gyroWeight, 0.35f)
                    : 0f;
            }

            relativeVelocity *= gyroWeight;
            imu1Pose.OpeningVelocityDegreesPerSecond = relativeVelocity;
            imu2Pose.OpeningVelocityDegreesPerSecond = 0f;
            if (deltaTime > 0f)
            {
                imu1Pose.OpeningAngleDegrees = ClampOpeningAngle(
                    imu1Pose.OpeningAngleDegrees + relativeVelocity * deltaTime);
                imu2Pose.OpeningAngleDegrees = 0f;
            }

            var hasAccelerationOpening = TryReadRelativeAccelerationOpening(sample, out var accelerationOpening);
            var accelerationCorrectionAllowed = false;
            imu1Pose.RelativeOpeningGyroWeight = gyroWeight;
            imu1Pose.RelativeOpeningGyroMotionDegreesPerSecond = gyroMotion;
            imu1Pose.RelativeOpeningVelocityDominance = velocityDominance;
            imu1Pose.HasRelativeOpeningAcceleration = hasAccelerationOpening;
            imu1Pose.RelativeOpeningAccelerationDegrees = hasAccelerationOpening ? accelerationOpening : 0f;
            imu2Pose.RelativeOpeningGyroWeight = gyroWeight;
            imu2Pose.RelativeOpeningGyroMotionDegreesPerSecond = gyroMotion;
            imu2Pose.RelativeOpeningVelocityDominance = velocityDominance;
            imu2Pose.HasRelativeOpeningAcceleration = hasAccelerationOpening;
            imu2Pose.RelativeOpeningAccelerationDegrees = hasAccelerationOpening ? accelerationOpening : 0f;

            if (hasAccelerationOpening)
            {
                var correctionMotionWeight = 1f - Mathf.InverseLerp(
                    Mathf.Max(0f, RelativeOpeningGyroFullWeightBelowDps),
                    Mathf.Max(RelativeOpeningGyroFullWeightBelowDps + 1f, RelativeOpeningGyroZeroWeightAboveDps),
                    gyroMotion);
                var jump = Mathf.Abs(accelerationOpening - imu1Pose.OpeningAngleDegrees);
                var jumpLimit = Mathf.Max(OpeningAngleStaticJumpRejectDegrees, 1f);
                var currentLooksClosed = Mathf.Abs(imu1Pose.OpeningAngleDegrees) <= jumpLimit;
                var accelerationLooksClosed = Mathf.Abs(accelerationOpening) <= jumpLimit;
                var correctionDirection = accelerationOpening - imu1Pose.OpeningAngleDegrees;
                var gyroSupportsCorrection = Mathf.Abs(correctionDirection) > 0.001f
                    && Mathf.Abs(relativeVelocity) >= Mathf.Max(OpeningAngleStaticVelocityThresholdDps, 1f)
                    && Mathf.Sign(relativeVelocity) == Mathf.Sign(correctionDirection);
                accelerationCorrectionAllowed = jump <= jumpLimit
                    || (currentLooksClosed && accelerationLooksClosed)
                    || (gyroSupportsCorrection && jump <= jumpLimit * 2f);
                if (accelerationCorrectionAllowed)
                {
                    var correctionRate = Mathf.Clamp01(RelativeOpeningAccelerationWeight)
                        * Mathf.Clamp01(correctionMotionWeight)
                        * 6f;
                    var correctionAlpha = correctionRate <= 0f || deltaTime <= 0f
                        ? 0f
                        : 1f - Mathf.Exp(-correctionRate * deltaTime);
                    imu1Pose.OpeningAngleDegrees = ClampOpeningAngle(Mathf.Lerp(
                        imu1Pose.OpeningAngleDegrees,
                        accelerationOpening,
                        correctionAlpha));
                }
            }

            imu1Pose.RelativeOpeningAccelerationCorrectionAllowed = accelerationCorrectionAllowed;
            imu2Pose.RelativeOpeningAccelerationCorrectionAllowed = accelerationCorrectionAllowed;

            imu1Pose.HasIntegratedPose = true;
            imu1Pose.LastSampleRealtime = sample.RealtimeReceived;
            imu1Pose.LastSampleSequence = sample.Sequence;
            imu2Pose.HasIntegratedPose = true;
            imu2Pose.LastSampleRealtime = sample.RealtimeReceived;
            imu2Pose.LastSampleSequence = sample.Sequence;
            upperRotation = BuildOpeningAxisOutput(imu1Pose.OpeningAngleDegrees, HashiImuId.Imu1, openingAxis);
            lowerRotation = Quaternion.identity;
            return true;
        }

        private static bool UpdateImuIntegratedRotation(
            ref ImuPoseTracker tracker,
            HashiBleSample sample,
            float fallbackDeltaTime,
            HashiImuId imuId,
            out Quaternion outputRotation)
        {
            outputRotation = Quaternion.identity;
            if (!TryEnsureImuNeutral(ref tracker, sample, imuId))
            {
                return false;
            }

            if (!tracker.HasIntegratedPose)
            {
                tracker.StartIntegratedPose(sample);
                outputRotation = BuildImuOutput(tracker.IntegratedRotation, imuId);
                return true;
            }

            if (sample.Sequence == tracker.LastSampleSequence)
            {
                outputRotation = BuildImuOutput(tracker.IntegratedRotation, imuId);
                return true;
            }

            var deltaTime = sample.RealtimeReceived - tracker.LastSampleRealtime;
            if (deltaTime < 0.001f)
            {
                deltaTime = Mathf.Max(0f, fallbackDeltaTime);
            }

            deltaTime = Mathf.Clamp(deltaTime, 0f, 0.25f);
            if (deltaTime > 0f)
            {
                tracker.IntegratedRotation = IntegrateImuPose(
                    tracker.IntegratedRotation,
                    MapImuGyro(sample, imuId),
                    MapImuAcceleration(sample, imuId),
                    tracker.NeutralAcceleration,
                    deltaTime);
            }

            tracker.LastSampleRealtime = sample.RealtimeReceived;
            tracker.LastSampleSequence = sample.Sequence;
            outputRotation = BuildImuOutput(tracker.IntegratedRotation, imuId);
            return true;
        }

        private static float ReadOpeningAxisVelocity(Vector3 gyroDegreesPerSecond, HashiImuId imuId)
        {
            var velocity = ReadAxis(gyroDegreesPerSecond, GetImuOpeningAxis(imuId))
                * GetImuOpeningDirection(imuId)
                * GetImuOpeningScale(imuId);
            return Mathf.Abs(velocity) < Mathf.Max(0f, GyroDeadzoneDegreesPerSecond) ? 0f : velocity;
        }

        private static float RelativeOpeningGyroIntegrationWeight(
            Vector3 imu1Gyro,
            Vector3 imu2Gyro,
            float relativeOpeningVelocity,
            out float gyroMotion,
            out float velocityDominance)
        {
            gyroMotion = Mathf.Max(imu1Gyro.magnitude, imu2Gyro.magnitude);
            var fullWeightBelow = Mathf.Max(0f, RelativeOpeningGyroFullWeightBelowDps);
            var zeroWeightAbove = Mathf.Max(fullWeightBelow + 1f, RelativeOpeningGyroZeroWeightAboveDps);
            float motionWeight;
            if (gyroMotion <= fullWeightBelow)
            {
                motionWeight = 1f;
            }
            else if (gyroMotion >= zeroWeightAbove)
            {
                motionWeight = 0f;
            }
            else
            {
                motionWeight = 1f - Mathf.InverseLerp(fullWeightBelow, zeroWeightAbove, gyroMotion);
            }

            var openingSpeed = Mathf.Abs(relativeOpeningVelocity);
            velocityDominance = gyroMotion > 0.001f ? openingSpeed / gyroMotion : 0f;
            var openingDominanceWeight = Mathf.InverseLerp(0.35f, 0.85f, velocityDominance);
            return Mathf.Max(motionWeight, openingDominanceWeight);
        }

        public static bool IsGyroSaturated(HashiBleSample sample, HashiImuMotionSource source)
        {
            switch (source)
            {
                case HashiImuMotionSource.Imu1:
                    return sample.HasImu1 && MaxAbs(sample.Imu1GyroRaw) >= 32700f;
                case HashiImuMotionSource.Imu2:
                    return sample.HasImu2 && MaxAbs(sample.Imu2GyroRaw) >= 32700f;
                case HashiImuMotionSource.Average:
                case HashiImuMotionSource.Difference:
                    return (sample.HasImu1 && MaxAbs(sample.Imu1GyroRaw) >= 32700f)
                        || (sample.HasImu2 && MaxAbs(sample.Imu2GyroRaw) >= 32700f);
                default:
                    return false;
            }
        }

        private static bool CaptureRelativeOpeningNeutral(HashiBleSample sample)
        {
            if (!TryReadRelativeAccelerationVectors(sample, out var imu1Acceleration, out var imu2Acceleration))
            {
                return false;
            }

            var angleDegrees = Vector3.Angle(imu1Acceleration, imu2Acceleration);
            var lowerToUpperRotation = Quaternion.FromToRotation(imu2Acceleration, imu1Acceleration);
            imu1Pose.NeutralAcceleration = imu1Acceleration;
            imu1Pose.HasNeutral = true;
            imu1Pose.RelativeOpeningNeutralAngleDegrees = angleDegrees;
            imu1Pose.RelativeOpeningLowerToUpperRotation = lowerToUpperRotation;
            imu1Pose.HasRelativeOpeningNeutral = true;
            imu2Pose.NeutralAcceleration = imu2Acceleration;
            imu2Pose.HasNeutral = true;
            imu2Pose.RelativeOpeningNeutralAngleDegrees = angleDegrees;
            imu2Pose.RelativeOpeningLowerToUpperRotation = lowerToUpperRotation;
            imu2Pose.HasRelativeOpeningNeutral = true;
            return true;
        }

        private static bool TryReadRelativeAccelerationOpening(HashiBleSample sample, out float openingDegrees)
        {
            openingDegrees = 0f;
            if (!imu1Pose.HasRelativeOpeningNeutral
                || !imu1Pose.HasNeutral
                || !imu2Pose.HasNeutral
                || !TryReadRelativeAccelerationVectors(sample, out var imu1Acceleration, out var imu2Acceleration))
            {
                return false;
            }

            if (!TryReadAccelerationOpeningDelta(imu1Pose.NeutralAcceleration, imu1Acceleration, HashiImuId.Imu1, out var imu1Opening)
                || !TryReadAccelerationOpeningDelta(imu2Pose.NeutralAcceleration, imu2Acceleration, HashiImuId.Imu2, out var imu2Opening))
            {
                return false;
            }

            var relativeOpening = imu1Opening - imu2Opening;
            openingDegrees = ClampOpeningAngle(relativeOpening);
            return true;
        }

        private static bool TryReadAccelerationOpeningDelta(
            Vector3 neutralAcceleration,
            Vector3 currentAcceleration,
            HashiImuId imuId,
            out float openingDegrees)
        {
            openingDegrees = 0f;
            if (neutralAcceleration.sqrMagnitude < 0.000001f || currentAcceleration.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            var openingAxis = GetImuOpeningAxisVector(imuId);
            var neutralProjected = Vector3.ProjectOnPlane(neutralAcceleration, openingAxis);
            var currentProjected = Vector3.ProjectOnPlane(currentAcceleration, openingAxis);
            if (neutralProjected.sqrMagnitude < 0.000001f || currentProjected.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            var signedAngle = Vector3.SignedAngle(
                neutralProjected.normalized,
                currentProjected.normalized,
                openingAxis);
            if (float.IsNaN(signedAngle))
            {
                return false;
            }

            openingDegrees = signedAngle * GetImuOpeningDirection(imuId) * GetImuOpeningScale(imuId);
            return true;
        }

        private static bool TryReadRelativeAccelerationAngle(HashiBleSample sample, out float angleDegrees)
        {
            angleDegrees = 0f;
            if (!TryReadRelativeAccelerationVectors(sample, out var imu1Acceleration, out var imu2Acceleration))
            {
                return false;
            }

            angleDegrees = Vector3.Angle(imu1Acceleration, imu2Acceleration);
            return !float.IsNaN(angleDegrees);
        }

        private static bool TryReadRelativeAccelerationVectors(
            HashiBleSample sample,
            out Vector3 imu1Acceleration,
            out Vector3 imu2Acceleration)
        {
            imu1Acceleration = Vector3.zero;
            imu2Acceleration = Vector3.zero;
            if (!sample.HasDualImu)
            {
                return false;
            }

            imu1Acceleration = MapImu1Acceleration(sample.Imu1AccelerationRaw);
            imu2Acceleration = MapImu2Acceleration(sample.Imu2AccelerationRaw);
            if (imu1Acceleration.sqrMagnitude < 0.000001f || imu2Acceleration.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            var toleranceG = Mathf.Max(0f, AccelerationCorrectionMagnitudeToleranceG);
            if (toleranceG > 0f)
            {
                var imu1MagnitudeG = imu1Acceleration.magnitude * RawAccelerationToG;
                var imu2MagnitudeG = imu2Acceleration.magnitude * RawAccelerationToG;
                if (Mathf.Abs(imu1MagnitudeG - 1f) > toleranceG
                    || Mathf.Abs(imu2MagnitudeG - 1f) > toleranceG)
                {
                    return false;
                }
            }

            imu1Acceleration.Normalize();
            imu2Acceleration.Normalize();
            return true;
        }

        private static HashiImuAxis GetImuOpeningAxis(HashiImuId imuId)
        {
            return imuId == HashiImuId.Imu1 ? Imu1OpeningAxis : Imu2OpeningAxis;
        }

        private static float GetImuOpeningDirection(HashiImuId imuId)
        {
            return Sign(imuId == HashiImuId.Imu1 ? Imu1OpeningDirection : Imu2OpeningDirection);
        }

        private static float GetImuOpeningScale(HashiImuId imuId)
        {
            return Mathf.Max(0f, imuId == HashiImuId.Imu1 ? Imu1OpeningScale : Imu2OpeningScale);
        }

        private static Vector3 GetImuOpeningAxisVector(HashiImuId imuId)
        {
            switch (GetImuOpeningAxis(imuId))
            {
                case HashiImuAxis.X:
                    return Vector3.right;
                case HashiImuAxis.Y:
                    return Vector3.up;
                case HashiImuAxis.Z:
                    return Vector3.forward;
                default:
                    return Vector3.right;
            }
        }

        private static float ClampOpeningAngle(float angle)
        {
            var min = Mathf.Min(OpeningAngleMinDegrees, OpeningAngleMaxDegrees);
            var max = Mathf.Max(OpeningAngleMinDegrees, OpeningAngleMaxDegrees);
            return Mathf.Clamp(angle, min, max);
        }

        private static bool TryGetImuAccelerationRelativeRotation(
            ref ImuPoseTracker tracker,
            HashiBleSample sample,
            HashiImuId imuId,
            out Quaternion outputRotation)
        {
            outputRotation = Quaternion.identity;
            if (!TryEnsureImuNeutral(ref tracker, sample, imuId))
            {
                return false;
            }

            return TryBuildRelativeRotation(
                tracker.NeutralAcceleration,
                MapImuAcceleration(sample, imuId),
                IsImuPoseInverted(imuId),
                GetImuOutputEuler(imuId),
                out outputRotation);
        }

        private static bool TryEnsureImuNeutral(ref ImuPoseTracker tracker, HashiBleSample sample, HashiImuId imuId)
        {
            if (!HasImuSample(sample, imuId))
            {
                return false;
            }

            if (AutoCaptureIndependentNeutral && !tracker.HasNeutral)
            {
                CaptureImuNeutral(ref tracker, sample, imuId);
            }

            return tracker.HasNeutral;
        }

        private static Quaternion IntegrateImuPose(
            Quaternion currentRotation,
            Vector3 gyroDegreesPerSecond,
            Vector3 accelerationRaw,
            Vector3 neutralAcceleration,
            float deltaTime)
        {
            gyroDegreesPerSecond = ApplyGyroDeadzone(gyroDegreesPerSecond);

            if (gyroDegreesPerSecond.sqrMagnitude > 0.000001f)
            {
                var angularSpeed = gyroDegreesPerSecond.magnitude;
                currentRotation *= Quaternion.AngleAxis(
                    angularSpeed * Mathf.Max(0f, deltaTime),
                    gyroDegreesPerSecond / angularSpeed);
                currentRotation = Normalize(currentRotation);
            }

            return ApplyAccelerationCorrection(currentRotation, neutralAcceleration, accelerationRaw);
        }

        private static Vector3 ApplyGyroDeadzone(Vector3 gyroDegreesPerSecond)
        {
            var threshold = Mathf.Max(0f, GyroDeadzoneDegreesPerSecond);
            if (threshold <= 0f)
            {
                return gyroDegreesPerSecond;
            }

            return new Vector3(
                Mathf.Abs(gyroDegreesPerSecond.x) < threshold ? 0f : gyroDegreesPerSecond.x,
                Mathf.Abs(gyroDegreesPerSecond.y) < threshold ? 0f : gyroDegreesPerSecond.y,
                Mathf.Abs(gyroDegreesPerSecond.z) < threshold ? 0f : gyroDegreesPerSecond.z);
        }

        private static Quaternion ApplyAccelerationCorrection(
            Quaternion currentRotation,
            Vector3 neutralAcceleration,
            Vector3 accelerationRaw)
        {
            if (AccelerationCorrectionWeight <= 0f
                || neutralAcceleration.sqrMagnitude < 0.000001f
                || accelerationRaw.sqrMagnitude < 0.000001f)
            {
                return currentRotation;
            }

            var magnitudeG = accelerationRaw.magnitude * RawAccelerationToG;
            var toleranceG = Mathf.Max(0f, AccelerationCorrectionMagnitudeToleranceG);
            if (Mathf.Abs(magnitudeG - 1f) > toleranceG)
            {
                return currentRotation;
            }

            var predictedAcceleration = currentRotation * neutralAcceleration.normalized;
            var measuredAcceleration = accelerationRaw.normalized;
            var correctionAngle = Vector3.Angle(predictedAcceleration, measuredAcceleration);
            if (float.IsNaN(correctionAngle)
                || correctionAngle < Mathf.Max(0f, AccelerationCorrectionDeadzoneDegrees))
            {
                return currentRotation;
            }

            var correctionRotation = Quaternion.FromToRotation(predictedAcceleration, measuredAcceleration);
            var weightedCorrection = Quaternion.Slerp(
                Quaternion.identity,
                correctionRotation,
                Mathf.Clamp01(AccelerationCorrectionWeight));
            return Normalize(weightedCorrection * currentRotation);
        }

        private static Quaternion ApplyIndependentOutputWeight(Quaternion rotation, bool invert, Vector3 outputEuler)
        {
            var outputBasis = Quaternion.Euler(outputEuler);
            rotation = outputBasis * rotation * Quaternion.Inverse(outputBasis);

            if (invert)
            {
                rotation = Quaternion.Inverse(rotation);
            }

            return Quaternion.Slerp(Quaternion.identity, rotation, Mathf.Clamp01(IndependentPoseWeight));
        }

        private static Quaternion BuildImuOutput(Quaternion rotation, HashiImuId imuId)
        {
            return ApplyIndependentOutputWeight(rotation, IsImuPoseInverted(imuId), GetImuOutputEuler(imuId));
        }

        private static Quaternion BuildOpeningAxisOutput(float openingAngleDegrees, HashiImuId imuId, Vector3 openingAxis)
        {
            var axis = openingAxis.sqrMagnitude > 0.0001f ? openingAxis.normalized : Vector3.forward;
            var angle = IsImuPoseInverted(imuId) ? -openingAngleDegrees : openingAngleDegrees;
            return Quaternion.AngleAxis(angle * Mathf.Clamp01(IndependentPoseWeight), axis);
        }

        private static bool HasImuSample(HashiBleSample sample, HashiImuId imuId)
        {
            return imuId == HashiImuId.Imu1 ? sample.HasImu1 : sample.HasImu2;
        }

        private static Vector3 MapImuAcceleration(HashiBleSample sample, HashiImuId imuId)
        {
            return imuId == HashiImuId.Imu1
                ? MapImu1Acceleration(sample.Imu1AccelerationRaw)
                : MapImu2Acceleration(sample.Imu2AccelerationRaw);
        }

        private static Vector3 MapImuGyro(HashiBleSample sample, HashiImuId imuId)
        {
            return imuId == HashiImuId.Imu1
                ? MapImu1Gyro(sample.Imu1GyroRaw)
                : MapImu2Gyro(sample.Imu2GyroRaw);
        }

        private static bool IsImuPoseInverted(HashiImuId imuId)
        {
            return imuId == HashiImuId.Imu1 ? InvertImu1Pose : InvertImu2Pose;
        }

        private static Vector3 GetImuOutputEuler(HashiImuId imuId)
        {
            return imuId == HashiImuId.Imu1 ? UpperOutputEuler : LowerOutputEuler;
        }

        private static HashiImuPoseDebugState BuildDebugState(ImuPoseTracker tracker)
        {
            return new HashiImuPoseDebugState
            {
                HasNeutral = tracker.HasNeutral,
                NeutralAcceleration = tracker.NeutralAcceleration,
                HasIntegratedPose = tracker.HasIntegratedPose,
                IntegratedRotation = tracker.IntegratedRotation,
                OpeningAngleDegrees = tracker.OpeningAngleDegrees,
                OpeningVelocityDegreesPerSecond = tracker.OpeningVelocityDegreesPerSecond,
                HasRelativeOpeningNeutral = tracker.HasRelativeOpeningNeutral,
                RelativeOpeningNeutralAngleDegrees = tracker.RelativeOpeningNeutralAngleDegrees,
                RelativeOpeningLowerToUpperRotation = tracker.RelativeOpeningLowerToUpperRotation,
                HasRelativeOpeningAcceleration = tracker.HasRelativeOpeningAcceleration,
                RelativeOpeningAccelerationDegrees = tracker.RelativeOpeningAccelerationDegrees,
                RelativeOpeningGyroWeight = tracker.RelativeOpeningGyroWeight,
                RelativeOpeningGyroMotionDegreesPerSecond = tracker.RelativeOpeningGyroMotionDegreesPerSecond,
                RelativeOpeningVelocityDominance = tracker.RelativeOpeningVelocityDominance,
                RelativeOpeningAccelerationCorrectionAllowed = tracker.RelativeOpeningAccelerationCorrectionAllowed,
                LastSampleSequence = tracker.LastSampleSequence,
                LastSampleRealtime = tracker.LastSampleRealtime,
                ZeroSequence = tracker.ZeroSequence
            };
        }

        private static HashiPositionDebugState BuildPositionDebugState(PositionTracker tracker)
        {
            return new HashiPositionDebugState
            {
                HasNeutral = tracker.HasNeutral,
                NeutralAccelerationG = tracker.NeutralAccelerationG,
                LinearAccelerationG = tracker.LinearAccelerationG,
                Velocity = tracker.Velocity,
                Offset = tracker.Offset,
                LastSampleSequence = tracker.LastSampleSequence,
                LastSampleRealtime = tracker.LastSampleRealtime
            };
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

        private static bool TryExtractTwist(Quaternion rotation, Vector3 axis, out Quaternion twist)
        {
            twist = Quaternion.identity;
            if (axis.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            axis.Normalize();
            var vectorPart = new Vector3(rotation.x, rotation.y, rotation.z);
            var projected = Vector3.Project(vectorPart, axis);
            twist = Normalize(new Quaternion(projected.x, projected.y, projected.z, rotation.w));
            return true;
        }

        private static bool TryBuildRelativeRotation(
            Vector3 neutralAcceleration,
            Vector3 currentAcceleration,
            bool invert,
            Vector3 outputEuler,
            out Quaternion rotation)
        {
            rotation = Quaternion.identity;

            if (neutralAcceleration.sqrMagnitude < 0.000001f || currentAcceleration.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            rotation = Quaternion.FromToRotation(neutralAcceleration.normalized, currentAcceleration.normalized);
            if (invert)
            {
                rotation = Quaternion.Inverse(rotation);
            }

            rotation = ApplyIndependentOutputWeight(rotation, false, outputEuler);
            return true;
        }

        private static Vector3 MultiplyComponents(Vector3 value, Vector3 multiplier)
        {
            return new Vector3(
                value.x * multiplier.x,
                value.y * multiplier.y,
                value.z * multiplier.z);
        }

        private static Vector3 MapVector(
            Vector3 raw,
            HashiImuAxis mapX,
            HashiImuAxis mapY,
            HashiImuAxis mapZ,
            Vector3 signs)
        {
            return new Vector3(
                ReadAxis(raw, mapX) * Sign(signs.x),
                ReadAxis(raw, mapY) * Sign(signs.y),
                ReadAxis(raw, mapZ) * Sign(signs.z));
        }

        private static float Sign(float value)
        {
            return value < 0f ? -1f : 1f;
        }

        private static float MaxAbs(Vector3 value)
        {
            return Mathf.Max(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private struct ImuPoseTracker
        {
            public bool HasNeutral;
            public Vector3 NeutralAcceleration;
            public bool HasIntegratedPose;
            public Quaternion IntegratedRotation;
            public float OpeningAngleDegrees;
            public float OpeningVelocityDegreesPerSecond;
            public bool HasRelativeOpeningNeutral;
            public float RelativeOpeningNeutralAngleDegrees;
            public Quaternion RelativeOpeningLowerToUpperRotation;
            public bool HasRelativeOpeningAcceleration;
            public float RelativeOpeningAccelerationDegrees;
            public float RelativeOpeningGyroWeight;
            public float RelativeOpeningGyroMotionDegreesPerSecond;
            public float RelativeOpeningVelocityDominance;
            public bool RelativeOpeningAccelerationCorrectionAllowed;
            public float LastSampleRealtime;
            public int LastSampleSequence;
            public int ZeroSequence;

            public static ImuPoseTracker Create()
            {
                var tracker = new ImuPoseTracker();
                tracker.Clear();
                return tracker;
            }

            public void Clear()
            {
                HasNeutral = false;
                NeutralAcceleration = Vector3.zero;
                ResetIntegratedPose();
            }

            public void CaptureNeutral(Vector3 neutralAcceleration, HashiBleSample sample)
            {
                NeutralAcceleration = neutralAcceleration;
                HasNeutral = neutralAcceleration.sqrMagnitude > 0.000001f;
                ResetIntegratedPose();
                LastSampleRealtime = sample.RealtimeReceived;
                LastSampleSequence = sample.Sequence;
            }

            public void ResetIntegratedPose()
            {
                HasIntegratedPose = false;
                IntegratedRotation = Quaternion.identity;
                OpeningAngleDegrees = 0f;
                OpeningVelocityDegreesPerSecond = 0f;
                HasRelativeOpeningNeutral = false;
                RelativeOpeningNeutralAngleDegrees = 0f;
                RelativeOpeningLowerToUpperRotation = Quaternion.identity;
                HasRelativeOpeningAcceleration = false;
                RelativeOpeningAccelerationDegrees = 0f;
                RelativeOpeningGyroWeight = 1f;
                RelativeOpeningGyroMotionDegreesPerSecond = 0f;
                RelativeOpeningVelocityDominance = 0f;
                RelativeOpeningAccelerationCorrectionAllowed = false;
                LastSampleRealtime = -1f;
                LastSampleSequence = -1;
                ZeroSequence = -1;
            }

            public void StartIntegratedPose(HashiBleSample sample)
            {
                HasIntegratedPose = true;
                IntegratedRotation = Quaternion.identity;
                OpeningAngleDegrees = 0f;
                OpeningVelocityDegreesPerSecond = 0f;
                LastSampleRealtime = sample.RealtimeReceived;
                LastSampleSequence = sample.Sequence;
            }

            public void ZeroIntegratedPose(HashiBleSample sample)
            {
                StartIntegratedPose(sample);
                ZeroSequence = sample.Sequence;
            }
        }

        private struct PositionTracker
        {
            public bool HasNeutral;
            public Vector3 NeutralAccelerationG;
            public Vector3 LinearAccelerationG;
            public Vector3 Velocity;
            public Vector3 Offset;
            public float LastSampleRealtime;
            public int LastSampleSequence;

            public static PositionTracker Create()
            {
                var tracker = new PositionTracker();
                tracker.Clear();
                return tracker;
            }

            public void Clear()
            {
                HasNeutral = false;
                NeutralAccelerationG = Vector3.zero;
                LinearAccelerationG = Vector3.zero;
                ResetMotion();
            }

            public void CaptureNeutral(Vector3 neutralAccelerationG, HashiBleSample sample)
            {
                NeutralAccelerationG = neutralAccelerationG;
                HasNeutral = neutralAccelerationG.sqrMagnitude > 0.000001f;
                ResetMotion();
                LastSampleRealtime = sample.RealtimeReceived;
                LastSampleSequence = sample.Sequence;
            }

            public void ResetMotion()
            {
                LinearAccelerationG = Vector3.zero;
                Velocity = Vector3.zero;
                Offset = Vector3.zero;
                LastSampleRealtime = -1f;
                LastSampleSequence = -1;
            }
        }
    }
}
