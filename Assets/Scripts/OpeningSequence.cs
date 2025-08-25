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
    [Header("Tank Light 1 Settings")]
    public float minTankLight1Interval = 0.5f;
    public float maxTankLight1Interval = 3f;
    public float minTankLight1Intensity = 0.1f;
    public float maxTankLight1Intensity = 2f;
    public float minTankLight1Duration = 0.05f;
    public float maxTankLight1Duration = 0.2f;
    
    [Header("Tank Light 2 Settings")]
    public float minTankLight2Interval = 0.8f;
    public float maxTankLight2Interval = 4f;
    public float minTankLight2Intensity = 0.1f;
    public float maxTankLight2Intensity = 2f;
    public float minTankLight2Duration = 0.08f;
    public float maxTankLight2Duration = 0.25f;

    IEnumerator Start()
    {
        dragAndMove.enabled = false;

        FadeEffect.Instance.FadeIn(1f);
        yield return new WaitForSeconds(1f);
        
        // Start the repeating glitch effect
        StartCoroutine(RepeatGlitchEffect());
        
        // Start the tank light flicker effects (separate for each light)
        StartCoroutine(FlickerTankLight1());
        StartCoroutine(FlickerTankLight2());

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

    IEnumerator FlickerTankLight1()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = Random.Range(minTankLight1Interval, maxTankLight1Interval);
            yield return new WaitForSeconds(waitTime);
            
            // Random flicker duration
            float flickerDuration = Random.Range(minTankLight1Duration, maxTankLight1Duration);
            
            // Random intensity for light 1
            float randomIntensity = Random.Range(minTankLight1Intensity, maxTankLight1Intensity);
            
            // Store original intensity
            float originalIntensity = tankLight1.intensity;
            
            // Flicker light 1 to random intensity
            tankLight1.intensity = randomIntensity;
            
            // Wait for flicker duration
            yield return new WaitForSeconds(flickerDuration);
            
            // Return to original intensity
            tankLight1.intensity = originalIntensity;
        }
    }

    IEnumerator FlickerTankLight2()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = Random.Range(minTankLight2Interval, maxTankLight2Interval);
            yield return new WaitForSeconds(waitTime);
            
            // Random flicker duration
            float flickerDuration = Random.Range(minTankLight2Duration, maxTankLight2Duration);
            
            // Random intensity for light 2
            float randomIntensity = Random.Range(minTankLight2Intensity, maxTankLight2Intensity);
            
            // Store original intensity
            float originalIntensity = tankLight2.intensity;
            
            // Flicker light 2 to random intensity
            tankLight2.intensity = randomIntensity;
            
            // Wait for flicker duration
            yield return new WaitForSeconds(flickerDuration);
            
            // Return to original intensity
            tankLight2.intensity = originalIntensity;
        }
    }
}