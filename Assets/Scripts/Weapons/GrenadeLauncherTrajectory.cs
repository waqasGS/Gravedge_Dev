using UnityEngine;
using Invector.vShooter;
using Invector.Throw;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(vShooterWeapon))]
public class GrenadeLauncherTrajectory : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public bool showTrajectoryWhileAiming = true;
    public vThrowSettings throwSettings;
    public vThrowVisualSettings visualSettings;
    public GameObject grenadePrefab;
    
    [Header("Trajectory Adjustments")]
    [Tooltip("Initial velocity of the projectile")]
    public float initialVelocity = 20f;
    [Tooltip("Minimum and maximum time for trajectory calculation")]
    public Vector2 trajectoryTimeRange = new Vector2(0.1f, 0.2f);
    [Tooltip("Maximum distance the trajectory can reach")]
    public float maxTrajectoryDistance = 100f;
    [Tooltip("Maximum velocity of the projectile")]
    public float maxTrajectoryVelocity = 100f;
    [Tooltip("How detailed the trajectory line is")]
    public float trajectoryLineStep = 0.01f;
    [Tooltip("Right offset multiplier for the throw start point")]
    public float throwStartRightOffset = 1f;
    [Tooltip("Vertical offset for the throw start point")]
    public float throwStartVerticalOffset = 0f;

    private vShooterWeapon weapon;
    private vThrowManager throwManager;
    private vThirdPersonInput tpInput;
    private Vector3 originalThrowStartPosition;

    private void Start()
    {
        weapon = GetComponent<vShooterWeapon>();
        if (weapon == null)
        {
            Debug.LogError("[GrenadeLauncher] vShooterWeapon component not found!");
            return;
        }

        if (weapon.muzzle == null)
        {
            Debug.LogError("[GrenadeLauncher] Weapon muzzle is not set!");
            return;
        }

        if (grenadePrefab == null)
        {
            Debug.LogError("[GrenadeLauncher] Grenade prefab is not assigned!");
            return;
        }

        // Find vThirdPersonInput and vThrowManager in parent hierarchy
        tpInput = GetComponentInParent<vThirdPersonInput>();
        if (tpInput == null)
        {
            Debug.LogError("[GrenadeLauncher] vThirdPersonInput not found in parent hierarchy!");
            return;
        }

        throwManager = tpInput.GetComponent<vThrowManager>();
        if (throwManager == null)
        {
            Debug.LogError("[GrenadeLauncher] vThrowManager not found on character!");
            return;
        }

        // Store original throw start position
        if (throwManager.throwStartPoint != null)
        {
            originalThrowStartPosition = throwManager.throwStartPoint.localPosition;
        }

        // Setup settings if not provided
        if (throwSettings == null)
        {
            throwSettings = ScriptableObject.CreateInstance<vThrowSettings>();
        }
        
        // Apply trajectory adjustments
        throwSettings.metersPerSeconds = initialVelocity;
        throwSettings.minMaxTime = trajectoryTimeRange;
        throwSettings.maxDistance = maxTrajectoryDistance;
        throwSettings.maxVelocity = maxTrajectoryVelocity;
        throwSettings.lineStepPerTime = trajectoryLineStep;
        throwSettings.maxLineLength = maxTrajectoryDistance;
        
        // Apply throw start offset
        throwManager.useThrowStartRightOffset = true;
        throwManager.throwStartRightOffsetMultiplier = throwStartRightOffset;
        
        if (visualSettings == null)
        {
            visualSettings = ScriptableObject.CreateInstance<vThrowVisualSettings>();
            visualSettings.useLine = true;
            visualSettings.lineRendererColor = Color.white;
            visualSettings.lineRendererWidth = 0.1f;
        }

        // Subscribe to weapon events
        weapon.onEnableAim.AddListener(OnWeaponAimStart);
        weapon.onDisableAim.AddListener(OnWeaponAimEnd);
        weapon.onInstantiateProjectile.AddListener((vProjectileControl projectile) => OnProjectileInstantiated(projectile.gameObject));

        // Apply initial vertical offset
        UpdateThrowStartVerticalOffset();
    }

    private void OnDisable()
    {
        // Unsubscribe from weapon events
        if (weapon != null)
        {
            weapon.onEnableAim.RemoveListener(OnWeaponAimStart);
            weapon.onDisableAim.RemoveListener(OnWeaponAimEnd);
            weapon.onInstantiateProjectile.RemoveListener((vProjectileControl projectile) => OnProjectileInstantiated(projectile.gameObject));
        }

        // Reset throw start position
        if (throwManager != null && throwManager.throwStartPoint != null)
        {
            throwManager.throwStartPoint.localPosition = originalThrowStartPosition;
        }
    }

    private void OnWeaponAimStart()
    {
        if (showTrajectoryWhileAiming && throwManager != null)
        {
            throwManager.drawTrajectory = true;
            UpdateTrajectory();
        }
    }

    private void OnWeaponAimEnd()
    {
        if (showTrajectoryWhileAiming && throwManager != null)
        {
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
            return;
        }
        throwManager.ForceTrajectoryUpdate();
    }

    private void OnProjectileInstantiated(GameObject projectile)
    {
        var projectileControl = projectile.GetComponent<vProjectileControl>();
        if (projectileControl != null)
        {
            projectileControl.enabled = false;
        }

        // Get the trajectory points from the line renderer
        List<Vector3> trajectoryPoints = new List<Vector3>();
        if (throwManager != null && throwManager.lineRenderer != null)
        {
            // Ensure line renderer is enabled and has points
            throwManager.lineRenderer.enabled = true;
            throwManager.ForceTrajectoryUpdate();
            
            int pointCount = throwManager.lineRenderer.positionCount;
            if (pointCount > 0)
            {
                // Convert line renderer positions to world space
                for (int i = 0; i < pointCount; i++)
                {
                    Vector3 localPos = throwManager.lineRenderer.GetPosition(i);
                    Vector3 worldPos = throwManager.lineRenderer.transform.TransformPoint(localPos);
                    trajectoryPoints.Add(worldPos);
                }
            }
            else
            {
                Debug.LogWarning("[GrenadeLauncher] Line renderer has no points!");
            }
        }
        else
        {
            Debug.LogWarning("[GrenadeLauncher] Line renderer not found!");
        }

        // Add our trajectory follower
        var trajectoryFollower = projectile.AddComponent<TrajectoryFollower>();
        trajectoryFollower.Initialize(trajectoryPoints, trajectoryTimeRange.y);
    }

    /// <summary>
    /// Updates the vertical offset of the throw start point
    /// </summary>
    public void UpdateThrowStartVerticalOffset()
    {
        if (throwManager != null && throwManager.throwStartPoint != null)
        {
            Vector3 newPosition = originalThrowStartPosition;
            newPosition.y += throwStartVerticalOffset;
            throwManager.throwStartPoint.localPosition = newPosition;
        }
    }

    /// <summary>
    /// Sets a new vertical offset for the throw start point
    /// </summary>
    /// <param name="offset">The new vertical offset value</param>
    public void SetThrowStartVerticalOffset(float offset)
    {
        throwStartVerticalOffset = offset;
        UpdateThrowStartVerticalOffset();
    }
} 