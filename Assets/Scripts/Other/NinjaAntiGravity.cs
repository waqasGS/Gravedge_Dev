using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Invector.vCharacterController.AI;
using Invector.vMelee;
using UnityEngine.SocialPlatforms.Impl;

public class NinjaAntiGravity : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform rotationRoot;
    public vSimpleMeleeAI_SphereSensor playerDetection;
    public vMeleeWeapon meleeWeapon;

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

    public float dropDelay = 1.5f;

    public enum State { Idle, GoingUp, Floating, GoingDown }
    public State currentState = State.Idle;

    public bool toStealthKill;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void ActivateAntiGravity()
    {
        if (currentState != State.Idle || rotationRoot == null) return;
        isGravityActivate = true;
        rotationRoot.localPosition = transform.position + setOff;

        originalLocalPosition = rotationRoot.localPosition;
        originalRotation = rotationRoot.localRotation;
        targetLocalUpPosition = originalLocalPosition + new Vector3(0f, targetHeight, 0f);

        if (animator) animator.enabled = false;

        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        //navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        playerDetection.enabled = false;
        transform.localPosition = -setOff;
        rotationAxis = Random.onUnitSphere.normalized;
        targetRotationAxis = Random.onUnitSphere.normalized;
        axisChangeTimer = 0f;
        riseTimeElapsed = 0f;

        currentState = State.GoingUp;
    }

    void Update()
    {
        if (!isGravityActivate)
        {
            //navMeshAgent.isStopped = true;
            navMeshAgent.enabled = true;
        }
        else
        {
            navMeshAgent.enabled = false;
        }
        if (toStealthKill)
        {
            navMeshAgent.enabled = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
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
                //shooterWeapon.FireWeapon();
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
    public void StealthKill()
    {
        toStealthKill = true;

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
        playerDetection.enabled = true;

        currentState = State.Idle;
    }


    public void DropSword()
    {
        StartCoroutine(DropSwordWithDelay());
    }
    private IEnumerator DropSwordWithDelay()
    {
        GameObject sword = meleeWeapon.gameObject;
        yield return new WaitForSeconds(dropDelay);

        sword.transform.parent = null;

        Rigidbody rb = sword.GetComponent<Rigidbody>();
        if (rb == null) rb = sword.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;

        Collider col = sword.GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

}