using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using static Invector.vItemManager.vCheckItemIsEquipped;

public class GrenadeSwitch : MonoBehaviour
{
    public vCheckItemIsEquipped vCheckItemIsEquipped;
    public ButtonHandler buttonHandler;
    public bool isGrenadeEquip;
    public vEquipArea shootEquip;
    public vEquipmentDisplay rightDisplay;

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
            vCheckItemIsEquipped.UnEquipingUI();
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
        StartCoroutine(SwitchToNextWeapon());
    }
    public void ButtonUp()
    {

        CrossPlatformInputManager.SetButtonUp("GB");
    }
    public void IsAlwaysEquiped()
    {
        isGrenadeEquip = true;
    }

    IEnumerator SwitchToNextWeapon()
    {
        //shootEquip.EquipCurrentSlot();
        //shootEquip.UnequipCurrentItem();
        yield return new WaitForSeconds(0.57f);
        shootEquip.SwitchToNextWeapon();
    }
}
