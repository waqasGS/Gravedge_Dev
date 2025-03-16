using UnityEngine;
using UnityEngine.UI;
public class FPSDisplay : MonoBehaviour
{
    public Text FpsText;
    private float deltaTime = 0.0f;



    void Start()
    {
        Application.targetFrameRate = 60; // Set your desired FPS
        QualitySettings.vSyncCount = 0; // Disable V-Sync to unlock frame rate
    }
    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        FpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
    }
}