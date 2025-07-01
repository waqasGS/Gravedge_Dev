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
        unmountBikeButton.gameObject.SetActive(true);
        unmountBikeButton.trigger = genericAction;
        unmountBikeButton.motorcycleSetup = this;
    }

    public void Unmount()
    {
        genericAction.gameObject.SetActive(true);
    }
}