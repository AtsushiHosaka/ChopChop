using UnityEngine;

namespace ChopChop.BLE
{
    public struct HashiDualImuPose
    {
        public bool HasPose;
        public bool IsCalibrated;
        public bool IsCalibrating;
        public int CalibrationCount;
        public int CalibrationTarget;
        public Quaternion ParentRotation;
        public float OpeningDegrees;
        public float OpeningVelocityDegreesPerSecond;
        public Vector3 UpperAccelerationG;
        public Vector3 LowerAccelerationG;
        public Vector3 UpperGyroDps;
        public Vector3 LowerGyroDps;
        public float LastDeltaTime;
        public double LastSampleTimeSeconds;
    }

    public sealed class HashiDualImuFusion
    {
        public float RawAccelerationToG = HashiBleDefaults.RawAccelerationToG;
        public float RawGyroToDegreesPerSecond = HashiBleDefaults.RawGyroToDegreesPerSecond;
        public bool Imu1IsUpper = true;
        public HashiImuAxisMapping Imu1Mapping = new HashiImuAxisMapping();
        public HashiImuAxisMapping Imu2Mapping = new HashiImuAxisMapping();

        public HashiImuAxis OpeningAxis = HashiImuAxis.X;
        public float OpeningDirection = 1f;
        public float OpeningMinDegrees = 0f;
        public float OpeningMaxDegrees = 35f;
        public float OpeningGyroDeadzoneDegreesPerSecond = 1.5f;
        public float OpeningAccelerationCorrectionRate = 10f;

        public HashiParentMotionSource ParentMotionSource = HashiParentMotionSource.Lower;
        public float ParentGyroDeadzoneDegreesPerSecond = 1.0f;
        public float ParentTiltCorrectionRate = 4f;
        public float AccelerationMagnitudeToleranceG = 0.35f;
        public float CorrectionFullBelowDegreesPerSecond = 50f;
        public float CorrectionZeroAboveDegreesPerSecond = 260f;
        public float MaxDeltaTimeSeconds = 0.12f;
        public float MaxSampleGapSeconds = 0.7f;

        private bool isCalibrated;
        private bool isCalibrating;
        private int calibrationTarget = 50;
        private int calibrationCount;
        private Vector3 upperNeutralAccelerationG;
        private Vector3 lowerNeutralAccelerationG;
        private Vector3 upperGyroBiasDps;
        private Vector3 lowerGyroBiasDps;
        private Vector3 calibrationUpperAccSum;
        private Vector3 calibrationLowerAccSum;
        private Vector3 calibrationUpperGyroSum;
        private Vector3 calibrationLowerGyroSum;
        private Quaternion parentRotation = Quaternion.identity;
        private float openingDegrees;
        private float openingVelocityDegreesPerSecond;
        private double lastSampleTimeSeconds = -1.0;
        private int lastSequence = -1;
        private HashiDualImuPose currentPose;

        public HashiDualImuPose CurrentPose
        {
            get { return currentPose; }
        }

        public bool IsCalibrated
        {
            get { return isCalibrated; }
        }

        public bool IsCalibrating
        {
            get { return isCalibrating; }
        }

        public void Reset()
        {
            isCalibrated = false;
            isCalibrating = false;
            calibrationCount = 0;
            upperNeutralAccelerationG = Vector3.zero;
            lowerNeutralAccelerationG = Vector3.zero;
            upperGyroBiasDps = Vector3.zero;
            lowerGyroBiasDps = Vector3.zero;
            calibrationUpperAccSum = Vector3.zero;
            calibrationLowerAccSum = Vector3.zero;
            calibrationUpperGyroSum = Vector3.zero;
            calibrationLowerGyroSum = Vector3.zero;
            parentRotation = Quaternion.identity;
            openingDegrees = 0f;
            openingVelocityDegreesPerSecond = 0f;
            lastSampleTimeSeconds = -1.0;
            lastSequence = -1;
            currentPose = new HashiDualImuPose();
        }

