using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;
using TimeTravelBanana.Timeline;

namespace TimeTravelBanana.UI
{
    public class TimeDirectionDisplay : MonoBehaviour
    {
        [SerializeField] private int defaultFontSize = 36;
        [SerializeField] private int rewindFontSize = 36;
        [SerializeField] private float defaultBottomOffset = 30f;
        [SerializeField] private float rewindBottomOffset = 30f;
        [SerializeField] private Vector2 rewindImageSize = new Vector2(480f, 80f);
        [SerializeField] private float rewindImageOffsetY = 90f;
        [SerializeField] private float rewindFps = 8f;
        [SerializeField] private Color fontColor = Color.red;
        [SerializeField] private float rewindBlinkPeriod = 0.6f;

        private Text label;
        private string lastText;

        private Image rewindImage;
        private Sprite[] rewindFrames;
        private float frameTimer;
        private int frameIndex;

        private float blinkTimer;
        private bool isRewindActive;

        private RectTransform rt;
        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogWarning("TimeDirectionDisplay: no Canvas in scene; UI not built."); return; }

            var go = new GameObject("TimeDirectionText", typeof(RectTransform), typeof(Text));
            rt = (RectTransform)go.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(360f, 56f);

            label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = fontColor;
            label.raycastTarget = false;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.text = "";
            SetDefaultLabelProperties();

            rewindFrames = new Sprite[4];
            for (int i = 0; i < 4; i++)
                rewindFrames[i] = Resources.Load<Sprite>("Art/rewind/frame " + (i + 1));

            var imgGo = new GameObject("RewindAnimation", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)imgGo.transform;
            irt.SetParent(canvas.transform, false);
            irt.anchorMin = new Vector2(0.5f, 0f);
            irt.anchorMax = new Vector2(0.5f, 0f);
            irt.pivot = new Vector2(0.5f, 0f);
            irt.sizeDelta = rewindImageSize;
            irt.anchoredPosition = new Vector2(0f, rewindImageOffsetY);


            rewindImage = imgGo.GetComponent<Image>();
            rewindImage.raycastTarget = false;
            rewindImage.preserveAspect = false;
            if (rewindFrames[0] != null) rewindImage.sprite = rewindFrames[0];
            else Debug.LogWarning("TimeDirectionDisplay: could not load Resources/Art/rewind/frame 1.");
            imgGo.SetActive(false);
        }
        void SetDefaultLabelProperties()
        {
            label.fontSize = defaultFontSize;
            rt.anchoredPosition = new Vector2(0f, defaultBottomOffset); 
        }

        void SetRewindLabelProperties()
        {
            label.fontSize = rewindFontSize;
            rt.anchoredPosition = new Vector2(0f, rewindBottomOffset); 
            
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
                if(next == "<<RR")
                    SetRewindLabelProperties();
                else
                    SetDefaultLabelProperties();
                label.text = next;
                lastText = next;
                if (rewindImage != null)
                {
                    bool show = next == "<<RR" && rewindFrames != null && rewindFrames.Length > 0 && rewindFrames[0] != null;
                    rewindImage.gameObject.SetActive(show);
                    if (show)
                    {
                        frameIndex = 0;
                        frameTimer = 0f;
                        rewindImage.sprite = rewindFrames[0];
                    }
                }

                bool nowRewind = next == "<<RR";
                if (nowRewind != isRewindActive)
                {
                    isRewindActive = nowRewind;
                    blinkTimer = 0f;
                    label.enabled = true;
                }
            }

            if (isRewindActive && rewindBlinkPeriod > 0f)
            {
                blinkTimer += Time.deltaTime;
                float halfPeriod = rewindBlinkPeriod * 0.5f;
                while (blinkTimer >= halfPeriod)
                {
                    blinkTimer -= halfPeriod;
                    label.enabled = !label.enabled;
                }
            }

            if (rewindImage != null && rewindImage.gameObject.activeSelf && rewindFps > 0f)
            {
                frameTimer += Time.deltaTime;
                float frameDuration = 1f / rewindFps;
                while (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    frameIndex = (frameIndex + 1) % rewindFrames.Length;
                    var s = rewindFrames[frameIndex];
                    if (s != null) rewindImage.sprite = s;
                }
            }
        }
    }
}
