using UnityEngine;

namespace ArcadeBP_Pro
{
    public class BikeInputProvider : MonoBehaviour
    {
        public ArcadeBikeControllerPro arcadeBikeControllerPro;

        [Header("Keyboard Inputs")]

        [Tooltip("The key to accelerate the bike. Default is usually `KeyCode.W`.")]
        public KeyCode AccelerateKey;

        [Tooltip("The key to reverse the bike. Default is `KeyCode.S`.")]
        public KeyCode ReverseKey;

        [Tooltip("The key to apply the hand brake. Default is `KeyCode.Space`.")]
        public KeyCode HandBrakeKey;

        [Tooltip("The key to steer the bike left. Default is `KeyCode.A`.")]
        public KeyCode SteeringLeftKey;

        [Tooltip("The key to steer the bike right. Default is `KeyCode.D`.")]
        public KeyCode SteeringRightKey;

        [Tooltip("The key to perform a wheelie. Default is usually `KeyCode.LeftShift`.")]
        public KeyCode WheelieKey;

        [Header("Mobile Inputs")]

        [Tooltip("UI button for accelerating the bike.")]
        public UiButton_ABP_Pro AccelerateButton;

        [Tooltip("UI button for reversing the bike.")]
        public UiButton_ABP_Pro ReverseButton;

        [Tooltip("UI button for applying the handbrake.")]
        public UiButton_ABP_Pro HandBrakeButton;

        [Tooltip("UI button for steering the bike to the left.")]
        public UiButton_ABP_Pro SteeringLeftButton;

        [Tooltip("UI button for steering the bike to the right.")]
        public UiButton_ABP_Pro SteeringRightButton;

        [Tooltip("UI button for performing a wheelie.")]
        public UiButton_ABP_Pro WheelieButton;

        // inputs to provide
        private float Accelerate, Reverse, HandBrake, SteeringLeft, SteeringRight, Wheelie;

        [ContextMenu("Set Default Inputs")]
        private void setDefaultInput()
        {
            AccelerateKey = KeyCode.W;
            ReverseKey = KeyCode.S;
            HandBrakeKey = KeyCode.Space;
            SteeringLeftKey = KeyCode.A;
            SteeringRightKey = KeyCode.D;
            WheelieKey = KeyCode.LeftShift;
        }

        private void Update()
        {
            // set inputs
            SetPlayerInput();

        }

        private void SetPlayerInput()
        {
            // get inputs
            Accelerate = (Input.GetKey(AccelerateKey) || AccelerateButton?.isPressed == true) ? 1f : 0f;
            Reverse = (Input.GetKey(ReverseKey) || ReverseButton?.isPressed == true) ? 1f : 0f;
            HandBrake = (Input.GetKey(HandBrakeKey) || HandBrakeButton?.isPressed == true) ? 1f : 0f;
            SteeringLeft = (Input.GetKey(SteeringLeftKey) || SteeringLeftButton?.isPressed == true) ? 1f : 0f;
            SteeringRight = (Input.GetKey(SteeringRightKey) || SteeringRightButton?.isPressed == true) ? 1f : 0f;
            Wheelie = (Input.GetKey(WheelieKey) || WheelieButton?.isPressed == true) ? 1f : 0f;

            // Note : You can also use your custom inputs above to provide inputs to the bike controller
            // provide inputs to the bike controller
            arcadeBikeControllerPro.provideInput(Accelerate, Reverse, HandBrake, SteeringLeft, SteeringRight, Wheelie);
        }

    }
}
