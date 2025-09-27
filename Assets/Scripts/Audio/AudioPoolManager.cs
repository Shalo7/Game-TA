using UnityEngine;
using AudioData;

public class AudioPoolManager : MonoBehaviour
{
    public static AudioPoolManager instance;
    public void Awake()
    {
        if (instance != null) { Destroy(this.gameObject); return; }
        instance = this;
    }


    public void RequestPlayAudio(AudioSpawnData spawnData)
    {
        foreach (Transform audSrcTransform in transform)
        {
            if (!audSrcTransform.TryGetComponent(out AudioSource aud)) continue;
            if (aud.isPlaying) continue;
            aud.Stop();
            aud.clip = spawnData.audClip;
            if (!spawnData.is3D) { aud.spatialBlend = 0; aud.Play(); return; }
            audSrcTransform.position = spawnData.position;
            aud.spatialBlend = 1f;
            aud.Play();
            return;
        }
    }
}
