using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Saare UI Buttons ka Enum (Dropdown me show hoga)
/// </summary>


public abstract class BaseUIManager : MonoBehaviour
{
    [Tooltip("PlayButton, 0\n" +
              "SettingsButton, 1\n" +
              "CreditsButton, 2\n" +
              "QuitButton, 3\n" +
              "PauseButton, 4\n" +
              "ResumeButton, 5\n" +
              "RestartButton, 6\n" +
              "MainMenuButton, 7\n" +
              "WinPanel, 8\n" +
              "LosePanel, 9\n" +
              "NextLevelButton, 10")]
    public string buttonTooltip = "UI Button Index Guide"; // Inspector me ye field dikhayega


    public static BaseUIManager Instance { get; private set; } // Singleton instance

    protected virtual void Awake()
    {
        // Ensure Singleton pattern (ek hi instance ho)
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Jab koi bhi button click hoga toh yeh function call hoga (Dropdown se selectable)
    /// </summary>
    public virtual void OnButtonClick(int buttonType)
    {
        Debug.Log($"Button Clicked: {buttonType}");
    }

    /// <summary>
    /// Scene change karne ke liye function
    /// </summary>
    public virtual void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Panel toggle karega (agar open hai toh close hoga, agar close hai toh open hoga)
    /// </summary>
    public virtual void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    /// <summary>
    /// Game ko quit karne ka function
    /// </summary>
    public virtual void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Panel ko delay ke saath show karega
    /// </summary>
    protected IEnumerator ShowPanelWithDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null)
            panel.SetActive(true);
    }
}
