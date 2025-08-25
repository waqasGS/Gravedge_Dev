using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class OpeningSequence : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public Image glitchImage;
    public DragAndMove dragAndMove;
    public TapProgressManager tapProgressManager;

    public Light tankLight1;
    public Light tankLight2;
 
    [Header("Glitch Effect Settings")]
    public float minGlitchInterval = 2f;
    public float maxGlitchInterval = 8f;
    public float minFadeDuration = 0.1f;
    public float maxFadeDuration = 0.4f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1.0f;

    [Header("Tank Light Flicker Settings")]
    public float minTankLightInterval = 0.5f;
    public float maxTankLightInterval = 3f;
    public float minTankLightIntensity = 0.1f;
    public float maxTankLightIntensity = 2f;
    public float minTankLightDuration = 0.05f;
    public float maxTankLightDuration = 0.2f;

    IEnumerator Start()
    {
        dragAndMove.enabled = false;

        FadeEffect.Instance.FadeIn(1f);
        yield return new WaitForSeconds(1f);
        
        // Start the repeating glitch effect
        StartCoroutine(RepeatGlitchEffect());
        
        // Start the tank light flicker effect
        StartCoroutine(FlickerTankLights());

        yield return new WaitForSeconds(1f);

        dragAndMove.enabled = true;
        instructionText.text = "Drag to look around";
        
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(WaitForDragThreshold());
        
        dragAndMove.enabled = false;
        
        dragAndMove.ResetToOriginalPosition();
        tapProgressManager.ResetProgress();
        instructionText.text = "Tap repeatedly to Struggle";
        tapProgressManager.OnProgressComplete.AddListener(OnProgressComplete);
        tapProgressManager.SetProgressBarActive(true); 
        
        Debug.Log("Coroutine finished!");
    }

    IEnumerator WaitForDragThreshold()
    {
        // Wait until the drag threshold is reached
        while (!dragAndMove.IsThresholdReached())
        {
            yield return null;
        }
    }

    void OnProgressComplete()
    {
        instructionText.text = "";
        tapProgressManager.SetProgressBarActive(false); 
    }

    IEnumerator RepeatGlitchEffect()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = Random.Range(minGlitchInterval, maxGlitchInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Random fade durations
            float fadeInDuration = Random.Range(minFadeDuration, maxFadeDuration);
            float fadeOutDuration = Random.Range(minFadeDuration, maxFadeDuration);
            
            // Random alpha value
            float randomAlpha = Random.Range(minAlpha, maxAlpha);
            
            // Random wait time between fade in and out
            float randomWaitTime = Random.Range(minWaitTime, maxWaitTime);
            
            // Fade in glitch effect smoothly (random duration and alpha)
            glitchImage.DOFade(randomAlpha, fadeInDuration);
            yield return new WaitForSeconds(randomWaitTime);
            
            // Fade out glitch effect smoothly (random duration)
            glitchImage.DOFade(0f, fadeOutDuration);
        }
    }

    IEnumerator FlickerTankLights()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = Random.Range(minTankLightInterval, maxTankLightInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Random flicker duration
            float flickerDuration = Random.Range(minTankLightDuration, maxTankLightDuration);
            
            // Random intensity for both lights
            float randomIntensity1 = Random.Range(minTankLightIntensity, maxTankLightIntensity);
            float randomIntensity2 = Random.Range(minTankLightIntensity, maxTankLightIntensity);
            
            // Store original intensities
            float originalIntensity1 = tankLight1.intensity;
            float originalIntensity2 = tankLight2.intensity;
            
            // Flicker both lights to random intensity
            tankLight1.intensity = randomIntensity1;
            tankLight2.intensity = randomIntensity2;
            
            // Wait for flicker duration
            yield return new WaitForSeconds(flickerDuration);
            
            // Return to original intensity
            tankLight1.intensity = originalIntensity1;
            tankLight2.intensity = originalIntensity2;
        }
    }
}