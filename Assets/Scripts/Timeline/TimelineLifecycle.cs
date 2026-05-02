using UnityEngine;

[DisallowMultipleComponent]
public class TimelineLifecycle : TimelineRecorderBase
{
    [SerializeField] private MonoBehaviour[] behavioursToToggle;

    private Renderer[] cachedRenderers;
    private Collider2D[] cachedColliders;

    private void Awake()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider2D>(true);
    }

    public override void CaptureState(float time) { }

    public override void RestoreState(float time)
    {
        RestoreLifecycle(time);
    }

    public override void TruncateFuture(float time)
    {
        TruncateLifecycle(time);
    }

    protected override void OnLifecycleChanged(bool isAlive)
    {
        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
                if (cachedRenderers[i] != null) cachedRenderers[i].enabled = isAlive;
        }
        if (cachedColliders != null)
        {
            for (int i = 0; i < cachedColliders.Length; i++)
                if (cachedColliders[i] != null) cachedColliders[i].enabled = isAlive;
        }
        if (behavioursToToggle != null)
        {
            for (int i = 0; i < behavioursToToggle.Length; i++)
                if (behavioursToToggle[i] != null) behavioursToToggle[i].enabled = isAlive;
        }
    }
}
