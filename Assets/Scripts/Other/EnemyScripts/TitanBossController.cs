using System.Collections;
using System.Collections.Generic;
using com.mobilin.games;
using UnityEngine;
using UnityEngine.AI;

public class TitanBossController : MonoBehaviour
{
    public Transform player;
    public float longRange = 15f;
    public float shortRange = 5f;
    public float moveSpeed = 3f;
    public float stompCooldown = 5f;
    public float lastStompTime;
    private MissileLauncher missileLauncher;
    private NavMeshAgent agent;
    public Animator animator;
    public bool playStompAgain = true;

    public mvHealthController healthController;
    public bool isDead = false;
    private bool isActive = false; // 👈 Activation flag
    //private StompAttack stompAttack;

    public List<GameObject> objectsToScale;
    public float delayToScale;
    public float startScale = 0.6f;
    public float endScale = 1.6f;
    public float duration = 1f;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        missileLauncher = GetComponent<MissileLauncher>();
        animator = GetComponent<Animator>();
        healthController = GetComponent<mvHealthController>();
        //stompAttack = GetComponent<StompAttack>();
    }

    public void ActivateBoss(Transform _player)
    {
        isActive = true;
        player = _player;
    }
    public void DeactivateBoss()
    {
        isActive = false;
        player = null;

        if (agent != null)
        {
            //agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("Walk", false);
        }
    }

    void Update()
    {
        if (healthController != null && healthController.isDead && !isDead)
        {
            HandleDeath();
            return;
        }
        if (isDead || !isActive || player.GetComponent<mvThirdPersonController>().isDead) return; // ⛔ Ignore until triggered
        //if (player.GetComponent<mvThirdPersonController>().isDead) return;
        float distance = Vector3.Distance(transform.position, player.position);

        FacePlayerHorizontally();

        if (distance <= shortRange)
        {

            if (Time.time >= lastStompTime + stompCooldown)
            {
                playStompAgain = true;

            }
            StompAttack();

            //stompAttack.TryStomp(player);
        }
        else if (distance > shortRange && distance <= longRange)
        {
            Debug.Log("long");
            agent.SetDestination(player.position);
            agent.isStopped = false;
            agent.speed = moveSpeed;
            animator.SetBool("Stomp", false);
            animator.SetBool("Walk", true);
            missileLauncher.FireMissile(player);
        }
    }

    void FacePlayerHorizontally()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    public void StompAttack()
    {
        if (playStompAgain)
        {
            Debug.Log("Short");

            agent.isStopped = true;
            animator.SetBool("Walk", false);
            animator.SetBool("Stomp", true);

        }
        else if (!playStompAgain)
        {
            animator.SetBool("Stomp", false);
            animator.SetBool("Walk", false);
        }
    }
    public void StopStompAfterFirstPlay()
    {
        Debug.Log(Time.time + "run time");
        lastStompTime = Time.time; // Reset cooldown timer
        playStompAgain = false;
    }
    void HandleDeath()
    {
        isDead = true;
        isActive = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        animator.SetBool("Stomp", false);
        animator.SetBool("Walk", false);
        animator.SetTrigger("Die");
        StartScalingParticles();
        // Optional: disable this script after death animation finishes
        StartCoroutine(DisableAfterDeath());
    }

    IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(10f); // adjust based on death anim length
        gameObject.SetActive(false); // Or destroy it
    }

    public void StartScalingParticles()
    {
        StartCoroutine(ScaleObjects());
    }
    IEnumerator ScaleObjects()
    {
        float timer = 0f;

        // Set all to start scale
        foreach (GameObject obj in objectsToScale)
        {
            if (obj != null)
                obj.transform.localScale = Vector3.one * startScale;
        }
        yield return new WaitForSeconds(delayToScale);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float currentScale = Mathf.Lerp(startScale, endScale, t);

            foreach (GameObject obj in objectsToScale)
            {
                if (obj != null)
                    obj.transform.localScale = Vector3.one * currentScale;
            }

            yield return null;
        }

        // Ensure final scale is exact
        foreach (GameObject obj in objectsToScale)
        {
            if (obj != null)
                obj.transform.localScale = Vector3.one * endScale;
        }
    }

}
