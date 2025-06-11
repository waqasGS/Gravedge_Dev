using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityTestCylinder : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<ActivatingGravityEffect>() != null)
        {
            other.GetComponent<ActivatingGravityEffect>().gravityActivating.Invoke();
        }
    }
}
