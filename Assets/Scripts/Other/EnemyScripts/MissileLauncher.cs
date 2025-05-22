using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Invector.vShooter;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    //public GameObject missilePrefab;
    //public Transform launchPoint;
    public Transform launcherPivot; // Part that aims vertically
    public Transform launcherPivot1; // Part that aims vertically
    public List<vShooterWeapon> shooterWeapon;
    public float fireCooldown = 5f;
    public float aimSpeed = 5f;
    public float shotDelay = 0.5f; // delay between shots
    public float setOffX;
    public float setOffY;


    private float lastFireTime;

    public void FireMissile(Transform target)
    {
        if (Time.time - lastFireTime < fireCooldown) return;

        AimLauncher(target); // Vertical aiming


        //GameObject missile = Instantiate(missilePrefab, launchPoint.position, launchPoint.rotation);
        //missile.GetComponent<Missile>().SetTarget(target);

        lastFireTime = Time.time;
    }

    void AimLauncher(Transform target)
    {
        Vector3 direction = target.position - launcherPivot.position;
        Quaternion targetRot = Quaternion.LookRotation(direction);

        // Apply custom Euler rotation offset
        Quaternion offsetRotation = Quaternion.Euler(setOffX, setOffY, 0f);
        Quaternion finalRot = targetRot * offsetRotation;

        launcherPivot.rotation = Quaternion.Slerp(launcherPivot.rotation, targetRot, aimSpeed * Time.deltaTime);
        launcherPivot1.rotation = Quaternion.Slerp(launcherPivot1.rotation, finalRot, aimSpeed * Time.deltaTime);
        FireWeapon();
    }


    public void FireWeapon()
    {
        StopAllCoroutines(); // Optional: avoid overlapping fires
        StartCoroutine(FireWeaponRoutine());
    }

    private IEnumerator FireWeaponRoutine()
    {
        foreach (var weapon in shooterWeapon)
        {
            if (weapon != null && weapon.ammo > 0)
            {
                //if (droneAnimator != null)
                //    droneAnimator.SetTrigger("Shoot");

                weapon.Shoot();

                yield return new WaitForSeconds(shotDelay); // delay before next weapon
            }
        }
    }

}
