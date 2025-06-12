using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("UI References")]
    public Slider loadingSlider;
    public Text loadingText;

    [Header("Scene To Load")]
    public string sceneToLoad = "MainMenu";

    [Header("Settings")]
    public float minimumLoadTime = 10f;

    private void Start()
    {
        StartCoroutine(LoadSceneWithProgress());
    }

    IEnumerator LoadSceneWithProgress()
    {
        float elapsedTime = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            float targetProgress = Mathf.Min(loadProgress, timeProgress);

            loadingSlider.value = targetProgress;
            loadingText.text = $"LOADING {Mathf.RoundToInt(targetProgress * 100f)}%";

            // Automatically allow scene activation when both are done
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                loadingSlider.value = 1f;
                loadingText.text = "LOADING 100%";
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
