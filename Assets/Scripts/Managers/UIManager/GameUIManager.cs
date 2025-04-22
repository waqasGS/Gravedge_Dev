using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : BaseUIManager
{
    [Header("Panels")]
    public GameObject pauseMenu;
    public GameObject winPanel;
    public GameObject losePanel;
    public float winPanelDelay;
    public float losePanelDelay;
    /// <summary>
    /// Main Menu buttons ke liye click handling
    /// </summary>
    public override void OnButtonClick(int buttonType)
    {

        switch (buttonType)
        {
            case 4:
                TogglePauseMenu();
                break;
            case 5:
                TogglePauseMenu();
                break;
            case 6:
                LoadScene("GameScene");
                break;
            case 7:
                LoadScene("MainMenu");
                break;
            case 8:
                ShowWinPanel();
                break;
            case 9:
                ShowLosePanel();
                break;
            case 10: // ✅ New Case for Next Level
                LoadNextLevel();
                break;
            default:
                Debug.Log($"No action assigned for: {buttonType}");
                break;
        }
    }
    /// <summary>
    /// Pause Menu ko toggle karega (Open/Close)
    /// </summary>
    public void TogglePauseMenu()
    {
        if (pauseMenu != null)
        {
            bool isPaused = !pauseMenu.activeSelf;
            pauseMenu.SetActive(isPaused);
            Time.timeScale = isPaused ? 0 : 1; // Game pause/unpause karega
        }
    }

    /// <summary>
    /// Win Panel ko delay ke saath show karega
    /// </summary>
    public void ShowWinPanel()
    {
        StartCoroutine(ShowPanelWithDelay(winPanel, winPanelDelay));
    }

    /// <summary>
    /// Lose Panel ko delay ke saath show karega
    /// </summary>
    public void ShowLosePanel()
    {
        StartCoroutine(ShowPanelWithDelay(losePanel, losePanelDelay));
    }
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1; // 🔥 Get next scene index
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) // ✅ Ensure index is valid
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Redirecting to Main Menu.");
            LoadScene("MainMenu"); // 🔥 If no more levels, go to Main Menu
        }
    }
}

