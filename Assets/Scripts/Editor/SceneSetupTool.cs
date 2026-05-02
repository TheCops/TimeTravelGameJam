#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TimeTravelBanana.Game;
using TimeTravelBanana.UI;

namespace TimeTravelBanana.EditorTools
{
    public static class SceneSetupTool
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string MainMenuPath = ScenesFolder + "/MainMenu.unity";
        private const string Level1Path = ScenesFolder + "/Level1.unity";

        [MenuItem("TimeTravelBanana/Scenes/Create MainMenu Scene")]
        public static void CreateMainMenuScene()
        {
            EnsureScenesFolder();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var title = CreateText(canvasGo.transform, "Title", "TIME-TRAVEL BANANA", 96, new Vector2(0f, 300f), new Vector2(1400, 200));
            title.alignment = TextAnchor.MiddleCenter;

            var playBtn = CreateButton(canvasGo.transform, "PlayButton", "PLAY", new Vector2(0f, 60f));
            var instructionsBtn = CreateButton(canvasGo.transform, "InstructionsButton", "INSTRUCTIONS", new Vector2(0f, -40f));
            var quitBtn = CreateButton(canvasGo.transform, "QuitButton", "QUIT", new Vector2(0f, -140f));

            var panel = CreatePanel(canvasGo.transform, "InstructionsPanel");
            var panelText = CreateText(panel.transform, "Body",
                "Drag objects from the tray to plan a path.\n" +
                "Press SPACE to launch the banana.\n" +
                "Land it in the fruit bowl before time runs out.\n" +
                "Special blocks change time speed and direction.\n" +
                "Press R to retry.",
                36, Vector2.zero, new Vector2(1400, 600));
            panelText.alignment = TextAnchor.MiddleCenter;
            var backBtn = CreateButton(panel.transform, "BackButton", "BACK", new Vector2(0f, -350f));

            var controllerGo = new GameObject("MainMenuController");
            var controller = controllerGo.AddComponent<MainMenuController>();
            var so = new SerializedObject(controller);
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("instructionsButton").objectReferenceValue = instructionsBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("instructionsPanel").objectReferenceValue = panel;
            so.FindProperty("instructionsBackButton").objectReferenceValue = backBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainMenuPath);
            AddSceneToBuildSettings(MainMenuPath, 0);
            Debug.Log("Created " + MainMenuPath);
        }

        [MenuItem("TimeTravelBanana/Scenes/Create Level1 Scene")]
        public static void CreateLevel1Scene()
        {
            EnsureScenesFolder();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            BuildLevel1World();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGo.AddComponent<TrayController>();
            canvasGo.AddComponent<Playtester>();
            canvasGo.AddComponent<ScoreTracker>();

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var winPanel = CreatePanel(canvasGo.transform, "WinPanel");
            CreateText(winPanel.transform, "WinText", "YOU WIN", 96, new Vector2(0f, 100f), new Vector2(1200, 200));
            var winMenuBtn = CreateButton(winPanel.transform, "MainMenuButton", "MAIN MENU", new Vector2(-200f, -100f));
            var winRetryBtn = CreateButton(winPanel.transform, "RetryButton", "RETRY", new Vector2(200f, -100f));

            var losePanel = CreatePanel(canvasGo.transform, "LosePanel");
            CreateText(losePanel.transform, "LoseText", "TIME'S UP", 96, new Vector2(0f, 100f), new Vector2(1200, 200));
            var loseMenuBtn = CreateButton(losePanel.transform, "MainMenuButton", "MAIN MENU", new Vector2(-200f, -100f));
            var loseRetryBtn = CreateButton(losePanel.transform, "RetryButton", "RETRY", new Vector2(200f, -100f));

            var overlayGo = new GameObject("LevelOverlay");
            var overlay = overlayGo.AddComponent<LevelOverlay>();
            var so = new SerializedObject(overlay);
            so.FindProperty("winPanel").objectReferenceValue = winPanel;
            so.FindProperty("losePanel").objectReferenceValue = losePanel;
            var menuArr = so.FindProperty("mainMenuButtons");
            menuArr.arraySize = 2;
            menuArr.GetArrayElementAtIndex(0).objectReferenceValue = winMenuBtn;
            menuArr.GetArrayElementAtIndex(1).objectReferenceValue = loseMenuBtn;
            var retryArr = so.FindProperty("retryButtons");
            retryArr.arraySize = 2;
            retryArr.GetArrayElementAtIndex(0).objectReferenceValue = winRetryBtn;
            retryArr.GetArrayElementAtIndex(1).objectReferenceValue = loseRetryBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, Level1Path);
            AddSceneToBuildSettings(Level1Path, 1);
            Debug.Log("Created " + Level1Path);
        }

        [MenuItem("TimeTravelBanana/Scenes/Create Both Scenes")]
        public static void CreateBoth()
        {
            CreateMainMenuScene();
            CreateLevel1Scene();
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        private static void AddSceneToBuildSettings(string scenePath, int index)
        {
            var current = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            current.RemoveAll(s => s.path == scenePath);
            var entry = new EditorBuildSettingsScene(scenePath, true);
            if (index >= current.Count) current.Add(entry);
            else current.Insert(index, entry);
            EditorBuildSettings.scenes = current.ToArray();
        }

        private static void BuildLevel1World()
        {
            const float floorY = -4f, ceilingY = 5f, leftX = -10f, rightX = 10f;
            float width = rightX - leftX;
            float height = ceilingY - floorY;
            CreateStaticBox("Floor",    new Vector2((leftX + rightX) * 0.5f, floorY),    new Vector2(width, 0.5f),  new Color(0.3f, 0.25f, 0.2f));
            CreateStaticBox("Ceiling",  new Vector2((leftX + rightX) * 0.5f, ceilingY),  new Vector2(width, 0.5f),  new Color(0.3f, 0.25f, 0.2f), bouncy: true);
            CreateStaticBox("LeftWall", new Vector2(leftX,  (floorY + ceilingY) * 0.5f), new Vector2(0.5f, height), new Color(0.3f, 0.25f, 0.2f));
            CreateStaticBox("RightWall",new Vector2(rightX, (floorY + ceilingY) * 0.5f), new Vector2(0.5f, height), new Color(0.3f, 0.25f, 0.2f));

            CreateBucket(new Vector2(8f, -3.0f));

            var launcherGo = new GameObject("Launcher");
            launcherGo.transform.position = new Vector3(-8f, -2.5f, 0f);
            var launcher = launcherGo.AddComponent<Launcher>();
            launcher.SetLaunchAngle(60f);
            launcher.SetLaunchSpeed(13f);

            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            var so = new SerializedObject(gm);
            so.FindProperty("launcher").objectReferenceValue = launcher;
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 80f);
            rt.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var trt = (RectTransform)textGo.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var t = textGo.GetComponent<Text>();
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 36;
            t.color = Color.white;
            return go.GetComponent<Button>();
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var t = go.GetComponent<Text>();
            t.text = content;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            return t;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
            return go;
        }
    }
}
#endif
