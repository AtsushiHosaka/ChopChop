using System.Collections.Generic;
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

        private SukiyakiStageController CreateStage()
        {
            var stageObject = new GameObject("Sukiyaki Stage");
            createdObjects.Add(stageObject);
            return stageObject.AddComponent<SukiyakiStageController>();
        }
    }
}
