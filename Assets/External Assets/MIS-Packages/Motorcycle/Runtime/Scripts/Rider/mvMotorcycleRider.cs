#pragma warning disable 0414

using Invector;
using Invector.vCharacterController;
#if INVECTOR_MELEE || INVECTOR_SHOOTER
using Invector.vItemManager;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public abstract class mvMotorcycleRider : mvMonoBehaviour
    {
#if MIS_MOTORCYCLE && INVECTOR_BASIC
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Input", order = 0)]
        [Header("Control")]
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
        public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
        public float moveInputSmoothDamp = 3f;

        public GenericInput LeftControlInput = new GenericInput("LeftControl", "", "");

        [Header("Enter/Exit")]
        public GenericInput enterInput = new GenericInput("E", "", "");
        public GenericInput exitInput = new GenericInput("Q", "", "");
        protected Vector3 exitWorldPoint;  // The Exit position should be the position at the time the player started


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Settings", order = 1)]
        public bool applyVehicleSpeedOnExit = true;
        [Tooltip("The min/max distance to get on the vehicle. The min value should be greater than 3.5f in order to prevent overlaping with the vehicle while Auto Drive")]
        public mvFloatMinMax maxEnterDistance = new mvFloatMinMax(4f, 10f);
        protected float enterDistanceProportion;
        protected float towardVehicleRotationThreshold = 0.4f;

        protected AnimationHash getOnNearLeftAnimation, getOnNearRightAnimation;
        protected AnimationHash getOnFarLeftAnimation, getOnFarRightAnimation;
        protected AnimationHash getOffLeftAnimation, getOffRightAnimation;
        protected AnimationHash getOnDummyAnimation, getOffDummyAnimation;

        [Tooltip("Obstacle LayerMask to prevent getting off.")]
        public LayerMask exitObstacleLayerMask = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;

        [Header("Rider Capsule Collider")]
        public Vector3 riderCenter = Vector3.zero;
        public float riderHeight = 1.2f;

        [Header("Audio")]
        public AudioSource riderAudioSource;
        [Tooltip("Set audio clips when calling a vehicle.")]
        public List<AudioClip> callingVehicleClipList = new List<AudioClip>();


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("ChainedAction", order = 98)]
        [mvReadOnly] [SerializeField] string chainedAction = "Chained-Actions are provided as an option";
#if MIS_AIRDASH
        public bool allowFromAirDash = true;
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
        public bool allowFromVehicleRider = true;
#endif
#if MIS_CRAWLING
        public bool allowFromCrawling = true;
#endif
#if MIS_FREEFLYING
        public bool allowFromFreeFlying = true;
#endif
#if MIS_GRAPPLINGROPE
        public bool allowFromGrapplingRope = true;
#endif
#if MIS_GRAPPLINGHOOK
        public bool allowFromGrapplingHook = true;
#endif
#if MIS_GROUNDDASH
        public bool allowFromGroundDash = true;
#endif
#if MIS_LEDGECLIMB1 || MIS_LEDGECLIMB2
        public bool allowFromLedgeClimb = true;
#endif
#if MIS_SOFTFLYING
        public bool allowFromSoftFlying = true;
#endif
#if MIS_WALLRUN
        public bool allowFromWallRun = true;
#endif
#if MIS_WATERDASH
        //public bool allowFromWaterDash = true;
#endif

#if MIS_INVECTOR_BUILDER
        public bool allowFromBuilder = true;
#endif
#if MIS_INVECTOR_FREECLIMB
        public bool allowFromFreeClimb = true;
#endif
#if MIS_INVECTOR_PARACHUTE
        public bool allowFromParachute = true;
#endif
#if MIS_INVECTOR_SHOOTERCOVER
        public bool allowFromShooterCover = true;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Events", order = 99)]
        public UnityEvent OnStartGetOn;
        public UnityEvent OnFinishGetOn;
        public UnityEvent OnFailGetOn;
        public UnityEvent OnStartGetOff;
        public UnityEvent OnFinishGetOff;
        public UnityEvent OnStartAnimation;
        public UnityEvent OnFinishAnimation;


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Debug", order = 100)]
        public bool debugMode = false;
        [mvReadOnly] public bool isAvailable;
        [mvReadOnly] public bool isOnAction;
        [mvReadOnly] public Vector3 input;
        [mvReadOnly] public bool lockInput;
        [mvReadOnly] public bool lockMoveInput;
        protected Vector3 inputSmooth;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvThirdPersonInput tpInput;
        protected vHeadTrack headTrack;
        protected vRagdoll ragdoll;
        protected mvMotorcycleInput vcInput;

