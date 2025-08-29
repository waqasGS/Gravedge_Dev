using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

public class TapProgressManager : MonoBehaviour
{
    [Header("Progress Settings")]
    [SerializeField] private float maxProgress = 100f;
    [SerializeField] private Vector2 progressPerTapRange = new Vector2(8f, 12f); // Min/Max progress per tap
    [SerializeField] private Vector2 decayRateRange = new Vector2(4f, 6f); // Min/Max progress lost per second
    [SerializeField] private Vector2 decayDelayRange = new Vector2(0.3f, 0.7f); // Min/Max delay before decay starts
    
    [Header("Session Settings")]
    [SerializeField] private float sessionTimeout = 1f; // Time in seconds to consider tapping "in session"
    
    [Header("Smooth Transitions")]
    [SerializeField] private float progressBarTweenDuration = 0.3f; // Duration for progress bar animation
    [SerializeField] private float progressValueLerpSpeed = 5f; // Speed for progress value lerping
    
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText; // Optional text display
    
    [Header("Events")]
    [SerializeField] private UnityEvent onProgressComplete;
    [SerializeField] private UnityEvent onSessionStart;
    [SerializeField] private UnityEvent onSessionEnd;
    
    // Public properties
    public float CurrentProgress { get; private set; }
    public bool IsInSession { get; private set; }
    public bool IsProgressComplete { get; private set; }
    
    // Private variables
    private float lastTapTime;
    private Coroutine decayCoroutine;
    private Coroutine sessionCoroutine;
    private float targetProgress; // Target progress for smooth transitions
    private Coroutine progressLerpCoroutine;
    
    // Events that other scripts can subscribe to
    public UnityEvent OnProgressComplete => onProgressComplete;
    public UnityEvent OnSessionStart => onSessionStart;
    public UnityEvent OnSessionEnd => onSessionEnd;
    
    private void Start()
    {
        InitializeProgress();
        SetupUI();
    }
    
