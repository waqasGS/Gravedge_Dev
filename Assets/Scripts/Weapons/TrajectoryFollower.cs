using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class TrajectoryFollower : MonoBehaviour
{
    private Rigidbody rb;
    private List<Vector3> trajectoryPoints;
    private float startTime;
    private bool isFollowing = true;
    private float maxLifetime = 10f;
    private List<Collider> projectileColliders = new List<Collider>();
    private float totalTrajectoryTime = 2f; // Time to complete the trajectory
    private Vector3 startPosition;
    private float collisionDelay = 0.1f; // Time to wait before enabling collisions
    private bool showDebug = true; // Toggle debug visualization

    public void Initialize(List<Vector3> positions, float trajectoryTime = 2f)
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Store the starting position
        startPosition = transform.position;
        
        // Store the trajectory points
        trajectoryPoints = new List<Vector3>(positions);
        totalTrajectoryTime = trajectoryTime;
        startTime = Time.time;
        // Get all colliders on this projectile
        projectileColliders.Clear();
        projectileColliders.AddRange(GetComponents<Collider>());
        projectileColliders.AddRange(GetComponentsInChildren<Collider>());

        // Disable colliders initially
        foreach (var collider in projectileColliders)
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        // Start coroutine to enable colliders after delay
        StartCoroutine(EnableCollidersAfterDelay());
    }

    private IEnumerator EnableCollidersAfterDelay()
    {
        yield return new WaitForSeconds(collisionDelay);
        
        foreach (var collider in projectileColliders)
        {
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isFollowing || trajectoryPoints == null || trajectoryPoints.Count == 0) return;

        float timeSinceStart = Time.time - startTime;
        if (timeSinceStart > maxLifetime)
        {
            StopFollowingTrajectory();
            return;
        }

        // Calculate progress through the trajectory (0 to 1)
        float progress = Mathf.Clamp01(timeSinceStart / totalTrajectoryTime);
        
        // Calculate the index in the trajectory points array
        float exactIndex = progress * (trajectoryPoints.Count - 1);
        int index = Mathf.FloorToInt(exactIndex);
        float t = exactIndex - index;

        // Get the current position by interpolating between points
        Vector3 currentPosition;
        if (index >= trajectoryPoints.Count - 1)
        {
            currentPosition = trajectoryPoints[trajectoryPoints.Count - 1];
        }
        else
        {
            currentPosition = Vector3.Lerp(trajectoryPoints[index], trajectoryPoints[index + 1], t);
        }

        // Move the projectile to the current position
        rb.MovePosition(currentPosition);
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || trajectoryPoints == null) return;

        // Draw the trajectory points
        Gizmos.color = Color.yellow;
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            Gizmos.DrawSphere(trajectoryPoints[i], 0.1f);
            if (i < trajectoryPoints.Count - 1)
            {
                Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
            }
        }

        // Draw the current position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFollowing)
        {
            StopFollowingTrajectory();
        }
    }

    private void StopFollowingTrajectory()
    {
        isFollowing = false;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
} 