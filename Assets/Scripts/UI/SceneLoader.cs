using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("UI References")]
    public Slider loadingSlider;
    public Text loadingText;
    public GameObject tapToContinueText; // Optional UI message like "Tap to Continue"

    [Header("Scene To Load")]
    public string sceneToLoad = "MainMenu";

    [Header("Settings")]
    public float minimumLoadTime = 10f;

    private bool readyToContinue = false;

    private void Start()
    {
        if (tapToContinueText != null)
            tapToContinueText.SetActive(false);

        StartCoroutine(LoadSceneWithProgress());
    }

    IEnumerator LoadSceneWithProgress()
    {
        float elapsedTime = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float targetProgress = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            targetProgress = Mathf.Min(loadProgress, timeProgress);

            loadingSlider.value = targetProgress;
            loadingText.text = $"LOADING {Mathf.RoundToInt(targetProgress * 100f)}%";

            // When loading and timer both complete, show "Tap to Continue"
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime && !readyToContinue)
            {
                readyToContinue = true;
                loadingSlider.value = 1f;
                loadingText.text = "LOADING 100%";

                if (tapToContinueText != null)
                    tapToContinueText.SetActive(true); // show message
            }

            // Wait for tap/click
            if (readyToContinue && Input.GetMouseButtonDown(0))
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
