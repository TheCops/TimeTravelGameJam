public interface ITimelineAffected
{
    void CaptureState(float time);
    void RestoreState(float time);
    void TruncateFuture(float time);
    void OnTimelineModeChanged(TimelineMode previous, TimelineMode next);
}
