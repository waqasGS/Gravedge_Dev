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
    public float minGlitchInterval = 2f;
    public float maxGlitchInterval = 8f;
    public float minFadeDuration = 0.1f;
    public float maxFadeDuration = 0.4f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1.0f;

    IEnumerator Start()
    {
        FadeEffect.Instance.FadeIn(1f);
        yield return new WaitForSeconds(1f);
        
        // Start the repeating glitch effect
        StartCoroutine(RepeatGlitchEffect());

        yield return new WaitForSeconds(1f);

        instructionText.text = "Drag to look around";
        yield return new WaitForSeconds(1f);
        
        Debug.Log("Coroutine finished!");
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
}