        public void BeginCalibration(int sampleCount)
        {
            isCalibrated = false;
            isCalibrating = true;
            calibrationTarget = Mathf.Max(5, sampleCount);
            calibrationCount = 0;
            calibrationUpperAccSum = Vector3.zero;
            calibrationLowerAccSum = Vector3.zero;
            calibrationUpperGyroSum = Vector3.zero;
            calibrationLowerGyroSum = Vector3.zero;
            parentRotation = Quaternion.identity;
            openingDegrees = 0f;
            openingVelocityDegreesPerSecond = 0f;
            lastSampleTimeSeconds = -1.0;
            lastSequence = -1;
            UpdatePose(new Measurement(), 0f);
        }

        public void ZeroPose()
        {
            parentRotation = Quaternion.identity;
            openingDegrees = 0f;
            openingVelocityDegreesPerSecond = 0f;
            lastSampleTimeSeconds = -1.0;
            lastSequence = -1;
            UpdatePose(new Measurement(), 0f);
        }

        public bool Consume(HashiBleSample sample, float fallbackDeltaTime)
        {
            if (!sample.HasDualImu)
            {
                return false;
            }

            var measurement = BuildMeasurement(sample);
            if (!measurement.IsValid)
            {
                return false;
            }

            if (isCalibrating)
            {
                AccumulateCalibration(measurement, sample);
                return false;
            }

            if (!isCalibrated)
            {
                CaptureNeutral(measurement, sample);
            }

            var deltaTime = ResolveDeltaTime(sample, fallbackDeltaTime);
            if (deltaTime <= 0f)
            {
                UpdatePose(measurement, 0f);
                return true;
            }

            var upperGyro = ApplyDeadzone(measurement.UpperGyroDps - upperGyroBiasDps, ParentGyroDeadzoneDegreesPerSecond);
            var lowerGyro = ApplyDeadzone(measurement.LowerGyroDps - lowerGyroBiasDps, ParentGyroDeadzoneDegreesPerSecond);
            var parentGyro = SelectParentGyro(upperGyro, lowerGyro);
            parentRotation = IntegrateRotation(parentRotation, parentGyro, deltaTime);
            parentRotation = CorrectRotationWithAcceleration(
                parentRotation,
                SelectParentNeutralAcceleration(),
                SelectParentAcceleration(measurement),
                parentGyro,
                deltaTime,
                ParentTiltCorrectionRate);

            var relativeGyro = upperGyro - lowerGyro;
            openingVelocityDegreesPerSecond = HashiImuAxisMapping.ReadAxis(relativeGyro, OpeningAxis) * Sign(OpeningDirection);
            if (Mathf.Abs(openingVelocityDegreesPerSecond) < Mathf.Max(0f, OpeningGyroDeadzoneDegreesPerSecond))
            {
                openingVelocityDegreesPerSecond = 0f;
            }

            openingDegrees = ClampOpening(openingDegrees + openingVelocityDegreesPerSecond * deltaTime);

            if (TryReadAccelerationOpening(measurement, out var accelerationOpening)
                && ShouldUseAccelerationForOpening(measurement, upperGyro, lowerGyro))
            {
                var blend = ExponentialBlend(OpeningAccelerationCorrectionRate, deltaTime);
                openingDegrees = ClampOpening(Mathf.Lerp(openingDegrees, accelerationOpening, blend));
            }

            UpdatePose(measurement, deltaTime);
            return true;
        }

