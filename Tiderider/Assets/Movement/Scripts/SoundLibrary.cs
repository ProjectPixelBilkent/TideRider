using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SoundEntry
{
    public string id;
    [Range(0f, 1f)] public float volume;
    public AudioClip audioClip;
}

public class SoundLibrary : MonoBehaviour
{
    [SerializeField] private SoundEntry[] entries;

    private Dictionary<string, SoundEntry> entriesDict;
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    public static SoundLibrary Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        entriesDict = new Dictionary<string, SoundEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            entriesDict[entries[i].id] = entries[i];
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0;
    }

    private void Update()
    {
        if (bgmSource.isPlaying)
        {
            string currentClipId = GetCurrentBGMId();
            if (currentClipId != null && entriesDict.TryGetValue(currentClipId, out SoundEntry entry))
            {
                float globalMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
                bgmSource.volume = entry.volume * globalMusicVol;
            }
        }
    }

    private string GetCurrentBGMId()
    {
        foreach (var kvp in entriesDict)
        {
            if (bgmSource.clip == kvp.Value.audioClip) return kvp.Key;
        }
        return null;
    }

    public void Play(string id, float volumeMultiplier = 1f, float startOffset = 0f)
    {
        if (entriesDict.TryGetValue(id, out SoundEntry entry))
        {
            float globalSfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float finalVolume = entry.volume * globalSfxVol * volumeMultiplier;
            sfxSource.PlayOneShot(entry.audioClip, finalVolume);
        }
        else
        {
            Debug.LogWarning($"Sound with id '{id}' not found.");
        }
    }

    public void PlayBGM(string id)
    {
        if (entriesDict.TryGetValue(id, out SoundEntry entry))
        {
            if (bgmSource.clip == entry.audioClip && bgmSource.isPlaying)
                return;

            bgmSource.clip = entry.audioClip;
            float globalMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            bgmSource.volume = entry.volume * globalMusicVol;
            bgmSource.Play();
        }
    }

    public void StopBGM() => bgmSource.Stop();
}