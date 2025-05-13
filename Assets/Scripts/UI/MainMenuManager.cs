using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Loading")]
    public string SceneToLoad;
    public GameObject LoadingScreen;
    public Slider ProgressBar;
    public Text ProgressText; // 👈 Add this for percentage display

    [Header("Menu Panels")]
    public GameObject UserInputPanel;
    public GameObject QuitPanel;
    public GameObject SettingPanel;
    public GameObject RateUsPanel;
    public GameObject MorePanel;
    public GameObject SharePanel;

    public void OnUserInput() => TogglePanel(UserInputPanel);
    public void OnSetting() => TogglePanel(SettingPanel);
    public void OnQuit() => TogglePanel(QuitPanel);
    public void OnRateUs() => TogglePanel(RateUsPanel);
    public void OnMore() => TogglePanel(MorePanel);
    public void OnShare() => TogglePanel(SharePanel);

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
        if (LoadingScreen != null)
            LoadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneToLoad);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (ProgressBar != null)
            {
                progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime * 0.5f);
                ProgressBar.value = progress;

                if (ProgressText != null)
                    ProgressText.text = $"LOADING {Mathf.RoundToInt(progress * 100f)}%";
                

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