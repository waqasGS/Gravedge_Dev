using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToActivateUseButton : MonoBehaviour
{
    public UnityEvent myEvent;
    public UnityEvent myEvent2;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            myEvent.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            myEvent2.Invoke();
        }
    }
}
