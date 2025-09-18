#pragma warning disable 0414

using Invector;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public class mvMotorcycleBase : mvHealthController
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = 1)]
        [Header("Stamina")]
        public mvFloatOrigin maxStamina = new mvFloatOrigin(500f, 500f);
        public mvFloatOrigin staminaRecovery = new mvFloatOrigin(1.2f, 1.2f);
        internal float staminaRecoveryDelay = 0f;

        [Header("Inventory Camera Position")]
        public float inventoryCameraPosition = -0.15f;

        [Header("Jump")]
        [Tooltip("How many times the vehicle is able to jump.")]
        public mvIntOrigin maxJumpCount = new mvIntOrigin(3, 3);
        public float jumpStamina = 20f;
        public float jumpStaminaRecoveryDelay = 2f;
        [Tooltip("How much time the vehicle will jump")]
        public mvFloatOrigin jumpDuration = new mvFloatOrigin(0.4f, 0f);
        public float jumpHeight = 5f;
        public float jumpForce = 8f;

        [Header("Boost")]
        [Tooltip("This value is added to Motor Torue when you press hold down the Boost key")]
        public float boostMultiplier = 2f;
        public float boostStamina = 15f;
        public float bostStaminaRecoveryDelay = 2f;
        [Tooltip("The extra force while Jump using Booost")]
        public float jumpBoostForceMultiplier = 5f;

        [Header("Overturned")]
        [Range(0f, 90f)] public float overturnedAngle = 55f;
        //[Min(0f)] public float recoverOverturnedDelay = 1f;   // WIP


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Vehicle", order = 2)]
        [Header("Body")]
        public Transform bodyTransform;
        public Transform centerOfMass;

        [Header("Driver Steering Wheel")]
        public Transform steeringWheel;
        public Axis steeringWheelAxis = Axis.Y;
        public mvFloatOrigin steeringAngle = new mvFloatOrigin(30f, 0f);
        protected Quaternion orginSteeringWheelRotation;

        [Header("Speed")]
        [Tooltip("Min: Max Backward Speed, Max: Max Forward Speed")]
        public mvFloatMinMax maxSpeed = new mvFloatMinMax(5f, 30f);
        public mvFloatMinMax boostSpeed = new mvFloatMinMax(40f, 50f);
        protected const float MIN_SPEED_THRESHOLD = 1f;

        [Header("Gear")]
        [Tooltip("0: Neutral, 1: Forward, -1: Backward")]
        public int gear = 0;

        [Header("Engine")]
        public mvFloatMinMax engineRPM = new mvFloatMinMax(800f, 8000f);
        [Tooltip("Shifting occurs when the engine RPM is greater than this value.")]
        public float gearShiftUpRPM = 6000f;
        [Tooltip("Shifting occurs when the engine RPM is less than this value.")]
        public float gearShiftDownRPM = 2500f;
        public float engineRPMInertia = 0.1f;

        [Header("Wheel")]
        public float maxMotorTorque = 2500f;
        public float maxBrakeTorque = 5000f;

        [Header("IK Targets")]
        public Transform ikLeftHand;
        public Transform ikRightHand;
        public Transform ikLeftFoot;
        public Transform ikRightFoot;
        public Transform ikSpineHint;

#if MIS_INVECTOR_SWIMMING
        [Header("Water")]
        public float inWaterSpeedMultiplier = 0.5f;
        public float vehicleHeightOffset = 0.6f;
        [vReadOnly(false)]
        public GameObject water;
        [vReadOnly(false)]
        public bool inTheWater;
        [vReadOnly(false), SerializeField]
        public bool isUnderWater;
        protected float waterHeightLevel;
        [vReadOnly(false)]
        public float curretVehicleDepth;
        [Tooltip("Check the Rigibody.Y of the character to trigger the ImpactEffect Particle")]
        public float velocityToImpact = -4f;
