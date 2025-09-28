using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip spookyClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private AudioSource audioSource;

    private bool hasPlayedSpooky = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource component missing on AudioManager.");
            return;
        }

        if (spookyClip != null)
        {
            audioSource.clip = spookyClip;
            audioSource.loop = false;
            audioSource.Play();
            hasPlayedSpooky = true;
        }
        else
        {
            PlayLoopAudio();
        }
    }

    void Update()
    {
        if (hasPlayedSpooky && !audioSource.isPlaying)
        {
            PlayLoopAudio();
            hasPlayedSpooky = false;
        }
    }

    private void PlayLoopAudio()
    {
        if (loopClip != null && audioSource != null)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
