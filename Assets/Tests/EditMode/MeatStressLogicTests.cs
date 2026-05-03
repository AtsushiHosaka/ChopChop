using System.Collections.Generic;
using ChopChop.Food.Meat;
using ChopChop.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace ChopChop.Tests.EditMode
{
    public sealed class MeatStressLogicTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void TearReasonClassifierChoosesDominantMetric()
        {
            var profile = CreateProfile();

            Assert.That(TearReasonClassifier.ClassifyFrame(profile, 0.9f, 0f, 0f, 0f, 0f), Is.EqualTo(MeatTearReason.Poke));
            Assert.That(TearReasonClassifier.ClassifyFrame(profile, 0f, 0.9f, 0f, 0f, 0f), Is.EqualTo(MeatTearReason.Shear));
            Assert.That(TearReasonClassifier.ClassifyFrame(profile, 0f, 0f, 430f, 0f, 0f), Is.EqualTo(MeatTearReason.Twist));
            Assert.That(TearReasonClassifier.ClassifyFrame(profile, 0f, 0f, 0f, 0.13f, 0f), Is.EqualTo(MeatTearReason.Tension));
            Assert.That(TearReasonClassifier.ClassifyFrame(profile, 0f, 0f, 0f, 0f, 1f), Is.EqualTo(MeatTearReason.Compression));
        }

        [Test]
        public void MeatContactAnalyzerDetectsPoke()
        {
            var frame = Analyze(SingleContact(Vector3.zero, Vector3.zero, Vector3.down * 1.1f, 0.2f), 0.1f);

            Assert.That(frame.HasContact, Is.True);
            Assert.That(frame.Poke, Is.GreaterThan(1f));
            Assert.That(frame.DominantReason, Is.EqualTo(MeatTearReason.Poke));
        }

        [Test]
        public void MeatContactAnalyzerDetectsShear()
        {
            var frame = Analyze(SingleContact(Vector3.zero, Vector3.zero, Vector3.right * 0.9f, 0.2f), 0.1f);

            Assert.That(frame.Shear, Is.GreaterThan(0.8f));
            Assert.That(frame.DominantReason, Is.EqualTo(MeatTearReason.Shear));
        }

        [Test]
        public void MeatContactAnalyzerDetectsTension()
        {
            var frame = Analyze(SingleContact(Vector3.zero, Vector3.right * 0.13f, Vector3.zero, 0.2f), 0.1f);

            Assert.That(frame.Tension, Is.GreaterThan(0.12f));
            Assert.That(frame.DominantReason, Is.EqualTo(MeatTearReason.Tension));
        }

        [Test]
        public void MeatContactAnalyzerDetectsCompression()
        {
            var lower = CreateTip("Lower", new Vector3(0f, 0f, 0f));
            var upper = CreateTip("Upper", new Vector3(0.004f, 0f, 0f));
            var contacts = new[]
            {
                Contact(lower, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, 1f),
                Contact(upper, new Vector3(0.004f, 0f, 0f), new Vector3(0.004f, 0f, 0f), Vector3.zero, Vector3.zero, Vector3.zero, 1f)
            };

            var frame = Analyze(contacts, 0.1f);

            Assert.That(frame.Compression, Is.GreaterThan(0.9f));
            Assert.That(frame.DominantReason, Is.EqualTo(MeatTearReason.Compression));
        }

        [Test]
        public void MeatContactAnalyzerDetectsTwistAwayFromAllowedFlipAxis()
        {
            var lower = CreateTip("Lower", new Vector3(-0.5f, 0f, 0f));
            var upper = CreateTip("Upper", new Vector3(0.5f, 0f, 0f));
            var contacts = new[]
            {
                Contact(lower, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, 0.2f),
                Contact(upper, Vector3.forward, Vector3.right, Vector3.zero, Vector3.zero, Vector3.zero, 0.2f)
            };

            var frame = Analyze(contacts, 0.1f);

            Assert.That(frame.Twist, Is.GreaterThan(800f));
            Assert.That(frame.DominantReason, Is.EqualTo(MeatTearReason.Twist));
        }

        private MeatStressFrame Analyze(IReadOnlyList<MeatTipContact> contacts, float deltaTime)
        {
            var analyzer = new MeatContactAnalyzer();
            return analyzer.Analyze(contacts, CreateZone(), CreateProfile(), deltaTime);
        }

        private MeatContactZone CreateZone()
        {
            var zoneObject = new GameObject("Meat Zone");
            createdObjects.Add(zoneObject);
            zoneObject.AddComponent<BoxCollider>();
            return zoneObject.AddComponent<MeatContactZone>();
        }

        private MeatTearProfile CreateProfile()
        {
            var profile = ScriptableObject.CreateInstance<MeatTearProfile>();
            createdObjects.Add(profile);
            return profile;
        }

        private MeatTipContact[] SingleContact(Vector3 initialLocal, Vector3 currentLocal, Vector3 velocity, float pinch)
        {
            var tip = CreateTip("Tip", Vector3.zero);
            return new[]
            {
                Contact(tip, Vector3.zero, Vector3.zero, initialLocal, currentLocal, velocity, pinch)
            };
        }

        private ChopstickTip CreateTip(string name, Vector3 position)
        {
            var tipObject = new GameObject(name);
            createdObjects.Add(tipObject);
            tipObject.transform.position = position;
            return tipObject.AddComponent<ChopstickTip>();
        }

        private static MeatTipContact Contact(
            ChopstickTip tip,
            Vector3 worldPoint,
            Vector3 previousWorldPoint,
            Vector3 initialLocal,
            Vector3 currentLocal,
            Vector3 velocity,
            float pinch)
        {
            return new MeatTipContact(tip, worldPoint, previousWorldPoint, initialLocal, currentLocal, velocity, pinch, 0.1f);
        }
    }
}
