using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string homeSceneName = "MainMenu";
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;   // Enemy prefabs
    [SerializeField] private Transform[] spawnPoints;     // Spawn points
    [SerializeField] private Transform enemyParent;       // Parent object to hold spawned enemies

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            fadePanel.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }

       // SpawnEnemies(); // Spawn enemies at start
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    public void Msgshow(string msg)
    {
        Debug.Log(msg);
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < Mathf.Min(enemyPrefabs.Length, spawnPoints.Length); i++)
        {
            GameObject enemy = Instantiate(enemyPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation, enemyParent);
        }
    }
}
