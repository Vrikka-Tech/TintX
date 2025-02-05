using UnityEngine;
using System.IO;
using SFB;
using System.Collections; // Standalone File Browser for Save Dialog

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance;
    public GameObject canvas; // Assign your Canvas here
    public Texture2D logoTexture; // Assign your logo in the Inspector
    private string screenshotFolderPath;

    private void Start()
    {
        screenshotFolderPath = Application.persistentDataPath; // Default save path
    }

    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }
    }

    public void CaptureScreenshot()
    {
        StartCoroutine(TakeScreenshot());
    }

    private IEnumerator TakeScreenshot()
    {
        // Disable UI Canvas before capturing
        if (canvas != null)
            canvas.SetActive(false);

        yield return new WaitForEndOfFrame();

        // Capture the screen
        int width = Screen.width;
        int height = Screen.height;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // Re-enable UI Canvas after capturing
        if (canvas != null)
            canvas.SetActive(true);

        // Add Logo to Screenshot
        if (logoTexture != null)
        {
            screenshot = AddLogoToScreenshot(screenshot, logoTexture);
        }

        // Open Save File Dialog
        string savePath = GetSaveFilePath();
        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllBytes(savePath, screenshot.EncodeToPNG());
            Debug.Log($"Screenshot saved: {savePath}");
        }

        // Clean up
        Destroy(screenshot);
    }

    private string GetSaveFilePath()
    {
        var extensions = new[] { new ExtensionFilter("PNG Image", "png") };
        string path = StandaloneFileBrowser.SaveFilePanel("Save Screenshot", screenshotFolderPath, $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}", extensions);
        return !string.IsNullOrEmpty(path) ? path : "";

    }

    private Texture2D AddLogoToScreenshot(Texture2D screenshot, Texture2D logo)
    {
        int margin = 20; // Margin from the edge
        int logoWidth = logo.width / 4; // Resize logo to 25% of original
        int logoHeight = logo.height / 4;

        // Create new texture to hold both screenshot and logo
        Texture2D finalImage = new Texture2D(screenshot.width, screenshot.height, TextureFormat.RGB24, false);

        // Copy screenshot to final image
        finalImage.SetPixels(screenshot.GetPixels());

        // Copy resized logo to final image at top-left corner
        for (int x = 0; x < logoWidth; x++)
        {
            for (int y = 0; y < logoHeight; y++)
            {
                Color logoColor = logo.GetPixelBilinear((float)x / logoWidth, (float)y / logoHeight);
                if (logoColor.a > 0) // Ignore transparent pixels
                {
                    finalImage.SetPixel(x + margin, finalImage.height - logoHeight - margin + y, logoColor);
                }
            }
        }

        finalImage.Apply();
        return finalImage;
    }
}
