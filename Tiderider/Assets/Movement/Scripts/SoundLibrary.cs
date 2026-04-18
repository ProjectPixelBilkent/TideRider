using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SoundEntry
{
    public string id;
    public AudioClip audioClip;
}

public class SoundLibrary : MonoBehaviour
{
    [SerializeField] private SoundEntry[] entries;
    [SerializeField, Range(0f, 1f)] private float world1BgmMultiplier = 0.65f;
    [SerializeField, Range(0f, 1f)] private float world2BgmMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float world3BgmMultiplier = 0.1f;

    private Dictionary<string, AudioClip> entriesDict;
    private AudioSource bgmSource;
    private string currentBgmId;
    public static SoundLibrary Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        entriesDict = new Dictionary<string, AudioClip>();
        for(int i=0; i<entries.Length; i++)
        {
            entriesDict[entries[i].id] = entries[i].audioClip;
        }

        bgmSource = GetComponent<AudioSource>();
        if(bgmSource==null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = GetBgmVolumeMultiplier(null) * GetSavedMusicVolume();
    }

    public void Play(string id, float volumeMultiplier = 1f, float startOffset = 0f)
    {
        if (entriesDict == null)
        {
            Debug.LogWarning("SoundLibrary not initialized.");
            return;
        }

        if (entriesDict.TryGetValue(id, out AudioClip clip))
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found for sound playback.");
                return;
            }

            GameObject tempAudio = new GameObject($"One shot audio: {id}");
            tempAudio.transform.position = mainCamera.transform.position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Max(0f, volumeMultiplier) * PlayerPrefs.GetFloat("SFXVolume", 1f);
            source.spatialBlend = 0f;
            source.playOnAwake = false;

            float safeOffset = Mathf.Clamp(startOffset, 0f, Mathf.Max(0f, clip.length - 0.01f));
            source.time = safeOffset;
            source.Play();

            Destroy(tempAudio, Mathf.Max(0.1f, clip.length - safeOffset) + 0.1f);
        }
        else
        {
            Debug.LogWarning($"Sound with id '{id}' not found.");
        }
    }

    public void PlayBGM(string id)
    {
        if (entriesDict.TryGetValue(id, out AudioClip clip))
        {
            bgmSource.volume = GetBgmVolumeMultiplier(id) * GetSavedMusicVolume();

            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return; // already playing

            bgmSource.clip = clip;
            currentBgmId = id;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM with id '{id}' not found.");
        }
    }

    // ⏹ Stop background music
    public void StopBGM()
    {
        bgmSource.Stop();
        currentBgmId = null;
    }

    private float GetSavedMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    private float GetBgmVolumeMultiplier(string id)
    {
        return id switch
        {
            "world_1" => world1BgmMultiplier,
            "world_2" => world2BgmMultiplier,
            "world_3" => world3BgmMultiplier,
            _ => 1f
        };
    }

    private void OnValidate()
    {
        if (bgmSource == null || string.IsNullOrEmpty(currentBgmId))
            return;

        bgmSource.volume = GetBgmVolumeMultiplier(currentBgmId) * GetSavedMusicVolume();
    }
}
