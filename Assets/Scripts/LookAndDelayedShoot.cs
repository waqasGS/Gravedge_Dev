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
    private vHealthController playerHealth;
    private vHealthController enemyHealth; // <-- Enemy AI health
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

        // Get player health
        if (player != null)
        {
            playerHealth = player.GetComponent<vHealthController>();
        }

        // Get shooter component
        shooterComponent = GetComponent<Invector.vShooter.vShooterWeaponBase>();
        if (shooterComponent == null)
            Debug.LogError("Shooter component not found on this GameObject!");

        // Get enemy (self) health
        enemyHealth = GetComponentInParent<vHealthController>();
        if (enemyHealth == null)
            Debug.LogError("Enemy health component not found on parent!");
    }

    void Update()
    {
        if (player == null || _parentTransform == null || playerHealth == null || enemyHealth == null)
            return;

        // Stop if player is dead
        if (playerHealth.currentHealth <= 0)
            return;

        // Stop shooting if enemy is dead
        if (enemyHealth.currentHealth <= 0)
            return;

        Vector3 directionToPlayer = player.position - _parentTransform.position;
        float distance = directionToPlayer.magnitude;

        // Horizontal LookAt
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            _parentTransform.rotation = Quaternion.Slerp(_parentTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Shoot if within range
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
