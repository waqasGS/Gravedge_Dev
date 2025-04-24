using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [RequireComponent(typeof(Light))]
    [vClassHeader("Head Light", iconName = "misIconRed")]
    public class mvHeadLight : mvVehicleLight
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        public bool isManual = false;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void Start()
        {
            base.Start();

            if (vcInput)
            {
                vcInput.vc.OnStartAction.RemoveListener(OnStartAction);
                vcInput.vc.OnStartAction.AddListener(OnStartAction);

                vcInput.vc.OnFinishAction.RemoveListener(OnFinishAction);
                vcInput.vc.OnFinishAction.AddListener(OnFinishAction);

                vcInput.vc.onDead.RemoveListener(OnDead);
                vcInput.vc.onDead.AddListener(OnDead);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void SetLight(bool activate)
        {
            if (activate && !vcInput.vc.isDead)
            {
                vcLight.intensity = lightIntensity;
                vcLensFlare.brightness = flareBrightness;
            }
            else
            {
                vcLight.intensity = 0f;
                vcLensFlare.brightness = 0f;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnStartAction(GameObject player)
        {
            if (!isManual)
                SetLight(true);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnFinishAction(GameObject player)
        {
            if (!isManual)
                SetLight(false);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnDead(GameObject player)
        {
            vcInput.vc.OnStartAction.RemoveListener(OnStartAction);
            vcInput.vc.OnFinishAction.RemoveListener(OnFinishAction);
            vcInput.vc.onDead.RemoveListener(OnDead);

            Destroy(this.gameObject);
        }
#endif
    }
}