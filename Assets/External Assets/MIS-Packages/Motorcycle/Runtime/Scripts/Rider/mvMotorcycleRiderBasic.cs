using System.Collections;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("MotorcycleRider Basic", iconName = "misIconRed")]
    public class mvMotorcycleRiderBasic : mvMotorcycleRider
    {
#if MIS_MOTORCYCLE && INVECTOR_BASIC
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Settings", order = 1)]
        [Tooltip("Y offset curve while Get-In from far distance. Turn Gizmos on to adjust the movement path")]
        public AnimationCurve farGetInYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 0.01f, 0.25f, 0.25f), new Keyframe(0.4f, 0.2f), new Keyframe(0.8f, 0f), new Keyframe(1f, 0f));

        [Header("UI")]
        public bool useTachometer = true;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvMotorcycleInput vcBikeInput;


        // ----------------------------------------------------------------------------------------------------
        // Animation
        const string GETON_L_NEAR = "GetOnMotorcycle_L_Near";
        const string GETON_R_NEAR = "GetOnMotorcycle_R_Near";
        const string GETON_L_FAR = "GetOnMotorcycle_L_Far";
        const string GETON_R_FAR = "GetOnMotorcycle_R_Far";

        const string GETOFF_L = "GetOffMotorcycle_L";
        const string GETOFF_R = "GetOffMotorcycle_R";

        const string GETON_DUMMY = "GetOnMotorcycle_Dummy";
        const string GETOFF_DUMMY = "GetOffMotorcycle_Dummy";


        // ----------------------------------------------------------------------------------------------------
        // 
        protected override bool InActionAnimation
        {
            get
            {
                if (RiderState == (int)MotorcycleRidingState.Enter)
                {
                    if (entryPoint.useAnimation)
                    {
                        if (entryPoint.isNearGetOn)
                        {
                            if (entryPoint.side == EntrySide.Left)
                                return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOnNearLeftAnimation.animationHash;
                            else
                                return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOnNearRightAnimation.animationHash;
                        }
                        else
                        {
                            if (entryPoint.side == EntrySide.Left)
                                return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOnFarLeftAnimation.animationHash;
                            else
                                return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOnFarRightAnimation.animationHash;
                        }
                    }
                    else
                    {
                        return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOnDummyAnimation.animationHash;
                    }
                }
                else if (RiderState == (int)MotorcycleRidingState.Exit)
                {
                    if (entryPoint.useAnimation)
                    {
                        if (entryPoint.side == EntrySide.Left)
                            return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOffLeftAnimation.animationHash;
                        else
                            return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOffRightAnimation.animationHash;
                    }
                    else
                    {
                        return tpInput.cc.animatorStateInfos.stateInfos[0].shortPathHash == getOffDummyAnimation.animationHash;
                    }
                }

                return false;
            }
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override IEnumerator Start()
        {
            yield return StartCoroutine(base.Start());

            if (IsAvailable)
            {
                getOnNearLeftAnimation = new AnimationHash(GETON_L_NEAR);
                getOnNearRightAnimation = new AnimationHash(GETON_R_NEAR);
                getOnFarLeftAnimation = new AnimationHash(GETON_L_FAR);
                getOnFarRightAnimation = new AnimationHash(GETON_R_FAR);

                getOffLeftAnimation = new AnimationHash(GETOFF_L);
                getOffRightAnimation = new AnimationHash(GETOFF_R);

                getOnDummyAnimation = new AnimationHash(GETON_DUMMY);
                getOffDummyAnimation = new AnimationHash(GETOFF_DUMMY);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void FixedUpdate()
        {
            if (!IsAvailable || !IsOnAction || vcBikeInput == null)
                return;

            deltaTime = Time.fixedDeltaTime;

            InputHandle();
            UpdateAnimator(deltaTime);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void Update()
        {
            if (!IsAvailable)
                return;

            EnterInput();
            ExitInput();

            if (!IsOnAction || vcBikeInput == null)
                return;

            deltaTime = Time.deltaTime;

            SetRiderCapsuleCollider();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void OnAnimatorIK()
        {
            if (!IsAvailable || !IsOnAction || RiderState != (int)MotorcycleRidingState.Riding || vcBikeInput == null)
                return;

            if (vcBikeInput.vc.ikLeftHand == null)
            {
                tpInput.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                tpInput.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            }
            else
            {
                tpInput.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                tpInput.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                tpInput.animator.SetIKPosition(AvatarIKGoal.LeftHand, vcBikeInput.vc.ikLeftHand.position);
                tpInput.animator.SetIKRotation(AvatarIKGoal.LeftHand, vcBikeInput.vc.ikLeftHand.rotation);
            }

            if (vcBikeInput.vc.ikRightHand == null)
            {
                tpInput.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
                tpInput.animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
            }
            else
            {
                tpInput.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                tpInput.animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
                tpInput.animator.SetIKPosition(AvatarIKGoal.RightHand, vcBikeInput.vc.ikRightHand.position);
                tpInput.animator.SetIKRotation(AvatarIKGoal.RightHand, vcBikeInput.vc.ikRightHand.rotation);
            }

            if (inputSmooth.z >= 0.1f)
            {
                if (vcBikeInput.vc.ikLeftFoot != null)
                {
                    tpInput.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                    tpInput.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
                    tpInput.animator.SetIKPosition(AvatarIKGoal.LeftFoot, vcBikeInput.vc.ikLeftFoot.position);
                    tpInput.animator.SetIKRotation(AvatarIKGoal.LeftFoot, vcBikeInput.vc.ikLeftFoot.rotation);
                }

                if (vcBikeInput.vc.ikRightFoot != null)
                {
                    tpInput.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
                    tpInput.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
                    tpInput.animator.SetIKPosition(AvatarIKGoal.RightFoot, vcBikeInput.vc.ikRightFoot.position);
                    tpInput.animator.SetIKRotation(AvatarIKGoal.RightFoot, vcBikeInput.vc.ikRightFoot.rotation);
                }
            }
            else
            {
                if (vcBikeInput.vc.ikLeftFoot != null)
                {
                    tpInput.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                    tpInput.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
                }

                if (vcBikeInput.vc.ikRightFoot != null)
                {
                    tpInput.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
                    tpInput.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void MoveInput()
        {
            if (!lockMoveInput)
            {
                input.x = horizontalInput.GetAxisRaw();
                input.z = verticallInput.GetAxisRaw();

                vcBikeInput.MoveInput(input);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override bool HasWeapon()
        {
            return false;
        }
        protected override bool HasLeftHandWeapon()
        {
            return false;
        }
        protected override bool HasRightHandWeapon()
        {
            return false;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void EvaluateToTargetPosition(float animationNormalizedTime)
        {
            if (entryPoint.seat == null || (RiderState != (int)MotorcycleRidingState.Enter && RiderState != (int)MotorcycleRidingState.Exit))
                return;

            float evaluatedXZ = 0f;
            float evaluatedY = 0f;

            Vector3 matchTargetPosition = Vector3.zero;
            Vector3 rootPosition = Vector3.zero;

            if (RiderState == (int)MotorcycleRidingState.Enter)
            {
                evaluatedXZ = entryPoint.matchGetInXZCurve.Evaluate(animationNormalizedTime);
                evaluatedY = entryPoint.matchGetInYCurve.Evaluate(animationNormalizedTime);

                matchTargetPosition = vcBikeInput.vc.transform.TransformPoint(entryPoint.seat.localPosition);
            }
            else if (RiderState == (int)MotorcycleRidingState.Exit)
            {
                evaluatedXZ = entryPoint.matchGetOutXZCurve.Evaluate(animationNormalizedTime);
                evaluatedY = entryPoint.matchGetOutYCurve.Evaluate(animationNormalizedTime);

                //matchTargetPosition = vcBikeInput.vc.transform.TransformPoint(entryPoint.point.localPosition);
                matchTargetPosition = exitWorldPoint;
            }

            rootPosition = tpInput.cc.animator.rootPosition;

            if (evaluatedXZ < 1f)
            {
                rootPosition.x = Mathf.Lerp(rootPosition.x, matchTargetPosition.x, evaluatedXZ);
                rootPosition.z = Mathf.Lerp(rootPosition.z, matchTargetPosition.z, evaluatedXZ);
                finishMatchXZPosition = true;
            }
            else if (finishMatchXZPosition)
            {
                finishMatchXZPosition = false;
                rootPosition.x = matchTargetPosition.x;
                rootPosition.z = matchTargetPosition.z;
            }

            if (evaluatedY < 1f)
            {
                rootPosition.y = Mathf.Lerp(rootPosition.y, matchTargetPosition.y, evaluatedY);
                if (RiderState == (int)MotorcycleRidingState.Enter)
                    rootPosition.y += farGetInYCurve.Evaluate(animationNormalizedTime) * enterDistanceProportion;
                finishMatchYPosition = true;
            }
            else if (finishMatchYPosition)
            {
                finishMatchYPosition = false;
                rootPosition.y = matchTargetPosition.y;
            }

            transform.position = rootPosition;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void EvaluateToTargetRotation(float animationNormalizedTime)
        {
            Vector3 targetEuler;
            Quaternion targetRotation;
            Quaternion rootRotation = tpInput.cc.animator.rootRotation;

            if (animationNormalizedTime < towardVehicleRotationThreshold)
            {
                if (RiderState == (int)MotorcycleRidingState.Enter)
                {
                    Vector3 targetDirection = entryPoint.seat.position - transform.position;
                    targetDirection.y = 0f;
                    targetRotation = Quaternion.LookRotation(targetDirection);
                }
                else
                {
                    targetEuler = new Vector3(transform.eulerAngles.x, entryPoint.point.eulerAngles.y, transform.eulerAngles.z);
                    targetRotation = Quaternion.Euler(targetEuler);
                }
                rootRotation = Quaternion.Slerp(rootRotation, targetRotation, animationNormalizedTime);
            }
            else if (animationNormalizedTime < 1f)
            {
                if (RiderState == (int)MotorcycleRidingState.Enter)
                    targetEuler = new Vector3(transform.eulerAngles.x, entryPoint.seat.eulerAngles.y, transform.eulerAngles.z);
                else
                    targetEuler = new Vector3(transform.eulerAngles.x, entryPoint.point.eulerAngles.y, transform.eulerAngles.z);
                targetRotation = Quaternion.Euler(targetEuler);

                rootRotation = Quaternion.Slerp(rootRotation, targetRotation, animationNormalizedTime);

                finishMatchRotation = true;
            }
            else if (finishMatchRotation)
            {
                finishMatchRotation = false;

                if (RiderState == (int)MotorcycleRidingState.Enter)
                    targetEuler = new Vector3(transform.eulerAngles.x, entryPoint.seat.eulerAngles.y, transform.eulerAngles.z);
                else
                    targetEuler = new Vector3(transform.eulerAngles.x, entryPoint.point.eulerAngles.y, transform.eulerAngles.z);

                rootRotation = Quaternion.Euler(targetEuler);
            }

            transform.rotation = rootRotation;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void EnterEntry(EntryPoint entryPoint)
        {
            base.EnterEntry(entryPoint);

            vcBikeInput = vcInput as mvMotorcycleInput;
            Physics.IgnoreCollision(tpInput.cc._capsuleCollider, vcBikeInput.vc.GetComponent<Collider>(), true);

            if (entryPoint.useAnimation)
            {
                IsPlayingAnimation = true;

                if (entryPoint.isNearGetOn)
                    tpInput.cc.animator.CrossFadeInFixedTime(entryPoint.side == EntrySide.Left ? getOnNearLeftAnimation.animationHash : getOnNearRightAnimation.animationHash, 0.25f);
                else
                    tpInput.cc.animator.CrossFadeInFixedTime(entryPoint.side == EntrySide.Left ? getOnFarLeftAnimation.animationHash : getOnFarRightAnimation.animationHash, 0.25f);

                OnStartGetOn.Invoke();
            }
            else
            {
                tpInput.cc.animator.rootPosition = entryPoint.seat.position;
                transform.rotation = entryPoint.seat.rotation;

                //IsPlayingAnimation = true;
                tpInput.cc.animator.CrossFadeInFixedTime(getOnDummyAnimation.animationHash, 0f);

                FinishEnterExitAction();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void ExitEntry(bool ragdolled = false)
        {
            base.ExitEntry(ragdolled);

            if (ragdolled)
                return;

            if (entryPoint.useAnimation)
            {
                IsPlayingAnimation = true;
                tpInput.cc.animator.CrossFadeInFixedTime(entryPoint.side == EntrySide.Left ? getOffLeftAnimation.animationHash : getOffRightAnimation.animationHash, 0.25f);
                
                OnStartGetOff.Invoke();
            }
            else
            {
                //IsPlayingAnimation = true;
                //tpInput.cc.animator.CrossFadeInFixedTime(getOffDummyAnimation.animationHash, 0f);

                transform.position = entryPoint.point.position;
                transform.rotation = entryPoint.point.rotation;

                FinishEnterExitAction();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void EnterActionState()
        {
            base.EnterActionState();

            vcBikeInput.vc.EnterActionState(this);

            // Produces the effect of the ShockAbsorber swaying when getting on
            vcBikeInput.vc.rb.AddForce(-vcBikeInput.vc.transform.up * vcBikeInput.vc.rb.mass, ForceMode.Impulse);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void ExitActionState(bool byRagdoll)
        {
            base.ExitActionState(byRagdoll);

            vcBikeInput.vc.ExitActionState(byRagdoll);

            Physics.IgnoreCollision(tpInput.cc._capsuleCollider, vcBikeInput.vc.GetComponent<Collider>(), false);
        }
#endif
    }
}