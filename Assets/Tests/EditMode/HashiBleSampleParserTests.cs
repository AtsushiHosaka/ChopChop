using ChopChop.App;
using ChopChop.BLE;
using ChopChop.Input;
using NUnit.Framework;
using UnityEngine;

namespace ChopChop.Tests.EditMode
{
    public sealed class HashiBleSampleParserTests
    {
        [SetUp]
        public void SetUp()
        {
            HashiImuMappingRuntime.ResetDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            HashiImuMappingRuntime.ResetDefaults();
        }

        [Test]
        public void ScreenshotStylePayloadParsesAllKnownFields()
        {
            var json = "{\"theta\":26.57,\"pinch\":163.3,\"vel\":15.1,\"acc\":1422.6,\"ax\":1.42,\"ay\":-0.09,\"az\":0.19,\"gx\":-0.52,\"gy\":-8.58,\"gz\":-4.55,\"ts\":182142.890}";

            Assert.That(HashiBleSampleParser.TryParse(json, out var sample, out var error), Is.True, error);
            Assert.That(sample.ThetaValue, Is.EqualTo(26.57f).Within(0.001f));
            Assert.That(sample.PinchValue, Is.EqualTo(163.3f).Within(0.001f));
            Assert.That(sample.VelocityValue, Is.EqualTo(15.1f).Within(0.001f));
            Assert.That(sample.AccelerationMagnitudeValue, Is.EqualTo(1422.6f).Within(0.01f));
            Assert.That(sample.AxValue, Is.EqualTo(1.42f).Within(0.001f));
            Assert.That(sample.AyValue, Is.EqualTo(-0.09f).Within(0.001f));
            Assert.That(sample.AzValue, Is.EqualTo(0.19f).Within(0.001f));
            Assert.That(sample.GxValue, Is.EqualTo(-0.52f).Within(0.001f));
            Assert.That(sample.GyValue, Is.EqualTo(-8.58f).Within(0.001f));
            Assert.That(sample.GzValue, Is.EqualTo(-4.55f).Within(0.001f));
            Assert.That(sample.TimestampValue, Is.EqualTo(182142.890).Within(0.001));
        }

        [Test]
        public void MissingFieldsDoNotCrashOrInventValues()
        {
            Assert.That(HashiBleSampleParser.TryParse("{\"theta\":18.5,\"gx\":2.25}", out var sample, out var error), Is.True, error);
            Assert.That(sample.HasTheta, Is.True);
            Assert.That(sample.HasGx, Is.True);
            Assert.That(sample.HasGy, Is.False);
            Assert.That(sample.HasGz, Is.False);
            Assert.That(sample.HasAccelerationVector, Is.False);
        }

        [Test]
        public void DualImuCsvPayloadParsesEsp32NotifyFormat()
        {
            var payload = "IMU1,0,0,2048,0,100,0,3\nIMU2,0,1024,1774,0,200,0,3";

            Assert.That(HashiBleSampleParser.TryParse(payload, out var sample, out var error), Is.True, error);
            Assert.That(sample.ThetaValue, Is.EqualTo(30f).Within(0.1f));
            Assert.That(sample.AccelerationVector.z, Is.EqualTo(2048f * HashiBleDefaults.RawAccelerationToG).Within(0.0001f));
            Assert.That(sample.GyValue, Is.EqualTo(100f * HashiBleDefaults.RawGyroToDegreesPerSecond).Within(0.0001f));
            Assert.That(sample.AccelerationMagnitudeValue, Is.EqualTo(sample.AccelerationVector.magnitude).Within(0.0001f));
        }

        [Test]
        public void DualImuCsvPayloadRequiresBothRows()
        {
            Assert.That(HashiBleSampleParser.TryParse("IMU1,0,0,2048,0,100,0,3", out _, out var error), Is.False);
            Assert.That(error, Does.Contain("both IMU1 and IMU2"));
        }

