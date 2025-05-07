using UnityEngine;
using UnityEngine.UI;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [mvClassHeader("Speedometer", iconName = "misIconRed")]
    public class mvSpeedometer : mvMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("UI")]
        [Header("Speedometer")]
        public GameObject speedometerBG;

        public RectTransform needleTransform;
        [Tooltip("Set by adjusting the rotation Z of NeedleImage. Set the starting value to min and max to max.")]
        public mvFloatMinMax needleAngle = new mvFloatMinMax(180f, -90f);
        mvFloatMinMax engineRPM;

        public Text speedText;
        public Text gearText;

        [Header("Guages")]
        public Slider healthSlider;
        public Slider staminaSlider;

        // ----------------------------------------------------------------------------------------------------
        // 
        [mvEditorToolbar("Status Color")]
        public Color positiveColor = Color.green;
        public Color neutralColor = Color.white;
        public Color negativeColor = Color.red;

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnDisable()
        {
            ResetRPMNeedle();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void InitilaizeEngineRPM(float minRPM, float maxRPM)
        {
            engineRPM.min = minRPM;
            engineRPM.max = maxRPM;

            ResetRPMNeedle();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void InitilaizeMaxHealth(float maxHealth)
        {
            if (healthSlider)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = maxHealth;
                healthSlider.value = healthSlider.maxValue;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void InitilaizeMaxStamina(float maxStamina)
        {
            if (staminaSlider)
            {
                staminaSlider.minValue = 0f;
                staminaSlider.maxValue = maxStamina;
                staminaSlider.value = staminaSlider.maxValue;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void ResetRPMNeedle()
        {
            needleTransform.rotation = Quaternion.Euler(0f, 0f, needleAngle.min);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void DisplaySpeedometer(bool enable)
        {
            speedometerBG.SetActive(enable);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void SetSpeed(int speed, float rpm, int gear)
        {
            speedText.text = speed.ToString();

            float remapped = MISMath.Remap(rpm, 0f, engineRPM.max, needleAngle.min, needleAngle.max);
            needleTransform.rotation = Quaternion.Euler(0f, 0f, remapped);

            if (gear == 0)
            {
                gearText.text = "N";
                gearText.color = neutralColor;
            }
            else if (gear < 0)
            {
                gearText.text = "R";
                gearText.color = negativeColor;
            }
            else
            {
                gearText.text = "D";
                gearText.color = positiveColor;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnChangeMaxHealth(float maxHealth)
        {
            if (healthSlider)
                healthSlider.maxValue = maxHealth;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnChangedHealth(float health)
        {
            if (healthSlider)
                healthSlider.value = health;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnChangeMaxStamina(float maxStamina)
        {
            if (staminaSlider)
                staminaSlider.maxValue = maxStamina;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void OnChangeStamina(float stamina)
        {
            if (staminaSlider)
                staminaSlider.value = stamina;
        }
    }
}