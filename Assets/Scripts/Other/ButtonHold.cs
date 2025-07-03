using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ButtonHold : MonoBehaviour
{
    public ButtonHandler _buttonHandler;

    public void OnClickDownButton()
    {
        Debug.Log("OnClickDownButton");
        _buttonHandler.SetDownState();
        _buttonHandler.SetUpState();
    }
    public void OnClickUpButton()
    {
        Debug.Log("OnClickUpButton");
        _buttonHandler.SetDownState();
        _buttonHandler.SetUpState();
    }
}
