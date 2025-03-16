#pragma warning disable 0414

#if INVECTOR_BASIC
using Invector;
using Invector.vCharacterController;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("WallRun", iconName = "misIconRed")]
    public class mvWallRun : vMonoBehaviour
    {
#if MIS_WALLRUN && INVECTOR_BASIC
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Input", order = 0)]
        public GenericInput forwardInput = new GenericInput("W", "", "");
        public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        public GenericInput wallJumpInput = new GenericInput("Space", "X", "X");


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("WallRun")]
        //[Tooltip("This mask is set automatically in Start()")]
        //public LayerMask moveStopLayerMask = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;
        //public float maxMoveStopCheckDistance = 3f;
        //float moveStopWeight;

        [Header("Settings")]
        public string wallRunTag = MISRuntimeTagLayer.TAG_WALLRUN;
        public LayerMask wallLayer = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;
        [Min(0.2f)] public float minGroundDistance = 0.3f;
        [Min(0.5f)] public float maxWallDistance = 1.2f;
        public float wallRunHeight = 1.5f;
        [Min(3f)] public float wallRunPositioningSpeed = 3f;

        [Header("WallRun")]
        public float wallRunSpeed = 5f;
        public float wallRunSprintSpeed = 8f;
        [Tooltip("How much time the character will move using WallRun. If this value is zero, there is no time limit.")]
        [Min(0f)] public float wallRunTime = 0f;
        public bool useSprintToStartWallRun = false;

        [Header("Camera")]
        public string cameraStateLSide = "WallRunL";
        public string cameraStateRSide = "WallRunR";


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("WallJump")]
        public bool useWallJump = true;
        //public bool useCameraDirectionForWallJump = false;    //? WIP
        public bool cancelWhenWallJumpFail = true;
        [Min(1f)] public float maxWallJumpDistance = 10f;
        public float wallJumpSpeed = 12f;
        [Tooltip("Length of WallJump animation clip")]
        public float wallJumpAnimationLength = 1f;
        [Tooltip("Minimum delay required to transition to WallJump state")]
        [Min(0.3f)] public float wallExitTime = 0.3f;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Stamina")]
        [Min(0f)] public float wallRunStamina = 5f;
        [Min(0f)] public float wallRunSprintStamina = 8f;
        [Min(0f)] public float wallJumpStamina = 10f;
        [Min(0f)] public float staminaRecoveryDelay = 2.5f;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("ChainedAction", order = 98)]
        [SerializeField] string chainedAction = "Chained-Actions are provided as an option";
#if MIS_AIRDASH
        public bool allowFromAirDash = true;
#endif
#if MIS_GRAPPLINGHOOK
        public bool allowFromGrapplingHook = true;
#endif
#if MIS_GRAPPLINGROPE
        public bool allowFromGrapplingRope = true;
#endif
#if MIS_LEDGECLIMB1 || MIS_LEDGECLIMB2
        public bool allowFromLedgeClimb = true;
