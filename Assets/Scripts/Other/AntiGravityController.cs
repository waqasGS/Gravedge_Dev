using UnityEngine;

public class AntiGravityController : MonoBehaviour
{
    public PA_DroneShooterEnemy shooterWeapon;
    [Header("Anti-Gravity Settings")]
    public float targetHeight = 5f;
    public float riseDuration = 1.5f;
    public float floatDuration = 3f;
    public float rotationSpeed = 90f;
    public AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Rigidbody rb;
    private Animator animator;

    private Quaternion originalRotation;
    private Vector3 originalLocalPosition;
    private Vector3 targetLocalUpPosition;
    private Vector3 rotationAxis;

    private float timer = 0f;
    private float riseTimeElapsed = 0f;
    private float fallTimeElapsed = 0f;

    // Add these at the top with the other variables
    private Vector3 targetRotationAxis;
    private float axisChangeTimer = 0f;
    public float axisChangeInterval = 1f; // Change axis every 1 second
    public float axisLerpSpeed = 1f; // How fast to blend to new axis



    public enum State { Idle, GoingUp, Floating, GoingDown }
    public State currentState = State.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void ActivateAntiGravity()
    {
        if (currentState != State.Idle) return;

        originalLocalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        targetLocalUpPosition = originalLocalPosition + new Vector3(0f, targetHeight, 0f);

        if (animator) animator.enabled = false;

        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        rotationAxis = Random.onUnitSphere.normalized;
        targetRotationAxis = Random.onUnitSphere.normalized;
        axisChangeTimer = 0f;
        riseTimeElapsed = 0f;
        currentState = State.GoingUp;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.GoingUp:
                riseTimeElapsed += Time.deltaTime;
                float tUp = Mathf.Clamp01(riseTimeElapsed / riseDuration);
                float curvedTUp = floatCurve.Evaluate(tUp);
                transform.localPosition = Vector3.Lerp(Vector3.zero, targetLocalUpPosition, curvedTUp);
                RotateObject();

                if (tUp >= 1f)
                {
                    timer = 0f;
                    currentState = State.Floating;
                }
                break;

            case State.Floating:
                timer += Time.deltaTime;

                axisChangeTimer += Time.deltaTime;
                if (axisChangeTimer >= axisChangeInterval)
                {
                    axisChangeTimer = 0f;
                    //targetRotationAxis = Random.onUnitSphere.normalized;
                }

                shooterWeapon.TimeToShoot();
                // Smoothly blend current rotation axis to target
                //rotationAxis = Vector3.Slerp(rotationAxis, targetRotationAxis, Time.deltaTime * axisLerpSpeed);

                RotateObject();

                if (timer >= floatDuration)
                {
                    fallTimeElapsed = 0f;
                    currentState = State.GoingDown;
                }
                break;


            case State.GoingDown:
                fallTimeElapsed += Time.deltaTime;
                float tDown = Mathf.Clamp01(fallTimeElapsed / riseDuration);
                float curvedTDown = floatCurve.Evaluate(tDown);
                transform.localPosition = Vector3.Lerp(targetLocalUpPosition, Vector3.zero, curvedTDown);
                RotateToOriginal();

                if (tDown >= 1f)
                {
                    transform.localPosition = originalLocalPosition;
                    transform.localRotation = originalRotation;
                    ResetSystem();
                }
                break;
        }
    }

    void RotateObject()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self); // local rotation
    }

    void RotateToOriginal()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * 2f);
    }

    void ResetSystem()
    {
        if (rb)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (animator) animator.enabled = true;

        currentState = State.Idle;
    }
}
