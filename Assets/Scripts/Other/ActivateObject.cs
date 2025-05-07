using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    public GameObject destroyObject;
    public GameObject fineObject;
    public float DelayDestory;
    public GameObject fullDestory;
    public GameObject stunnedParticle;
    



    public void ActivateDestoryObject()
    {
        fineObject.SetActive(false);
        destroyObject.SetActive(true);
        Invoke(nameof(DestoryObject), DelayDestory);
    }
    public void DestoryObject()
    {
        Destroy(fullDestory);
    }

    public void ActivateStunParticle()
    {
        stunnedParticle.SetActive(true);
    }
    public void DectivateStunParticle()
    {
        stunnedParticle.SetActive(false);
    }
    
}
