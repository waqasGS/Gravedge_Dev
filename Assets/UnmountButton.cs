using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController.vActions;
using UnityEngine;

public class UnmountButton : MonoBehaviour
{
    public vTriggerGenericAction trigger;

    [Header("Runtime")]
    public MotorcycleSetup motorcycleSetup;

    [Header("UI Reference")]
    public GameObject bikeUI; 

    public void Unmount()
    {
        if (bikeUI != null)
            bikeUI.SetActive(false); 

        motorcycleSetup.Unmount();    
    }
}
