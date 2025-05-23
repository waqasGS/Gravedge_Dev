using com.mobilin.games;
using UnityEngine;
using UnityEngine.AI;

public class TitanBossController : MonoBehaviour
{
    public Transform player;
    public float longRange = 15f;
    public float shortRange = 5f;
    public float moveSpeed = 3f;
    private MissileLauncher missileLauncher;
    private NavMeshAgent agent;
    public Animator animator;
    //private StompAttack stompAttack;

    private bool isActive = false; // 👈 Activation flag

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        missileLauncher = GetComponent<MissileLauncher>();
        animator = GetComponent<Animator>();
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
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("Walk", false);
        }
    }

    void Update()
    {
        if (!isActive || player.GetComponent<mvThirdPersonController>().isDead || GetComponent<mvHealthController>().isDead) return; // ⛔ Ignore until triggered
        //if (player.GetComponent<mvThirdPersonController>().isDead) return;
        float distance = Vector3.Distance(transform.position, player.position);

        FacePlayerHorizontally();

        if (distance <= shortRange)
        {
            agent.isStopped = true;
            animator.SetBool("Walk", false);
            animator.SetBool("Stomp", true);
            //stompAttack.TryStomp(player);
        }
        else if (distance <= longRange)
        {
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
}
