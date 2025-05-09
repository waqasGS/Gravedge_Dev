using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    #region Singleton

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playingAudioSources = new List<PlayingSound>();
        loopingAudioSources = new List<PlayingSound>();
        LoadSettings(); // Load saved settings on start
    }

    #endregion

    #region Classes

    [System.Serializable]
    private class SoundInfo
    {
        public string id = ""; // Unique identifier for the sound
        public AudioClip audioClip = null; // The actual audio clip
        public SoundType type = SoundType.SoundEffect; // Type of sound (Music or SFX)
        public bool playAndLoopOnStart = false; // Should the sound loop on start?

        [Range(0, 1)] public float clipVolume = 1; // Volume level of the clip
    }

    private class PlayingSound
    {
        public SoundInfo soundInfo = null; // Reference to the sound info
        public AudioSource audioSource = null; // Audio source playing the sound
    }

    #endregion

    #region Enums

    public enum SoundType
    {
        SoundEffect,
        Music
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private List<SoundInfo> soundInfos = null; // List of all sounds

    #endregion

    #region Member Variables

    private List<PlayingSound> playingAudioSources;
    private List<PlayingSound> loopingAudioSources;

    public bool IsMusicOn { get; private set; }
    public bool IsSoundEffectsOn { get; private set; }

    #endregion

    #region Unity Methods

    private void Start()
    {
        // Play and loop sounds marked for auto-play
        foreach (var soundInfo in soundInfos)
        {
            if (soundInfo.playAndLoopOnStart)
            {
                Play(soundInfo.id, true, 0);
            }
        }
    }

    private void Update()
    {
        // Remove audio sources that have finished playing
        for (int i = 0; i < playingAudioSources.Count; i++)
        {
            if (!playingAudioSources[i].audioSource.isPlaying)
            {
                Destroy(playingAudioSources[i].audioSource.gameObject);
                playingAudioSources.RemoveAt(i);
                i--;
            }
        }
    }

    #endregion

    #region Public Methods

    public void Play(string id)
    {
        Play(id, false, 0);
    }

    public void PlayLoop(string id)
    {
        Play(id, true, 0);
    }

    public void Play(string id, bool loop, float playDelay)
    {
        SoundInfo soundInfo = GetSoundInfo(id);
        if (soundInfo == null) return;

        AudioSource audioSource = CreateAudioSource(id);
        audioSource.clip = soundInfo.audioClip;
        audioSource.loop = loop;

        // Adjust volume based on settings
        float volumeMultiplier = (soundInfo.type == SoundType.Music && IsMusicOn) ||
                                 (soundInfo.type == SoundType.SoundEffect && IsSoundEffectsOn) ? 1f : 0f;
        audioSource.volume = soundInfo.clipVolume * volumeMultiplier;

        if (playDelay > 0)
            audioSource.PlayDelayed(playDelay);
        else
            audioSource.Play();

        PlayingSound playingSound = new PlayingSound { soundInfo = soundInfo, audioSource = audioSource };

        if (loop)
            loopingAudioSources.Add(playingSound);
        else
            playingAudioSources.Add(playingSound);
    }

    public void Stop(string id)
    {
        StopAllSounds(id, playingAudioSources);
        StopAllSounds(id, loopingAudioSources);
    }

    public void SetMusicMute(bool isMuted)
    {
        IsMusicOn = !isMuted;
        AdjustVolumeForType(SoundType.Music, IsMusicOn ? 1f : 0f);
        PlayerPrefs.SetInt("MusicMute", isMuted ? 1 : 0);
    }

    public void SetSFXMute(bool isMuted)
    {
        IsSoundEffectsOn = !isMuted;
        AdjustVolumeForType(SoundType.SoundEffect, IsSoundEffectsOn ? 1f : 0f);
        PlayerPrefs.SetInt("SFXMute", isMuted ? 1 : 0);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);

        if (IsMusicOn)
        {
            AdjustVolumeForType(SoundType.Music, volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);

        if (IsSoundEffectsOn)
        {
            AdjustVolumeForType(SoundType.SoundEffect, volume);
        }
    }

    #endregion

    #region Private Methods

    private void AdjustVolumeForType(SoundType type, float volumeMultiplier)
    {
        foreach (PlayingSound sound in playingAudioSources)
        {
            if (sound.soundInfo.type == type)
                sound.audioSource.volume = sound.soundInfo.clipVolume * volumeMultiplier;
        }
        foreach (PlayingSound sound in loopingAudioSources)
        {
            if (sound.soundInfo.type == type)
                sound.audioSource.volume = sound.soundInfo.clipVolume * volumeMultiplier;
        }
    }

    private void StopAllSounds(string id, List<PlayingSound> playingSounds)
    {
        for (int i = 0; i < playingSounds.Count; i++)
        {
            if (id == playingSounds[i].soundInfo.id)
            {
                playingSounds[i].audioSource.Stop();
                Destroy(playingSounds[i].audioSource.gameObject);
                playingSounds.RemoveAt(i);
                i--;
            }
        }
    }

    private SoundInfo GetSoundInfo(string id)
    {
        return soundInfos.Find(s => s.id == id);
    }

    private AudioSource CreateAudioSource(string id)
    {
        GameObject obj = new GameObject("sound_" + id);
        obj.transform.SetParent(transform);
        return obj.AddComponent<AudioSource>();
    }

    private void LoadSettings()
    {
        IsMusicOn = PlayerPrefs.GetInt("MusicMute", 0) == 0;
        IsSoundEffectsOn = PlayerPrefs.GetInt("SFXMute", 0) == 0;
    }

    #endregion
}
