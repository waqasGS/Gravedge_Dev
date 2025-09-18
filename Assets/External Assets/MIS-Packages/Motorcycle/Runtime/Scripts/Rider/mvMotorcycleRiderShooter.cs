#if INVECTOR_BASIC
using Invector.vCharacterController;
#endif
#if INVECTOR_SHOOTER
using Invector.vShooter;
#endif
using System.Collections;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Motorcycle Rider Shooter", iconName = "misIconRed")]
    public class mvMotorcycleRiderShooter : mvMotorcycleRiderMelee
    {
#if MIS && MIS_MOTORCYCLE && INVECTOR_SHOOTER
#if MIS_VEHICLEWEAPONS
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Input", order = 0)]
        public GenericInput toggleWeaponInput = new GenericInput("CapsLock", "", "");
        public GenericInput gunInput = new GenericInput("Mouse0", "", "");
        public GenericInput rocketInput = new GenericInput("Mouse1", "", "");
        bool isVehicleWeaponMode = false;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Settings", order = 1)]
        [Header("Shooter")]
        public bool useHipFireOnRiding = true;


        // ----------------------------------------------------------------------------------------------------
        // 
        mvShooterMeleeInput shooterMeleeInput;
        protected vShooterManager shooterManager;
        protected mvDrawHideShooterWeapons drawHideShooterWeapons;

        bool oldUseLeftIK, oldUseRightIK;
        bool oldHipFireShot;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override IEnumerator Start()
        {
            yield return StartCoroutine(base.Start());

            if (IsAvailable)
            {
                shooterMeleeInput = tpInput as mvShooterMeleeInput;
                shooterManager = GetComponent<vShooterManager>();
                drawHideShooterWeapons = GetComponent<mvDrawHideShooterWeapons>();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------

       private bool mount = true;
       private bool togglePressed = false; // To prevent repeated toggling while holding the key
        public override void Update()
        {
            if (!IsAvailable)
                return;


            // //Toggle bike 
            // Handle mount/dismount input with single key press detection
            if (Input.GetKeyDown(KeyCode.T) && !togglePressed)
            {
                if (mount)
                {
                    EnterInput();
                }
                else
                {
                    ExitInput();
                }
            
                mount = !mount;
                togglePressed = true;
            }
            
            // Reset togglePressed when key is released
            if (Input.GetKeyUp(KeyCode.T))
            {
                togglePressed = false;
            }






            //real code
          // EnterInput();
          // ExitInput();

            if (!IsOnAction || vcBikeInput == null)
                return;

            deltaTime = Time.deltaTime;

            SetRiderCapsuleCollider();

#if MIS_VEHICLEWEAPONS
            ToggleWeaponInput();
            GunLaunchInput();
            RocketLaunchInput();
#endif
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
                if (HasLeftHandWeapon() || shooterManager.isReloadingWeapon)
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
            }

            if (vcBikeInput.vc.ikRightHand == null)
            {
                tpInput.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
                tpInput.animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
            }
            else
            {
                //if (shooterManager.IsCurrentWeaponActive())
                if (HasRightHandWeapon())
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
            }

            if (vcBikeInput.vc.ikSpineHint == null)
            {
                tpInput.animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.identity);
            }
            else
            {
                //tpInput.animator.SetBoneLocalRotation(HumanBodyBones.Spine, vcBikeInput.vc.ikSpineHint.rotation);
            }

            if (inputSmooth.z >= 0.1f || tpInput.cc.IsAnimatorTag("Attack"))
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
        protected override bool HasWeapon()
        {
            if (HasLeftHandWeapon() || HasRightHandWeapon())
                return true;
            
            return base.HasWeapon();
        }
        protected override bool HasLeftHandWeapon()
        {
            if (shooterManager != null)
            {
                if (shooterManager.lWeapon && shooterManager.lWeapon.gameObject.activeInHierarchy)
                    return true;
            }

            return base.HasLeftHandWeapon();
        }
        protected override bool HasRightHandWeapon()
        {
            if (shooterManager != null)
            {
                if (shooterManager.rWeapon && shooterManager.rWeapon.gameObject.activeInHierarchy)
                    return true;
            }

            return base.HasRightHandWeapon();
        }

#if MIS_VEHICLEWEAPONS
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ToggleWeaponInput()
        {
            if (toggleWeaponInput.useInput && toggleWeaponInput.GetButtonDown())
            {
                if (vcBikeInput == null || RiderState != (int)MotorcycleRidingState.Riding)
                    return;

                if (!vcBikeInput.vc.HasVehicleWeapon)
                    return;

                isVehicleWeaponMode = !isVehicleWeaponMode;

                shooterMeleeInput.SetLockShooterInput(isVehicleWeaponMode);
                shooterMeleeInput.SetLockMeleeInput(isVehicleWeaponMode);

                if (isVehicleWeaponMode)
                    drawHideShooterWeapons.ForceHideWeapons(false);

                if (useHipFireOnRiding)
                    shooterManager.hipfireShot = !isVehicleWeaponMode;

                vcBikeInput.vc.EnableLaunchers(transform, isVehicleWeaponMode);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void GunLaunchInput()
        {
            if (gunInput.useInput && gunInput.GetButton() && isVehicleWeaponMode)
            {
                if (vcBikeInput == null || RiderState != (int)MotorcycleRidingState.Riding)
                    return;

                if (vcBikeInput.vc.gunLauncher != null)
                    vcBikeInput.vc.LaunchGun();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void RocketLaunchInput()
        {
            if (rocketInput.useInput && rocketInput.GetButton() && isVehicleWeaponMode)
            {
                if (vcBikeInput == null || RiderState != (int)MotorcycleRidingState.Riding)
                    return;

                if (vcBikeInput.vc.rocketLauncher != null)
                    vcBikeInput.vc.LaunchRocket();
            }
        }
#endif

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void EnterActionState()
        {
            base.EnterActionState();

            oldUseLeftIK = shooterManager.useLeftIK;
            oldUseRightIK = shooterManager.useRightIK;
            shooterManager.useLeftIK = false;
            shooterManager.useRightIK = false;

            if (useHipFireOnRiding)
            {
                oldHipFireShot = shooterManager.hipfireShot;
                shooterManager.hipfireShot = true;
            }

            vcBikeInput.vc.EnterActionState(this);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void ExitActionState(bool ragdolled = false)
        {
            base.ExitActionState(ragdolled);

            Physics.IgnoreCollision(tpInput.cc._capsuleCollider, vcBikeInput.vc.GetComponent<Collider>(), false);

            shooterManager.useLeftIK = oldUseLeftIK;
            shooterManager.useRightIK = oldUseRightIK;

            if (useHipFireOnRiding)
                shooterManager.hipfireShot = oldHipFireShot;
        }
#endif
    }
}