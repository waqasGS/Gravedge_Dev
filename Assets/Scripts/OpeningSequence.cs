using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

public class OpeningSequence : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public Image glitchImage;
    public AudioSource glitchAudioSource; // Audio source for glitch effect
    public AudioSource tankLight1AudioSource; // Audio source for tank light 1
    public AudioSource tankLight1OffAudioSource; // Audio source for tank light 1 turning off
    public AudioSource tankLight2AudioSource; // Audio source for tank light 2
    public AudioSource tankLight2OffAudioSource; // Audio source for tank light 2 turning off
    public DragAndMove dragAndMove;
    public TapProgressManager tapProgressManager;
    public Animator animator;
    public PostProcessVolume postProcessVolume;

    public Light tankLight1;
    public Light tankLight2;
    
    [Header("Struggle Camera Shake")]
    public CinemachineImpulseSource impulseSource;
    public float minShakeInterval = 0.08f;
    public float maxShakeInterval = 0.2f;
    public float shakeDuration = 0.12f;
    public GameObject tankCrack1;
    public GameObject tankCrack2;
    public GameObject tankCrack3;
    public GameObject tankCrack4;
    
    [Header("Impulse Force Settings")]
   public Vector2 velocityRangeX = new Vector2(0.5f, 2.0f);
   public Vector2 velocityRangeY = new Vector2(0.5f, 2.0f);
   public Vector2 velocityRangeZ = new Vector2(0.5f, 2.0f);
    
    Coroutine struggleShakeRoutine;
 
    [Header("Glitch Effect Settings")]
    public float minGlitchInterval = 2f;
    public float maxGlitchInterval = 8f;
    public float minFadeDuration = 0.1f;
    public float maxFadeDuration = 0.4f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1.0f;
    public float audioThresholdGlitch = 0.1f; // Threshold for when audio should start/stop based on transparency

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
    
    [Header("Audio Thresholds")]
    public float tankLight1AudioThreshold = 0.5f; // Threshold for tank light 1 intensity to trigger audio
    public float tankLight2AudioThreshold = 0.5f; // Threshold for tank light 2 intensity to trigger audio
    
    [Header("Audio Volume Variation")]
    public float tankLight1VolumeMin = 0.7f; // Minimum volume multiplier for tank light 1 (percentage of max)
    public float tankLight1VolumeMax = 1.0f; // Maximum volume multiplier for tank light 1 (percentage of max)
    public float tankLight2VolumeMin = 0.7f; // Minimum volume multiplier for tank light 2 (percentage of max)
    public float tankLight2VolumeMax = 1.0f; // Maximum volume multiplier for tank light 2 (percentage of max)
    public float glitchVolumeMin = 0.6f; // Minimum volume multiplier for glitch audio (percentage of max)
    public float glitchVolumeMax = 1.0f; // Maximum volume multiplier for glitch audio (percentage of max)
    
    // Private variables to track audio state for tank lights
    private bool tankLight1AudioShouldPlay = true;
    private bool tankLight2AudioShouldPlay = true;
    
    // Private variables to store original volumes
    private float tankLight1OriginalVolume;
    private float tankLight1OffOriginalVolume;
    private float tankLight2OriginalVolume;
    private float tankLight2OffOriginalVolume;
    private float glitchOriginalVolume;

    IEnumerator Start()
    {
        dragAndMove.enabled = false;

        // Hide tank cracks by default
        if (tankCrack1 != null) tankCrack1.SetActive(false);
        if (tankCrack2 != null) tankCrack2.SetActive(false);
        if (tankCrack3 != null) tankCrack3.SetActive(false);
        if (tankCrack4 != null) tankCrack4.SetActive(false);

        FadeEffect.Instance.FadeIn(1f);

        var vignette = postProcessVolume.profile.GetSetting<Vignette>();
        float startIntensity = vignette.intensity.value;
        float targetIntensity = 1f;

        Sequence vignetteSequence = DOTween.Sequence();
        vignetteSequence.Append(
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, targetIntensity, 1f)
        );
        vignetteSequence.AppendInterval(2f);
        vignetteSequence.Append(
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.4f, 3f)
        );

        yield return new WaitForSeconds(1f);
        
        // Store original volumes and initialize glitch audio source to be paused initially
        if (glitchAudioSource != null)
        {
            glitchOriginalVolume = glitchAudioSource.volume;
            glitchAudioSource.Play();
            glitchAudioSource.Pause();
        }
        
        if (tankLight1AudioSource != null)
        {
            tankLight1OriginalVolume = tankLight1AudioSource.volume;
        }
        
        if (tankLight1OffAudioSource != null)
        {
            tankLight1OffOriginalVolume = tankLight1OffAudioSource.volume;
        }
        
        if (tankLight2AudioSource != null)
        {
            tankLight2OriginalVolume = tankLight2AudioSource.volume;
        }
        
        if (tankLight2OffAudioSource != null)
        {
            tankLight2OffOriginalVolume = tankLight2OffAudioSource.volume;
        }
        
        // Start the repeating glitch effect
        StartCoroutine(RepeatGlitchEffect());
        
        // Start the tank light flicker effects (separate for each light)
        StartCoroutine(FlickerTankLight1());
        StartCoroutine(FlickerTankLight2());

        yield return new WaitForSeconds(6f);

        dragAndMove.enabled = true;
        instructionText.text = "Drag to look around";
        
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(WaitForDragThreshold());
        
        dragAndMove.enabled = false;
        
        dragAndMove.ResetToOriginalPosition();
        tapProgressManager.ResetProgress();
        instructionText.text = "Tap repeatedly to Struggle";
        tapProgressManager.OnProgressComplete.AddListener(OnProgressComplete);
        tapProgressManager.OnSessionStart.AddListener(StartStruggleShake);
        tapProgressManager.OnSessionEnd.AddListener(StopStruggleShake);
        tapProgressManager.SetProgressBarActive(true); 

        // Start monitoring progress for tank cracks
        StartCoroutine(MonitorProgressForTankCracks());
        
        Debug.Log("Coroutine finished!");
    }
    
    // Helper method to set random volume for an audio source based on original volume and range
    private void SetRandomVolume(AudioSource audioSource, float originalVolume, float minMultiplier, float maxMultiplier)
    {
        if (audioSource != null)
        {
            float randomMultiplier = Random.Range(minMultiplier, maxMultiplier);
            audioSource.volume = originalVolume * randomMultiplier;
        }
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
        StopStruggleShake();
    }

    void StartStruggleShake()
    {
        // Prevent duplicates
        if (struggleShakeRoutine != null)
            return;
        struggleShakeRoutine = StartCoroutine(StruggleShakeLoop());
    }

    void StopStruggleShake()
    {
        if (struggleShakeRoutine != null)
        {
            StopCoroutine(struggleShakeRoutine);
            struggleShakeRoutine = null;
        }
    }

    IEnumerator StruggleShakeLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minShakeInterval, maxShakeInterval);
            yield return new WaitForSeconds(waitTime);
            yield return StartCoroutine(ShakeOnce());
        }
    }

    IEnumerator ShakeOnce()
    {
        if (impulseSource == null)
            yield break;
            
        // Generate random velocity and force for this impulse (x, y, z)
        float randomVelocityX = Random.Range(velocityRangeX.x, velocityRangeX.y);
        float randomVelocityY = Random.Range(velocityRangeY.x, velocityRangeY.y);
        float randomVelocityZ = Random.Range(velocityRangeZ.x, velocityRangeZ.y);
        
        // Generate the impulse with updated settings
        impulseSource.GenerateImpulse(new Vector3(randomVelocityX, randomVelocityY, randomVelocityZ));

        animator.Play("JabCross");
        
        // Wait for shake duration
        yield return new WaitForSeconds(shakeDuration);
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
            glitchImage.DOFade(randomAlpha, fadeInDuration).OnUpdate(() => {
                // Check if transparency crosses the threshold to start audio
                if (glitchAudioSource != null && !glitchAudioSource.isPlaying && glitchImage.color.a >= audioThresholdGlitch)
                {
                    // Set random volume before unpausing
                    SetRandomVolume(glitchAudioSource, glitchOriginalVolume, glitchVolumeMin, glitchVolumeMax);
                    glitchAudioSource.UnPause();
                }
            });
            
            yield return new WaitForSeconds(randomWaitTime);
            
            // Fade out glitch effect smoothly (random duration)
            glitchImage.DOFade(0f, fadeOutDuration).OnUpdate(() => {
                // Check if transparency crosses the threshold to pause audio
                if (glitchAudioSource != null && glitchAudioSource.isPlaying && glitchImage.color.a < audioThresholdGlitch)
                {
                    glitchAudioSource.Pause();
                }
            });
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
            
            // Control audio based on intensity threshold using PlayOneShot
            if (tankLight1AudioSource != null)
            {
                if (randomIntensity >= tankLight1AudioThreshold && tankLight1AudioShouldPlay)
                {
                    // Set random volume before playing
                    SetRandomVolume(tankLight1AudioSource, tankLight1OriginalVolume, tankLight1VolumeMin, tankLight1VolumeMax);
                    tankLight1AudioSource.PlayOneShot(tankLight1AudioSource.clip);
                    tankLight1AudioShouldPlay = false; // Prevent repeated playing while above threshold
                }
                else if (randomIntensity < tankLight1AudioThreshold)
                {
                    tankLight1AudioShouldPlay = true; // Allow playing again when below threshold
                }
            }
            
            // Don't play off audio here - wait until we actually return to original intensity
            
            // Wait for flicker duration
            yield return new WaitForSeconds(flickerDuration);
            
            // Return to original intensity
            tankLight1.intensity = originalIntensity;
            
            // Play "off" audio when returning to original intensity if it's below threshold
            if (tankLight1OffAudioSource != null && originalIntensity < tankLight1AudioThreshold)
            {
                // Set random volume before playing
                SetRandomVolume(tankLight1OffAudioSource, tankLight1OffOriginalVolume, tankLight1VolumeMin, tankLight1VolumeMax);
                tankLight1OffAudioSource.PlayOneShot(tankLight1OffAudioSource.clip);
            }
            
            // Reset audio state when returning to original intensity if it's below threshold
            if (originalIntensity < tankLight1AudioThreshold)
            {
                tankLight1AudioShouldPlay = true;
            }
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
            
            // Control audio based on intensity threshold using PlayOneShot
            if (tankLight2AudioSource != null)
            {
                if (randomIntensity >= tankLight2AudioThreshold && tankLight2AudioShouldPlay)
                {
                    // Set random volume before playing
                    SetRandomVolume(tankLight2AudioSource, tankLight2OriginalVolume, tankLight2VolumeMin, tankLight2VolumeMax);
                    tankLight2AudioSource.PlayOneShot(tankLight2AudioSource.clip);
                    tankLight2AudioShouldPlay = false; // Prevent repeated playing while above threshold
                }
                else if (randomIntensity < tankLight2AudioThreshold)
                {
                    tankLight2AudioShouldPlay = true; // Allow playing again when below threshold
                }
            }
            
            // Don't play off audio here - wait until we actually return to original intensity
            
            // Wait for flicker duration
            yield return new WaitForSeconds(flickerDuration);
            
            // Return to original intensity
            tankLight2.intensity = originalIntensity;
            
            // Play "off" audio when returning to original intensity if it's below threshold
            if (tankLight2OffAudioSource != null && originalIntensity < tankLight2AudioThreshold)
            {
                // Set random volume before playing
                SetRandomVolume(tankLight2OffAudioSource, tankLight2OffOriginalVolume, tankLight2VolumeMin, tankLight2VolumeMax);
                tankLight2OffAudioSource.PlayOneShot(tankLight2OffAudioSource.clip);
            }
            
            // Reset audio state when returning to original intensity if it's below threshold
            if (originalIntensity < tankLight2AudioThreshold)
            {
                tankLight2AudioShouldPlay = true;
            }
        }
    }

    IEnumerator MonitorProgressForTankCracks()
    {
        while (true)
        {
            float currentProgress = tapProgressManager.CurrentProgress;

            if (tankCrack1 != null)
            {
                if (currentProgress >= 20f)
                {
                    tankCrack1.SetActive(true);
                }
                else
                {
                    tankCrack1.SetActive(false);
                }
            }

            if (tankCrack2 != null)
            {
                if (currentProgress >= 40f)
                {
                    tankCrack2.SetActive(true);
                }
                else
                {
                    tankCrack2.SetActive(false);
                }
            }

            if (tankCrack3 != null)
            {
                if (currentProgress >= 60f)
                {
                    tankCrack3.SetActive(true);
                }
                else
                {
                    tankCrack3.SetActive(false);
                }
            }

            if (tankCrack4 != null)
            {
                if (currentProgress >= 80f)
                {
                    tankCrack4.SetActive(true);
                }
                else
                {
                    tankCrack4.SetActive(false);
                }
            }

            yield return null;
        }
    }
}