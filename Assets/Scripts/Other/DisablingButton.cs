using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisablingButton : MonoBehaviour
{
    public float delayTime;
    public List<Button> buttons;

    public void StartToDisable()
    {
        StartCoroutine(DisableAndEnable());
    }
    IEnumerator DisableAndEnable()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = true;
        }
    }
}
