using UnityEngine;
using System.IO;

public class ObjectScreenshot : MonoBehaviour
{
    public Camera screenshotCamera;
    public int width = 1024;
    public int height = 1024;

    public string fileName = "ObjectScreenshot.png";

    public void TakeScreenshot()
    {
        // Set camera to transparent background
        screenshotCamera.clearFlags = CameraClearFlags.SolidColor;
        screenshotCamera.backgroundColor = new Color(1f, 1f, 1f, 0f); // Transparent white

        // RenderTexture with alpha support
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        screenshotCamera.targetTexture = rt;

        // Texture2D with alpha support
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        screenshotCamera.Render();

        // Read the pixels from the RenderTexture
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // Clean up
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Save to Resources/Screenshots folder
        string folderPath = Path.Combine(Application.dataPath, "Resources/Screenshots");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());

        Debug.Log("Screenshot saved to: " + fullPath);
    }
}