#endif
#if MIS_SWIMMING
        [Header("Water")]
        [vHelpBox("Water splash effect when entering water")]
        public GameObject waterSplashFXPrefab;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Core", order = 3)]
        [Header("Input")]
        public float throttleInputSmooth;
        public float throttleSmoothDamp = 2f;
        [HideInInspector] public float throttleInputAbs;

        public float steerInputAbs;
        public mvFloatMinMax steeringSmoothDamp = new mvFloatMinMax(50f, 80f, 10f);   // min: normal, max: handbrake, current; neutral

        public float brakeInput;
        public bool handBrakeInput;
        public bool boostInput;
        public float gearShiftingInput;

        protected Vector3 moveDirection;

        [Header("Wheel")]
        [Tooltip("The WheelCollider mass is automatically calculated relative to the main rigidbody mass.")]
        [Range(0.05f, 0.2f)] public float wheelColliderMassRatio = 0.12f;
        public float maxAngularVelocity = 8f;

        [Header("Down Force")]
        public float downForce = 15f;

        [Header("Friction Data")]
        public mvWheelFrictionData wheelFrictionData;

        [Header("Ground Data")]
        public mvGroundData groundData;
        // To-Do
        //[HideInInspector] public TerrainData terrainData;
        //[HideInInspector] public int alphamapWidth;
        //[HideInInspector] public int alphamapHeight;
        //[HideInInspector] public float textureCount;
        //[HideInInspector] public float[,,] splatmapData;

        [Header("ESP")]
        [Range(0.05f, 0.5f)] public float ESPThreshold = 0.5f;
        [Range(0.05f, 1f)] public float ESPStrength = 0.25f;
        [HideInInspector] public float frontSlip, rearSlip;

        [Header("Stabilizer")]
        public float stabilierCheckDelay = 1f;
        protected Coroutine stabilizerCoroutine = null;


