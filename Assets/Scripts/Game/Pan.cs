using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Pan : MonoBehaviour
    {
        private void Awake()
        {
            if (GetComponent<Contraption>() == null) gameObject.AddComponent<Contraption>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var banana = collision.collider.GetComponentInParent<Banana>();
            if (banana == null) return;
            banana.Cook();
        }
    }
}
