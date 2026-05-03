using UnityEngine;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.Game
{
    public class TimelineDestructible : TimelineRecorderBase, IDestructible
    {
        private SpriteRenderer[] renderers;
        private Collider2D[] colliders;
        private TimelineRecorderBase[] siblingRecorders;

        private void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            colliders = GetComponentsInChildren<Collider2D>(true);
            siblingRecorders = GetComponents<TimelineRecorderBase>();
        }

        public void DestroyByImpact()
        {
            var tm = Timeline;
            if (tm == null)
            {
                var gm = GameManager.Instance;
                if (gm != null) tm = gm.Timeline;
            }
            if (tm == null) return;

            float t = tm.CurrentTime;
            for (int i = 0; i < siblingRecorders.Length; i++)
            {
                if (siblingRecorders[i] != null) siblingRecorders[i].MarkDeath(t);
            }
            SetVisible(false);
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
            SetVisible(isAlive);
        }

        private void SetVisible(bool visible)
        {
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                    if (renderers[i] != null) renderers[i].enabled = visible;
            }
            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                    if (colliders[i] != null) colliders[i].enabled = visible;
            }
        }
    }
}
