using UnityEngine;
using Invector.vShooter;

public class WallRunAimController : MonoBehaviour
{
    private vShooterManager shooterManager;

    void Start()
    {
        shooterManager = FindObjectOfType<vShooterManager>();
    }

    public void OnWallRunStart()
    {
        if (shooterManager != null)
        {
            shooterManager.alwaysAiming = false;
        }
    }

    public void OnWallRunEnd()
    {
        if (shooterManager != null)
        {
            shooterManager.alwaysAiming = true;
        }
    }
}