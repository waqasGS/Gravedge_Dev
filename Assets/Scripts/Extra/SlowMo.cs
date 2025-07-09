using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlowMo : MonoBehaviour/*, IPointerClickHandler*/
{
    [Header("Slow‑Mo Settings")]
    public float slowMoFactor = 0.3f;
    public float depletionRate = 0.2f;
    public float rechargeRate = 0.1f;

    [Header("UI")]
    public Image slowMoSlider;

    private bool isSlowMoActive = false;
    public float slowMoEnergy = 1f;

    void Update()
    {
        if (isSlowMoActive)
        {
            Time.timeScale = slowMoFactor;
            slowMoEnergy -= depletionRate * Time.unscaledDeltaTime;
            slowMoEnergy = Mathf.Clamp(slowMoEnergy, 0f, 1f);

            if (slowMoEnergy <= 0f)          // <-- energy 0 → auto‑stop
                StopSlowMo();
        }
        else
        {
            Time.timeScale = 1f;
            slowMoEnergy += rechargeRate * Time.unscaledDeltaTime;
            slowMoEnergy = Mathf.Clamp(slowMoEnergy, 0f, 1f);
        }

        if (slowMoSlider)
            slowMoSlider.fillAmount = slowMoEnergy;
    }

    // ---------- PUBLIC CONTROLS ----------
    public void OnPointerClick()
    {
        // toggle, lekin sirf tab jab energy > 0
        if (isSlowMoActive)
            StopSlowMo();
        else if (slowMoEnergy > 0f)
            StartSlowMo();
        // agar energy 0 hai, tap ignore ho jaayega
    }

    public void StartSlowMo()
    {
        if (slowMoEnergy <= 0f || isSlowMoActive) return;

        isSlowMoActive = true;   // TimeScale set Update mein ho raha hai
    }

    public void StopSlowMo()
    {
        if (!isSlowMoActive) return;

        isSlowMoActive = false;
        Time.timeScale = 1f;     // backup—Update bhi reset karega
    }
}
