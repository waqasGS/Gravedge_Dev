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

    public AudioSource doorSound;

    public bool isDoorOpen = false; // track door state
    public bool toReuse = true;
    bool doorReuse = true;

    private void Start()
    {
        initialPos = door.transform.localPosition;
        targetPos = initialPos;
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
        // agar door already same state me hai to kuch na karo



        targetPos = initialPos;

        if (moveX) targetPos.x = initialPos.x + value;
        if (moveY) targetPos.y = initialPos.y + value;
        if (moveZ) targetPos.z = initialPos.z + value;

        // pehle se chal rahi coroutine stop kar do
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        // nayi coroutine start karo
        moveRoutine = StartCoroutine(MoveDoor(opening));
    }

    private System.Collections.IEnumerator MoveDoor(bool opening)
    {
        doorSound.Play();
        isDoorOpen = opening;
        while ((door.transform.localPosition - targetPos).sqrMagnitude > 0.001f)
        {
            door.transform.localPosition = Vector3.SmoothDamp(
                door.transform.localPosition,
                targetPos,
                ref velocity,
                smoothTime
            );
            yield return null;
        }

        // final snap
        door.transform.localPosition = targetPos;
        doorSound.Stop();
        if (!doorReuse)
        {
            Destroy(GetComponent<BoxCollider>());
            Destroy(this);
        }
        // state update karo

    }
}