#endif
#if MIS_WATERDASH
        public bool allowFromWaterDash = true;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Events", order = 99)]
        public UnityEvent OnStartAction = new UnityEvent();
        public UnityEvent OnFinishAction = new UnityEvent();


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Debug", order = 100)]
        public bool debugMode = false;
        [mvReadOnly] public bool isAvailable;
        [mvReadOnly] public bool isOnAction;
        [mvReadOnly] public bool canStartAction;
        [mvReadOnly] public bool hasLeftWall, hasRightWall;
        [mvReadOnly] public float wallRunTimer = 0f;
        [mvReadOnly] public float wallExitTimer = 0f;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvThirdPersonInput tpInput;
        protected Transform tr;
        RaycastHit wallHit;
        Vector3 wallRunDirection;
        Vector3 wallJumpDirection;
        Vector3 wallJumpStartPoint;
        float wallRunTargetHeight;
        float wallDistance;


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
        protected int animationSpeedHash = Animator.StringToHash("AnimationSpeed");
        protected int wallRunStateHash = Animator.StringToHash("WallRunState");

        public virtual float AnimationSpeed
        {
            get => tpInput.animator.GetFloat(animationSpeedHash);
            set => tpInput.animator.SetFloat(animationSpeedHash, value);
        }

        public enum WallRunStateType
        {
            None = 0,
            RunL,
            RunR,
            Jump
        };
        protected WallRunStateType WallRunState
        {
            get => (WallRunStateType)tpInput.animator.GetInteger(wallRunStateHash);
            set => tpInput.animator.SetInteger(wallRunStateHash, (int)value);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        Vector3 WallCheckPosition
        {
            get => tr.position + (Vector3.up * 0.2f);
        }
        Vector3 LeftWallCheckDirection
        {
            get => (-tr.right + tr.forward).normalized;
        }
        Vector3 RightWallCheckDirection
        {
            get => (tr.right + tr.forward).normalized;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        float WallJumpRemainDistance
        {
            get => maxWallJumpDistance - Vector3.Distance(wallJumpStartPoint, tr.position);
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            tr = transform;

            if (TryGetComponent(out tpInput))
            {
                wallDistance = tpInput.cc._capsuleCollider.radius * 2f;

                IsAvailable = true;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void FixedUpdate()
        {
            if (!IsAvailable || !tpInput.enabled)
                return;

            CheckForWall();

            if (IsOnAction)
            {
                if (tpInput.cc.customAction)
                {
                    ExitActionState();
                    return;
                }

                if (tpInput.cc.groundDistance < minGroundDistance)
                {
                    if (wallRunTime > 0f)
                        wallRunTimer -= Time.deltaTime;

                    if (wallExitTimer <= 0f)
                    {
                        if (debugMode)
                            Debug.LogWarning("[MIS-WallRun] WallRun has been canceled due to the ground constraint.");

                        ExitActionState();
                        return;
                    }
                }

                WallJumpInput();
                WallRunControl();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual bool JumpInput()
        {
            if (IsOnAction)
                return true;

            if (WallRunStartCondition())
            {
                EnterActionState();
                return true;
            }

            return false;
        }
        //[ContextMenu("Wall Input")]
        //public void OnwallEnable()
        //{
        //    wallJumpInput.useInput = true;
        //}
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void WallJumpInput()
        {
            if (wallJumpInput.useInput && wallJumpInput.GetButtonDown())
            {
                if (WallRunState == WallRunStateType.RunL || WallRunState == WallRunStateType.RunR)
                {
                    if (useWallJump)
                    {
                        if (tpInput.cc.currentStamina < wallJumpStamina)
                        {
                            if (debugMode)
                                Debug.LogWarning("[MIS-WallRun] Lack of WallJump stamina");

                            return;
                        }

                        //wallJumpDirection = /*useCameraDirectionForWallJump ? tpInput.cameraMain.transform.forward : */wallHit.normal;
                        wallJumpDirection = WallRunState == WallRunStateType.RunL ? tr.right : -tr.right;
                        //wallJumpDirection.Normalize();

                        if (Physics.Raycast(tr.position, wallJumpDirection, out RaycastHit hit, maxWallJumpDistance, wallLayer))
                        {
                            if (hit.collider.gameObject.CompareTag(wallRunTag))
                            {
                                wallJumpStartPoint = tr.position;
                                wallRunTargetHeight = hit.point.y;

                                tpInput.cc.ReduceStamina(wallJumpStamina, true);

                                AnimationSpeed = wallJumpAnimationLength / (hit.distance / wallJumpSpeed);

                                wallExitTimer = wallExitTime;
                                WallRunState = WallRunStateType.Jump;
                                return;
                            }
                        }

                        if (cancelWhenWallJumpFail)
                            ExitActionState();
                    }
                    else
                    {
                        ExitActionState();
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckForWall()
        {
            RaycastHit leftWallHit = new RaycastHit();
            RaycastHit rightWallHit = new RaycastHit();

            hasLeftWall = hasRightWall = false;

            if (WallRunState == WallRunStateType.RunL)
            {
                if (Physics.Raycast(WallCheckPosition, LeftWallCheckDirection, out leftWallHit, maxWallDistance, wallLayer))
                    hasLeftWall = leftWallHit.collider.gameObject.CompareTag(wallRunTag);

                if (debugMode && hasLeftWall)
                    Debug.DrawRay(WallCheckPosition, LeftWallCheckDirection * wallHit.distance, Color.red);

                hasRightWall = false;
            }
            else if (WallRunState == WallRunStateType.RunR)
            {
                if (Physics.Raycast(WallCheckPosition, RightWallCheckDirection, out rightWallHit, maxWallDistance, wallLayer))
                    hasRightWall = rightWallHit.collider.gameObject.CompareTag(wallRunTag);

                if (debugMode && hasRightWall)
                    Debug.DrawRay(WallCheckPosition, RightWallCheckDirection * wallHit.distance, Color.red);

                hasLeftWall = false;
            }
            else if (wallExitTimer <= 0f)
            {
                if (Physics.Raycast(WallCheckPosition, LeftWallCheckDirection, out leftWallHit, maxWallDistance, wallLayer))
                    hasLeftWall = leftWallHit.collider.gameObject.CompareTag(wallRunTag);
                if (Physics.Raycast(WallCheckPosition, RightWallCheckDirection, out rightWallHit, maxWallDistance, wallLayer))
                    hasRightWall = rightWallHit.collider.gameObject.CompareTag(wallRunTag);

                if (hasLeftWall && hasRightWall)
                {
                    hasLeftWall = hasRightWall = false;
                                   
                    if (WallRunState == WallRunStateType.Jump)
                        ExitActionState();

                    return;
                }

                if (debugMode)
                {
                    if (hasLeftWall)
                        Debug.DrawRay(WallCheckPosition, LeftWallCheckDirection * wallHit.distance, Color.red);
                    else
                        Debug.DrawRay(WallCheckPosition, LeftWallCheckDirection * maxWallDistance, Color.green);

                    if (hasRightWall)
                        Debug.DrawRay(WallCheckPosition, RightWallCheckDirection * wallHit.distance, Color.red);
                    else
                        Debug.DrawRay(WallCheckPosition, RightWallCheckDirection * maxWallDistance, Color.green);
                }
            }


            wallHit = hasLeftWall ? leftWallHit : rightWallHit;

            wallRunDirection = Vector3.Cross(wallHit.normal, tr.up);

            if ((tr.forward - wallRunDirection).sqrMagnitude > (tr.forward - -wallRunDirection).sqrMagnitude)
                wallRunDirection *= -1f;

            if (debugMode)
            {
                Debug.DrawRay(WallCheckPosition, wallRunDirection.normalized * 5f, Color.blue);

                if (hasLeftWall || hasRightWall)
                    Debug.DrawRay(wallHit.point, wallHit.normal * 3f, Color.cyan);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual bool WallRunStartCondition()
        {
            canStartAction =
                IsAvailable
                && !IsOnAction
                && WallRunState == WallRunStateType.None
                && tpInput.cc.input.z > 0.25f
                && (hasLeftWall || hasRightWall)
                && tpInput.cc.currentStamina >= wallRunStamina
                && (useSprintToStartWallRun ? (tpInput.cc.isSprinting ? true : false) : true)
                && !tpInput.cc.IsAnimatorTag("Attack")
                && !tpInput.cc.isCrouching
#if MIS_AIRDASH
                && (tpInput.cc.IsAirDashOnAction ? (allowFromAirDash ? true : false) : true)
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
                && !tpInput.cc.IsVehicleRiderOnAction
#endif
#if MIS_CRAWLING
                && !tpInput.cc.IsCrawlingOnAction
#endif
#if MIS_FREEFLYING
                && !tpInput.cc.IsFreeFlyingOnAction
#endif
#if MIS_GRAPPLINGHOOK
                && (tpInput.cc.IsGrapplingHookOnAction ? (allowFromGrapplingHook ? true : false) : true)
#endif
#if MIS_GRAPPLINGROPE
                && ((tpInput.cc.IsGrapplingRopeOnAction || tpInput.cc.IsGrapplingRopeOnMoveAction) ? (allowFromGrapplingRope ? true : false) : true)
#endif
#if MIS_GROUNDDASH
                && !tpInput.cc.IsGroundDashOnAction
#endif
#if MIS_LEDGECLIMB1 || MIS_LEDGECLIMB2
                && (tpInput.cc.IsLedgeClimbOnAction ? (allowFromLedgeClimb ? true : false) : true)
#endif
#if MIS_MOTORCYCLE
                && !tpInput.cc.IsRiderOnAction
#endif
#if MIS_SOFTFLYING
                && !tpInput.cc.IsSoftFlyingOnAction
#endif
#if MIS_SWIMMING
                && !tpInput.cc.IsSwimOnAction
#endif
#if MIS_WATERDASH
                && (tpInput.cc.IsWaterDashOnAction ? (allowFromWaterDash ? true : false) : true)
#endif

#if MIS_INVECTOR_BUILDER
                && !tpInput.cc.IsVBuildManagerOnAction
#endif
#if MIS_INVECTOR_FREECLIMB
                && !tpInput.cc.IsVFreeClimbOnAction
#endif
#if MIS_INVECTOR_PARACHUTE
                && !tpInput.cc.IsVParachuteOnAction
#endif
#if MIS_INVECTOR_PUSH
                && !tpInput.cc.IsVPushOnAction
#endif
#if MIS_INVECTOR_SHOOTERCOVER
                && !tpInput.cc.IsVShooterCoverOnAction
#endif
#if MIS_INVECTOR_ZIPLINE
                && !tpInput.cc.IsVZiplineOnAction
#endif
                ;

            return canStartAction;
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
#if MIS_GRAPPLINGHOOK
            if (tpInput.cc.IsGrapplingHookOnAction && allowFromGrapplingHook)
                tpInput.cc.misGrapplingHook.Interrupt();
#endif
#if MIS_GRAPPLINGROPE
            if (tpInput.cc.IsGrapplingRopeOnAction && allowFromGrapplingRope)
                tpInput.cc.misGrapplingRope.Interrupt();
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
#if MIS_WATERDASH
            if (tpInput.cc.IsWaterDashOnAction && allowFromWaterDash)
                tpInput.cc.misWaterDash.ExitActionState();
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void SetCameraState()
        {
            if (hasLeftWall)
                tpInput.ChangeCameraState(cameraStateLSide, true);
            else if (hasRightWall)
                tpInput.ChangeCameraState(cameraStateRSide, true);
            else
                tpInput.ResetCameraState();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool StaminaConsumption()
        {
            tpInput.cc.ReduceStamina((sprintInput.useInput && sprintInput.GetButton()) ? wallRunSprintStamina : wallRunStamina, true);
            tpInput.cc.currentStaminaRecoveryDelay = staminaRecoveryDelay;

            return tpInput.cc.currentStamina > 0;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void WallRunControl()
        {
            if (WallRunState == WallRunStateType.Jump)
            {
                tpInput.cc._rigidbody.velocity = wallJumpDirection * wallJumpSpeed;

                if (!forwardInput.GetButton() || WallJumpRemainDistance <= 0f)
                {
                    ExitActionState();
                    return;
                }

                if (wallExitTimer > 0f)
                {
                    wallExitTimer -= Time.fixedDeltaTime;
                    return;
                }

                SetCameraState();

                if (hasLeftWall || hasRightWall)
                {
                    AnimationSpeed = 1f;
                    WallRunState = hasLeftWall ? WallRunStateType.RunL : WallRunStateType.RunR;
                }
                else
                {
                    return;
                }
            }

            if (wallRunTime > 0f)
                wallRunTimer -= Time.deltaTime;

            if ((wallRunTime > 0f && wallRunTimer < 0f) 
                || !forwardInput.GetButton()
                || !(hasLeftWall || hasRightWall) 
                || !StaminaConsumption())
            {
                ExitActionState();
                return;
            }

            tpInput.cc.RotateToDirection(wallRunDirection, false);

            // Keep a certain distance from the wall
            if (wallHit.distance > wallDistance)
                wallRunDirection += -wallHit.normal * (wallRunPositioningSpeed * Time.fixedDeltaTime);
            else if (wallHit.distance < wallDistance)
                wallRunDirection += wallHit.normal * (wallRunPositioningSpeed * Time.fixedDeltaTime);

            Vector3 velocity = wallRunDirection.normalized * ((sprintInput.useInput && sprintInput.GetButton()) ? wallRunSprintSpeed : wallRunSpeed);

            // Keep the position in WallRun Height
            if (tr.position.y < wallRunTargetHeight)
                velocity.y += wallRunPositioningSpeed;
            if (tr.position.y > wallRunTargetHeight)
                velocity.y = 0f;

            tpInput.cc._rigidbody.velocity = velocity;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnterActionState()
        {


            print("Enter Action State");
            CheckChainedAction();

            IsOnAction = true;

            AnimationSpeed = 1f;
            
            wallRunTimer = wallRunTime;
            WallRunState = hasLeftWall ? WallRunStateType.RunL : WallRunStateType.RunR;

            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.isCrouching = false;

            tpInput.SetLockBasicInput(true);
            tpInput.cc.lockMovement = true;
            tpInput.cc.lockRotation = true;
            tpInput.cc.lockSetMoveSpeed = true;
            tpInput.cc.verticalVelocity = 0f;

            // Don't let this character rotate towards to the camera forward
            tpInput.SetLockUpdateMoveDirection(true);
            tpInput.cc.RotateToDirection(wallRunDirection, true);

            tpInput.cc._rigidbody.useGravity = false;
            wallRunTargetHeight = tr.position.y + wallRunHeight;
            wallExitTimer = wallExitTime;

            SetCameraState();

            OnStartAction.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitActionState()
        {
            print("Exit Action State");

            if (!IsOnAction)
                return;
            IsOnAction = false;

            AnimationSpeed = 1f;

            wallRunTimer = 0f;
            wallExitTimer = 0f;
            WallRunState = WallRunStateType.None;

            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.isCrouching = false;

            tpInput.SetLockBasicInput(false);
            tpInput.cc.lockMovement = false;
            tpInput.cc.lockRotation = false;
            tpInput.cc.lockSetMoveSpeed = false;

            tpInput.SetLockUpdateMoveDirection(false);

            tpInput.cc._rigidbody.useGravity = true;

            tpInput.ResetCameraState();

            OnFinishAction.Invoke();
        }
#endif

        //public void StartWallRun()
        //{
        //    if (WallRunStartCondition())
        //    {
        //        Debug.Log("StartWallRun");
        //        EnterActionState();
        //    }
        //}

        //public void StopWallRun()
        //{
        //    ExitActionState();
        //}

        //public void PerformWallJump()
        //{
        //    Debug.Log("PerformWallJump");
        //    WallJumpInput();
        //}

    }
}