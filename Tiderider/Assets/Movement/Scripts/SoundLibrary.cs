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

    // Mapping IDs to the full entry so we can access volume easily
    private Dictionary<string, SoundEntry> entriesDict;
    private AudioSource bgmSource;
    private AudioSource sfxSource; // Dedicated source for SFX

    public static SoundLibrary Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        entriesDict = new Dictionary<string, SoundEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            entriesDict[entries[i].id] = entries[i];
        }

        // Setup BGM Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0; // 2D

        // Setup SFX Source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0;
    }

    public void Play(string id, float volumeMultiplier = 1f, float startOffset = 0f)
    {
        if (entriesDict.TryGetValue(id, out SoundEntry entry))
        {
            sfxSource.PlayOneShot(entry.audioClip, entry.volume);
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
            bgmSource.volume = entry.volume;
            bgmSource.Play();
        }
    }

    public void StopBGM() => bgmSource.Stop();
}