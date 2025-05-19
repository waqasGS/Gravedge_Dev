using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText; 

    
    [Header("Menu Panels")]
    [SerializeField] private GameObject userInputPanel;
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject rateUsPanel;
    [SerializeField] private GameObject morePanel;
    [SerializeField] private GameObject sharePanel;

    public void OnUserInput() => TogglePanel(userInputPanel);
    public void OnSetting() => TogglePanel(settingPanel);
    public void OnQuit() => TogglePanel(quitPanel);
    public void OnRateUs() => TogglePanel(rateUsPanel);
    public void OnMore() => TogglePanel(morePanel);
    public void OnShare() => TogglePanel(sharePanel);

    private void TogglePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    public void LoadScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
            {
                progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime * 0.5f);
                progressBar.value = progress;

                if (progressText != null)
                    progressText.text = $"LOADING {Mathf.RoundToInt(progress * 100f)}%";
                

            }

            if (operation.progress >= 0.9f && progress >= 1f)
            {
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}