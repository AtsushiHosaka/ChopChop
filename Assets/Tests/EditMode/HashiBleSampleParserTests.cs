using ChopChop.BLE;
using ChopChop.Input;
using NUnit.Framework;

namespace ChopChop.Tests.EditMode
{
    public sealed class HashiBleSampleParserTests
    {
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
        public void ParserOutputMatchesUdpCompatiblePayloadRequirements()
        {
            var json = "{\"theta\":12.3,\"pinch\":80,\"ax\":0.1,\"ay\":-0.9,\"az\":0.2,\"gx\":1.2,\"gy\":0.0,\"gz\":-0.4}";

            Assert.That(HashiBleSampleParser.TryParse(json, out var sample, out var error), Is.True, error);
            Assert.That(sample.HasTheta, Is.True);
            Assert.That(sample.HasPinch, Is.True);
            Assert.That(sample.HasAccelerationVector, Is.True);
            Assert.That(sample.HasGyroVector, Is.True);
        }

        [Test]
        public void SixFieldCsvPayloadParsesCurrentEsp32Format()
        {
            var csv = "0.1,-0.9,0.2,1.2,0.0,-0.4";

            Assert.That(HashiBleSampleParser.TryParse(csv, out var sample, out var error), Is.True, error);
            Assert.That(sample.HasTheta, Is.False);
            Assert.That(sample.HasPinch, Is.False);
            Assert.That(sample.AxValue, Is.EqualTo(0.1f).Within(0.001f));
            Assert.That(sample.AyValue, Is.EqualTo(-0.9f).Within(0.001f));
            Assert.That(sample.AzValue, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(sample.GxValue, Is.EqualTo(1.2f).Within(0.001f));
            Assert.That(sample.GyValue, Is.EqualTo(0f).Within(0.001f));
            Assert.That(sample.GzValue, Is.EqualTo(-0.4f).Within(0.001f));
        }

        [Test]
        public void EightFieldCsvPayloadParsesThetaAndPinch()
        {
            var csv = "12.3,80,0.1,-0.9,0.2,1.2,0.0,-0.4";

            Assert.That(HashiBleSampleParser.TryParse(csv, out var sample, out var error), Is.True, error);
            Assert.That(sample.ThetaValue, Is.EqualTo(12.3f).Within(0.001f));
            Assert.That(sample.PinchValue, Is.EqualTo(80f).Within(0.001f));
            Assert.That(sample.HasAccelerationVector, Is.True);
            Assert.That(sample.HasGyroVector, Is.True);
        }
    }
}
