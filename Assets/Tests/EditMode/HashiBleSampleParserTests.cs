using ChopChop.BLE;
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
    }
}
