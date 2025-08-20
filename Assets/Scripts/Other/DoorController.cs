using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject door;
    [Header("Axis Control")]
    public bool moveX;
    public bool moveY;
    public bool moveZ;

    [Header("Target Values")]
    public float openValue = 5f;
    public float closeValue = 0f;
    public float smoothTime = 0.2f;

    private Vector3 initialPos;
    private Vector3 targetPos;
    private Vector3 velocity = Vector3.zero;
    private Coroutine moveRoutine;

    private void Start()
    {
        initialPos = door.transform.localPosition;
        targetPos = initialPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetTarget(openValue);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetTarget(closeValue);
        }
    }

    void SetTarget(float value)
    {
        targetPos = initialPos;

        if (moveX) targetPos.x = initialPos.x + value;
        if (moveY) targetPos.y = initialPos.y + value;
        if (moveZ) targetPos.z = initialPos.z + value;

        // agar pehle se koi coroutine chal rahi ho to usse stop kar do
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        // nayi coroutine start karo
        moveRoutine = StartCoroutine(MoveDoor());
    }

    private System.Collections.IEnumerator MoveDoor()
    {
        while ((door.transform.localPosition - targetPos).sqrMagnitude > 0.001f)
        {
            door.transform.localPosition = Vector3.SmoothDamp(
                door.transform.localPosition,
                targetPos,
                ref velocity,
                smoothTime
            );
            yield return null; // next frame tak ruk
        }

        // exact target pe snap kar do (floating point issues avoid karne ke liye)
        door.transform.localPosition = targetPos;
    }
}
