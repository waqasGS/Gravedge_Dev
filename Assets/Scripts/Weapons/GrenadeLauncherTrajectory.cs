using Invector.Throw;
using Invector.vCharacterController;
using Invector.vItemManager;
using Invector.vShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(vShooterWeapon))]
public class GrenadeLauncherTrajectory : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public bool showTrajectoryWhileAiming = true;
    public bool showTrajectoryWhenEquipped = true;
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
    private vShooterManager shooterManager;
    private vEquipArea equipArea;

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

        shooterManager = tpInput.GetComponent<vShooterManager>();
        if (shooterManager == null)
        {
            Debug.LogError("[GrenadeLauncher] vShooterManager not found on character!");
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
        weapon.onDisable.AddListener(OnWeaponDisable);
        shooterManager.onEquipWeapon.AddListener(OnWeaponEquip);
        shooterManager.onUnequipWeapon.AddListener(OnWeaponUnequip);

        // Find and subscribe to inventory events
        StartCoroutine(FindAndSubscribeToInventory());

        // Apply initial vertical offset
        UpdateThrowStartVerticalOffset();
    }

    private IEnumerator FindAndSubscribeToInventory()
    {
        // Wait for a frame to ensure inventory is initialized
        yield return null;

        // Find the inventory in parent hierarchy
        var inventory = GetComponentInParent<vInventory>();
        if (inventory == null)
        {
            // Try to find inventory in the character's root object
            var characterRoot = transform.root;
            inventory = characterRoot.GetComponentInChildren<vInventory>(true);
            
            if (inventory == null)
            {
                Debug.LogError("[GrenadeLauncher] vInventory not found in character hierarchy! Make sure the inventory is a child of the character root object.");
                yield break;
            }
        }

        // Wait for equip areas to be initialized
        while (inventory.equipAreas == null || inventory.equipAreas.Length == 0)
        {
            yield return null;
        }

        // Subscribe to all equip areas that could potentially hold this weapon
        foreach (var area in inventory.equipAreas)
        {
            // Subscribe to equip area events
            area.onEquipItem.AddListener(OnEquipAreaItemEquip);
            area.onUnequipItem.AddListener(OnEquipAreaItemUnequip);
        }
        
        // Subscribe to inventory events
        inventory.onEquipItem.AddListener(OnInventoryEquipItem);
        inventory.onUnequipItem.AddListener(OnInventoryUnequipItem);
    }

    private void OnEquipAreaItemEquip(vEquipArea area, vItem item)
    {
        // Check if this is our weapon
        if (item.originalObject == gameObject)
        {
            equipArea = area; // Store the equip area for this weapon
            if (showTrajectoryWhenEquipped && throwManager != null)
            {
                throwManager.drawTrajectory = true;
                throwManager.lineRenderer.enabled = true;
                UpdateTrajectory();
            }
        }
    }

    private void OnEquipAreaItemUnequip(vEquipArea area, vItem item)
    {
        // Check if this is our weapon
        if (item.originalObject == gameObject)
        {
            if (throwManager != null)
            {
                // Only hide trajectory if we're not re-equipping the same weapon
                if (area.currentEquippedItem == null || area.currentEquippedItem.originalObject != gameObject)
                {
                    //throwManager.drawTrajectory = false;
                    if (throwManager.lineRenderer && throwManager.lineRenderer.gameObject.activeInHierarchy)
                    {
                        throwManager.lineRenderer.gameObject.SetActive(false);
                    }
                    //if (throwManager.lineRenderer)
                    //{
                    //    throwManager.lineRenderer.enabled = false;
                    //}
                    if (throwManager.throwEnd && throwManager.throwEnd.activeSelf)
                    {
                        throwManager.throwEnd.SetActive(false);
                    }
                    //if (throwManager.throwEnd)
                    //{
                    //    throwManager.throwEnd.SetActive(false);
                    //}
                }
            }
        }
    }

    private void OnInventoryEquipItem(vEquipArea area, vItem item)
    {
        // Check if this is our weapon
        if (item.originalObject == gameObject)
        {
            equipArea = area; // Store the equip area for this weapon
            if (showTrajectoryWhenEquipped && throwManager != null)
            {
                throwManager.drawTrajectory = true;
                throwManager.lineRenderer.enabled = true;
                UpdateTrajectory();
            }
        }
    }

    private void OnInventoryUnequipItem(vEquipArea area, vItem item)
    {
        // Check if this is our weapon
        if (item.originalObject == gameObject)
        {
            if (throwManager != null)
            {
                //throwManager.drawTrajectory = false;
                if (throwManager.lineRenderer && throwManager.lineRenderer.gameObject.activeInHierarchy)
                {
                    throwManager.lineRenderer.gameObject.SetActive(false);
                }
                //if (throwManager.lineRenderer)
                //{
                //    throwManager.lineRenderer.enabled = false;
                //}
                if (throwManager.throwEnd && throwManager.throwEnd.activeSelf)
                {
                    throwManager.throwEnd.SetActive(false);
                }
                //if (throwManager.throwEnd)
                //{
                //    throwManager.throwEnd.SetActive(false);
                //}
            }
        }
    }

    private void OnWeaponDisable()
    {
        // Disable trajectory visualization when weapon is disabled
        if (throwManager != null)
        {
            //throwManager.drawTrajectory = false;
            if (throwManager.lineRenderer && throwManager.lineRenderer.gameObject.activeInHierarchy)
            {
                throwManager.lineRenderer.gameObject.SetActive(false);
            }
            //if (throwManager.lineRenderer)
            //{
            //    throwManager.lineRenderer.enabled = false;
            //}
            if (throwManager.throwEnd && throwManager.throwEnd.activeSelf)
            {
                throwManager.throwEnd.SetActive(false);
            }
            //if (throwManager.throwEnd)
            //{
            //    throwManager.throwEnd.SetActive(false);
            //}
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
            //throwManager.drawTrajectory = false;
            if (throwManager.lineRenderer && throwManager.lineRenderer.gameObject.activeInHierarchy)
            {
                throwManager.lineRenderer.gameObject.SetActive(false);
            }
            //if (throwManager.lineRenderer)
            //{
            //    throwManager.lineRenderer.enabled = false;
            //}
            if (throwManager.throwEnd && throwManager.throwEnd.activeSelf)
            {
                throwManager.throwEnd.SetActive(false);
            }
            //if (throwManager.throwEnd)
            //{
            //    throwManager.throwEnd.SetActive(false);
            //}
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
            Debug.LogWarning("[GrenadeLauncher] Cannot update trajectory - throwManager or weapon.muzzle is null");
            return;
        }
        throwManager.throwStartRightOffsetMultiplier = throwStartRightOffset;
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

    private void OnWeaponEquip(vShooterWeapon newWeapon, bool isLeftWeapon)
    {
        if (newWeapon.gameObject == gameObject && showTrajectoryWhenEquipped && throwManager != null)
        {
            throwManager.drawTrajectory = true;
            throwManager.lineRenderer.enabled = true;
            UpdateTrajectory();
        }
    }

    private void OnWeaponUnequip(vShooterWeapon oldWeapon, bool isLeftWeapon)
    {
        if (oldWeapon.gameObject == gameObject && throwManager != null)
        {
            //throwManager.drawTrajectory = false;
            if (throwManager.lineRenderer && throwManager.lineRenderer.gameObject.activeInHierarchy)
            {
                throwManager.lineRenderer.gameObject.SetActive(false);
            }
            //if (throwManager.lineRenderer)
            //{
            //    throwManager.lineRenderer.enabled = false;
            //}
            if (throwManager.throwEnd && throwManager.throwEnd.activeSelf)
            {
                throwManager.throwEnd.SetActive(false);
            }
            //if (throwManager.throwEnd)
            //{
            //    throwManager.throwEnd.SetActive(false);
            //}
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnWeaponDrop()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from weapon events
        if (weapon != null)
        {
            weapon.onEnableAim.RemoveListener(OnWeaponAimStart);
            weapon.onDisableAim.RemoveListener(OnWeaponAimEnd);
            weapon.onInstantiateProjectile.RemoveListener((vProjectileControl projectile) => OnProjectileInstantiated(projectile.gameObject));
            weapon.onDisable.RemoveListener(OnWeaponDisable);
        }

        if (shooterManager != null)
        {
            shooterManager.onEquipWeapon.RemoveListener(OnWeaponEquip);
            shooterManager.onUnequipWeapon.RemoveListener(OnWeaponUnequip);
        }

        // Unsubscribe from inventory events
        var inventory = GetComponentInParent<vInventory>();
        if (inventory == null)
        {
            var characterRoot = transform.root;
            inventory = characterRoot.GetComponentInChildren<vInventory>(true);
        }

        if (inventory != null)
        {
            // Unsubscribe from all equip areas
            foreach (var area in inventory.equipAreas)
            {
                area.onEquipItem.RemoveListener(OnEquipAreaItemEquip);
                area.onUnequipItem.RemoveListener(OnEquipAreaItemUnequip);
            }

            // Unsubscribe from inventory events
            inventory.onEquipItem.RemoveListener(OnInventoryEquipItem);
            inventory.onUnequipItem.RemoveListener(OnInventoryUnequipItem);
        }

        // Reset throw start position
        if (throwManager != null && throwManager.throwStartPoint != null)
        {
            throwManager.throwStartPoint.localPosition = originalThrowStartPosition;
        }
    }
} 