using System.Collections;
using System.Collections.Generic;
using Invector.vShooter;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class DelayInAimToShoot : MonoBehaviour
{
    public ButtonHandler buttonHandler;
    public vShooterManager shooterManager;

    public void StartShooting()
    {
        StartCoroutine(OnStartShoot());
    }

    public void StopShooting()
    {
        shooterManager.DisableAim();
        GetComponent<ButtonHandler>().SetUpState();
        // StartCoroutine(OnStopShoot());
    }

    IEnumerator OnStartShoot()
    {
        shooterManager.EnableAim();
        yield return new WaitForSeconds(0.2f);
        GetComponent<ButtonHandler>().SetDownState();
    }

    IEnumerator OnStopShoot()
    {
        shooterManager.DisableAim();
        yield return new WaitForSeconds(0.1f);
        GetComponent<ButtonHandler>().SetUpState();
    }
}