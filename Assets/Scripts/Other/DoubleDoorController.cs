using UnityEngine;

public class DoubleDoorController : MonoBehaviour
{
    [Header("Door References")]
    public GameObject leftDoor;
    public GameObject rightDoor;

    [Header("Axis Control")]
    public bool moveX;
    public bool moveY;
    public bool moveZ;

    [Header("Target Values")]
    public float openValue = 5f;   // dono doors opposite directions me move karenge
    public float closeValue = 0f;
    public float smoothTime = 0.2f;

    private Vector3 leftInitialPos;
    private Vector3 rightInitialPos;

    private Vector3 leftTargetPos;
    private Vector3 rightTargetPos;

    private Vector3 leftVelocity = Vector3.zero;
    private Vector3 rightVelocity = Vector3.zero;

    private Coroutine moveRoutine;

    public AudioSource doorSound;

    public bool isDoorOpen = false; // track door state
    public bool toReuse = true;
    private bool doorReuse = true;

    private void Start()
    {
        leftInitialPos = Vector3.zero;
        rightInitialPos = Vector3.zero;

        leftTargetPos = leftInitialPos;
        rightTargetPos = rightInitialPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isDoorOpen) return;
            SetTarget(openValue, true); // open karna
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isDoorOpen) return;
            doorReuse = toReuse;
            SetTarget(closeValue, false); // close karna
        }
    }

    public void SetTarget(float value, bool opening)
    {
        // reset targets to initial
        leftTargetPos = leftInitialPos;
        rightTargetPos = rightInitialPos;

        // left door ko ek taraf move karo
        if (moveX)
        {
            leftTargetPos.x = leftInitialPos.x - value;
            rightTargetPos.x = rightInitialPos.x - value;
        }
        if (moveY)
        {
            leftTargetPos.y = leftInitialPos.y - value;
            rightTargetPos.y = rightInitialPos.y - value;
        }
        if (moveZ)
        {
            leftTargetPos.z = leftInitialPos.z - value;
            rightTargetPos.z = rightInitialPos.z - value;
        }

        // pehle se chal rahi coroutine stop kar do
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        // nayi coroutine start karo
        moveRoutine = StartCoroutine(MoveDoors(opening));
    }

    private System.Collections.IEnumerator MoveDoors(bool opening)
    {
        if (doorSound) doorSound.Play();
        isDoorOpen = opening;

        while ((leftDoor.transform.localPosition - leftTargetPos).sqrMagnitude > 0.001f ||
               (rightDoor.transform.localPosition - rightTargetPos).sqrMagnitude > 0.001f)
        {
            leftDoor.transform.localPosition = Vector3.SmoothDamp(
                leftDoor.transform.localPosition,
                leftTargetPos,
                ref leftVelocity,
                smoothTime
            );

            rightDoor.transform.localPosition = Vector3.SmoothDamp(
                rightDoor.transform.localPosition,
                rightTargetPos,
                ref rightVelocity,
                smoothTime
            );

            yield return null;
        }

        // final snap
        leftDoor.transform.localPosition = leftTargetPos;
        rightDoor.transform.localPosition = rightTargetPos;

        if (doorSound) doorSound.Stop();

        if (!doorReuse)
        {
            Destroy(GetComponent<BoxCollider>());
            Destroy(this);
        }
    }
}
