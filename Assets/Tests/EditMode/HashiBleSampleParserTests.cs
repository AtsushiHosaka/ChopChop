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
        public void RelativeOpeningAccelerationDoesNotOpenFromClosedWithoutGyroSupport()
        {
            HashiImuMappingRuntime.IndependentPoseMode = HashiIndependentPoseMode.RelativeOpeningIntegrated;
            HashiImuMappingRuntime.RelativeOpeningAccelerationWeight = 1f;
            HashiImuMappingRuntime.RawAccelerationToG = 1f;
            HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond = 0f;
            HashiImuMappingRuntime.OpeningAngleStaticJumpRejectDegrees = 12f;
            HashiImuMappingRuntime.OpeningAngleStaticVelocityThresholdDps = 20f;

            var neutral = ParseDualImuCsv("0,0,1,0,0,0,0,0,1,0,0,0");
            var falseOpenFromAccelerationOnly = ParseDualImuCsv("0,-0.573576,0.819152,0,0,0,0,0,1,0,0,0");

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                neutral,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                falseOpenFromAccelerationOnly,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);

            var imu1State = HashiImuMappingRuntime.Imu1PoseDebugState;
            Assert.That(imu1State.RelativeOpeningAccelerationDegrees, Is.GreaterThan(20f));
            Assert.That(imu1State.RelativeOpeningAccelerationCorrectionAllowed, Is.False);
            Assert.That(imu1State.OpeningAngleDegrees, Is.LessThan(1f));
        }

        [Test]
        public void RelativeOpeningStillRespondsWhenOpeningGyroSaturates()
        {
            HashiImuMappingRuntime.IndependentPoseMode = HashiIndependentPoseMode.RelativeOpeningIntegrated;
            HashiImuMappingRuntime.RelativeOpeningAccelerationWeight = 0f;
            HashiImuMappingRuntime.RawAccelerationToG = 1f;
            HashiImuMappingRuntime.RawGyroToDegreesPerSecond = 1f;
            HashiImuMappingRuntime.GyroDeadzoneDegreesPerSecond = 0f;

            var neutral = ParseDualImuCsv("0,0,1,0,0,0,0,0,1,0,0,0");
            var saturatedOpening = ParseDualImuCsv("0,0,1,32767,0,0,0,0,1,0,0,0");

            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                neutral,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);
            Assert.That(HashiImuMappingRuntime.UpdateIndependentRotations(
                saturatedOpening,
                0.1f,
                Vector3.up,
                out _,
                out _), Is.True);

            Assert.That(HashiImuMappingRuntime.Imu1PoseDebugState.OpeningAngleDegrees, Is.GreaterThan(20f));
        }

        private static HashiBleSample ParseDualImuCsv(string payload)
        {
            Assert.That(HashiBleSampleParser.TryParse(payload, out var sample, out var error), Is.True, error);
            return sample;
        }
    }
}
