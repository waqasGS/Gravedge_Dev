using TMPro;
using UnityEngine;

public class StealthTutorial : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public Sprite image;
    void Start()
    {
        tutorialText.text =
            "Approach an enemy quietly from behind.\n" +
            "When the <sprite=image> appears, press it to eliminate the enemy with a stealth takedown.";
    }
}
