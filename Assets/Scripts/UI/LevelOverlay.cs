using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TimeTravelBanana.UI
{
    public class LevelOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button[] mainMenuButtons;
        [SerializeField] private Button[] retryButtons;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
            if (mainMenuButtons != null)
                foreach (var b in mainMenuButtons) if (b != null) b.onClick.AddListener(OnMainMenu);
            if (retryButtons != null)
                foreach (var b in retryButtons) if (b != null) b.onClick.AddListener(OnRetry);
        }

        public void ShowWin()
        {
            if (winPanel != null) winPanel.SetActive(true);
        }

        public void ShowLose()
        {
            if (losePanel != null) losePanel.SetActive(true);
        }

        private void OnMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnRetry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
