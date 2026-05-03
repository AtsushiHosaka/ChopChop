using System.Collections.Generic;
using System.Reflection;
using ChopChop.Food.Meat;
using ChopChop.Food.Tearing;
using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using NUnit.Framework;
using UnityEngine;

namespace ChopChop.Tests.EditMode
{
    public sealed class SukiyakiStageControllerTests
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
        public void CompleteCurrentStepAdvancesThroughSukiyakiFlow()
        {
            var stage = CreateStage();
            var changedSteps = new List<SukiyakiStep>();
            var succeededSteps = new List<SukiyakiStep>();
            LevelRunResult completedResult = default;
            var completed = false;

            stage.StepChanged += changedSteps.Add;
            stage.StepSucceeded += succeededSteps.Add;
            stage.Completed += result =>
            {
                completedResult = result;
                completed = true;
            };

            stage.StartLevel();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.Calibration));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.MixEgg));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.PickMeat));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.PutInBroth));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.FlipMeat));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.DipEgg));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.DeliverToMouth));

            stage.CompleteCurrentStep();
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.Result));
            Assert.That(completed, Is.True);
            Assert.That(completedResult.IsComplete, Is.True);
            Assert.That(changedSteps, Does.Contain(SukiyakiStep.Result));
            Assert.That(succeededSteps, Is.EquivalentTo(new[]
            {
                SukiyakiStep.Calibration,
                SukiyakiStep.MixEgg,
                SukiyakiStep.PickMeat,
                SukiyakiStep.PutInBroth,
                SukiyakiStep.FlipMeat,
                SukiyakiStep.DipEgg,
                SukiyakiStep.DeliverToMouth
            }));
        }

        [Test]
        public void CompleteCurrentStepDoesNothingBeforeStart()
        {
            var stage = CreateStage();

            stage.CompleteCurrentStep();

            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.Calibration));
            Assert.That(stage.IsRunning, Is.False);
        }

        [Test]
        public void PhysicalMeatTearDuringFlipRetriesFlipStep()
        {
            var stage = CreateStage();
            var tearController = CreateMeatTearController(out var meatActor, out var tearDriver);
            var failedSteps = new List<SukiyakiStep>();

            stage.Configure(null, null, tearController);
            stage.StepFailed += failedSteps.Add;

            stage.StartLevel();
            AdvanceTo(stage, SukiyakiStep.FlipMeat);

            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.FlipMeat));
            Assert.That(meatActor.MeatState, Is.EqualTo(MeatState.Flipping));

            tearDriver.RequestTearAt(Vector3.zero, MeatTearReason.PhysicalTear);

            Assert.That(stage.FailureCount, Is.EqualTo(1));
            Assert.That(stage.CurrentStep, Is.EqualTo(SukiyakiStep.FlipMeat));
            Assert.That(tearController.IsTorn, Is.False);
            Assert.That(meatActor.MeatState, Is.EqualTo(MeatState.Flipping));
            Assert.That(failedSteps, Is.EquivalentTo(new[] { SukiyakiStep.FlipMeat }));
        }

        private SukiyakiStageController CreateStage()
        {
            var stageObject = new GameObject("Sukiyaki Stage");
            createdObjects.Add(stageObject);
            return stageObject.AddComponent<SukiyakiStageController>();
        }

        private MeatTearController CreateMeatTearController(out MeatInteractionActor meatActor, out ManualMeatTearDriver tearDriver)
        {
            var meatObject = new GameObject("Meat");
            createdObjects.Add(meatObject);

            meatActor = meatObject.AddComponent<MeatInteractionActor>();

            var driverObject = new GameObject("Manual Tear Driver");
            driverObject.transform.SetParent(meatObject.transform);
            tearDriver = driverObject.AddComponent<ManualMeatTearDriver>();
            SetPrivateField(tearDriver, "raisePhysicalEventOnGameplayTear", true);

            return meatObject.AddComponent<MeatTearController>();
        }

        private static void AdvanceTo(SukiyakiStageController stage, SukiyakiStep targetStep)
        {
            while (stage.CurrentStep < targetStep)
            {
                stage.CompleteCurrentStep();
            }
        }

        private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
