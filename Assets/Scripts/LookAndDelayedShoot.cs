using Invector;
using UnityEngine;
using Invector.vCharacterController;

public class LookAndDelayedShoot : MonoBehaviour
{
    public Transform player;
    public float minRange = 5f;
    public float maxRange = 20f;
    public float rotationSpeed = 5f;
    public float fireDelay = 2f;

    private Transform _parentTransform;
    private Invector.vShooter.vShooterWeaponBase shooterComponent;
    private vHealthController playerHealth; // <-- Player health reference
    private float timeSinceLastShot = 0f;

    void Start()
    {
        _parentTransform = transform.parent;

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Get player health component
        if (player != null)
        {
            playerHealth = player.GetComponent<vHealthController>();
        }

        shooterComponent = GetComponent<Invector.vShooter.vShooterWeaponBase>();
        if (shooterComponent == null)
            Debug.LogError("Shooter component not found on this GameObject!");
    }

    void Update()
    {
        if (player == null || _parentTransform == null || playerHealth == null)
            return;

        // Don't do anything if player is dead
        if (playerHealth.currentHealth <= 0)
            return;

        Vector3 directionToPlayer = player.position - _parentTransform.position;
        float distance = directionToPlayer.magnitude;

        // LookAt only horizontally
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            _parentTransform.rotation = Quaternion.Slerp(_parentTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Shoot if in range and alive
        if (distance >= minRange && distance <= maxRange)
        {
            timeSinceLastShot += Time.deltaTime;

            if (timeSinceLastShot >= fireDelay)
            {
                shooterComponent?.Shoot(); // or TryShoot()
                timeSinceLastShot = 0f;
            }
        }
        else
        {
            timeSinceLastShot = 0f;
        }
    }
}
