using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeEffect : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool fadeOnStart = false;
    [SerializeField] private bool startFadedOut = true;
    
    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fadeImage;
    
    [Header("Scene Transition")]
    [SerializeField] private bool enableSceneTransition = false;
    [SerializeField] private string nextSceneName = "";
    
    private Coroutine currentFadeCoroutine;
    
    public bool IsFading { get; private set; }
    public float CurrentAlpha { get; private set; }
    
    public static FadeEffect Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find components if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();
            
        // Set initial alpha
        if (startFadedOut)
        {
            SetAlpha(1f);
        }
        else
        {
            SetAlpha(0f);
        }
    }
    
    private void Start()
    {
        if (fadeOnStart)
        {
            if (startFadedOut)
                FadeIn();
            else
                FadeOut();
        }
    }
    
    /// <summary>
    /// Fade from current alpha to 0 (fade in)
    /// </summary>
    public void FadeIn()
    {
        FadeIn(fadeDuration);
    }
    
    /// <summary>
    /// Fade from current alpha to 0 (fade in) with custom duration
    /// </summary>
    public void FadeIn(float duration)
    {
        if (IsFading)
            StopCoroutine(currentFadeCoroutine);
            
        currentFadeCoroutine = StartCoroutine(FadeCoroutine(CurrentAlpha, 0f, duration));
    }
    
    /// <summary>
    /// Fade from current alpha to 1 (fade out)
    /// </summary>
    public void FadeOut()
    {
        FadeOut(fadeDuration);
    }
    
    /// <summary>
    /// Fade from current alpha to 1 (fade out) with custom duration
    /// </summary>
    public void FadeOut(float duration)
    {
        if (IsFading)
            StopCoroutine(currentFadeCoroutine);
            
        currentFadeCoroutine = StartCoroutine(FadeCoroutine(CurrentAlpha, 1f, duration));
    }
    
    /// <summary>
    /// Fade to specific alpha value
    /// </summary>
    public void FadeTo(float targetAlpha, float duration)
    {
        if (IsFading)
            StopCoroutine(currentFadeCoroutine);
            
        currentFadeCoroutine = StartCoroutine(FadeCoroutine(CurrentAlpha, targetAlpha, duration));
    }
    
    /// <summary>
    /// Fade out, then fade in (useful for scene transitions)
    /// </summary>
    public void FadeOutThenIn(float fadeOutDuration = -1, float fadeInDuration = -1)
    {
        if (fadeOutDuration < 0) fadeOutDuration = fadeDuration;
        if (fadeInDuration < 0) fadeInDuration = fadeDuration;
        
        StartCoroutine(FadeOutThenInCoroutine(fadeOutDuration, fadeInDuration));
    }
    
    /// <summary>
    /// Fade out, load scene, then fade in
    /// </summary>
    public void FadeToScene(string sceneName, float fadeOutDuration = -1, float fadeInDuration = -1)
    {
        if (fadeOutDuration < 0) fadeOutDuration = fadeDuration;
        if (fadeInDuration < 0) fadeInDuration = fadeDuration;
        
        StartCoroutine(FadeToSceneCoroutine(sceneName, fadeOutDuration, fadeInDuration));
    }
    
    /// <summary>
    /// Set alpha immediately without animation
    /// </summary>
    public void SetAlpha(float alpha)
    {
        CurrentAlpha = Mathf.Clamp01(alpha);
        
        if (canvasGroup != null)
            canvasGroup.alpha = CurrentAlpha;
            
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = CurrentAlpha;
            fadeImage.color = color;
        }
    }
    
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        IsFading = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = fadeCurve.Evaluate(progress);
            
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            SetAlpha(currentAlpha);
            
            yield return null;
        }
        
        SetAlpha(endAlpha);
        IsFading = false;
        currentFadeCoroutine = null;
    }
    
    private IEnumerator FadeOutThenInCoroutine(float fadeOutDuration, float fadeInDuration)
    {
        yield return StartCoroutine(FadeCoroutine(CurrentAlpha, 1f, fadeOutDuration));
        yield return StartCoroutine(FadeCoroutine(1f, 0f, fadeInDuration));
    }
    
    private IEnumerator FadeToSceneCoroutine(string sceneName, float fadeOutDuration, float fadeInDuration)
    {
        // Fade out
        yield return StartCoroutine(FadeCoroutine(CurrentAlpha, 1f, fadeOutDuration));
        
        // Load scene
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        
        // Wait for scene to load
        yield return new WaitForSeconds(0.1f);
        
        // Fade in
        yield return StartCoroutine(FadeCoroutine(1f, 0f, fadeInDuration));
    }
    
    /// <summary>
    /// Stop current fade animation
    /// </summary>
    public void StopFade()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
            IsFading = false;
        }
    }
    
    /// <summary>
    /// Check if currently faded out (alpha = 1)
    /// </summary>
    public bool IsFadedOut()
    {
        return Mathf.Approximately(CurrentAlpha, 1f);
    }
    
    /// <summary>
    /// Check if currently faded in (alpha = 0)
    /// </summary>
    public bool IsFadedIn()
    {
        return Mathf.Approximately(CurrentAlpha, 0f);
    }
}
