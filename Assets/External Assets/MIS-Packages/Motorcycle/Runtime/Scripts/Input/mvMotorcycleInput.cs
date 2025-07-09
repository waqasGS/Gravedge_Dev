using Invector.vCharacterController;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Motorcycle Input", iconName = "misIconRed")]
    public class mvMotorcycleInput : mvMonoBehaviour
    {
#if MIS_MOTORCYCLE
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Input", order = 0)]
        [Header("Motorcycle")]
        public GenericInput boostInput = new GenericInput("LeftShift", "", "");
        public GenericInput handBrakeInput = new GenericInput("Space", "", "");
#if UNITY_EDITOR
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
        public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
#endif
        public GenericInput jumpInput = new GenericInput("LeftControl", "", "");


        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Debug", order = 100)]
        public bool lockInput;
        public bool lockMoveInput;
#if UNITY_EDITOR
        public bool useSelfRiding = true;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        public delegate void OnUpdateEvent(float deltaTime);
        public event OnUpdateEvent onUpdate;

        [HideInInspector] public mvMotorcycle vc;

        protected float deltaTime;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected void Awake()
        {
            vc = GetComponent<mvMotorcycle>();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Start()
        {
            if (vc != null)
                vc.Init();
        }
        void LateUpdate()                     // **ADD transform lock here (optional)**
        {
            if (vc == null) return;
            Vector3 r = vc.transform.eulerAngles;
            r.x = 0f;
            vc.transform.eulerAngles = r;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FixedUpdate()
        {
            if (vc == null || Time.timeScale == 0)
                return;

            deltaTime = Time.fixedDeltaTime;

            vc.UpdateMotor(deltaTime);
            vc.UpdateAnimator(deltaTime);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Update()
        {
            if (vc == null || Time.timeScale == 0)
                return;

            deltaTime = Time.deltaTime;

            onUpdate?.Invoke(deltaTime);

#if UNITY_EDITOR
            if (!useSelfRiding)
#endif
                if (vc.rider == null)
                    return;

            InputHandle();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SetLockAllInput(bool value)
        {
            lockInput = value;
            lockMoveInput = value;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void InputHandle()
        {
            if (lockInput)
                return;

            //MoveInput();  // Called from mvRider or mvVehicleAutoDrive
            BoostInput();
            HandbrakeInput();
            JumpInput();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------

        public virtual void MoveInput(Vector3 input)
        {
            if (!lockMoveInput)
            {
                vc.input = input;
                vc.UpdateInput(input, deltaTime);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void JumpInput()
        {
            if (jumpInput.GetButtonDown())
                vc.Jump();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void BoostInput(bool isManual = false)
        {
            vc.boostInput = isManual ? true : (boostInput.useInput && boostInput.GetButton());
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void HandbrakeInput()
        {
            vc.handBrakeInput = handBrakeInput.useInput && handBrakeInput.GetButton();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExternalBrakeInput(float brakeInput)
        {
            vc.brakeInput = brakeInput;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExternalHandBrakeInput(bool handBrakeInput)
        {
            vc.handBrakeInput = handBrakeInput;
        }
#endif
    }
}