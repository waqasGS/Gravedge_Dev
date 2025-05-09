using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [RequireComponent(typeof(ParticleSystem))]
    [vClassHeader("Boost Player", iconName = "misIconRed")]
    public class mvBoostPlayer : mvParticlePlayer
    {
#if MIS_MOTORCYCLE
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
                Stop();
                isOnAction = false;
            }

            if (!isOnAction)
                return;

            if (vcInput.vc.boostInput)
                Play();
            else
                Stop();
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