#if MIS_VEHICLEWEAPONS
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Operation", order = 4)]
        public mvVWLauncher gunLauncher;
        public mvVWRocketLauncher rocketLauncher;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Events", order = 99)]
        public UnityEvent<float> onChangeStamina;
        public UnityEvent<float> onChangeMaxStamina;
        public UnityEvent onStaminaEnd;
        public UnityEvent<GameObject> OnStartAction = new UnityEvent<GameObject>();     // Character object
        public UnityEvent<GameObject> OnFinishAction = new UnityEvent<GameObject>();    // Character object
        public UnityEvent OnJumpOnGround = new UnityEvent();
        public UnityEvent OnJumpOnAir = new UnityEvent();
        public UnityEvent OnLackWorkingTime = new UnityEvent();
        public UnityEvent OnCrashed = new UnityEvent();


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Debug", order = 100)]
        public bool debugMode = false;
        [mvReadOnly] public bool isAvailable;
        [mvReadOnly] public bool isOnAction;
        [mvReadOnly] public bool isEngineOn;
        [mvReadOnly] public Vector3 input;
        [mvReadOnly] public bool lockMovement;
        [mvReadOnly] public bool isGrounded;
        [mvReadOnly] public bool isDisabledCheckGround = false;
        [mvReadOnly] public float rpmProportion;
        [mvReadOnly] public float speedProportion;
        [mvReadOnly] public int allWheelCount, powerWheelCount;
        [mvReadOnly] public float wheelRPM;
        [mvReadOnly] public bool changingGear;
        [mvReadOnly] public bool isJumping;
        [mvReadOnly] public bool isOverturned;
        [mvReadOnly] public float heightReached;
        [mvReadOnly] public float localVelocityZ;
        [mvReadOnly] public float velocityMagnitude;


        // ----------------------------------------------------------------------------------------------------
        // 
        //[vEditorToolbar("ChainedAction", order = 98)]
        //[mvReadOnly] [SerializeField] string chainedAction = "Chained-Actions are provided as an option";


        // ----------------------------------------------------------------------------------------------------
        // 
        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public CapsuleCollider capsuleCollider;
        protected mvEngineSound engineSound;

        [HideInInspector] public mvMotorcycleRider rider;

        protected bool triggerActionState;


        // ----------------------------------------------------------------------------------------------------
        // if true, it means this action is not blocked and can be used
        public virtual bool IsAvailable
        {
            get => isAvailable;
            set => isAvailable = value;
        }

        // ----------------------------------------------------------------------------------------------------
        // if true, it means this action is currently being used
        public bool IsOnAction
        {
            get => isOnAction;
            set => isOnAction = value;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        public float CurrentStamina
        {
            get => maxStamina.now;
        }

        // ----------------------------------------------------------------------------------------------------
        // Animator
        public Animator animator
        {
            get; private set;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        public bool IsUnderSteering
        {
            get; set;
        }

        public bool IsOverSteering
        {
            get; set;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        public bool isESPWorking
        {
            get; set;
        }

#if MIS_INVECTOR_SWIMMING
        // ----------------------------------------------------------------------------------------------------
        // 
        public virtual Vector3 VehicleCenter
        {
            get => transform.position + (vehicleHeightOffset * Vector3.up);
        }
#endif

#if MIS_VEHICLEWEAPONS
        // ----------------------------------------------------------------------------------------------------
        // 
        public virtual bool HasVehicleWeapon
        {
            get => gunLauncher != null || rocketLauncher != null;
        }
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();

            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
            rb.isKinematic = true;

            engineSound = GetComponent<mvEngineSound>();

            steeringAngle.now = 0f;
            engineRPM.now = 0f;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void Start()
        {
            base.Start();

            heightReached = transform.position.y;

            if (steeringWheel)
                orginSteeringWheelRotation = steeringWheel.localRotation;

            // To-Do
            //Terrain.activeTerrain.GetTerrainData(out terrainData, out alphamapWidth, out alphamapHeight, out splatmapData, out textureCount);

            IsAvailable = true;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Init()
        {
            animator = GetComponent<Animator>();

            // avoid collision detection with inside colliders 
            Collider[] allColliders = this.GetComponentsInChildren<Collider>();
            for (int i = 0; i < allColliders.Length; i++)
                Physics.IgnoreCollision(capsuleCollider, allColliders[i]);

            // Health
            currentHealth = maxHealth;
            currentHealthRecoveryDelay = healthRecoveryDelay;

            // Stamina
            maxStamina.now = maxStamina.origin;
            staminaRecovery.now = staminaRecovery.origin;
        }

        // ----------------------------------------------------------------------------------------------------
        // FixedUpdate
        // ----------------------------------------------------------------------------------------------------

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateAnimator(float deltaTime)
        {
            if (animator == null || !animator.enabled)
                return;
        }
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ReduceStamina(float value, bool accumulative)
        {
            if (accumulative)
                maxStamina.now -= value * Time.fixedDeltaTime;
            else
                maxStamina.now -= value;

            onChangeStamina.Invoke(maxStamina.now);

            if (maxStamina.now < 0)
            {
                maxStamina.now = 0;
                onStaminaEnd.Invoke();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ChangeStamina(int value)
        {
            maxStamina.now += value;
            maxStamina.now = Mathf.Clamp(maxStamina.now, 0, maxStamina.origin);

            onChangeStamina.Invoke(maxStamina.now);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ChangeMaxStamina(int value)
        {
            maxStamina.origin += value;

            if (maxStamina.origin < 0f)
                maxStamina.origin = 0f;

            onChangeMaxStamina.Invoke(maxStamina.origin);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void StaminaRecovery(float deltaTime)
        {
            if (staminaRecoveryDelay > 0)
            {
                staminaRecoveryDelay -= deltaTime;
            }
            else
            {
                if (maxStamina.now > maxStamina.origin)
                    maxStamina.now = maxStamina.origin;

                if (maxStamina.now < maxStamina.origin)
                    maxStamina.now += staminaRecovery.now;

                onChangeStamina.Invoke(maxStamina.now);
            }
        }

        /*
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void TakeDamage(vDamage damage)
        {
            base.TakeDamage(damage);

            float proportion = Mathf.InverseLerp(0, maxHealth, damage.damageValue);

            if (proportion >= explosionRatio)
            {
                ChangeHealth(0);
                Explode();
            }
        }*/

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void RemoveComponents()
        {
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();

            for (int i = 0; i < components.Length; i++)
                Destroy(components[i]);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateBoostStamina(float deltaTime)
        {
            if (isDead)
            {
                boostInput = false;
                return;
            }

            if (boostInput && maxSpeed.now > MIN_SPEED_THRESHOLD)
            {
                if (boostStamina > 0f && CurrentStamina >= boostStamina)
                {
                    ReduceStamina(boostStamina, true);
                    staminaRecoveryDelay = bostStaminaRecoveryDelay;
                }
                else
                {
                    boostInput = false;
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        //
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateAirControl(float deltaTime)
        {
            if (transform.position.y > heightReached)
                heightReached = transform.position.y;

            UpdateJump(deltaTime);
        }

        // ----------------------------------------------------------------------------------------------------
        //
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateJump(float deltaTime)
        {
            if (!isJumping)
                return;

            jumpDuration.now -= deltaTime;

            if (jumpDuration.now <= 0)
            {
                jumpDuration.now = 0;
                isJumping = false;
            }

            // apply extra force to the jump height   
            Vector3 vel = rb.velocity;
            vel.y = jumpHeight;
            rb.velocity = vel;

            if (boostInput)
                rb.AddRelativeForce(Vector3.forward * jumpForce * jumpBoostForceMultiplier, ForceMode.Acceleration);
        }

        // ----------------------------------------------------------------------------------------------------
        //
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateDownForce()
        {
            // It's better to continue to apply Down Force under External Force
            if (!isGrounded /*|| IsUnderExternalForce*/)
                return;

            rb.AddRelativeForce(Vector3.down * maxSpeed.now * downForce, ForceMode.Force);
        }

        // ----------------------------------------------------------------------------------------------------
        //
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateFlip()
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, 180f);
            transform.rotation = transform.rotation * rotation;
        }

        // ----------------------------------------------------------------------------------------------------
        // Update
        // ----------------------------------------------------------------------------------------------------

#if MIS_VEHICLEWEAPONS
        public virtual void EnableLaunchers(Transform sender, bool enable)
        {
            if (gunLauncher != null)
                gunLauncher.SetEnable(sender, rb, enable);

            if (rocketLauncher != null)
                rocketLauncher.SetEnable(sender, rb, enable);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void LaunchGun()
        {
            if (gunLauncher != null)
                gunLauncher.Shoot();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void LaunchRocket()
        {
            if (rocketLauncher != null)
                rocketLauncher.Shoot();
        }
#endif

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateGear(float deltaTime)
        {
            if (!isEngineOn || handBrakeInput)
            {
                SetGear(0);
                return;
            }

            if (input.z > 0.2f)
                SetGear(1);
            else if (input.z < -0.2f)
                SetGear(gear - 1);
            else
                SetGear(0);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void SetGear(int targetGear)
        {
            if (gear == targetGear || targetGear < -1 || targetGear > 1)
                return;

            engineSound.PlayOneShot(AudioSourceType.GearShifting);
            gear = targetGear;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Jump()
        {
            if (!isEngineOn)
                return;

            if (isGrounded)
                maxJumpCount.now = 0;

            if (maxJumpCount.now >= maxJumpCount.origin || CurrentStamina < jumpStamina)
                return;
            maxJumpCount.now++;

            if (jumpStamina > 0f && CurrentStamina >= jumpStamina)
            {
                ReduceStamina(jumpStamina, false);
                staminaRecoveryDelay = jumpStaminaRecoveryDelay;
            }

            rb.AddForce(Vector3.up * Mathf.Sign(15f) * jumpForce + Vector3.right * Mathf.Cos(15f), ForceMode.Impulse);

            jumpDuration.now = jumpDuration.origin;
            isJumping = true;

            if (isGrounded)
                OnJumpOnGround.Invoke();
            else
                OnJumpOnAir.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual Vector3 ClampMaxVelocityBySpeed(Vector3 sourceVelocity, float maxSpeed)
        {
            Vector3 velocity = sourceVelocity;
            velocity.y = sourceVelocity.y;
            return Vector3.ClampMagnitude(velocity, maxSpeed / 3.6f);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected IEnumerator VehicleStabilizer(UnityAction callback)
        {
            yield return new WaitForSeconds(stabilierCheckDelay);

            while (!isGrounded || rb.velocity.magnitude < 0.001f)
                yield return new WaitForEndOfFrame();

            stabilizerCoroutine = null;

            callback?.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public bool IsAvailableGetOn(mvMotorcycleRider rider)
        {
            if (this.rider != null && this.rider != rider)
                return false;

            if (!IsAvailable || isDead || stabilizerCoroutine != null)
                return false;

            return true;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void OnActiveRagdoll(vDamage damage = null)
        {
            if (rider && rider.RiderState == (int)MotorcycleRidingState.Riding)
                rider.ExitByForce(true);
        }

#if MIS_INVECTOR_SWIMMING
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnTriggerEnter(Collider other)
        {
            /*
            if (other.gameObject.CompareTag("Water"))
            {
                water = other.gameObject;
                inTheWater = true;
                isUnderWater = false;

                if (rb.velocity.y <= velocityToImpact && waterSplashFXPrefab)
                {
                    Vector3 newPos = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);
                    Instantiate(waterSplashFXPrefab, newPos, transform.rotation).transform.SetParent(vObjectContainer.root, true);
                }
            }*/
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Water"))
            {
                if (other.gameObject == water)
                {
                    water = null;
                    inTheWater = false;
                    isUnderWater = false;
                }
            }
        }
#endif

#if MIS_INVECTOR_SWIMMING
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnDrawGizmosSelected()
        {
            //if (water)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(VehicleCenter, 0.05f);
            }
        }
#endif
#endif
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    public enum Axis
    {
        X = 0,
        Y,
        Z
    }
}