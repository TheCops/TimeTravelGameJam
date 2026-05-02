using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.UI;

namespace TimeTravelBanana.Game
{
    public class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private bool buildFloorAndWalls = true;
        [SerializeField] private float floorY = -4f;
        [SerializeField] private float ceilingY = 5f;
        [SerializeField] private float leftWallX = -10f;
        [SerializeField] private float rightWallX = 10f;
        [SerializeField] private int trampolineStock = 3;
        [SerializeField] private int blockStock = 3;

        private void Awake()
        {
            if (Object.FindFirstObjectByType<GameStateController>() != null) return;

            if (buildFloorAndWalls)
            {
                float width = rightWallX - leftWallX;
                float height = ceilingY - floorY;
                CreateStaticBox("Floor",   new Vector2((leftWallX + rightWallX) * 0.5f, floorY),    new Vector2(width, 0.5f),  new Color(0.3f, 0.25f, 0.2f));
                CreateStaticBox("Ceiling", new Vector2((leftWallX + rightWallX) * 0.5f, ceilingY),  new Vector2(width, 0.5f),  new Color(0.3f, 0.25f, 0.2f), bouncy: true);
                CreateStaticBox("LeftWall",  new Vector2(leftWallX,  (floorY + ceilingY) * 0.5f), new Vector2(0.5f, height), new Color(0.3f, 0.25f, 0.2f));
                CreateStaticBox("RightWall", new Vector2(rightWallX, (floorY + ceilingY) * 0.5f), new Vector2(0.5f, height), new Color(0.3f, 0.25f, 0.2f));
            }

            CreateBucket(new Vector2(8f, -3.0f));

            var launcherGo = new GameObject("Launcher");
            launcherGo.transform.position = new Vector3(-8f, -2.5f, 0f);
            var launcher = launcherGo.AddComponent<Launcher>();
            launcher.SetLaunchAngle(60f);
            launcher.SetLaunchSpeed(13f);

            var stateGo = new GameObject("GameStateController");
            var state = stateGo.AddComponent<GameStateController>();
            state.Configure(launcher, null);

            BuildTray(state);
            BuildPlaytester(state, launcher);
        }

        private void BuildTray(GameStateController state)
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("SceneBootstrap: no Canvas in scene; tray UI will not be built.");
                return;
            }

            var panelGo = new GameObject("TrayPanel", typeof(RectTransform), typeof(Image));
            var panelRt = (RectTransform)panelGo.transform;
            panelRt.SetParent(canvas.transform, false);
            panelRt.anchorMin = new Vector2(0f, 0f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 0.5f);
            panelRt.sizeDelta = new Vector2(150f, 0f);
            panelRt.anchoredPosition = Vector2.zero;
            panelGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            panelRt.SetAsFirstSibling();

            var trayGo = new GameObject("TrayController");
            var tray = trayGo.AddComponent<TrayController>();

            var entries = new System.Collections.Generic.List<TrayController.Entry>
            {
                new TrayController.Entry
                {
                    label = "Trampoline",
                    stock = trampolineStock,
                    iconColor = new Color(0.3f, 0.7f, 1f),
                    spawn = pos => Spawner.Trampoline(pos)
                },
                new TrayController.Entry
                {
                    label = "Block",
                    stock = blockStock,
                    iconColor = new Color(0.85f, 0.7f, 0.4f),
                    spawn = pos => Spawner.Block(pos)
                },
            };
            tray.Configure(panelRt, state, Camera.main, entries);
        }

        private void BuildPlaytester(GameStateController state, Launcher launcher)
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var btnGo = new GameObject("PlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)btnGo.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(220f, 70f);
            rt.anchoredPosition = new Vector2(0f, -20f);
            btnGo.GetComponent<Image>().color = new Color(0.15f, 0.5f, 0.25f, 0.95f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.SetParent(rt, false);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<Text>();
            label.text = "PLAY";
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 32;
            label.color = Color.white;

            var ptGo = new GameObject("Playtester");
            var pt = ptGo.AddComponent<Playtester>();
            pt.Configure(state, launcher, btnGo.GetComponent<Button>(), label);
        }

        private static void CreateStaticBox(string name, Vector2 pos, Vector2 size, Color color, bool bouncy = false)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteSquare;
            sr.color = color;
            var col = go.AddComponent<BoxCollider2D>();
            if (bouncy)
                col.sharedMaterial = new PhysicsMaterial2D("CeilingBounce") { bounciness = 0.85f, friction = 0.1f };
        }

        private static void CreateBucket(Vector2 pos)
        {
            var root = new GameObject("Bucket");
            root.transform.position = pos;

            CreateChildBox(root, "Bottom", new Vector3(0f,    0f,   0f), new Vector2(2.0f, 0.3f), Color.cyan);
            CreateChildBox(root, "Left",   new Vector3(-0.85f, 0.7f, 0f), new Vector2(0.3f, 1.4f), Color.cyan);
            CreateChildBox(root, "Right",  new Vector3(0.85f,  0.7f, 0f), new Vector2(0.3f, 1.4f), Color.cyan);

            var trigger = new GameObject("Trigger");
            trigger.transform.SetParent(root.transform, false);
            trigger.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var triggerCol = trigger.AddComponent<BoxCollider2D>();
            triggerCol.size = new Vector2(1.4f, 0.6f);
            triggerCol.isTrigger = true;
            trigger.AddComponent<Bucket>();
        }

        private static void CreateChildBox(GameObject parent, string name, Vector3 localPos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.WhiteSquare;
            sr.color = color;
            go.AddComponent<BoxCollider2D>();
        }
    }
}
