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
        //StopCoroutine(OnStopShoot());
        StartCoroutine(OnStartShoot());
    }

    public void StopShooting()
    {
        //shooterManager.DisableAim();
        //GetComponent<ButtonHandler>().SetUpState();
        StartCoroutine(OnStopShoot());
    }

    IEnumerator OnStartShoot()
    {
        StopCoroutine(OnStopShoot());
        shooterManager.EnableAim();
        yield return new WaitForSeconds(0.01f);
        GetComponent<ButtonHandler>().SetDownState();
    }

    IEnumerator OnStopShoot()
    {
        yield return new WaitForSeconds(0.11f);
        StopCoroutine(OnStartShoot());
        shooterManager.DisableAim();
        yield return new WaitForSeconds(0.2f);
        GetComponent<ButtonHandler>().SetUpState();
    }
}