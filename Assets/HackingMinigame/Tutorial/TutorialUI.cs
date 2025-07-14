using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI messageText;
    public Image arrowImage;
    public Image highlightOverlay;
    public Button skipButton;
    public Button nextButton;
    public Button previousButton;
    public GameObject unmask;
    
    [Header("Arrow Settings")]
    public Sprite arrowSprite;
    public Vector2 arrowSize = new Vector2(50f, 50f);
    
    public void SetupArrow()
    {
        if (arrowImage != null && arrowSprite != null)
        {
            arrowImage.sprite = arrowSprite;
            arrowImage.rectTransform.sizeDelta = arrowSize;
        }
    }
} 