using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class TypeWriter : MonoBehaviour
{
    public GameObject notificationImage;
    public TextMeshProUGUI instructionText;
    [Header("Typewriter Effect Settings")]
    public float typewriterSpeed = 0.05f; // Time between each character appearing
    public bool useTypewriterEffect = true; // Toggle to enable/disable typewriter effect
    public AudioSource typewriterKeySound; // Audio source for the key sound
    public AudioClip notificatioSound;
    public AudioClip typingSound;

    public float delayToDisapear;

    public void StartTyping(string text)
    {
        StartCoroutine(TypewriterEffect(text));
    }


    private IEnumerator TypewriterEffect(string text)
    {
        yield return new WaitForSeconds(1.02f);
        if (!useTypewriterEffect)
        {
            instructionText.text = text;
            yield break;
        }

        instructionText.text = "";
        typewriterKeySound.clip = notificatioSound;
        typewriterKeySound.Play();
        notificationImage.transform.DOLocalMoveX(184.2073f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < text.Length; i++)
        {
            instructionText.text += text[i];

            // Play key sound for each character
            if (typewriterKeySound != null)
            {
                typewriterKeySound.clip = typingSound;
                typewriterKeySound.PlayOneShot(typewriterKeySound.clip);
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }
        yield return new WaitForSeconds(delayToDisapear);
        notificationImage.transform.DOLocalMoveX(-157.7926f, 0.5f).OnComplete(() => { gameObject.SetActive(false); });
        //gameObject.SetActive(false);
    }
}
