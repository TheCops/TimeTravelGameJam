using UnityEngine;

namespace TimeTravelBanana.Timeline.Behaviors
{
    [RequireComponent(typeof(EnemyAIController))]
    public class EnemyAIRecorder : TimelineRecorderBase<EnemyAIRecorder.Snapshot>
    {
        public struct Snapshot
        {
            public EnemyAIController.AIState State;
            public float StateTimer;
            public Vector2 PatrolDirection;
            public bool FiredThisAttack;
        }

        private EnemyAIController ai;

        private void Awake()
        {
            ai = GetComponent<EnemyAIController>();
        }

        protected override Snapshot CaptureSnapshot() => new Snapshot
        {
            State = ai.State,
            StateTimer = ai.StateTimer,
            PatrolDirection = ai.PatrolDirection,
            FiredThisAttack = ai.FiredThisAttack
        };

        protected override void ApplySnapshot(Snapshot snapshot)
        {
            ai.State = snapshot.State;
            ai.StateTimer = snapshot.StateTimer;
            ai.PatrolDirection = snapshot.PatrolDirection;
            ai.FiredThisAttack = snapshot.FiredThisAttack;
        }

        protected override Snapshot Interpolate(Snapshot before, Snapshot after, float alpha) => new Snapshot
        {
            State = before.State,
            StateTimer = Mathf.Lerp(before.StateTimer, after.StateTimer, alpha),
            PatrolDirection = before.PatrolDirection,
            FiredThisAttack = before.FiredThisAttack
        };

        protected override void OnEnterScrubbing()
        {
            ai.enabled = false;
        }

        protected override void OnExitScrubbing()
        {
            if (IsCurrentlyAlive) ai.enabled = true;
        }

        protected override void OnLifecycleChanged(bool isAlive)
        {
            ai.enabled = isAlive && timeline != null && timeline.Mode != TimelineMode.Scrubbing;
        }
    }
}
