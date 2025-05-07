using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [RequireComponent(typeof(ParticleSystem))]
    [vClassHeader("Boost OneShot Player", iconName = "misIconRed")]
    public class mvBoostOneShotPlayer : mvParticlePlayer
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        mvMotorcycleInput vcInput;
        bool trigger = false;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Start()
        {
            vcInput = GetComponentInParent<mvMotorcycleInput>();

            if (vcInput)
            {
                useAutoDestroy = false;
                onlyDeactivate = true;
                waitForAudio = true;

                vcInput.onUpdate -= onUpdate;
                vcInput.onUpdate += onUpdate;
            }
            else
            {
                this.enabled = false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void onUpdate(float deltaTime)
        {
            if (!vcInput.vc.isEngineOn)
                return;

            if (vcInput.vc.boostInput && !trigger)
            {
                trigger = true;
                Play();
            }
            else if (!vcInput.vc.boostInput)
            {
                trigger = false;
            }
        }
#endif
    }
}