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
        Debug.Log("StartStunned");

        _anim.SetBool("AfterShock", true);
        particle.SetActive(true); // Corrected usage of SetActive
        Invoke(nameof(StopStunned), stunnedDelay);

    }

    public void StopStunned()
    {
        _anim.SetBool("AfterShock", false);
        particle.SetActive(false); // Corrected usage of SetActive
    }
}
