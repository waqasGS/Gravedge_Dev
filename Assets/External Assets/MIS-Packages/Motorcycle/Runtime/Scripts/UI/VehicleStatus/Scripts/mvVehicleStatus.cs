using UnityEngine;
using UnityEngine.UI;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Vehicle Status", iconName = "misIconRed")]
    public class mvVehicleStatus : mvMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("UI")]
        [Header("Speedometer")]
        public GameObject vehicleIconObj;
        public float disabledDuration = 2f;
        Image vehicleIcon;

        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Status Color")]
        public Color positiveColor = Color.green;
        public Color negativeColor = Color.red;

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Awake()
        {
            vehicleIcon = vehicleIconObj.GetComponent<Image>();
            SetStatus(0);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void SetStatus(int status)
        {
            switch (status)
            {
            case (int)VehicleStatus.None:
                vehicleIconObj.SetActive(false);
                break;

            case (int)VehicleStatus.On:
                vehicleIconObj.SetActive(true);
                vehicleIcon.color = positiveColor;
                break;

            case (int)VehicleStatus.Disabled:
                vehicleIcon.color = negativeColor;
                Invoke("DelayedDeactivate", disabledDuration);
                break;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void DelayedDeactivate()
        {
            SetStatus(0);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    public enum VehicleStatus
    {
        None = 0,
        On,
        Disabled
    }
}