using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Pan : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var banana = collision.collider.GetComponentInParent<Banana>();
            if (banana == null) return;
            banana.Cook();
        }
    }
}
