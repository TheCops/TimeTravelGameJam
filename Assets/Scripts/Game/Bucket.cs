using UnityEngine;


    [RequireComponent(typeof(Collider2D))]
    public class Bucket : MonoBehaviour
    {
        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

    }

