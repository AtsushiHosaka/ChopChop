using System;
using System.Collections.Generic;
using ChopChop.Food.Tearing;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    [AddComponentMenu("ChopChop/Food/Meat Tear Controller")]
    [DisallowMultipleComponent]
    public sealed class MeatTearController : MonoBehaviour
    {
        [SerializeField] private MeatInteractionActor meatActor;
        [SerializeField] private MeatContactZone contactZone;
        [SerializeField] private MeatTearProfile tearProfile = null;
        [SerializeField] private MeatTearDriverBehaviour tearDriver;
        [Tooltip("When enabled, gameplay stress is evaluated during broth transfer and flip handling. Physical Obi tear events are still observed in every meat state.")]
        [SerializeField] private bool evaluateOnlyWhileFlipping = true;
        [SerializeField] private bool logStressFrames = false;
        [SerializeField] private float levelStartTime;

        private readonly List<MeatTipContact> activeContacts = new List<MeatTipContact>();
        private readonly List<MeatStressFrame> recentStressFrames = new List<MeatStressFrame>();
        private readonly MeatContactAnalyzer analyzer = new MeatContactAnalyzer();

        private const int MaxRecentStressFrames = 90;

        public MeatState MeatState => meatActor != null ? meatActor.MeatState : MeatState.Idle;
        public MeatTearProfile TearProfile => tearProfile;
        public MeatTearDriverBehaviour TearDriver => tearDriver;
        public float Damage { get; private set; }
        public float Damage01 => tearProfile != null && tearProfile.durability > 0f
            ? Mathf.Clamp01(Damage / tearProfile.durability)
            : Mathf.Clamp01(Damage / 100f);
        public MeatStressFrame LastStressFrame { get; private set; } = MeatStressFrame.None;
        public bool IsTorn => MeatState == MeatState.Torn;

        public event Action<MeatTearEvent> Torn;

        public void SetLevelStartTime(float startTime)
        {
            levelStartTime = startTime;
        }

        public void ResetMeat()
        {
            Damage = 0f;
            LastStressFrame = MeatStressFrame.None;
            recentStressFrames.Clear();

            if (meatActor != null)
            {
                meatActor.SetMeatState(MeatState.Idle);
            }
        }

        public void SetMeatState(MeatState state)
        {
            if (meatActor != null)
            {
                meatActor.SetMeatState(state);
            }
        }

        private void Reset()
        {
            meatActor = GetComponent<MeatInteractionActor>();
            contactZone = GetComponentInChildren<MeatContactZone>();
            tearDriver = GetComponentInChildren<MeatTearDriverBehaviour>();
        }

        private void Awake()
        {
            if (meatActor == null)
            {
                meatActor = GetComponent<MeatInteractionActor>();
            }

            if (contactZone == null)
            {
                contactZone = GetComponentInChildren<MeatContactZone>();
            }

            if (tearDriver == null)
            {
                tearDriver = GetComponentInChildren<MeatTearDriverBehaviour>();
            }
        }

        private void OnEnable()
        {
            if (tearDriver != null)
            {
                tearDriver.PhysicalTorn += HandlePhysicalTorn;
                tearDriver.Configure(tearProfile);
            }
        }

        private void OnDisable()
        {
            if (tearDriver != null)
            {
                tearDriver.PhysicalTorn -= HandlePhysicalTorn;
            }
        }

        private void Update()
        {
            if (tearProfile == null || contactZone == null || IsTorn)
            {
                return;
            }

            if (evaluateOnlyWhileFlipping && meatActor != null && !ShouldEvaluateStress(meatActor.MeatState))
            {
                RecoverDamage(Time.deltaTime);
                return;
            }

            contactZone.GetActiveContacts(activeContacts);
            LastStressFrame = analyzer.Analyze(activeContacts, contactZone, tearProfile, Time.deltaTime);
            AddRecentFrame(LastStressFrame);

            if (!LastStressFrame.HasContact)
            {
                RecoverDamage(Time.deltaTime);
                return;
            }

            if (meatActor != null && meatActor.MeatState == MeatState.Idle)
            {
                meatActor.SetMeatState(MeatState.Touched);
            }

            if (tearProfile.IsCritical(LastStressFrame, out var criticalReason))
            {
                TearByGameplay(criticalReason, LastStressFrame);
                return;
            }

            Damage += LastStressFrame.DamagePerSecond * Time.deltaTime;

            if (logStressFrames && LastStressFrame.DamagePerSecond > 0f)
            {
                Debug.Log($"[Meat Tear] damage={Damage:F1}/{tearProfile.durability:F1} reason={LastStressFrame.DominantReason} poke={LastStressFrame.Poke:F2} shear={LastStressFrame.Shear:F2} twist={LastStressFrame.Twist:F1} tension={LastStressFrame.Tension:F3} compression={LastStressFrame.Compression:F2}", this);
            }

            if (Damage >= tearProfile.durability)
            {
                var reason = TearReasonClassifier.ClassifyHistory(recentStressFrames, tearProfile);
                if (reason == MeatTearReason.Unknown)
                {
                    reason = MeatTearReason.AccumulatedStress;
                }

                TearByGameplay(reason, LastStressFrame);
            }
        }

        private void TearByGameplay(MeatTearReason reason, MeatStressFrame frame)
        {
            var worldPoint = frame.HasContact ? frame.WorldContactPoint : transform.position;
            var visualTearRequested = tearDriver != null && tearDriver.RequestTearAt(worldPoint, reason);
            CompleteTear(reason, frame, worldPoint, wasPhysicalTear: visualTearRequested);
        }

        private void HandlePhysicalTorn(Vector3 worldPoint)
        {
            if (IsTorn)
            {
                return;
            }

            var reason = TearReasonClassifier.ClassifyHistory(recentStressFrames, tearProfile);
            if (reason == MeatTearReason.Unknown)
            {
                reason = MeatTearReason.PhysicalTear;
            }

            var frame = LastStressFrame.HasContact
                ? LastStressFrame
                : new MeatStressFrame(0f, 0f, 0f, 0f, 0f, 0f, reason, worldPoint, true);
            CompleteTear(reason, frame, worldPoint, wasPhysicalTear: true);
        }

        private void CompleteTear(MeatTearReason reason, MeatStressFrame frame, Vector3 worldPoint, bool wasPhysicalTear)
        {
            Damage = tearProfile != null ? tearProfile.durability : Damage;

            if (meatActor != null)
            {
                meatActor.SetMeatState(MeatState.Torn);
                meatActor.RegisterFailure(reason.ToString(), Time.time - levelStartTime);
            }

            var tearEvent = new MeatTearEvent(reason, frame, Damage, worldPoint, wasPhysicalTear);
            Torn?.Invoke(tearEvent);
        }

        private void RecoverDamage(float deltaTime)
        {
            if (tearProfile == null || tearProfile.recoveryPerSecond <= 0f || Damage <= 0f)
            {
                return;
            }

            Damage = Mathf.Max(0f, Damage - tearProfile.recoveryPerSecond * deltaTime);
        }

        private void AddRecentFrame(MeatStressFrame frame)
        {
            recentStressFrames.Add(frame);
            if (recentStressFrames.Count > MaxRecentStressFrames)
            {
                recentStressFrames.RemoveAt(0);
            }
        }

        private static bool ShouldEvaluateStress(MeatState state)
        {
            return state == MeatState.FlipReady || state == MeatState.Flipping;
        }
    }
}
