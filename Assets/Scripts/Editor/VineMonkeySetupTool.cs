using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TimeTravelBanana.Game;

namespace TimeTravelBanana.EditorTools
{
    public static class VineMonkeySetupTool
    {
        [MenuItem("TimeTravelBanana/Tools/Add Vine Monkey To Current Scene")]
        public static void AddVineMonkey()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("AddVineMonkey: no active scene.");
                return;
            }
            var monkeySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Art/monkey.png");
            if (monkeySprite == null)
            {
                Debug.LogError("AddVineMonkey: Assets/Resources/Art/monkey.png missing or not imported as Sprite.");
                return;
            }
            var go = new GameObject("VineMonkey");
            go.transform.position = Vector3.zero;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = monkeySprite;
            sr.flipX = true;
            sr.sortingOrder = 4;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
            go.AddComponent<VineMonkey>();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = go;
            Debug.Log("VineMonkey added at origin. Drag it where you want — that position becomes the bob center.");
        }
    }
}
