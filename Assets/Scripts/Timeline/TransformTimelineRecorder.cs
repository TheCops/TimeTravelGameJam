using UnityEngine;

public class TransformTimelineRecorder : TimelineRecorderBase<TransformTimelineRecorder.Snapshot>
{
    public struct Snapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LocalScale;
    }

    protected override Snapshot CaptureSnapshot() => new Snapshot
    {
        Position = transform.position,
        Rotation = transform.rotation,
        LocalScale = transform.localScale
    };

    protected override void ApplySnapshot(Snapshot snapshot)
    {
        transform.position = snapshot.Position;
        transform.rotation = snapshot.Rotation;
        transform.localScale = snapshot.LocalScale;
    }

    protected override Snapshot Interpolate(Snapshot before, Snapshot after, float alpha) => new Snapshot
    {
        Position = Vector3.Lerp(before.Position, after.Position, alpha),
        Rotation = Quaternion.Slerp(before.Rotation, after.Rotation, alpha),
        LocalScale = Vector3.Lerp(before.LocalScale, after.LocalScale, alpha)
    };
}
