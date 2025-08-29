using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneAnimator : MonoBehaviour
{
    public GameObject hitEffectLeft;
    public GameObject hitEffectRight;

    public void PlayHitEffectLeft()
    {
        hitEffectLeft.SetActive(true);
    }

    public void PlayHitEffectRight()
    {
        hitEffectRight.SetActive(true);
    }
}