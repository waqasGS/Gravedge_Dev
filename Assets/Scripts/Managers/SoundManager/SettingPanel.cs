using UnityEngine;
using UnityEngine.UI;


public class SettingPanel : MonoBehaviour
{
    public Button musicToggleButton; // Button to toggle music on/off
    public Button sfxToggleButton; // Button to toggle sound effects on/off
    public Slider musicSlider; // Slider to control music volume
    public Slider sfxSlider; // Slider to control sound effects volume


    private bool isMusicOn;
    private bool isSFXOn;

    private void Start()
    {
        // Load saved mute states
        isMusicOn = PlayerPrefs.GetInt("MusicMute", 0) == 0;
        isSFXOn = PlayerPrefs.GetInt("SFXMute", 0) == 0;

        // Initialize sliders with saved volume levels
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Set up button listeners and update UI
        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.AddListener(ToggleMusic);
            UpdateMusicButton();
        }

        if (sfxToggleButton != null)
        {
            sfxToggleButton.onClick.AddListener(ToggleSFX);
            UpdateSFXButton();
        }
    }

    // Toggles music on/off
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        SoundManager.Instance.SetMusicMute(!isMusicOn);
        PlayerPrefs.SetInt("MusicMute", isMusicOn ? 0 : 1);
        UpdateMusicButton();
    }

    // Toggles sound effects on/off
    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        SoundManager.Instance.SetSFXMute(!isSFXOn);
        PlayerPrefs.SetInt("SFXMute", isSFXOn ? 0 : 1);
        UpdateSFXButton();
    }

    // Adjusts music volume
    public void SetMusicVolume(float volume)
    {
        Debug.Log(volume);
        SoundManager.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    // Adjusts sound effects volume
    public void SetSFXVolume(float volume)
    {
        SoundManager.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    // Updates music button text based on current state
    private void UpdateMusicButton()
    {
        //musicToggleButton.GetComponentInChildren<Text>().text = isMusicOn ? "Music: ON" : "Music: OFF";
    }

    // Updates sound effects button text based on current state
    private void UpdateSFXButton()
    {
        //sfxToggleButton.GetComponentInChildren<Text>().text = isSFXOn ? "SFX: ON" : "SFX: OFF";
    }
}