        [Test]
        public void DualImuCsvPayloadCanUseTunedThetaMapping()
        {
            HashiImuMappingRuntime.UseTunedTheta = true;
            HashiImuMappingRuntime.ThetaMode = HashiImuThetaMode.AccelerationAngle;
            HashiImuMappingRuntime.InputClosed = 0f;
            HashiImuMappingRuntime.InputOpen = 60f;
            HashiImuMappingRuntime.OutputClosed = 5f;
            HashiImuMappingRuntime.OutputOpen = 35f;

            var payload = "IMU1,0,0,2048,0,100,0,3\nIMU2,0,1024,1774,0,200,0,3";

            Assert.That(HashiBleSampleParser.TryParse(payload, out var sample, out var error), Is.True, error);
            Assert.That(sample.HasDualImu, Is.True);
            Assert.That(sample.Imu1AccelerationRaw.z, Is.EqualTo(2048f).Within(0.0001f));
            Assert.That(sample.ThetaValue, Is.EqualTo(20f).Within(0.1f));
        }

        [Test]
        public void MalformedJsonReturnsFalseWithoutThrowing()
        {
            HashiBleSample sample = default;
            string error = null;

            Assert.DoesNotThrow(() => HashiBleSampleParser.TryParse("{\"theta\":", out sample, out error));
            Assert.That(HashiBleSampleParser.TryParse("{\"theta\":", out sample, out error), Is.False);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void BleReconnectPolicyRetriesRecoverableNativeStatesOnly()
        {
            Assert.That(BleHashiInputSource.ShouldAutoReconnect("Stopped"), Is.True);
            Assert.That(BleHashiInputSource.ShouldAutoReconnect("Failed: peripheral disconnected"), Is.True);
            Assert.That(BleHashiInputSource.ShouldAutoReconnect("PoweredOff: waiting for Bluetooth"), Is.True);
            Assert.That(BleHashiInputSource.ShouldAutoReconnect("Native plugin missing"), Is.False);
            Assert.That(BleHashiInputSource.ShouldAutoReconnect("Unsupported platform"), Is.False);
            Assert.That(BleHashiInputSource.ShouldAutoReconnect("Unauthorized: Bluetooth permission denied"), Is.False);
        }

        [Test]
        public void ParentPlanarGyroUsesYawWhenVerticalPitchIsWeak()
        {
            HashiImuMappingRuntime.ParentPlanarYawOnly = true;
            HashiImuMappingRuntime.ParentPlanarYawDirection = -1f;
            HashiImuMappingRuntime.ParentVerticalPitchWhenDominant = true;
            HashiImuMappingRuntime.ParentVerticalPitchScale = 0f;

            var filtered = HashiImuMappingRuntime.FilterParentRotationGyro(new Vector3(0f, -200f, 80f));

            Assert.That(filtered.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(filtered.y, Is.EqualTo(200f).Within(0.0001f));
            Assert.That(filtered.z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ParentPlanarGyroBlendsPitchWhenPitchDominates()
        {
            HashiImuMappingRuntime.ParentPlanarYawOnly = true;
            HashiImuMappingRuntime.ParentPlanarYawDirection = -1f;
            HashiImuMappingRuntime.ParentVerticalPitchWhenDominant = true;
            HashiImuMappingRuntime.ParentVerticalPitchScale = 1.35f;

            var filtered = HashiImuMappingRuntime.FilterParentRotationGyro(new Vector3(0f, -20f, 120f));

            Assert.That(filtered.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(filtered.y, Is.EqualTo(7f).Within(0.0001f));
            Assert.That(filtered.z, Is.EqualTo(162f).Within(0.0001f));
        }

        [Test]
        public void ParentPlanarGyroKeepsDownwardPitchWhenYawIsAlsoPresent()
        {
            HashiImuMappingRuntime.ParentPlanarYawOnly = true;
            HashiImuMappingRuntime.ParentPlanarYawDirection = -1f;
            HashiImuMappingRuntime.ParentVerticalPitchWhenDominant = true;
            HashiImuMappingRuntime.ParentVerticalPitchScale = 1f;

            var filtered = HashiImuMappingRuntime.FilterParentRotationGyro(new Vector3(0f, -120f, 120f));

            Assert.That(filtered.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(filtered.y, Is.GreaterThan(0f));
            Assert.That(filtered.z, Is.GreaterThan(70f));
        }

        [Test]
        public void ParentVerticalPitchScaleUpgradesLegacySerializedValues()
        {
            HashiImuMappingRuntime.ParentVerticalPitchScale = 1f;
            Assert.That(HashiImuMappingRuntime.ParentVerticalPitchScale, Is.EqualTo(1.1f).Within(0.0001f));

            HashiImuMappingRuntime.ParentVerticalPitchScale = 0.55f;
            Assert.That(HashiImuMappingRuntime.ParentVerticalPitchScale, Is.EqualTo(1.1f).Within(0.0001f));

            HashiImuMappingRuntime.ParentVerticalPitchScale = 0f;
            Assert.That(HashiImuMappingRuntime.ParentVerticalPitchScale, Is.EqualTo(0f).Within(0.0001f));

            HashiImuMappingRuntime.ParentVerticalPitchScale = 1.5f;
            Assert.That(HashiImuMappingRuntime.ParentVerticalPitchScale, Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void ParentPlanarGyroCanBoostDownwardPitchPastNeutralWeight()
        {
            HashiImuMappingRuntime.ParentPlanarYawOnly = true;
            HashiImuMappingRuntime.ParentPlanarYawDirection = -1f;
            HashiImuMappingRuntime.ParentVerticalPitchWhenDominant = true;
            HashiImuMappingRuntime.ParentVerticalPitchDominance = 1.35f;
            HashiImuMappingRuntime.ParentVerticalPitchScale = 1.35f;

            var filtered = HashiImuMappingRuntime.FilterParentRotationGyro(new Vector3(0f, -120f, 120f));
            var weighted = HashiImuMappingRuntime.ApplyParentRotationRateWeight(filtered);

            Assert.That(filtered.z, Is.EqualTo(162f).Within(0.0001f));
            Assert.That(weighted.z, Is.GreaterThan(70f));
        }

        [Test]
        public void BleDisconnectedPromptIsLocalizedInEverySupportedLanguage()
        {
            Assert.That(
                LocalizationCatalog.Japanese[LocalizationKeys.Hud.ConnectHashi],
                Is.EqualTo("BLEが接続されていません"));
            Assert.That(
                LocalizationCatalog.Japanese[LocalizationKeys.Hud.ResetHashi],
                Is.EqualTo("リセット"));
            Assert.That(
                LocalizationCatalog.English[LocalizationKeys.Hud.ConnectHashi],
                Is.EqualTo("BLE is not connected."));
            Assert.That(
                LocalizationCatalog.English[LocalizationKeys.Hud.ResetHashi],
                Is.EqualTo("Reset"));
            Assert.That(
                LocalizationCatalog.Korean[LocalizationKeys.Hud.ConnectHashi],
                Is.EqualTo("BLE가 연결되어 있지 않습니다."));
            Assert.That(
                LocalizationCatalog.Korean[LocalizationKeys.Hud.ResetHashi],
                Is.EqualTo("초기화"));
            Assert.That(
                LocalizationCatalog.ChineseSimplified[LocalizationKeys.Hud.ConnectHashi],
                Is.EqualTo("BLE 未连接。"));
            Assert.That(
                LocalizationCatalog.ChineseSimplified[LocalizationKeys.Hud.ResetHashi],
                Is.EqualTo("重置"));
            Assert.That(
                LocalizationCatalog.French[LocalizationKeys.Hud.ConnectHashi],
                Is.EqualTo("Le BLE n'est pas connecté."));
            Assert.That(
                LocalizationCatalog.French[LocalizationKeys.Hud.ResetHashi],
                Is.EqualTo("Réinitialiser"));
            Assert.That(
                LocalizationCatalog.Spanish[LocalizationKeys.Hud.ConnectHashi],
                Is.EqualTo("BLE no está conectado."));
            Assert.That(
                LocalizationCatalog.Spanish[LocalizationKeys.Hud.ResetHashi],
                Is.EqualTo("Reiniciar"));
        }

        [Test]
        public void RelativeOpeningClosingGyroCanBeatStaleAccelerationOpen()
        {
            HashiImuMappingRuntime.IndependentPoseMode = HashiIndependentPoseMode.RelativeOpeningIntegrated;
            HashiImuMappingRuntime.RawAccelerationToG = 1f;
            HashiImuMappingRuntime.RawGyroToDegreesPerSecond = 1f;
            HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond = 0f;
            HashiImuMappingRuntime.RelativeOpeningAccelerationWeight = 0.9f;
            HashiImuMappingRuntime.OpeningAngleStaticVelocityThresholdDps = 20f;

            var neutral = ParseDualImuCsv("IMU1,0,0,1,0,0,0,1\nIMU2,0,0,1,0,0,0,1");
            var open = ParseDualImuCsv("IMU1,0,-0.573576,0.819152,350,0,0,2\nIMU2,0,0,1,0,0,0,2");
            var closeWithStaleOpenAcceleration = ParseDualImuCsv("IMU1,0,-0.422618,0.906308,-350,0,0,3\nIMU2,0,0,1,0,0,0,3");
            var heldClosedWithStaleOpenAcceleration = ParseDualImuCsv("IMU1,0,-0.422618,0.906308,0,0,0,4\nIMU2,0,0,1,0,0,0,4");

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                neutral,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                open,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.Imu1PoseDebugState.OpeningAngleDegrees, Is.GreaterThan(30f));

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                closeWithStaleOpenAcceleration,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.Imu1PoseDebugState.OpeningAngleDegrees, Is.LessThan(3f));

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                heldClosedWithStaleOpenAcceleration,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.Imu1PoseDebugState.OpeningAngleDegrees, Is.LessThan(3f));
            Assert.That(HashiImuMappingRuntime.Imu1PoseDebugState.RelativeOpeningAccelerationCorrectionAllowed, Is.False);
        }

        [Test]
        public void RelativeOpeningAccelerationIgnoresWholeHashiMotion()
        {
            HashiImuMappingRuntime.IndependentPoseMode = HashiIndependentPoseMode.RelativeOpeningIntegrated;
            HashiImuMappingRuntime.RawAccelerationToG = 1f;
            HashiImuMappingRuntime.RawGyroToDegreesPerSecond = 1f;
            HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond = 0f;
            HashiImuMappingRuntime.RelativeOpeningAccelerationWeight = 0.9f;

            var neutral = ParseDualImuCsv("IMU1,0,0,1,0,0,0,1\nIMU2,0,0,1,0,0,0,1");
            var wholeMotionFalseOpen = ParseDualImuCsv(
                "IMU1,0,-0.342020,0.939693,0,120,0,2\nIMU2,0,0,1,0,120,0,2");

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                neutral,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                wholeMotionFalseOpen,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);

            var debugState = HashiImuMappingRuntime.Imu1PoseDebugState;
            Assert.That(debugState.RelativeOpeningAccelerationDegrees, Is.GreaterThan(15f));
            Assert.That(debugState.RelativeOpeningVelocityDominance, Is.LessThan(0.25f));
            Assert.That(debugState.RelativeOpeningAccelerationCorrectionAllowed, Is.False);
            Assert.That(debugState.OpeningAngleDegrees, Is.LessThan(1f));
        }

        [Test]
        public void RelativeOpeningAccelerationRejectsProjectedFalseOpen()
        {
            HashiImuMappingRuntime.IndependentPoseMode = HashiIndependentPoseMode.RelativeOpeningIntegrated;
            HashiImuMappingRuntime.RawAccelerationToG = 1f;
            HashiImuMappingRuntime.RawGyroToDegreesPerSecond = 1f;
            HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond = 0f;
            HashiImuMappingRuntime.RelativeOpeningAccelerationWeight = 0.9f;

            var neutral = ParseDualImuCsv("IMU1,994,98,41,0,0,0,1\nIMU2,977,88,193,0,0,0,1");
            var falseOpen = ParseDualImuCsv("IMU1,-538,14916,-4293,0,0,0,2\nIMU2,81,14985,-5170,0,0,0,2");

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                neutral,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                falseOpen,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);

            var debugState = HashiImuMappingRuntime.Imu1PoseDebugState;
            Assert.That(debugState.HasRelativeOpeningAcceleration, Is.False);
            Assert.That(debugState.OpeningAngleDegrees, Is.LessThan(1f));
        }

        private static HashiBleSample ParseDualImuCsv(string payload)
        {
            Assert.That(HashiBleSampleParser.TryParse(payload, out var sample, out var error), Is.True, error);
            return sample;
        }
    }
}