        private void AccumulateCalibration(Measurement measurement, HashiBleSample sample)
        {
            calibrationUpperAccSum += SafeNormalized(measurement.UpperAccelerationG);
            calibrationLowerAccSum += SafeNormalized(measurement.LowerAccelerationG);
            calibrationUpperGyroSum += measurement.UpperGyroDps;
            calibrationLowerGyroSum += measurement.LowerGyroDps;
            calibrationCount++;

            if (calibrationCount < calibrationTarget)
            {
                UpdatePose(measurement, 0f);
                return;
            }

            upperNeutralAccelerationG = SafeNormalized(calibrationUpperAccSum / calibrationCount);
            lowerNeutralAccelerationG = SafeNormalized(calibrationLowerAccSum / calibrationCount);
            upperGyroBiasDps = calibrationUpperGyroSum / calibrationCount;
            lowerGyroBiasDps = calibrationLowerGyroSum / calibrationCount;
            isCalibrated = upperNeutralAccelerationG.sqrMagnitude > 0.000001f
                && lowerNeutralAccelerationG.sqrMagnitude > 0.000001f;
            isCalibrating = false;
            parentRotation = Quaternion.identity;
            openingDegrees = 0f;
            openingVelocityDegreesPerSecond = 0f;
            lastSampleTimeSeconds = sample.BestTimeSeconds;
            lastSequence = sample.HasSequence ? sample.Sequence : -1;
            UpdatePose(measurement, 0f);
        }

        private void CaptureNeutral(Measurement measurement, HashiBleSample sample)
        {
            upperNeutralAccelerationG = SafeNormalized(measurement.UpperAccelerationG);
            lowerNeutralAccelerationG = SafeNormalized(measurement.LowerAccelerationG);
            upperGyroBiasDps = Vector3.zero;
            lowerGyroBiasDps = Vector3.zero;
            isCalibrated = upperNeutralAccelerationG.sqrMagnitude > 0.000001f
                && lowerNeutralAccelerationG.sqrMagnitude > 0.000001f;
            parentRotation = Quaternion.identity;
            openingDegrees = 0f;
            openingVelocityDegreesPerSecond = 0f;
            lastSampleTimeSeconds = sample.BestTimeSeconds;
            lastSequence = sample.HasSequence ? sample.Sequence : -1;
        }

        private Measurement BuildMeasurement(HashiBleSample sample)
        {
            var imu1Acc = Imu1Mapping.MapAccelerationRaw(sample.Imu1AccelerationRaw) * RawAccelerationToG;
            var imu2Acc = Imu2Mapping.MapAccelerationRaw(sample.Imu2AccelerationRaw) * RawAccelerationToG;
            var imu1Gyro = Imu1Mapping.MapGyroRawToDps(sample.Imu1GyroRaw, RawGyroToDegreesPerSecond);
            var imu2Gyro = Imu2Mapping.MapGyroRawToDps(sample.Imu2GyroRaw, RawGyroToDegreesPerSecond);

            return new Measurement
            {
                IsValid = true,
                UpperAccelerationG = Imu1IsUpper ? imu1Acc : imu2Acc,
                UpperGyroDps = Imu1IsUpper ? imu1Gyro : imu2Gyro,
                LowerAccelerationG = Imu1IsUpper ? imu2Acc : imu1Acc,
                LowerGyroDps = Imu1IsUpper ? imu2Gyro : imu1Gyro
            };
        }

        private float ResolveDeltaTime(HashiBleSample sample, float fallbackDeltaTime)
        {
            if (sample.HasSequence && sample.Sequence == lastSequence)
            {
                return 0f;
            }

            var sampleTime = sample.BestTimeSeconds;
            var delta = lastSampleTimeSeconds > 0.0 ? sampleTime - lastSampleTimeSeconds : Mathf.Max(0f, fallbackDeltaTime);
            lastSampleTimeSeconds = sampleTime;
            lastSequence = sample.HasSequence ? sample.Sequence : -1;

            if (delta < 0.0005)
            {
                delta = Mathf.Max(0f, fallbackDeltaTime);
            }

            var maxGap = Mathf.Max(0f, MaxSampleGapSeconds);
            if (maxGap > 0f && delta > maxGap)
            {
                return 0f;
            }

            return Mathf.Clamp((float)delta, 0f, Mathf.Max(0.001f, MaxDeltaTimeSeconds));
        }

