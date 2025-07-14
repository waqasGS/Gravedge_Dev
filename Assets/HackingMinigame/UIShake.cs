using System.Collections;
using UnityEngine;
using DG.Tweening;

public class UIShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeStrength = 10f;
    public float shakeDuration = 0.5f;
    public int vibrato = 10;
    public float randomness = 90f;
    public bool fadeOut = true;
    
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("UIShake requires a RectTransform component!");
            enabled = false;
            return;
        }
        
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }
    
    public void Shake()
    {
        // Kill any existing shake tweens
        rectTransform.DOKill();
        
        // Reset to original position
        rectTransform.anchoredPosition = originalAnchoredPosition;
        
        // Start the shake
        rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, vibrato, randomness, false, fadeOut)
            .OnComplete(() => {
                // Ensure we return to the original position
                rectTransform.anchoredPosition = originalAnchoredPosition;
            });
    }
    
    public void Shake(float customStrength, float customDuration)
    {
        // Kill any existing shake tweens
        rectTransform.DOKill();
        
        // Reset to original position
        rectTransform.anchoredPosition = originalAnchoredPosition;
        
        // Start the shake with custom parameters
        rectTransform.DOShakeAnchorPos(customDuration, customStrength, vibrato, randomness, false, fadeOut)
            .OnComplete(() => {
                // Ensure we return to the original position
                rectTransform.anchoredPosition = originalAnchoredPosition;
            });
    }
    
    public void Shake(Vector2 customStrength, float customDuration)
    {
        // Kill any existing shake tweens
        rectTransform.DOKill();
        
        // Reset to original position
        rectTransform.anchoredPosition = originalAnchoredPosition;
        
        // Start the shake with custom vector strength
        rectTransform.DOShakeAnchorPos(customDuration, customStrength, vibrato, randomness, false, fadeOut)
            .OnComplete(() => {
                // Ensure we return to the original position
                rectTransform.anchoredPosition = originalAnchoredPosition;
            });
    }
    
    // Static method for easy access
    public static void ShakeUI(GameObject uiObject, float strength = 10f, float duration = 0.5f)
    {
        UIShake uiShake = uiObject.GetComponent<UIShake>();
        if (uiShake != null)
        {
            uiShake.Shake(strength, duration);
        }
        else
        {
            Debug.LogWarning($"UIShake component not found on {uiObject.name}");
        }
    }
} 