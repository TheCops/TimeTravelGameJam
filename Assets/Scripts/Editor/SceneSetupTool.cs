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

        [MenuItem("TimeTravelBanana/Tools/Add Monkey + Tree To Current Scene")]
        public static void AddMonkeyAndTree()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("AddMonkeyAndTree: no active scene.");
                return;
            }

            if (FindRoot("MonkeyTree") != null)
            {
                Debug.Log("MonkeyTree already exists in " + scene.name + " — leaving it alone.");
                return;
            }

            var treeSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Art/tree.png");
            var monkeySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Art/monkey.png");
            var armSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Art/monkey_arm.png");
            if (treeSprite == null || monkeySprite == null || armSprite == null)
            {
                Debug.LogError("AddMonkeyAndTree: one or more sprites missing or not imported as Sprite. Need tree.png, monkey.png, monkey_arm.png in Assets/Resources/Art/ with Texture Type = Sprite (2D and UI).");
                return;
            }

            var root = new GameObject("MonkeyTree");
            root.transform.position = new Vector3(-9f, -2f, 0f);
            var ctrl = root.AddComponent<MonkeyController>();

            var tree = new GameObject("Tree", typeof(SpriteRenderer));
            tree.transform.SetParent(root.transform, false);
            tree.transform.localPosition = Vector3.zero;
            var treeSr = tree.GetComponent<SpriteRenderer>();
            treeSr.sprite = treeSprite;
            treeSr.sortingOrder = -50;

            var monkey = new GameObject("Monkey", typeof(SpriteRenderer));
            monkey.transform.SetParent(root.transform, false);
            monkey.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var monkeySr = monkey.GetComponent<SpriteRenderer>();
            monkeySr.sprite = monkeySprite;
            monkeySr.sortingOrder = -40;

            var arm = new GameObject("Arm");
            arm.transform.SetParent(monkey.transform, false);
            arm.transform.localPosition = new Vector3(0.4f, 0.2f, 0f);

            var armSpriteGo = new GameObject("ArmSprite", typeof(SpriteRenderer));
            armSpriteGo.transform.SetParent(arm.transform, false);
            armSpriteGo.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            var armSr = armSpriteGo.GetComponent<SpriteRenderer>();
            armSr.sprite = armSprite;
            armSr.sortingOrder = -39;

            var hand = new GameObject("Hand");
            hand.transform.SetParent(arm.transform, false);
            hand.transform.localPosition = new Vector3(0f, 1.0f, 0f);

            var launcherGo = FindRoot("Launcher");
            if (launcherGo != null)
            {
                var launcher = launcherGo.GetComponent<Launcher>();
                if (launcher != null)
                {
                    var lso = new SerializedObject(launcher);
                    var monkeyProp = lso.FindProperty("monkey");
                    if (monkeyProp != null)
                    {
                        monkeyProp.objectReferenceValue = ctrl;
                        lso.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = root;
            Debug.Log("Added MonkeyTree to " + scene.name + ". Adjust positions of Tree/Monkey/Arm/Hand children in the Hierarchy.");
        }

        [MenuItem("TimeTravelBanana/Tools/Add Background To Current Scene")]
        public static void AddBackgroundToCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("AddBackgroundToCurrentScene: no active scene.");
                return;
            }

            if (FindRoot("LevelBackground") != null)
            {
                Debug.Log("LevelBackground already exists in " + scene.name + " — leaving it alone.");
                return;
            }

            const string spritePath = "Assets/Resources/Art/background.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogError("Could not load Sprite at " + spritePath + ". Make sure Texture Type is Sprite (2D and UI) in the Inspector.");
                return;
            }

            var go = new GameObject("LevelBackground");
            go.transform.position = new Vector3(0f, 0f, 10f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100;

            var cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                float worldH = cam.orthographicSize * 2f;
                float worldW = worldH * cam.aspect;
                float spriteW = sprite.bounds.size.x;
                float spriteH = sprite.bounds.size.y;
                float scale = Mathf.Max(worldW / spriteW, worldH / spriteH);
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = go;
            Debug.Log("Added LevelBackground to " + scene.name + ". Adjust its Transform; the change will save with the scene.");
        }

        [MenuItem("TimeTravelBanana/Tools/Place Level Kit In Current Scene")]
        public static void PlaceLevelKit()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("PlaceLevelKit: no active scene.");
                return;
            }

            EnsureStaticBox("Floor",     new Vector2(0f, -4f), new Vector2(20f, 0.5f), new Color(0.3f, 0.25f, 0.2f));
            EnsureStaticBox("Ceiling",   new Vector2(0f,  5f), new Vector2(20f, 0.5f), new Color(0.3f, 0.25f, 0.2f), bouncy: true);
            EnsureStaticBox("LeftWall",  new Vector2(-10f, 0.5f), new Vector2(0.5f, 9f), new Color(0.3f, 0.25f, 0.2f));
            EnsureStaticBox("RightWall", new Vector2( 10f, 0.5f), new Vector2(0.5f, 9f), new Color(0.3f, 0.25f, 0.2f));
            EnsureBucket("Bucket", new Vector2(8f, -3f));

            var launcher = EnsureLauncher("Launcher", new Vector2(-8f, -2.5f), 60f, 13f);
            EnsureGameManager(launcher);

            EnsureCanvasUiControllers();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Placed level kit into " + scene.name);
        }

        private static GameObject FindRoot(string name)
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == name) return go;
            return null;
        }

        private static void EnsureStaticBox(string name, Vector2 pos, Vector2 size, Color color, bool bouncy = false)
        {
            if (FindRoot(name) != null) return;
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

        private static void EnsureBucket(string name, Vector2 pos)
        {
            if (FindRoot(name) != null) return;
            var root = new GameObject(name);
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

        private static Launcher EnsureLauncher(string name, Vector2 pos, float angle, float speed)
        {
            var existing = FindRoot(name);
            if (existing != null)
            {
                var l = existing.GetComponent<Launcher>();
                if (l == null) l = existing.AddComponent<Launcher>();
                return l;
            }
            var go = new GameObject(name);
            go.transform.position = pos;
            var launcher = go.AddComponent<Launcher>();
            launcher.SetLaunchAngle(angle);
            launcher.SetLaunchSpeed(speed);
            return launcher;
        }

        private static void EnsureGameManager(Launcher launcher)
        {
            var existing = FindRoot("GameManager");
            GameManager gm;
            if (existing != null)
            {
                gm = existing.GetComponent<GameManager>();
                if (gm == null) gm = existing.AddComponent<GameManager>();
            }
            else
            {
                var go = new GameObject("GameManager");
                gm = go.AddComponent<GameManager>();
            }
            var so = new SerializedObject(gm);
            var prop = so.FindProperty("launcher");
            if (prop != null && prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = launcher;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureCanvasUiControllers()
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null) return;
            var go = canvas.gameObject;
            if (go.GetComponent<TrayController>() == null) go.AddComponent<TrayController>();
            if (go.GetComponent<Playtester>() == null)     go.AddComponent<Playtester>();
            if (go.GetComponent<ScoreTracker>() == null)   go.AddComponent<ScoreTracker>();
            if (go.activeSelf) go.SetActive(false);
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