        private Vector3 SelectParentGyro(Vector3 upperGyro, Vector3 lowerGyro)
        {
            switch (ParentMotionSource)
            {
                case HashiParentMotionSource.Upper:
                    return upperGyro;
                case HashiParentMotionSource.Average:
                    return (upperGyro + lowerGyro) * 0.5f;
                case HashiParentMotionSource.Lower:
                default:
                    return lowerGyro;
            }
        }

        private Vector3 SelectParentAcceleration(Measurement measurement)
        {
            switch (ParentMotionSource)
            {
                case HashiParentMotionSource.Upper:
                    return measurement.UpperAccelerationG;
                case HashiParentMotionSource.Average:
                    return (measurement.UpperAccelerationG + measurement.LowerAccelerationG) * 0.5f;
                case HashiParentMotionSource.Lower:
                default:
                    return measurement.LowerAccelerationG;
            }
        }

        private Vector3 SelectParentNeutralAcceleration()
        {
            switch (ParentMotionSource)
            {
                case HashiParentMotionSource.Upper:
                    return upperNeutralAccelerationG;
                case HashiParentMotionSource.Average:
                    return SafeNormalized(upperNeutralAccelerationG + lowerNeutralAccelerationG);
                case HashiParentMotionSource.Lower:
                default:
                    return lowerNeutralAccelerationG;
            }
        }

        private Quaternion CorrectRotationWithAcceleration(
            Quaternion currentRotation,
            Vector3 neutralAcceleration,
            Vector3 measuredAcceleration,
            Vector3 gyroDegreesPerSecond,
            float deltaTime,
            float correctionRate)
        {
            if (correctionRate <= 0f || deltaTime <= 0f)
            {
                return currentRotation;
            }

            if (!IsStableAcceleration(measuredAcceleration))
            {
                return currentRotation;
            }

            if (neutralAcceleration.sqrMagnitude < 0.000001f)
            {
                return currentRotation;
            }

            var motionWeight = 1f - Mathf.InverseLerp(
                Mathf.Max(0f, CorrectionFullBelowDegreesPerSecond),
                Mathf.Max(CorrectionFullBelowDegreesPerSecond + 1f, CorrectionZeroAboveDegreesPerSecond),
                gyroDegreesPerSecond.magnitude);
            motionWeight = Mathf.Clamp01(motionWeight);
            if (motionWeight <= 0f)
            {
                return currentRotation;
            }

            var predicted = currentRotation * neutralAcceleration.normalized;
            var measured = measuredAcceleration.normalized;
            if (predicted.sqrMagnitude < 0.000001f || measured.sqrMagnitude < 0.000001f)
            {
                return currentRotation;
            }

            var correction = Quaternion.FromToRotation(predicted, measured);
            var blend = ExponentialBlend(correctionRate, deltaTime) * motionWeight;
            return Normalize(Quaternion.Slerp(Quaternion.identity, correction, blend) * currentRotation);
        }

        private bool TryReadAccelerationOpening(Measurement measurement, out float degrees)
        {
            degrees = 0f;
            if (!TryReadOpeningDelta(upperNeutralAccelerationG, measurement.UpperAccelerationG, out var upperDelta)
                || !TryReadOpeningDelta(lowerNeutralAccelerationG, measurement.LowerAccelerationG, out var lowerDelta))
            {
                return false;
            }

            degrees = ClampOpening((upperDelta - lowerDelta) * Sign(OpeningDirection));
            return true;
        }

