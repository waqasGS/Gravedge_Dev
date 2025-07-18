using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController.vActions;
using UnityEngine;

public class MotorcycleSetup : MonoBehaviour
{
    public vTriggerGenericAction genericAction;
    
    [Header("Runtime")]
    public BikeUI bikeUI;
    public UnmountButton unmountBikeButton;

    private void Start()
    {
        bikeUI = FindAnyObjectByType<BikeUI>();
        unmountBikeButton = FindObjectOfType<UnmountButton>(true);
    }

    public void Mount()
    {
        bikeUI.MountBike();
        unmountBikeButton.trigger = genericAction;
        unmountBikeButton.motorcycleSetup = this;
        
        Invoke(nameof(InvokeMount), 1.5f);
    }

    private void InvokeMount()
    {
        unmountBikeButton.gameObject.SetActive(true);
    }

    public void Unmount()
    {
        Invoke(nameof(InvokeUnmount), 1.5f);
    }

    private void InvokeUnmount()
    {
        genericAction.gameObject.SetActive(true);
    }
}