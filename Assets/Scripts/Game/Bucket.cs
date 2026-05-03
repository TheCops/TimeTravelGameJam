using UnityEngine;

namespace TimeTravelBanana.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class Bucket : MonoBehaviour
    {

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var banana = other.GetComponentInParent<Banana>();
            if (banana == null || banana.Resolved) return;
            banana.HitBucket();
        }
    }
}
