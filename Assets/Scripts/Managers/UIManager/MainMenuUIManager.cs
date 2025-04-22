using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : BaseUIManager
{
  

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject creditsPanel;



    /// <summary>
    /// Main Menu buttons ke liye click handling
    /// </summary>
    public override void OnButtonClick(int buttonType)
    {
        switch (buttonType)
        {
            case 0:
                LoadScene("GameScene");
                break;
            case 1:
                TogglePanel(settingsPanel);
                break;
            case 2:
                TogglePanel(creditsPanel);
                break;
            case 3:
                QuitGame();
                break;
            default:
                Debug.Log($"No action assigned for: {buttonType}");
                break;
        }
    }
}
