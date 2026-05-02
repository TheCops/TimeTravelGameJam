using System.Collections.Generic;
using UnityEngine;

public class TimelineSpawner : MonoBehaviour
{
    private TimelineManager timeline;
    [SerializeField, Min(0f)] private float destroyGracePeriod = 1f;

    private struct Tracked
    {
        public GameObject Instance;
        public float DeathTime;
    }

    private readonly List<Tracked> pendingDestroys = new List<Tracked>();

    public TimelineManager Timeline { get => timeline; set => timeline = value; }

    private void Start()
    {
        if (timeline != null) return;
        var gm = GameManager.Instance;
        if (gm == null || gm.Timeline == null)
        {
            Debug.LogWarning($"{nameof(TimelineSpawner)} on {name}: GameManager.Instance.Timeline not available at Start.", this);
            return;
        }
        timeline = gm.Timeline;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (timeline == null || prefab == null) return null;

        GameObject instance = Instantiate(prefab, position, rotation);
        float now = timeline.CurrentTime;

        var recorders = instance.GetComponentsInChildren<TimelineRecorderBase>(true);
        for (int i = 0; i < recorders.Length; i++)
        {
            recorders[i].SetBirthTime(now);
            recorders[i].ConnectToTimeline(timeline);
        }

        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null || timeline == null) return;
        float now = timeline.CurrentTime;

        var recorders = instance.GetComponentsInChildren<TimelineRecorderBase>(true);
        for (int i = 0; i < recorders.Length; i++)
            recorders[i].MarkDeath(now);

        pendingDestroys.Add(new Tracked { Instance = instance, DeathTime = now });
    }

    private void Update()
    {
        if (timeline == null || pendingDestroys.Count == 0) return;
        float threshold = timeline.CurrentTime - timeline.MaxBufferSeconds - destroyGracePeriod;

        for (int i = pendingDestroys.Count - 1; i >= 0; i--)
        {
            Tracked t = pendingDestroys[i];
            if (t.Instance == null)
            {
                pendingDestroys.RemoveAt(i);
                continue;
            }
            if (timeline.Mode == TimelineMode.Recording && t.DeathTime < threshold)
            {
                Destroy(t.Instance);
                pendingDestroys.RemoveAt(i);
            }
        }
    }
}
