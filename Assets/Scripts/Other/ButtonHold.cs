using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ButtonHold : MonoBehaviour
{
    public ButtonHandler _buttonHandler;

    public void OnClickDownButton()
    {
        Debug.Log("A");
        _buttonHandler.SetDownState();
        _buttonHandler.SetUpState();
    }
    public void OnClickUpButton()
    {
        Debug.Log("B");
        _buttonHandler.SetDownState();
        _buttonHandler.SetUpState();
    }
}
