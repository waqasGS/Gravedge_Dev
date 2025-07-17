using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GrenadeSwitch : MonoBehaviour
{
    public ButtonHandler buttonHandler;
    public bool isGrenadeEquip;

    public void GrenadButtonClick()
    {
        ButtonToggle();
        CrossPlatformInputManager.SetButtonDown("GB");
        Invoke(nameof(ButtonUp), 0.5f);
    }
    public void ButtonToggle()
    {
        if (!isGrenadeEquip)
        {
            isGrenadeEquip = true;

        }
        else
        {
            isGrenadeEquip = false;
        }
    }
    public void SwitchGrenad()
    {
        if (isGrenadeEquip)
        {
            isGrenadeEquip = false;
            CrossPlatformInputManager.SetButtonDown("GB");
            Invoke(nameof(ButtonUp), 0.5f);
        }
    }
    public void ButtonUp()
    {

        CrossPlatformInputManager.SetButtonUp("GB");
    }
    public void IsAlwaysEquiped()
    {
        isGrenadeEquip = true;
    }
}
