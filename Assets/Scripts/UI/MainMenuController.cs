using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TimeTravelBanana.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private Button instructionsBackButton;
        [SerializeField] private string levelSceneName = "Level1";

        [Header("Boomerang")]
        [SerializeField] private float boomerangSize = 160f;
        [SerializeField] private float boomerangSpinDegPerSec = 540f;
        [SerializeField] private float boomerangPathSpeed = 1.4f;
        [SerializeField] private float boomerangRangeXFraction = 0.40f;
        [SerializeField] private float boomerangRangeYFraction = 0.18f;
        [SerializeField] private float boomerangCenterYFraction = 0.20f;

        private static readonly Color PlayBaseColor = new Color(1.00f, 0.85f, 0.20f);
        private static readonly Color InstructionsBaseColor = new Color(0.35f, 0.85f, 1.00f);
        private static readonly Color QuitBaseColor = new Color(1.00f, 0.35f, 0.45f);
        private static readonly Color BackBaseColor = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color BackgroundColor = new Color(0.10f, 0.06f, 0.22f);
        private static readonly Color TitleColor = new Color(1.00f, 0.85f, 0.15f);
        private static readonly Color TitleShadow = new Color(1.00f, 0.30f, 0.55f);
        private static readonly Color BezelColor = new Color(0f, 0f, 0f, 1f);
        private const float ButtonBezelThickness = 8f;

        private Canvas canvas;
        private RectTransform boomerangRect;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (instructionsButton != null) instructionsButton.onClick.AddListener(OnInstructions);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (instructionsBackButton != null) instructionsBackButton.onClick.AddListener(OnInstructionsBack);
            if (instructionsPanel != null) instructionsPanel.SetActive(false);

            canvas = Object.FindFirstObjectByType<Canvas>();

            CreateBackground();
            StylizeTitle();
            StylizeButton(playButton, PlayBaseColor, Color.black);
            StylizeButton(instructionsButton, InstructionsBaseColor, Color.black);
            StylizeButton(quitButton, QuitBaseColor, Color.white);
            StylizeButton(instructionsBackButton, BackBaseColor, Color.black);

            CreateBoomerang();
        }

        private void CreateBackground()
        {
            if (canvas == null) return;
            if (canvas.transform.Find("MenuBackground") != null) return;

            var go = new GameObject("MenuBackground", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.SetAsFirstSibling();

            var img = go.GetComponent<Image>();
            img.color = BackgroundColor;
            img.raycastTarget = false;
        }

        private void StylizeTitle()
        {
            if (canvas == null) return;
            var titleTr = canvas.transform.Find("Title");
            if (titleTr == null) return;
            var txt = titleTr.GetComponent<Text>();
            if (txt == null) return;

            txt.fontSize = 132;
            txt.fontStyle = FontStyle.Bold;
            txt.color = TitleColor;
            txt.alignment = TextAnchor.MiddleCenter;

            var shadow = txt.GetComponent<Shadow>();
            if (shadow == null) shadow = txt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = TitleShadow;
            shadow.effectDistance = new Vector2(8f, -8f);

            var outline = txt.GetComponent<Outline>();
            if (outline == null) outline = txt.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(4f, -4f);

            var rt = (RectTransform)titleTr;
            rt.sizeDelta = new Vector2(1600f, 240f);
        }

        private void StylizeButton(Button btn, Color baseColor, Color textColor)
        {
            if (btn == null) return;

            var img = btn.GetComponent<Image>();
            if (img != null) img.color = Color.white;

            AddBezel(btn);

            var colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.35f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = Color.Lerp(baseColor, Color.gray, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.05f;
            btn.colors = colors;

            var rt = (RectTransform)btn.transform;
            rt.sizeDelta = new Vector2(420f, 96f);

            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.color = textColor;
                txt.fontStyle = FontStyle.Bold;
                txt.fontSize = 44;
                txt.alignment = TextAnchor.MiddleCenter;

                var shadow = txt.GetComponent<Shadow>();
                if (shadow != null) Destroy(shadow);
                var outline = txt.GetComponent<Outline>();
                if (outline != null) Destroy(outline);
            }
        }

        private void AddBezel(Button btn)
        {
            if (btn == null) return;
            var btnRt = (RectTransform)btn.transform;
            if (btnRt.parent == null) return;
            string bezelName = btn.name + "_Bezel";
            if (btnRt.parent.Find(bezelName) != null) return;

            var bezelGo = new GameObject(bezelName, typeof(RectTransform), typeof(Image));
            var bezelRt = (RectTransform)bezelGo.transform;
            bezelRt.SetParent(btnRt.parent, false);
            bezelRt.anchorMin = btnRt.anchorMin;
            bezelRt.anchorMax = btnRt.anchorMax;
            bezelRt.pivot = btnRt.pivot;
            bezelRt.anchoredPosition = btnRt.anchoredPosition;
            bezelRt.sizeDelta = btnRt.sizeDelta + new Vector2(ButtonBezelThickness * 2f, ButtonBezelThickness * 2f);
            bezelRt.SetSiblingIndex(btnRt.GetSiblingIndex());

            var bezelImg = bezelGo.GetComponent<Image>();
            bezelImg.color = BezelColor;
            bezelImg.raycastTarget = false;
        }

        private void CreateBoomerang()
        {
            if (canvas == null) return;
            var sprite = Resources.Load<Sprite>("Art/banana");
            if (sprite == null) return;
            if (canvas.transform.Find("BananaBoomerang") != null) return;

            var go = new GameObject("BananaBoomerang", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            boomerangRect = (RectTransform)go.transform;
            boomerangRect.anchorMin = boomerangRect.anchorMax = new Vector2(0.5f, 0.5f);
            boomerangRect.pivot = new Vector2(0.5f, 0.5f);
            boomerangRect.sizeDelta = new Vector2(boomerangSize, boomerangSize);
            int backgroundIdx = canvas.transform.Find("MenuBackground") is Transform bg ? bg.GetSiblingIndex() : -1;
            boomerangRect.SetSiblingIndex(backgroundIdx + 1);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private void Update()
        {
            if (boomerangRect == null || canvas == null) return;

            var canvasRect = (RectTransform)canvas.transform;
            float w = canvasRect.rect.width;
            float h = canvasRect.rect.height;

            float t = Time.time * boomerangPathSpeed;
            float x = Mathf.Sin(t) * w * boomerangRangeXFraction;
            float y = Mathf.Cos(t * 1.7f) * h * boomerangRangeYFraction - h * boomerangCenterYFraction;

            boomerangRect.anchoredPosition = new Vector2(x, y);
            boomerangRect.localRotation = Quaternion.Euler(0f, 0f, -Time.time * boomerangSpinDegPerSec);
        }

        private void OnPlay()
        {
            SceneManager.LoadScene(levelSceneName);
        }

        private void OnInstructions()
        {
            if (instructionsPanel != null) instructionsPanel.SetActive(true);
        }

        private void OnInstructionsBack()
        {
            if (instructionsPanel != null) instructionsPanel.SetActive(false);
        }

        private void OnQuit()
        {
            Application.Quit();
        }
    }
}
