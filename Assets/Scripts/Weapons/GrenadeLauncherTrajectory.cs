using UnityEngine;
using Invector.vShooter;
using Invector.Throw;
using Invector.vCharacterController;
using System.Collections;

[RequireComponent(typeof(vShooterWeapon))]
public class GrenadeLauncherTrajectory : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public bool showTrajectoryWhileAiming = true;
    public vThrowSettings throwSettings;
    public vThrowVisualSettings visualSettings;
    public GameObject grenadePrefab;
    
    private vShooterWeapon weapon;
    private vThrowManager throwManager;
    private vThirdPersonInput tpInput;

    private void Start()
    {
        Debug.Log("GrenadeLauncherTrajectory Start called");
        weapon = GetComponent<vShooterWeapon>();
        if (weapon == null)
        {
            Debug.LogError("vShooterWeapon component not found!");
            return;
        }

        if (weapon.muzzle == null)
        {
            Debug.LogError("Weapon muzzle is not set!");
            return;
        }

        if (grenadePrefab == null)
        {
            Debug.LogError("Grenade prefab is not assigned!");
            return;
        }

        // Find vThirdPersonInput and vThrowManager in parent hierarchy
        tpInput = GetComponentInParent<vThirdPersonInput>();
        if (tpInput == null)
        {
            Debug.LogError("vThirdPersonInput not found in parent hierarchy!");
            return;
        }

        throwManager = tpInput.GetComponent<vThrowManager>();
        if (throwManager == null)
        {
            Debug.LogError("vThrowManager not found on character!");
            return;
        }

        // Setup settings if not provided
        if (throwSettings == null)
        {
            throwSettings = ScriptableObject.CreateInstance<vThrowSettings>();
            throwSettings.metersPerSeconds = 10f;
            throwSettings.minMaxTime = new Vector2(0.1f, 0.2f);
            throwSettings.maxDistance = 100f;
            throwSettings.maxVelocity = 100f;
            throwSettings.lineStepPerTime = 0.01f;
            throwSettings.maxLineLength = 100f;
        }
        
        if (visualSettings == null)
        {
            visualSettings = ScriptableObject.CreateInstance<vThrowVisualSettings>();
            visualSettings.useLine = true;
            visualSettings.lineRendererColor = Color.white;
            visualSettings.lineRendererWidth = 0.1f;
        }

        // Add grenade throwable
        Debug.Log("Adding grenade throwable");
        var throwable = grenadePrefab.GetComponent<vThrowableObject>();
        if (throwable == null)
        {
            Debug.LogError("Grenade prefab does not have vThrowableObject component!");
            return;
        }

        throwManager.AddThrowable("Grenade", weapon.muzzle, throwable, 1, 1);

        // Subscribe to weapon events
        Debug.Log("Subscribing to weapon events");
        weapon.onEnableAim.AddListener(OnWeaponAimStart);
        weapon.onDisableAim.AddListener(OnWeaponAimEnd);
    }

    private void OnDisable()
    {
        // Unsubscribe from weapon events
        if (weapon != null)
        {
            weapon.onEnableAim.RemoveListener(OnWeaponAimStart);
            weapon.onDisableAim.RemoveListener(OnWeaponAimEnd);
        }
    }

    private void OnWeaponAimStart()
    {
        Debug.Log("OnWeaponAimStart called");
        if (showTrajectoryWhileAiming && throwManager != null)
        {
            Debug.Log("Enabling trajectory visualization");
            throwManager.drawTrajectory = true;
            UpdateTrajectory();
        }
        else
        {
            Debug.LogWarning("Trajectory not enabled: showTrajectoryWhileAiming=" + showTrajectoryWhileAiming + ", throwManager=" + (throwManager != null));
        }
    }

    private void OnWeaponAimEnd()
    {
        Debug.Log("OnWeaponAimEnd called");
        if (showTrajectoryWhileAiming && throwManager != null)
        {
            Debug.Log("Disabling trajectory visualization");
            throwManager.drawTrajectory = false;
            if (throwManager.lineRenderer)
            {
                throwManager.lineRenderer.enabled = false;
            }
            if (throwManager.throwEnd)
            {
                throwManager.throwEnd.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Update trajectory visualization while aiming
        if (showTrajectoryWhileAiming && weapon.isAiming && throwManager != null)
        {
            UpdateTrajectory();
        }
    }

    private void UpdateTrajectory()
    {
        if (throwManager == null || !weapon.muzzle)
        {
            Debug.LogWarning("UpdateTrajectory failed: throwManager=" + (throwManager != null) + ", weapon.muzzle=" + (weapon.muzzle != null));
            return;
        }

        Debug.Log("Updating trajectory");
        throwManager.ForceTrajectoryUpdate();
    }
} 