using UnityEngine;
using Invector.vShooter;
using Invector.vCharacterController;
using Invector;
using com.mobilin.games;

public class PA_DroneShooterEnemy : MonoBehaviour
{
    public float fireRate = 1.5f;
    private float nextFireTime = 0f;

    public vShooterWeapon shooterWeapon; // child rifle
    public AntiGravityController antiGravityController;
    private vShooterMeleeInput weaponInput;

    private Transform targetPlayer;
    public Animator droneAnimator;
    private vHealthController droneHealth;
    private mvThirdPersonController playerHealth;
    public Transform muzzleTransform;
    public Vector3 muzzlePostion;

    public GameObject droneOnFire;

    private void Start()
    {
        weaponInput = GetComponentInChildren<vShooterMeleeInput>();
        if (weaponInput != null) weaponInput.enabled = false;

        droneHealth = GetComponentInChildren<vHealthController>(); // Drone's health
    }

    private void OnTriggerStay(Collider other)
    {
        if (antiGravityController.currentState == AntiGravityController.State.Idle)
        {
            if (other.CompareTag("Player"))
            {
                mvThirdPersonController tempHealth = other.GetComponent<mvThirdPersonController>();

                if (tempHealth != null)
                {
                    targetPlayer = other.transform;
                    playerHealth = tempHealth;
                    droneAnimator.SetBool("PlayerEnter", false);
                    Debug.Log("Player detected: " + other.name);
                }
                //else
                //{
                //    // Optional: Log only once or suppress
                //    // Debug.Log("Object has Player tag but no mvThirdPersonController: " + other.name);
                //}
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            droneAnimator.SetBool("PlayerEnter", true);
            targetPlayer = null;
            playerHealth = null;
        }
    }

    private void FixedUpdate()
    {
        if (targetPlayer != null && playerHealth != null && IsAlive(droneHealth) && !playerHealth.isDead)
        {
            if (droneAnimator.GetBool("AfterStunned"))
                return;
            LookAtPlayer();

            TimeToShoot();
        }
    }

    void LookAtPlayer()
    {
        Vector3 dir = (targetPlayer.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.fixedDeltaTime * 5f);
    }
    public void TimeToShoot()
    {
        if (antiGravityController.currentState == AntiGravityController.State.Idle)
        {
            muzzleTransform.localPosition = muzzlePostion;
        }
        else
        {
            muzzleTransform.localPosition = Vector3.zero;
        }
        if (Time.time >= nextFireTime)
        {
            FireWeapon();
            nextFireTime = Time.time + fireRate;
        }
    }
    void FireWeapon()
    {
        if (shooterWeapon != null)
        {
            if (shooterWeapon.ammo > 0)
            {
                droneAnimator.SetTrigger("Shoot");
                shooterWeapon.Shoot();
            }
        }
    }

    bool IsAlive(vHealthController health)
    {
        return health != null && health.currentHealth > 0;
    }

    public void OnDead()
    {
        droneOnFire.SetActive(true);
        droneAnimator.SetTrigger("Dead");
    }
}
