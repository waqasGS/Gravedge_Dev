using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController.vActions;
using UnityEngine;

public class UnmountButton : MonoBehaviour
{
    public vTriggerGenericAction trigger;
 
    [Header("Runtime")]
    public MotorcycleSetup motorcycleSetup;
    
    public void Unmount()
    {
        motorcycleSetup.Unmount();
    }
}