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

    public static class HashiImuMappingRuntime
    {
        public const float Esp32Lsm6RawAccelerationToG = 0.000061f;

        public static bool UseTunedTheta { get; set; }
        public static HashiImuThetaMode ThetaMode { get; set; } = HashiImuThetaMode.AccelerationAngle;
        public static HashiImuMotionSource MotionSource { get; set; } = HashiImuMotionSource.Imu1;
        public static HashiImuAxis Imu1Axis { get; set; } = HashiImuAxis.Z;
        public static HashiImuAxis Imu2Axis { get; set; } = HashiImuAxis.Z;
        public static bool InvertTheta { get; set; }
        public static float InputClosed { get; set; } = 0f;
        public static float InputOpen { get; set; } = 180f;
        public static float OutputClosed { get; set; } = 0f;
        public static float OutputOpen { get; set; } = 180f;
        public static float RawAccelerationToG { get; set; } = HashiBleDefaults.RawAccelerationToG;
        public static float RawGyroToDegreesPerSecond { get; set; } = HashiBleDefaults.RawGyroToDegreesPerSecond;
        public static float ParentRotationWeight { get; set; } = 1f;
        public static bool UseIndependentImuPose { get; set; } = true;
        public static bool AutoCaptureIndependentNeutral { get; set; } = true;
        public static float IndependentPoseWeight { get; set; } = 1f;
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
        public static bool HasIndependentNeutral => hasImu1Neutral && hasImu2Neutral;

        private static bool hasImu1Neutral;
        private static bool hasImu2Neutral;
        private static Vector3 imu1NeutralAcceleration;
        private static Vector3 imu2NeutralAcceleration;

        public static void ResetDefaults()
        {
            UseTunedTheta = false;
            ThetaMode = HashiImuThetaMode.AccelerationAngle;
            MotionSource = HashiImuMotionSource.Imu1;
            Imu1Axis = HashiImuAxis.Z;
            Imu2Axis = HashiImuAxis.Z;
            InvertTheta = false;
            InputClosed = 0f;
            InputOpen = 180f;
            OutputClosed = 0f;
            OutputOpen = 180f;
            RawAccelerationToG = HashiBleDefaults.RawAccelerationToG;
            RawGyroToDegreesPerSecond = HashiBleDefaults.RawGyroToDegreesPerSecond;
            ParentRotationWeight = 1f;
            UseIndependentImuPose = true;
            AutoCaptureIndependentNeutral = true;
            IndependentPoseWeight = 1f;
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
            ClearIndependentNeutral();
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
                    acceleration = sample.Imu1AccelerationG;
                    gyro = sample.Imu1GyroDegreesPerSecond;
                    return true;
                case HashiImuMotionSource.Imu2:
                    acceleration = sample.Imu2AccelerationG;
                    gyro = sample.Imu2GyroDegreesPerSecond;
                    return true;
                case HashiImuMotionSource.Average:
                    acceleration = (sample.Imu1AccelerationG + sample.Imu2AccelerationG) * 0.5f;
                    gyro = (sample.Imu1GyroDegreesPerSecond + sample.Imu2GyroDegreesPerSecond) * 0.5f;
                    return true;
                case HashiImuMotionSource.Difference:
                    acceleration = sample.Imu1AccelerationG - sample.Imu2AccelerationG;
                    gyro = sample.Imu1GyroDegreesPerSecond - sample.Imu2GyroDegreesPerSecond;
                    return true;
                default:
                    return false;
            }
        }

        public static Quaternion ApplyParentRotationWeight(Quaternion rotation)
        {
            return Quaternion.Slerp(Quaternion.identity, rotation, Mathf.Clamp01(ParentRotationWeight));
        }

        public static void CaptureIndependentNeutral(HashiBleSample sample)
        {
            if (!sample.HasDualImu)
            {
                return;
            }

            imu1NeutralAcceleration = MapImu1Acceleration(sample.Imu1AccelerationRaw).normalized;
            imu2NeutralAcceleration = MapImu2Acceleration(sample.Imu2AccelerationRaw).normalized;
            hasImu1Neutral = imu1NeutralAcceleration.sqrMagnitude > 0.000001f;
            hasImu2Neutral = imu2NeutralAcceleration.sqrMagnitude > 0.000001f;
        }

        public static void ClearIndependentNeutral()
        {
            hasImu1Neutral = false;
            hasImu2Neutral = false;
            imu1NeutralAcceleration = Vector3.zero;
            imu2NeutralAcceleration = Vector3.zero;
        }

        public static bool TryGetIndependentRotations(
            HashiBleSample sample,
            out Quaternion upperRotation,
            out Quaternion lowerRotation)
        {
            upperRotation = Quaternion.identity;
            lowerRotation = Quaternion.identity;

            if (!sample.HasDualImu)
            {
                return false;
            }

            if (AutoCaptureIndependentNeutral && !HasIndependentNeutral)
            {
                CaptureIndependentNeutral(sample);
            }

            if (!HasIndependentNeutral)
            {
                return false;
            }

            return TryBuildRelativeRotation(
                    imu1NeutralAcceleration,
                    MapImu1Acceleration(sample.Imu1AccelerationRaw),
                    InvertImu1Pose,
                    out upperRotation)
                & TryBuildRelativeRotation(
                    imu2NeutralAcceleration,
                    MapImu2Acceleration(sample.Imu2AccelerationRaw),
                    InvertImu2Pose,
                    out lowerRotation);
        }

        public static Vector3 MapImu1Acceleration(Vector3 raw)
        {
            return MapVector(raw, Imu1MapX, Imu1MapY, Imu1MapZ, Imu1MapSigns);
        }

        public static Vector3 MapImu2Acceleration(Vector3 raw)
        {
            return MapVector(raw, Imu2MapX, Imu2MapY, Imu2MapZ, Imu2MapSigns);
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

        private static bool TryBuildRelativeRotation(
            Vector3 neutralAcceleration,
            Vector3 currentAcceleration,
            bool invert,
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

            rotation = Quaternion.Slerp(Quaternion.identity, rotation, Mathf.Clamp01(IndependentPoseWeight));
            return true;
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
    }
}
