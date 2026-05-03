using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.UI
{
    public class TimeDirectionDisplay : MonoBehaviour
    {
        [SerializeField] private int fontSize = 36;
        [SerializeField] private float bottomOffset = 30f;

        private Text label;
        private string lastText;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogWarning("TimeDirectionDisplay: no Canvas in scene; UI not built."); return; }

            var go = new GameObject("TimeDirectionText", typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(360f, 56f);
            rt.anchoredPosition = new Vector2(0f, bottomOffset);

            label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.red;
            label.raycastTarget = false;
            label.text = "";
        }

        private void Update()
        {
            if (label == null) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.Timeline == null) return;

            string next = "PAUSE";
            if (gm.Timeline.Mode == TimelineMode.Recording)
            {
                next = "PLAY>";
            }
            else if (gm.Timeline.Mode == TimelineMode.Scrubbing
                     && gm.TimeController != null
                     && gm.TimeController.IsReverseScrubbing
                     && gm.TimeController.ObjectTime > 0f)
            {
                next = "<<RR";
            }

            if (next != lastText)
            {
                label.text = next;
                lastText = next;
            }
        }
    }
}
