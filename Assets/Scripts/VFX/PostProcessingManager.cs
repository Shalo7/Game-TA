using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager instance;
    void Awake()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
    }

    public Volume postProcessVolume;
    public Vignette vignette;
    public Color vignetteColor;

    void Start()
    {
        postProcessVolume.profile.TryGet(out vignette);
    }

    public void SetVignette(float val)
    {
        if (vignette == null) return;
        vignette.intensity.value = val;
        Debug.LogError("Intensified!");
    }
    public void SetVignetteColor(Color color)
    {
        if (vignette == null) return;
        vignette.color.value = color;
        Debug.LogError("Colored!");
    }
    public void ActivateVignette(bool val)
    {
        if (vignette == null) return;
        vignette.active = val;
    }
}
