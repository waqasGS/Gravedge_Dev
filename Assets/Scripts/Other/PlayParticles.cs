using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticles : MonoBehaviour
{
    public ParticleSystem ps;
    private void OnEnable()
    {
        ps.Play();
    }
}
