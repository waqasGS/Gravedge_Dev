using Invector;
using Invector.vCharacterController;
using System;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // Instead of vThirdPersonController class, mvThirdPersonController class will be mainly used for MIS and MIS Packages.
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("THIRD PERSON CONTROLLER", iconName = "misIconRed")]
    public partial class mvThirdPersonController : vThirdPersonController
    {

        public static Action OnBikeMountZoneEnter;
        public static Action OnBikeMountZoneExit;
#if MIS
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Debug", order = 9)]
        [Header("MIS")]
        [mvReadOnly] [SerializeField] protected bool ignoreFalling;
        [mvReadOnly] [SerializeField] protected bool lockGravity;


        // ----------------------------------------------------------------------------------------------------
        // 
        public bool IgnoreFalling
        {
            get => ignoreFalling;
            set
            {
                ignoreFalling = value;
                animator.SetBool(mvAnimatorParameters.IgnoreFalling, value);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        public bool LockGravity
        {
            get => lockGravity;
            set
            {
                lockGravity = value;

                _rigidbody.useGravity = !value;

                if (!_rigidbody.isKinematic)
                    _rigidbody.velocity = Vector3.zero;

                IgnoreFalling = value;
            }
        }

#if MIS_WATERDASH
        public override void Jump(bool consumeStamina = false)
        {
            if (!IsWaterDashOnAction)
                base.Jump(consumeStamina);
        }
#endif

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnableGravity()
        {
            LockGravity = false;
        }
        public virtual void DisableGravity()
        {
            LockGravity = true;
        }


       
        private new void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            if (other.CompareTag("BikeSettingTarget"))
            {
                OnBikeMountZoneEnter?.Invoke();
            }
        }

        private new void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("BikeSettingTarget"))
            {
                OnBikeMountZoneExit?.Invoke();
            }
        }
#endif
    }
}
