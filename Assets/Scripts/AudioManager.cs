using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip coinClip;
    public AudioClip hitClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayCoin()
    {
        if (sfxSource != null && coinClip != null) sfxSource.PlayOneShot(coinClip);
    }

    public void PlayHit()
    {
        if (sfxSource != null && hitClip != null) sfxSource.PlayOneShot(hitClip);
    }
}
