using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Playables;

public class FadeOnClick : MonoBehaviour
{
    public float fadeDuration = 1f;
    public Image fadeImage;
    public PlayableDirector _playableDirector;


    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    { 
        _playableDirector.Play();
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        
        fadeImage.gameObject.SetActive(false);
        
    }
}