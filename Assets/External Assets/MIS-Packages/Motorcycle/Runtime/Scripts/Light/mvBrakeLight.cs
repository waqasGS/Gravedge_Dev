using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [RequireComponent(typeof(Light))]
    [vClassHeader("Vehicle Light", iconName = "misIconRed")]
    public class mvBrakeLight : mvVehicleLight
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void Start()
        {
            base.Start();

            if (vcInput)
            {
                vcInput.onUpdate -= OnUpdate;
                vcInput.onUpdate += OnUpdate;

                vcInput.vc.onDead.RemoveListener(OnDead);
                vcInput.vc.onDead.AddListener(OnDead);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnUpdate(float deltaTime)
        {
            if (vcInput.vc.brakeInput > 0.2f || vcInput.vc.handBrakeInput)
            {
                IsActionEnabled = true;
                vcLight.intensity = lightIntensity;
            }
            else
            {
                IsActionEnabled = false;
                vcLight.intensity = 0f;
            }

            UpdateLensFlare(deltaTime);
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