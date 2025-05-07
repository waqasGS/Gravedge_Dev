using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [RequireComponent(typeof(ParticleSystem))]
    [vClassHeader("Ground Smoke Emitter", iconName = "misIconRed")]
    public class mvGroundSmokeEmitter : mvParticleEmitter
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        public float speedThreshold = 5f;

        // ----------------------------------------------------------------------------------------------------
        // 
        mvMotorcycleInput vcInput;
        bool isOnAction;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Start()
        {
            vcInput = GetComponentInParent<mvMotorcycleInput>();

            if (vcInput)
            {
                vcInput.onUpdate -= OnUpdate;
                vcInput.onUpdate += OnUpdate;

                vcInput.vc.onDead.RemoveListener(OnDead);
                vcInput.vc.onDead.AddListener(OnDead);
            }
            else
            {
                this.enabled = false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnUpdate(float deltaTime)
        {
            if (vcInput.vc.isEngineOn && !isOnAction)
            {
                isOnAction = true;
            }
            else if (isOnAction && !vcInput.vc.isEngineOn)
            {
                EmitMultiply(0);
                isOnAction = false;
            }

            if (!isOnAction)
                return;

            if (vcInput.vc.maxSpeed.now >= speedThreshold)
                EmitMultiply(1);
            else
                EmitMultiply(0);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnDead(GameObject player)
        {
            vcInput.onUpdate -= OnUpdate;

            Destroy(this.gameObject);
        }
#endif
    }
}