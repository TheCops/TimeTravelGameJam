using UnityEngine;

namespace TimeTravelBanana.Game
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private float framesPerSecond = 15f;
        [SerializeField] private int sortingOrder = 10;

        private static Sprite[] cachedFrames;
        private static AudioClip cachedSound;

        private SpriteRenderer sr;
        private float startTime;

        public static Explosion Spawn(Vector3 position, float scale = 0.5f)
        {
            var go = new GameObject("Explosion");
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            return go.AddComponent<Explosion>();
        }

        private void Awake()
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;

            if (cachedFrames == null)
            {
                var loaded = Resources.LoadAll<Sprite>("Art/explosion");
                System.Array.Sort(loaded, (a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
                cachedFrames = loaded;
            }
        }

        private void Start()
        {
            startTime = Time.time;
            if (cachedFrames != null && cachedFrames.Length > 0) sr.sprite = cachedFrames[0];

            if (cachedSound == null) cachedSound = Resources.Load<AudioClip>("Audio/explosion");
            if (cachedSound != null) AudioSource.PlayClipAtPoint(cachedSound, transform.position);
        }

        private void Update()
        {
            if (cachedFrames == null || cachedFrames.Length == 0)
            {
                Destroy(gameObject);
                return;
            }

            int idx = Mathf.FloorToInt((Time.time - startTime) * framesPerSecond);
            if (idx >= cachedFrames.Length)
            {
                Destroy(gameObject);
                return;
            }
            sr.sprite = cachedFrames[idx];
        }
    }
}
