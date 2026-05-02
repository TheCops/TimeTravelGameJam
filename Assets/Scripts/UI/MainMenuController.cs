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

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (instructionsButton != null) instructionsButton.onClick.AddListener(OnInstructions);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (instructionsBackButton != null) instructionsBackButton.onClick.AddListener(OnInstructionsBack);
            if (instructionsPanel != null) instructionsPanel.SetActive(false);
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
