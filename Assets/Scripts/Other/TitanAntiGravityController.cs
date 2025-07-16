using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController.AI;
using UnityEngine;
using UnityEngine.AI;

public class TitanAntiGravityController : MonoBehaviour
{
    public MissileLauncher shooterWeapon;
    public NavMeshAgent navMeshAgent;
    public Transform rotationRoot;
    //public vSimpleMeleeAI_SphereSensor playerDetection;

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
    public Vector3 setOff;
    public bool isGravityActivate;

    private float timer = 0f;
    private float riseTimeElapsed = 0f;
    private float fallTimeElapsed = 0f;

    private Vector3 targetRotationAxis;
    private float axisChangeTimer = 0f;

    [Header("Rotation Axis Settings")]
    public float axisChangeInterval = 1f;
    public float axisLerpSpeed = 1f;

    public enum State { Idle, GoingUp, Floating, GoingDown }
    public State currentState = State.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void ActivateAntiGravity()
    {
        if (currentState != State.Idle || rotationRoot == null) return;
        isGravityActivate = true;
        if (animator) animator.enabled = false;

        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        //navMeshAgent.isStopped = true;
        StartCoroutine(WaitToDisableAnimator());
    }
    IEnumerator WaitToDisableAnimator()
    {
        yield return new WaitForEndOfFrame();
        rotationRoot.position = transform.position + setOff;
        originalLocalPosition = rotationRoot.position;
        originalRotation = rotationRoot.localRotation;
        targetLocalUpPosition = originalLocalPosition + new Vector3(0f, targetHeight, 0f);

        //playerDetection.enabled = false;
        transform.localPosition = -setOff;
        rotationAxis = Random.onUnitSphere.normalized;
        targetRotationAxis = Random.onUnitSphere.normalized;
        axisChangeTimer = 0f;
        riseTimeElapsed = 0f;

        currentState = State.GoingUp;
    }

    void Update()
    {
        if (isGravityActivate)
        {
            //navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }
        else
        {
            navMeshAgent.enabled = true;
        }
        switch (currentState)
        {
            case State.GoingUp:

                riseTimeElapsed += Time.deltaTime;
                float tUp = Mathf.Clamp01(riseTimeElapsed / riseDuration);
                float curvedTUp = floatCurve.Evaluate(tUp);
                rotationRoot.localPosition = Vector3.Lerp(originalLocalPosition, targetLocalUpPosition, curvedTUp);
                RotateObject();

                if (tUp >= 1f)
                {
                    timer = 0f;
                    currentState = State.Floating;
                }
                break;

            case State.Floating:
                rotationRoot.localPosition = targetLocalUpPosition;
                timer += Time.deltaTime;

                axisChangeTimer += Time.deltaTime;
                if (axisChangeTimer >= axisChangeInterval)
                {
                    axisChangeTimer = 0f;
                    targetRotationAxis = Random.onUnitSphere.normalized;
                }
                shooterWeapon.FireWeapon();
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
                rotationRoot.localPosition = Vector3.Lerp(targetLocalUpPosition, originalLocalPosition, curvedTDown);
                RotateToOriginal();

                if (tDown >= 1f)
                {
                    rotationRoot.localPosition = originalLocalPosition;
                    rotationRoot.localRotation = originalRotation;
                    ResetSystem();
                }
                break;
        }
    }

    void RotateObject()
    {

        if (rotationRoot != null)
        {
            rotationRoot.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    void RotateToOriginal()
    {

        if (rotationRoot != null)
        {
            rotationRoot.localRotation = Quaternion.Slerp(rotationRoot.localRotation, originalRotation, Time.deltaTime * 2f);
        }
    }

    void ResetSystem()
    {
        if (rb)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (animator)
        {
            animator.enabled = true;
        }
        isGravityActivate = false;
        //playerDetection.enabled = true;

        currentState = State.Idle;
    }
}
