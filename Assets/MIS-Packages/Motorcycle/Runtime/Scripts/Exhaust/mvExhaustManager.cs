using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Exhaust Manager", iconName = "misIconRed")]
    public class mvExhaustManager : mvMonoBehaviour
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Condition")]
        [Range(0.1f, 0.5f)] public float throttleInputThreshold = 0.35f;
        public float startExhaustDuration = 0.5f;

        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Exhaust")]
        public mvFloatMinMax emissionMinMax = new mvFloatMinMax(5f, 50f);
        public mvFloatMinMax sizeMinMax = new mvFloatMinMax(2.5f, 5f);
        public mvFloatMinMax speedMinMax = new mvFloatMinMax(0.5f, 4f);
        public bool isExhaustOn = false;

        // ----------------------------------------------------------------------------------------------------
        // 
        mvParticleEmitter[] exhaustEmitters;

        // ----------------------------------------------------------------------------------------------------
        // 
        mvMotorcycleInput vcInput;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Awake()
        {
            exhaustEmitters = GetComponentsInChildren<mvParticleEmitter>();
        }

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
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnUpdate(float deltaTime)
        {
            if (vcInput == null || !vcInput.vc.isEngineOn)
            {
                for (int i = 0; i < exhaustEmitters.Length; i++)
                    exhaustEmitters[i].Emit(0f);

                return;
            }

            for (int i = 0; i < exhaustEmitters.Length; i++)
            {
                if (vcInput.vc.input.z >= throttleInputThreshold)
                {
                    exhaustEmitters[i].SetStartSpeed(Mathf.Lerp(speedMinMax.min, speedMinMax.max, vcInput.vc.speedProportion));
                    exhaustEmitters[i].SetStartSize(Mathf.Lerp(sizeMinMax.min, sizeMinMax.max, vcInput.vc.speedProportion));

                    exhaustEmitters[i].Emit(Mathf.Lerp(emissionMinMax.min, emissionMinMax.max, vcInput.vc.speedProportion));
                }
                else
                {
                    exhaustEmitters[i].SetStartSpeed(Mathf.Lerp(speedMinMax.min, speedMinMax.max, vcInput.vc.throttleInputAbs));
                    exhaustEmitters[i].SetStartSize(Mathf.Lerp(sizeMinMax.min, sizeMinMax.max, vcInput.vc.throttleInputAbs));

                    exhaustEmitters[i].Emit(Mathf.Lerp(emissionMinMax.min, emissionMinMax.max, vcInput.vc.throttleInputAbs));
                }
            }
        }
#endif
    }
}