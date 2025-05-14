using com.mobilin.games;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject PausePane;
    
    
    public void OnPausePane() => TogglePanel(PausePane);



    void TogglePanel(GameObject panel)
    {
        panel?.SetActive(!panel.activeSelf);
    }

}