using UnityEngine;
using UnityEngine.UI;

public class SlowMoManager : MonoBehaviour
{
    public float slowMoFactor = 0.3f; // Slow-mo intensity
    public float depletionRate = 0.2f; // Energy depletion speed
    public float rechargeRate = 0.1f; // Energy refill speed
    public Image slowMoSlider; // UI image to show energy level
    public Button slowMoButton; // UI button to activate slow-mo

    private bool isSlowMoActive = false;
    private float slowMoEnergy = 1f; // Energy level (1 = full, 0 = empty)

    void Start()
    {
        Application.targetFrameRate = 60;
        // Assign button click event
        //slowMoButton.onClick.AddListener(ToggleSlowMo);
        //Debug.Log("SlowMoManager Initialized. Energy: " + slowMoEnergy);
    }

    void Update()
    {
        if (isSlowMoActive)
        {
            Time.timeScale = slowMoFactor;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;

            // Force 60 FPS during slow-mo
            if (Application.targetFrameRate != 60)
                Application.targetFrameRate = 60;

            slowMoEnergy -= depletionRate * Time.unscaledDeltaTime;
            slowMoEnergy = Mathf.Clamp(slowMoEnergy, 0f, 1f);
            Debug.Log("Slow-Mo Active. Energy Depleting: " + slowMoEnergy);

            if (slowMoEnergy <= 0)
            {
                Debug.Log("Energy Depleted. Stopping Slow-Mo.");
                StopSlowMo();
            }
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // Reset frame rate when not in slow-mo
            if (Application.targetFrameRate != 60)
                Application.targetFrameRate = 60; // Or -1 if you want uncapped outside slow-mo

            slowMoEnergy += rechargeRate * Time.unscaledDeltaTime;
            slowMoEnergy = Mathf.Clamp(slowMoEnergy, 0f, 1f);
            //Debug.Log("Slow-Mo Inactive. Energy Recharging: " + slowMoEnergy);
        }

        // Update UI slider
        slowMoSlider.fillAmount = slowMoEnergy;
    }

    public void ToggleSlowMo()
    {
        if (!isSlowMoActive && slowMoEnergy > 0)
        {
            Debug.Log("Button Pressed. Activating Slow-Mo.");
            StartSlowMo();
        }
        else
        {
            Debug.Log("Button Released or Energy Empty. Stopping Slow-Mo.");
            StopSlowMo();
        }
    }

    public void StartSlowMo()
    {
        if (slowMoEnergy > 0)
        {
            isSlowMoActive = true;
            Application.targetFrameRate = 60; // Lock FPS during slow-mo
            Debug.Log("Slow-Mo Started. Time Scale: " + Time.timeScale);
        }
    }

    public void StopSlowMo()
    {
        isSlowMoActive = false;
        //Time.timeScale = 1f;
        //Time.fixedDeltaTime = 0.02f;
        Debug.Log("Slow-Mo Stopped. Time Scale Reset.");
    }
}
