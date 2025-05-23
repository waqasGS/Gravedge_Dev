using System.Collections;
using System.Collections.Generic;
using Invector;
using UnityEngine;

public class StompAttack : MonoBehaviour
{
    [System.Serializable]
    public class OnStomp : UnityEngine.Events.UnityEvent<Transform> { }
    public OnStomp onStompAttack;

    public TitanExplosion explosion;
    public BoxCollider _boxCollider;
    public GameObject playsound;
    public GameObject particles;


    public void OnStompAttack()
    {
        Debug.Log("AA");
        if (GetComponent<TitanBossController>().player != null)
        {
            //explosion.SetOverrideDamageSender(GetComponent<TitanBossController>().player);
            //_boxCollider.enabled = true;
            //explosion.enabled = true;

            Debug.Log("BB");
            onStompAttack.Invoke(GetComponent<TitanBossController>().player);
            explosion.ActiveExplosion();
        }
    }
    public void DeactivateParticle()
    {
        particles.SetActive(false);
    }

}