        private bool TryReadOpeningDelta(Vector3 neutralAcceleration, Vector3 currentAcceleration, out float degrees)
        {
            degrees = 0f;
            if (neutralAcceleration.sqrMagnitude < 0.000001f || currentAcceleration.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            var axis = HashiImuAxisMapping.AxisVector(OpeningAxis);
            var neutralProjected = Vector3.ProjectOnPlane(neutralAcceleration, axis);
            var currentProjected = Vector3.ProjectOnPlane(currentAcceleration, axis);
            if (neutralProjected.sqrMagnitude < 0.000001f || currentProjected.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            degrees = Vector3.SignedAngle(neutralProjected.normalized, currentProjected.normalized, axis);
            return !float.IsNaN(degrees);
        }

        private bool ShouldUseAccelerationForOpening(Measurement measurement, Vector3 upperGyro, Vector3 lowerGyro)
        {
            if (!IsStableAcceleration(measurement.UpperAccelerationG) || !IsStableAcceleration(measurement.LowerAccelerationG))
            {
                return false;
            }

            var gyroMotion = Mathf.Max(upperGyro.magnitude, lowerGyro.magnitude);
            return gyroMotion <= Mathf.Max(CorrectionFullBelowDegreesPerSecond + 1f, CorrectionZeroAboveDegreesPerSecond);
        }

        private bool IsStableAcceleration(Vector3 accelerationG)
        {
            var magnitude = accelerationG.magnitude;
            if (magnitude < 0.0001f)
            {
                return false;
            }

            var tolerance = Mathf.Max(0f, AccelerationMagnitudeToleranceG);
            return tolerance <= 0f || Mathf.Abs(magnitude - 1f) <= tolerance;
        }

        private float ClampOpening(float degrees)
        {
            var min = Mathf.Min(OpeningMinDegrees, OpeningMaxDegrees);
            var max = Mathf.Max(OpeningMinDegrees, OpeningMaxDegrees);
            return Mathf.Clamp(degrees, min, max);
        }

        private static Quaternion IntegrateRotation(Quaternion currentRotation, Vector3 gyroDegreesPerSecond, float deltaTime)
        {
            var speed = gyroDegreesPerSecond.magnitude;
            if (speed < 0.0001f || deltaTime <= 0f)
            {
                return currentRotation;
            }

            var deltaRotation = Quaternion.AngleAxis(speed * deltaTime, gyroDegreesPerSecond / speed);
            return Normalize(currentRotation * deltaRotation);
        }

        private static Vector3 ApplyDeadzone(Vector3 value, float deadzone)
        {
            if (deadzone <= 0f)
            {
                return value;
            }

            return new Vector3(
                Mathf.Abs(value.x) < deadzone ? 0f : value.x,
                Mathf.Abs(value.y) < deadzone ? 0f : value.y,
                Mathf.Abs(value.z) < deadzone ? 0f : value.z);
        }

        private static float ExponentialBlend(float rate, float deltaTime)
        {
            return rate <= 0f || deltaTime <= 0f ? 0f : 1f - Mathf.Exp(-rate * deltaTime);
        }

        private static Vector3 SafeNormalized(Vector3 value)
        {
            return value.sqrMagnitude < 0.000001f ? Vector3.zero : value.normalized;
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

        private static float Sign(float value)
        {
            return value < 0f ? -1f : 1f;
        }

        private void UpdatePose(Measurement measurement, float deltaTime)
        {
            currentPose = new HashiDualImuPose
            {
                HasPose = isCalibrated,
                IsCalibrated = isCalibrated,
                IsCalibrating = isCalibrating,
                CalibrationCount = calibrationCount,
                CalibrationTarget = calibrationTarget,
                ParentRotation = parentRotation,
                OpeningDegrees = openingDegrees,
                OpeningVelocityDegreesPerSecond = openingVelocityDegreesPerSecond,
                UpperAccelerationG = measurement.UpperAccelerationG,
                LowerAccelerationG = measurement.LowerAccelerationG,
                UpperGyroDps = measurement.UpperGyroDps - upperGyroBiasDps,
                LowerGyroDps = measurement.LowerGyroDps - lowerGyroBiasDps,
                LastDeltaTime = deltaTime,
                LastSampleTimeSeconds = lastSampleTimeSeconds
            };
        }

        private struct Measurement
        {
            public bool IsValid;
            public Vector3 UpperAccelerationG;
            public Vector3 LowerAccelerationG;
            public Vector3 UpperGyroDps;
            public Vector3 LowerGyroDps;
        }
    }
}
