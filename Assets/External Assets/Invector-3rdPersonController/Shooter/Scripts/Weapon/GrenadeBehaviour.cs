using System.Collections;
using UnityEngine;
using Invector;

public class GrenadeBehavior : MonoBehaviour
{
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionForce = 1000f;
    public GameObject explosionEffect;
    public GameObject visuals;
    public float explosionEffectDuration = 2f;
    public float damageValue = 50f;
    public bool ignoreDefense = true;
    public bool activeRagdoll = true;
    public float senselessTime = 3f;
    
    private void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }
    
    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }
    
    private void Explode()
    {
        // Apply damage and force to nearby objects immediately
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.GetComponent<ActivatingGravityEffect>() != null)
            {
                hit.GetComponent<ActivatingGravityEffect>().gravityActivating.Invoke();
            }
        }

        // Handle visual effects
        if (explosionEffect)
        {
            explosionEffect.SetActive(true);
            visuals.SetActive(false);
            StartCoroutine(DestroyAfterEffect());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterEffect()
    {
        yield return new WaitForSeconds(explosionEffectDuration);
        Destroy(gameObject);
    }
}