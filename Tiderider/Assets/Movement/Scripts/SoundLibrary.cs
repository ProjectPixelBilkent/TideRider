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

    private Dictionary<string, AudioClip> entriesDict;
    private AudioSource bgmSource;
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
    }

    public void Play(string id)
    {
        if (entriesDict == null)
        {
            Debug.LogWarning("SoundLibrary not initialized.");
            return;
        }

        if (entriesDict.TryGetValue(id, out AudioClip clip))
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
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
            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return; // already playing

            bgmSource.clip = clip;
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
    }
}
