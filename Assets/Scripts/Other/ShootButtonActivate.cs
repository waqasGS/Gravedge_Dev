using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootButtonActivate : MonoBehaviour
{
    public Button buttonClick; // This references the ButtonHandler script
    public GameObject shootButton;
    public GameObject unArmedButton;

    public void ToShowShootButton()
    {
        if (!shootButton.activeInHierarchy)
        {
            shootButton.SetActive(true);
            unArmedButton.SetActive(false);
            //buttonClick.onClick.Invoke();
            //CrossPlatformInputManager.SetButtonUp("GB");
        }
    }

    //public void CancelAiming()
    //{
    //    // Example usage: // simulate button press
    //    buttonHandler.SetUpState();   // simulate button release
    //}
}
