using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableActionButton : MonoBehaviour
{
    public List<GameObject> actionButtons;

    public void ToDisableActionButtons()
    {
        for (int i = 0; i < actionButtons.Count; i++)
        {
            actionButtons[i].SetActive(false);
        }
    }
    public void ToEnableActionButtons()
    {
        for (int i = 0; i < actionButtons.Count; i++)
        {
            actionButtons[i].SetActive(true);
        }
    }
}
