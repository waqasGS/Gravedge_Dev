using com.mobilin.games;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public TitanBossController titanBoss;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mvThirdPersonController tempHealth = other.GetComponent<mvThirdPersonController>();

            if (tempHealth != null)
            {
                titanBoss.ActivateBoss(other.transform);

            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            titanBoss.DeactivateBoss();
        }
    }
}
