using System;
using UnityEngine;

public class OpenDoorTrigger : MonoBehaviour
{
    public Animator anim;
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("Opened", true);
            anim.SetBool("Actived", true);
        }
    }



   
}
