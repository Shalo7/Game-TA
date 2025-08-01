using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--------Audio Sources--------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("--------Audio Clips--------")]
    public AudioClip background;

    public List<AudioClip> sfxClips = new List<AudioClip>();

    private void Awake()
    {
        // Singleton & Persist
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        musicSource.clip = background;
        musicSource.Play();
        if (!musicSource.isPlaying) {Debug.Log("Not Played");}
    }

    public void StopBGM()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        SFXSource.volume = volume;
    }
}
