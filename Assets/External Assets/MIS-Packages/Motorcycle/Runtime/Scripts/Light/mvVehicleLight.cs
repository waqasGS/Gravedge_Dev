using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [RequireComponent(typeof(Light))]
    public abstract class mvVehicleLight : vMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = 0)]
        public bool useImmediateOnOff = false;
        public float fadeSpeed = 10f;

        [Header("Light")]
        public float lightIntensity = 5f;

        [Header("Lense Flare")]
        public float flareBrightness = 1.5f;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvMotorcycleInput vcInput;
        protected Light vcLight;
        protected LensFlare vcLensFlare;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected bool isActionEnabled;
        public virtual bool IsActionEnabled
        {
            get
            {
                return isActionEnabled;
            }
            set
            {
                if (isActionEnabled != value)
                {
                    isActionEnabled = value;

                    if (isActionEnabled)
                    {
                        vcLight.intensity = 0f;
                        vcLensFlare.brightness = 0f;
                    }
                    else
                    {
                        vcLight.intensity = lightIntensity;
                        vcLensFlare.brightness = flareBrightness;
                    }
                }
            }
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            vcInput = GetComponentInParent<mvMotorcycleInput>();

            if (vcInput)
            {
                vcLight = GetComponent<Light>();
                vcLight.intensity = 0f;
                vcLight.renderMode = LightRenderMode.ForceVertex;

                if (TryGetComponent(out vcLensFlare))
                {
                    if (vcLight.flare != null)
                        vcLight.flare = null;

                    vcLensFlare.color = vcLight.color;
                    vcLensFlare.brightness = 0f;
                    vcLensFlare.fadeSpeed = fadeSpeed;
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Start()
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateLensFlare(float deltaTime)
        {
            if (IsActionEnabled)
            {
                if (useImmediateOnOff)
                    vcLensFlare.brightness = flareBrightness;
                else
                    vcLensFlare.brightness = Mathf.Lerp(vcLensFlare.brightness, flareBrightness, fadeSpeed * deltaTime);
            }
            else
            {
                if (useImmediateOnOff)
                    vcLensFlare.brightness = 0f;
                else
                    vcLensFlare.brightness = Mathf.Lerp(vcLensFlare.brightness, 0f, fadeSpeed * deltaTime);
            }
        }
    }
}