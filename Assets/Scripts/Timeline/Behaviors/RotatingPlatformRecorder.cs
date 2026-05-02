using UnityEngine;

[RequireComponent(typeof(RotatingPlatform))]
public class RotatingPlatformRecorder : TimelineRecorderBase<RotatingPlatformRecorder.Snapshot>
{
    public struct Snapshot
    {
        public RotatingPlatform.Phase Phase;
        public float PhaseTimer;
        public float Angle;
    }

    private RotatingPlatform platform;

    private void Awake()
    {
        platform = GetComponent<RotatingPlatform>();
    }

    protected override Snapshot CaptureSnapshot() => new Snapshot
    {
        Phase = platform.CurrentPhase,
        PhaseTimer = platform.PhaseTimer,
        Angle = platform.CurrentAngle
    };

    protected override void ApplySnapshot(Snapshot snapshot)
    {
        platform.CurrentPhase = snapshot.Phase;
        platform.PhaseTimer = snapshot.PhaseTimer;
        platform.CurrentAngle = snapshot.Angle;
    }

    protected override Snapshot Interpolate(Snapshot before, Snapshot after, float alpha) => new Snapshot
    {
        Phase = before.Phase,
        PhaseTimer = Mathf.Lerp(before.PhaseTimer, after.PhaseTimer, alpha),
        Angle = Mathf.Lerp(before.Angle, after.Angle, alpha)
    };

    protected override void OnEnterScrubbing()
    {
        platform.enabled = false;
    }

    protected override void OnExitScrubbing()
    {
        if (IsCurrentlyAlive) platform.enabled = true;
    }

    protected override void OnLifecycleChanged(bool isAlive)
    {
        platform.enabled = isAlive && timeline != null && timeline.Mode != TimelineMode.Scrubbing;
    }
}
