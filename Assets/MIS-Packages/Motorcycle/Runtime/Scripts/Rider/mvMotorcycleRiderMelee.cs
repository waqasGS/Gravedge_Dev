#if INVECTOR_BASIC
using Invector;
#endif
#if INVECTOR_MELEE
using Invector.vMelee;
#endif
using System.Collections;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Motorcycle Rider Melee", iconName = "misIconRed")]
    public class mvMotorcycleRiderMelee : mvMotorcycleRiderBasic
    {
#if MIS && MIS_MOTORCYCLE && INVECTOR_MELEE
        mvMeleeCombatInput meleeCombatInput;
        protected vMeleeManager meleeManager;
        protected vDrawHideMeleeWeapons drawHideMeleeWeapons;

        LayerMask oldHitRecoilLayer;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override IEnumerator Start()
        {
            yield return StartCoroutine(base.Start());

            if (IsAvailable)
            {
                meleeCombatInput = tpInput as mvMeleeCombatInput;

                TryGetComponent(out meleeManager);
                TryGetComponent(out drawHideMeleeWeapons);
            }
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
                if (HasLeftHandWeapon())
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
            if (meleeManager != null)
            {
                if (meleeManager.leftWeapon && meleeManager.leftWeapon.gameObject.activeInHierarchy)
                    return true;
            }

            return base.HasLeftHandWeapon();
        }
        protected override bool HasRightHandWeapon()
        {
            if (meleeManager != null)
            {
                if (meleeManager.rightWeapon && meleeManager.rightWeapon.gameObject.activeInHierarchy)
                    return true;
            }

            return base.HasRightHandWeapon();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void EnterActionState()
        {
            base.EnterActionState();

            // Prevent Hit Recoil
            oldHitRecoilLayer = meleeManager.hitProperties.hitRecoilLayer;
            meleeManager.hitProperties.hitRecoilLayer = 0;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void ExitActionState(bool ragdolled = false)
        {
            base.ExitActionState(ragdolled);

            meleeManager.hitProperties.hitRecoilLayer = oldHitRecoilLayer;
        }
#endif
    }
}