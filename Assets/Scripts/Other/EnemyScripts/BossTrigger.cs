using com.mobilin.games;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public TitanAntiGravityController titanAntiGravityController;
    public TitanBossController titanBoss;

    void OnTriggerStay(Collider other)
    {
        if (titanAntiGravityController.currentState == TitanAntiGravityController.State.Idle)
        {
            if (other.CompareTag("Player"))
            {
                //titanBoss.StartDetectionBehavior();
                mvThirdPersonController tempHealth = other.GetComponent<mvThirdPersonController>();

                if (tempHealth != null)
                {
                    titanBoss.ActivateBoss(other.transform);

                }

            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //titanBoss.StopDetectionBehavior();
            titanBoss.DeactivateBoss();
        }
    }
}
