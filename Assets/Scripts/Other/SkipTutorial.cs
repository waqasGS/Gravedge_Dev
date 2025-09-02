using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkipTutorial : MonoBehaviour
{
    public Image FadeIn;
    public PlayableDirector timeLine;
    public float duration = 0.3f; // fade time
    public string nextSceneName;
    public void ToSkipTutorial()
    {
        FadeIn.gameObject.SetActive(true);
        StartCoroutine(FadeAndSkip());
    }

    private IEnumerator FadeAndSkip()
    {        
        float elapsed = 0f;

        Color startColor = FadeIn.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // full alpha

        // pehle ensure kar lo alpha 0 se start ho
        startColor.a = 0f;
        FadeIn.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            FadeIn.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        FadeIn.color = endColor;

        // Fade complete -> timeline stop karo
        timeLine.Stop();
        Debug.Log("NextSceneStart");
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        Debug.Log("NextSceneStart1");
        SceneManager.LoadScene(nextSceneName);
    }
}
