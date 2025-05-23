using System.Collections;
using System.Collections.Generic;
using com.mobilin.games;
using Invector.vCharacterController.AI;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{

    private Transform targetPlayer;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mvThirdPersonController tempHealth = other.GetComponent<mvThirdPersonController>();

            if (tempHealth != null)
            {
                targetPlayer = other.transform;
                transform.parent.GetComponent<vSimpleMeleeAI_Controller>().SetCurrentTarget(targetPlayer);

                //else
                //{
                //    // Optional: Log only once or suppress
                //    // Debug.Log("Object has Player tag but no mvThirdPersonController: " + other.name);
                //}
            }
        }
    }
}