#if INVECTOR_MELEE || INVECTOR_SHOOTER
        protected Camera inventoryCamera;
        protected float oldInventoryCameraY;
#endif

        // ----------------------------------------------------------------------------------------------------
        // 
        protected float deltaTime;

        protected mvVehicleGetOnOff vehicleGetOnOff;
        protected EntryPoint entryPoint;
        protected int targetLayerMask;
        protected Collider[] vehicleColliders;

        protected bool finishMatchRotation;
        protected bool finishMatchXZPosition;
        protected bool finishMatchYPosition;

        float oldHeadTrackStrafeBodyWeight;
        float oldHeadTrackAimingBodyWeight;
        float oldHeadTrackFreeBodyWeight;


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
        // Animator
        protected int riderStateHash = Animator.StringToHash("RiderState");

        int riderState;
        public int RiderState
        {
            get => riderState;
            set
            {
                if (riderState != value)
                {
                    riderState = value;
                    tpInput.cc.animator.SetInteger(riderStateHash, value);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        protected abstract bool InActionAnimation
        {
            get;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool _isPlayingAnimation;
        public virtual bool IsPlayingAnimation
        {
            get
            {
                if (RiderState != (int)MotorcycleRidingState.Enter && RiderState != (int)MotorcycleRidingState.Exit)
                    return _isPlayingAnimation = false;

                if (!_isPlayingAnimation && InActionAnimation)
                {
                    _isPlayingAnimation = true;
                    OnStartAnimation.Invoke();
                }
                else if (_isPlayingAnimation && !InActionAnimation)
                {
                    _isPlayingAnimation = false;
                }
                return _isPlayingAnimation;
            }
            protected set
            {
                _isPlayingAnimation = true;
            }
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            targetLayerMask = 1 << MISRuntimeTagLayer.LAYER_VEHICLE;
            vehicleColliders = new Collider[16];

            // If this value is less than 0, the character and the vehicle may overlap
            if (maxEnterDistance.min <= 0)
                maxEnterDistance.min = 3f;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (TryGetComponent(out tpInput))
            {
                tpInput.cc.onReceiveDamage.RemoveListener(onReceiveDamage);
                tpInput.cc.onReceiveDamage.AddListener(onReceiveDamage);

                TryGetComponent(out ragdoll);
                TryGetComponent(out headTrack);

#if INVECTOR_MELEE || INVECTOR_SHOOTER
                vInventory inventory = GetComponentInChildren<vInventory>();
                if (inventory)
                {
                    inventoryCamera = inventory.transform.parent.GetComponentInChildren<Camera>();
                    if (inventoryCamera)
                        inventoryCamera.cullingMask |= 1 << MISRuntimeTagLayer.LAYER_VEHICLE;
                }
#endif

                RiderState = (int)MotorcycleRidingState.None;

                IsAvailable = true;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public abstract void FixedUpdate();

        public abstract void Update();

        public abstract void OnAnimatorIK();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void InputHandle()
        {
            if (lockInput)
                return;

            MoveInput();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected abstract void MoveInput();


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void UpdateAnimator(float deltaTime)
        {
            if (tpInput.cc.animator == null || !tpInput.cc.animator.enabled || vcInput == null)
                return;

            inputSmooth = Vector3.Lerp(inputSmooth, input, moveInputSmoothDamp * deltaTime);
            if (inputSmooth.x > -0.01f && inputSmooth.x < 0.01f)
                inputSmooth.x = 0f;
            if (inputSmooth.z > -0.01f && inputSmooth.z < 0.01f)
                inputSmooth.z = 0f;

            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, inputSmooth.z);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputHorizontal, inputSmooth.x);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected abstract bool HasWeapon();
        protected abstract bool HasLeftHandWeapon();
        protected abstract bool HasRightHandWeapon();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void LateUpdate()
        {
            if (IsPlayingAnimation)
            {
                float currentNormalizedTime = tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(0);

                if (entryPoint != null)
                {
                    EvaluateToTargetPosition(currentNormalizedTime);
                    EvaluateToTargetRotation(currentNormalizedTime);
                }

                if (currentNormalizedTime >= entryPoint.endExitTimeAnimation)
                    FinishEnterExitAction();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool EnterEntryCondition()
        {
            return
                IsAvailable
#if MIS_AIRDASH
                && (tpInput.cc.IsAirDashOnAction ? (allowFromAirDash ? true : false) : true)
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
                && (tpInput.cc.IsVehicleRiderOnAction ? (allowFromVehicleRider ? true : false) : true)
#endif
#if MIS_CRAWLING
                && (tpInput.cc.IsCrawlingOnAction ? (allowFromCrawling ? true : false) : true)
#endif
#if MIS_FREEFLYING
                && (tpInput.cc.IsFreeFlyingOnAction ? (allowFromFreeFlying ? true : false) : true)
#endif
#if MIS_GRAPPLINGHOOK
                && (tpInput.cc.IsGrapplingHookOnAction ? (allowFromGrapplingHook ? true : false) : true)
#endif
#if MIS_GRAPPLINGROPE
                && ((tpInput.cc.IsGrapplingRopeOnAction || tpInput.cc.IsGrapplingRopeOnMoveAction) ? (allowFromGrapplingRope ? true : false) : true)
#endif
#if MIS_GROUNDDASH
                && (tpInput.cc.IsGroundDashOnAction ? (allowFromGroundDash ? true : false) : true)
#endif
#if MIS_LEDGECLIMB1 || MIS_LEDGECLIMB2
                && (tpInput.cc.IsLedgeClimbOnAction ? (allowFromLedgeClimb ? true : false) : true)
#endif
#if MIS_SOFTFLYING
                && (tpInput.cc.IsSoftFlyingOnAction ? (allowFromSoftFlying ? true : false) : true)
#endif
#if MIS_SWIMMING
                && !tpInput.cc.IsSwimOnAction
#endif
#if MIS_WALLRUN
                && (tpInput.cc.IsWallRunOnAction ? (allowFromWallRun ? true : false) : true)
#endif
#if MIS_WATERDASH
                //&& (tpInput.cc.IsWaterDashOnAction ? (allowFromWaterDash ? true : false) : true)
                && !tpInput.cc.IsWaterDashOnAction
#endif

#if MIS_INVECTOR_BUILDER
                && (tpInput.cc.IsVBuildManagerOnAction ? (allowFromBuilder ? true : false) : true)
#endif
#if MIS_INVECTOR_FREECLIMB
                && (tpInput.cc.IsVFreeClimbOnAction ? (allowFromFreeClimb ? true : false) : true)
#endif
#if MIS_INVECTOR_PARACHUTE
                && (tpInput.cc.IsVParachuteOnAction ? (allowFromParachute ? true : false) : true)
#endif
#if MIS_INVECTOR_PUSH
                && !tpInput.cc.IsVPushOnAction
#endif
#if MIS_INVECTOR_SHOOTERCOVER
                && (tpInput.cc.IsVShooterCoverOnAction ? (allowFromShooterCover ? true : false) : true)
#endif
                ;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckChainedAction()
        {
#if MIS_AIRDASH
            if (tpInput.cc.IsAirDashOnAction && allowFromAirDash)
                tpInput.cc.misAirDash.ExitActionState();
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
            if (tpInput.cc.IsVehicleRiderOnAction && allowFromVehicleRider)
                tpInput.cc.misVehicleRider.Interrupt();
#endif
#if MIS_CRAWLING
            if (tpInput.cc.IsCrawlingOnAction && allowFromCrawling)
                tpInput.cc.misCrawling.Interrupt();
#endif
#if MIS_FREEFLYING
            if (tpInput.cc.IsFreeFlyingOnAction && allowFromFreeFlying)
                tpInput.cc.misFreeFlying.ExitActionState();
#endif
#if MIS_GRAPPLINGHOOK
            if (tpInput.cc.IsGrapplingHookOnAction && allowFromGrapplingHook)
                tpInput.cc.misGrapplingHook.Interrupt();
#endif
#if MIS_GRAPPLINGROPE
            if (tpInput.cc.IsGrapplingRopeOnAction && allowFromGrapplingRope)
                tpInput.cc.misGrapplingRope.Interrupt();
#endif
#if MIS_GROUNDDASH
            if (tpInput.cc.IsGroundDashOnAction && allowFromGroundDash)
                tpInput.cc.misGroundDash.ExitActionState();
#endif
#if MIS_LEDGECLIMB1 || MIS_LEDGECLIMB2
            if (tpInput.cc.IsLedgeClimbOnAction && allowFromLedgeClimb)
            {
#if MIS_LEDGECLIMB2
                tpInput.cc.misLedgeClimb2.ExitActionState(false);
#else
                tpInput.cc.misLedgeClimb1.ExitActionState(false);
#endif
            }
#endif
#if MIS_SOFTFLYING
            if (tpInput.cc.IsSoftFlyingOnAction && allowFromSoftFlying)
                tpInput.cc.misSoftFlying.ExitActionState();
#endif
#if MIS_WALLRUN
            if (tpInput.cc.IsWallRunOnAction && allowFromWallRun)
                tpInput.cc.misWallRun.ExitActionState();
#endif
            //#if MIS_WATERDASH
            //            if (tpInput.cc.IsWaterDashOnAction && allowFromWaterDash)
            //                tpInput.cc.misWaterDash.ExitActionState();
            //#endif

#if MIS_INVECTOR_BUILDER
            if (tpInput.cc.IsVBuildManagerOnAction && allowFromBuilder)
                tpInput.cc.vmisBuildManager.ExitBuildMode();
#endif
#if MIS_INVECTOR_FREECLIMB
            if (tpInput.cc.IsVFreeClimbOnAction && allowFromFreeClimb)
                tpInput.cc.vmisFreeClimb.Interrupt();
#endif
#if MIS_INVECTOR_PARACHUTE
            if (tpInput.cc.IsVParachuteOnAction && allowFromParachute)
                tpInput.cc.vmisParachute.ExitActionState();
#endif
#if MIS_INVECTOR_SHOOTERCOVER
            if (tpInput.cc.IsVShooterCoverOnAction && allowFromShooterCover)
                tpInput.cc.vmisShooterCover.ExitActionState(false);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnterInput()
        {
            if (!(RiderState == (int)MotorcycleRidingState.None || RiderState == (int)MotorcycleRidingState.WaitingAutoEnter))
                return;

#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
            if (tpInput.cc.IsVehicleRiderAvailable &&
                (tpInput.cc.misVehicleRider.VehicleRiderState == VehicleRidingState.Enter || tpInput.cc.misVehicleRider.VehicleRiderState == VehicleRidingState.Exit))
            {
                return;
            }
#endif

          // if (enterInput.useInput && enterInput.GetButtonDown())
            {
                if (FindEntryPoint() != null && EnterEntryCondition())
                {
                    CheckChainedAction();
                    EnterEntry(entryPoint);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitInput()
        {
            if (RiderState != (int)MotorcycleRidingState.Riding)
                return;

            
           //if (exitInput.useInput && exitInput.GetButtonDown())
            {
                if (CheckExitPoint())
                {
                    entryPoint.entryState = EntryState.GetOff;

                    ExitEntry(false);
                }
#if UNITY_EDITOR
                else if (debugMode)
                    Debug.LogWarning("There is a obstacle on Get Off point");
#endif
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual EntryPoint FindEntryPoint()
        {
            if (Physics.OverlapSphereNonAlloc(transform.position, maxEnterDistance.max, vehicleColliders, targetLayerMask) > 0)
            {
                if (vehicleColliders[0].gameObject.TryGetComponent(out mvMotorcycleBase vehicle))
                {
                    if (!vehicle.IsAvailableGetOn(this))
                        return null;
                }

                if (vehicleColliders[0].gameObject.TryGetComponent(out vehicleGetOnOff))
                {
                    if (vehicleGetOnOff.entryPoints == null)
                        return null;

                    int nearestEntryPoint = -1;
                    float nearestEntryPointDistance = 999f;

                    for (int j = 0; j < vehicleGetOnOff.entryPoints.Length; j++)
                    {
                        if (vehicleGetOnOff.entryPoints[j].hasTaken)
                            continue;

                        float distance = (vehicleGetOnOff.entryPoints[j].point.position - transform.position).sqrMagnitude;
                        if (distance < nearestEntryPointDistance)
                        {
                            nearestEntryPoint = j;
                            nearestEntryPointDistance = distance;
                        }
                    }

                    if (nearestEntryPoint >= 0)
                    {
                        if (vehicleColliders[0].gameObject.TryGetComponent(out mvMotorcycleBase vc) && !vc.isOverturned && !vc.isDead)
                        {
                            entryPoint = vehicleGetOnOff.entryPoints[nearestEntryPoint];
                            entryPoint.entryState = EntryState.GetOn;
                            entryPoint.isNearGetOn = nearestEntryPointDistance <= maxEnterDistance.min;

                            enterDistanceProportion = Mathf.InverseLerp(nearestEntryPointDistance, 0f, maxEnterDistance.max);

                            return entryPoint;
                        }
                    }
                }
            }

            return null;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnterEntry(EntryPoint entryPoint)
        {
#if UNITY_EDITOR
            if (debugMode)
                Debug.Log("EnterEntry()");
#endif

            transform.SetParent(entryPoint.seat, true);     // must set worldPositionStays to true
            //transform.localPosition = Vector3.zero;
            //transform.localRotation = Quaternion.identity;

            vcInput = vehicleGetOnOff.gameObject.GetComponent<mvMotorcycleInput>();
            vcInput.vc.GetOn();

            tpInput.cc.isCrouching = false;
            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.isStrafing = false;

            tpInput.SetLockAllInput(true);
            tpInput.lockMoveInput = true;
            tpInput.cc.lockMovement = true;
            tpInput.cc.lockRotation = true;

            tpInput.cc.isGrounded = true;
            tpInput.cc.disableCheckGround = true;
            tpInput.cc.disableSnapToGround = true;
            tpInput.cc.disableAnimations = true;
            tpInput.cc.ResetInputAnimatorParameters();

            tpInput.cc.lockSetMoveSpeed = true;
            tpInput.cc.verticalVelocity = 0f;

            tpInput.cc._rigidbody.useGravity = false;
            tpInput.cc._rigidbody.isKinematic = true;

            RiderState = (int)MotorcycleRidingState.Enter;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitEntry(bool byRagdoll)
        {
#if UNITY_EDITOR
            if (debugMode)
                Debug.Log("ExitEntry()");
#endif

            if (RiderState == (int)MotorcycleRidingState.None || RiderState == (int)MotorcycleRidingState.Exit)
                return;

            vcInput.vc.GetOff();

            transform.SetParent(null, true);
            //transform.localPosition = Vector3.zero;                   // The character must get off from the current position
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            transform.rotation = Quaternion.LookRotation(direction);    // However, the rotation must be initialized perpendicular to the ground

            exitWorldPoint = entryPoint.point.position;

            tpInput.SetLockAllInput(true);
            tpInput.lockMoveInput = true;
            tpInput.cc.lockMovement = true;
            tpInput.cc.lockRotation = true;

            tpInput.cc.isGrounded = true;
            tpInput.cc.disableCheckGround = true;
            tpInput.cc.disableSnapToGround = true;
            tpInput.cc.disableAnimations = true;
            tpInput.cc.ResetInputAnimatorParameters();

            tpInput.cc.lockSetMoveSpeed = true;
            tpInput.cc.verticalVelocity = 0f;
            tpInput.cc._rigidbody.useGravity = false;
            tpInput.cc._rigidbody.isKinematic = true;

            if (byRagdoll)
                RiderState = (int)MotorcycleRidingState.None;
            else
                RiderState = (int)MotorcycleRidingState.Exit;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void EvaluateToTargetPosition(float animationNormalizedTime)
        {
        }
        protected virtual void EvaluateToTargetRotation(float animationNormalizedTime)
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void FinishEnterExitAction()
        {
            if (RiderState == (int)MotorcycleRidingState.Enter)
            {
                RiderState = (int)MotorcycleRidingState.Riding;
                entryPoint.entryState = EntryState.Occupied;

                OnFinishAnimation.Invoke();
//                OnFinishGetOn.Invoke();  //it gives null ref
                EnterActionState();
            }

            if (RiderState == (int)MotorcycleRidingState.Exit)
            {
                RiderState = (int)MotorcycleRidingState.None;
                entryPoint.entryState = EntryState.None;

                tpInput.ResetCameraState();

                OnFinishAnimation.Invoke();
                OnFinishGetOff.Invoke();
                ExitActionState(false);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual bool CheckExitPoint()
        {
            float radius = tpInput.cc._capsuleCollider.radius;
            Vector3 startPoint = entryPoint.point.position;
            float maxDistance = Vector3.Distance(startPoint, startPoint + (Vector3.up * tpInput.cc.colliderHeightDefault));

            Ray ray = new Ray(startPoint, Vector3.up);
            if (Physics.SphereCast(ray, radius, maxDistance, exitObstacleLayerMask))
                return false;
            else
                return true;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SetRiderCapsuleCollider()
        {
            if (IsOnAction)
            {
                tpInput.cc._capsuleCollider.center = riderCenter;
                tpInput.cc._capsuleCollider.height = riderHeight;

            }
            else
            {
                tpInput.cc._capsuleCollider.center = tpInput.cc.colliderCenterDefault;
                tpInput.cc._capsuleCollider.height = tpInput.cc.colliderHeightDefault;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void onReceiveDamage(vDamage damage)
        {
            if (RiderState == (int)MotorcycleRidingState.Riding && damage.activeRagdoll)
                ExitByForce(true);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitByForce(bool ragdolled = false)
        {
            ExitEntry(ragdolled);
            ExitActionState(ragdolled);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnterActionState()
        {
            // In order to prevent releasing SetLockBasicInput after using Inventory, IsOnAction should be set before
            IsOnAction = true;
            RiderState = (int)MotorcycleRidingState.Riding;
            entryPoint.hasTaken = true;

            // would better initialize the position and rotation in order to make sure
            //transform.SetParent(entryPoint.seat, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            tpInput.SetLockAllInput(false);
            tpInput.SetLockBasicInput(true);

            SetRiderCapsuleCollider();

            if (headTrack)
            {
                oldHeadTrackStrafeBodyWeight = headTrack.strafeBodyWeight;
                oldHeadTrackAimingBodyWeight = headTrack.aimingBodyWeight;
                oldHeadTrackFreeBodyWeight = headTrack.freeBodyWeight;

                headTrack.strafeBodyWeight = 0f;
                headTrack.aimingBodyWeight = 0f;
                headTrack.freeBodyWeight = 0f;
            }

#if INVECTOR_MELEE || INVECTOR_SHOOTER
            if (inventoryCamera)
            {
                oldInventoryCameraY = inventoryCamera.transform.position.y;
                inventoryCamera.transform.localPosition = new Vector3(inventoryCamera.transform.localPosition.x, vcInput.vc.inventoryCameraPosition, inventoryCamera.transform.localPosition.z);
            }
#endif

            if (!string.IsNullOrEmpty(entryPoint.cameraState))
                tpInput.ChangeCameraState(entryPoint.cameraState, true);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitActionState(bool byRagdoll)
        {
            if (!IsOnAction)
                return;

            IsOnAction = false;
            RiderState = (int)MotorcycleRidingState.None;
            entryPoint.hasTaken = false;

            tpInput.SetLockAllInput(false);
            tpInput.lockMoveInput = false;
            tpInput.cc.lockMovement = false;
            tpInput.cc.lockRotation = false;

            tpInput.cc.isGrounded = true;
            tpInput.cc.disableCheckGround = false;
            tpInput.cc.disableSnapToGround = false;
            tpInput.cc.disableAnimations = false;
            tpInput.cc.ResetInputAnimatorParameters();

            tpInput.cc.lockSetMoveSpeed = false;
            tpInput.cc.verticalVelocity = 0f;
            tpInput.cc._rigidbody.useGravity = true;
            tpInput.cc._rigidbody.isKinematic = false;

            if (applyVehicleSpeedOnExit)
                tpInput.cc._rigidbody.velocity = vcInput.vc.rb.velocity;
            else
                tpInput.cc._rigidbody.velocity = Vector3.zero;

            SetRiderCapsuleCollider();

            if (headTrack)
            {
                headTrack.strafeBodyWeight = oldHeadTrackStrafeBodyWeight;
                headTrack.aimingBodyWeight = oldHeadTrackAimingBodyWeight;
                headTrack.freeBodyWeight = oldHeadTrackFreeBodyWeight;
            }

#if INVECTOR_MELEE || INVECTOR_SHOOTER
            if (inventoryCamera)
                inventoryCamera.transform.localPosition = new Vector3(inventoryCamera.transform.localPosition.x, oldInventoryCameraY, inventoryCamera.transform.localPosition.z);
#endif

            tpInput.ResetCameraState();

            if (!byRagdoll)
            {
                vcInput = null;
                entryPoint = null;
            }

            if (ragdoll && byRagdoll)
                ragdoll.ActivateRagdoll();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [Serializable]
    public struct AnimationHash
    {
        public string animationState;
        public int animationHash;

        public AnimationHash(string stateName)
        {
            if (!string.IsNullOrEmpty(stateName))
            {
                animationState = stateName;
                animationHash = Animator.StringToHash(stateName);
            }
            else
            {
                animationState = null;
                animationHash = -1;
            }
        }
#endif
    }
}