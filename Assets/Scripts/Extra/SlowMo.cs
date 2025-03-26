using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SlowMo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float slowMoFactor = 0.3f; // Slow-mo intensity
    public float depletionRate = 0.2f; // Energy depletion speed
    public float rechargeRate = 0.1f; // Energy refill speed
    public Image slowMoSlider; // UI image to show energy level


    private bool isSlowMoActive = false;
    private float slowMoEnergy = 1f; // Energy level (1 = full, 0 = empty)

   

    void Update()
    {
        if (isSlowMoActive)
        {


            Debug.Log("Time Scale Active : " + Time.timeScale);
            Time.timeScale = slowMoFactor;
            //Time.fixedDeltaTime = Time.timeScale * 0.02f;
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
            Debug.Log("Time Scale Deactive : " + Time.timeScale);
            // Refill energy when not in slow-mo
            Time.timeScale = 1f;
            //Time.fixedDeltaTime = 0.02f;
            slowMoEnergy += rechargeRate * Time.unscaledDeltaTime;
            slowMoEnergy = Mathf.Clamp(slowMoEnergy, 0f, 1f);
            Debug.Log("Slow-Mo Inactive. Energy Recharging: " + slowMoEnergy);
        }

        // Update UI slider
        slowMoSlider.fillAmount = slowMoEnergy;
    }

    //public void ToggleSlowMo()
    //{
    //    if (!isSlowMoActive && slowMoEnergy > 0)
    //    {
    //        Debug.Log("Button Pressed. Activating Slow-Mo.");
    //        StartSlowMo();
    //    }
    //    else
    //    {
    //        Debug.Log("Button Released or Energy Empty. Stopping Slow-Mo.");
    //        StopSlowMo();
    //    }
    //}

    public void StartSlowMo()
    {
        if (slowMoEnergy > 0)
        {
            isSlowMoActive = true;
            //Time.timeScale = slowMoFactor;
            //Time.fixedDeltaTime = Time.timeScale * 0.02f;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        StartSlowMo();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopSlowMo();
    }
}
