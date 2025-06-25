using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunnedTimer : MonoBehaviour
{
    public Animator _anim;
    public float stunnedDelay;
    public GameObject particle; // Fixed spelling from "particl"

    public void StartStunned()
    {
      

        _anim.SetBool("AfterShock", true);
        Invoke(nameof(StopStunned), stunnedDelay);
        
        particle.SetActive(true); // Corrected usage of SetActive
    }

    public void StopStunned()
    {
        _anim.SetBool("AfterShock", false);
        particle.SetActive(false); // Corrected usage of SetActive
    }
}