    private void Update()
    {
        // Detect taps on mobile devices
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleTap();
            }
        }
        
        // For testing in editor with mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            HandleTap();
        }
    }
    
    private void InitializeProgress()
    {
        CurrentProgress = 0f;
        targetProgress = 0f;
        IsProgressComplete = false;
        IsInSession = false;
        lastTapTime = 0f;
    }
    
    private void SetupUI()
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = maxProgress;
            progressBar.value = CurrentProgress;
        }
        
        UpdateProgressText();
    }
    
    private void HandleTap()
    {
        // Don't process taps if progress is already complete
        if (IsProgressComplete)
            return;
            
        // Get random progress per tap from range
        float progressPerTap = Random.Range(progressPerTapRange.x, progressPerTapRange.y);
        
        // Update target progress
        targetProgress = Mathf.Min(targetProgress + progressPerTap, maxProgress);
        
        // Check if progress is complete
        if (targetProgress >= maxProgress && !IsProgressComplete)
        {
            IsProgressComplete = true;
            onProgressComplete?.Invoke();
        }
        
        // Start smooth progress transition
        StartSmoothProgressTransition();
        
        // Handle session management
        HandleSessionManagement();
        
        // Handle decay
        HandleDecay();
    }
    
    private void HandleSessionManagement()
    {
        lastTapTime = Time.time;
        
        if (!IsInSession)
        {
            IsInSession = true;
            onSessionStart?.Invoke();
            
            // Start session timeout coroutine
            if (sessionCoroutine != null)
                StopCoroutine(sessionCoroutine);
            sessionCoroutine = StartCoroutine(SessionTimeoutCoroutine());
        }
        else
        {
            // Reset session timeout
            if (sessionCoroutine != null)
                StopCoroutine(sessionCoroutine);
            sessionCoroutine = StartCoroutine(SessionTimeoutCoroutine());
        }
    }
    
    private void HandleDecay()
    {
        // Stop existing decay coroutine
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);
            
        // Start new decay coroutine with delay
        decayCoroutine = StartCoroutine(DecayCoroutine());
    }
    
    private void StartSmoothProgressTransition()
    {
        // Stop existing progress lerp coroutine
        if (progressLerpCoroutine != null)
            StopCoroutine(progressLerpCoroutine);
            
        // Start new progress lerp coroutine
        progressLerpCoroutine = StartCoroutine(ProgressLerpCoroutine());
    }
    
    private IEnumerator ProgressLerpCoroutine()
    {
        while (Mathf.Abs(CurrentProgress - targetProgress) > 0.1f)
        {
            CurrentProgress = Mathf.Lerp(CurrentProgress, targetProgress, progressValueLerpSpeed * Time.deltaTime);
            UpdateProgressUI();
            yield return null;
        }
        
        // Ensure we reach the exact target
        CurrentProgress = targetProgress;
        UpdateProgressUI();
    }
    
    private IEnumerator DecayCoroutine()
    {
        // Get random decay delay from range
        float decayDelay = Random.Range(decayDelayRange.x, decayDelayRange.y);
        
        // Wait for decay delay
        yield return new WaitForSeconds(decayDelay);
        
        // Start decaying progress
        while (CurrentProgress > 0f && !IsProgressComplete)
        {
            yield return new WaitForSeconds(0.1f); // Update every 0.1 seconds for smooth decay
            
            // Get random decay rate from range for this decay cycle
            float decayRate = Random.Range(decayRateRange.x, decayRateRange.y);
            
            // Update target progress for decay
            targetProgress = Mathf.Max(targetProgress - (decayRate * 0.1f), 0f);
            
            // Start smooth transition for decay
            StartSmoothProgressTransition();
        }
    }
    
    private IEnumerator SessionTimeoutCoroutine()
    {
        yield return new WaitForSeconds(sessionTimeout);
        
        // Session has timed out
        IsInSession = false;
        onSessionEnd?.Invoke();
    }
    
    private void UpdateProgressUI()
    {
        if (progressBar != null)
        {
            // Use DOTween for smooth progress bar animation
            progressBar.DOValue(CurrentProgress, progressBarTweenDuration).SetEase(Ease.OutQuad);
        }
        
        UpdateProgressText();
    }
    
    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(CurrentProgress)}%";
        }
    }
    
    // Public methods for external control
    
    /// <summary>
    /// Reset the progress to zero and allow new progress
    /// </summary>
    public void ResetProgress()
    {
        targetProgress = 0f;
        IsProgressComplete = false;
        
        // Stop progress lerp
        if (progressLerpCoroutine != null)
        {
            StopCoroutine(progressLerpCoroutine);
            progressLerpCoroutine = null;
        }
        
        // Start smooth transition to zero
        StartSmoothProgressTransition();
        
        // Stop decay
        if (decayCoroutine != null)
        {
            StopCoroutine(decayCoroutine);
            decayCoroutine = null;
        }
    }
    
    /// <summary>
    /// Set the progress to a specific value
    /// </summary>
    /// <param name="value">Progress value (0 to maxProgress)</param>
    public void SetProgress(float value)
    {
        targetProgress = Mathf.Clamp(value, 0f, maxProgress);
        IsProgressComplete = (targetProgress >= maxProgress);
        
        // Start smooth transition to new value
        StartSmoothProgressTransition();
    }
    
    /// <summary>
    /// Add progress manually (useful for non-tap inputs)
    /// </summary>
    /// <param name="amount">Amount of progress to add</param>
    public void AddProgress(float amount)
    {
        if (IsProgressComplete)
            return;
            
        CurrentProgress = Mathf.Min(CurrentProgress + amount, maxProgress);
        
        if (CurrentProgress >= maxProgress && !IsProgressComplete)
        {
            IsProgressComplete = true;
            onProgressComplete?.Invoke();
        }
        
        UpdateProgressUI();
    }
    
    /// <summary>
    /// Get the progress as a normalized value (0.0 to 1.0)
    /// </summary>
    /// <returns>Normalized progress value</returns>
    public float GetNormalizedProgress()
    {
        return CurrentProgress / maxProgress;
    }
    
    /// <summary>
    /// Check if the player is actively tapping (in session)
    /// </summary>
    /// <returns>True if player tapped within session timeout</returns>
    public bool IsPlayerActive()
    {
        return IsInSession;
    }
    
    /// <summary>
    /// Enable or disable the progress bar
    /// </summary>
    /// <param name="active">True to enable, false to disable</param>
    public void SetProgressBarActive(bool active)
    {
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(active);
        }
        
        if (progressText != null)
        {
            progressText.gameObject.SetActive(active);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up coroutines
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);
        if (sessionCoroutine != null)
            StopCoroutine(sessionCoroutine);
        if (progressLerpCoroutine != null)
            StopCoroutine(progressLerpCoroutine);
    }
}
