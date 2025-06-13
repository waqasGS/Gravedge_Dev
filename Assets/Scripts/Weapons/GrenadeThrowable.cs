using UnityEngine;
using Invector.Throw;

[RequireComponent(typeof(Rigidbody))]
public class GrenadeThrowable : vThrowableObject
{
    [Header("Grenade Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 1000f;
    public float explosionDelay = 3f;
    public GameObject explosionEffect;
    public LayerMask explosionLayerMask;

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Invoke("Explode", explosionDelay);
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Create explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply explosion force to nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayerMask);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Destroy the grenade
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 