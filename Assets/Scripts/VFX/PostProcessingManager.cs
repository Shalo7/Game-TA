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
    private Vignette vignette;
    private Color vignetteColor;

    private LensDistortion lensDistortion;

    void Start()
    {
        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet(out lensDistortion);
    }

    public void SetVignette(float val)
    {
        if (vignette == null) return;
        vignette.intensity.value = val;
    }
    public void SetVignetteColor(Color color)
    {
        if (vignette == null) {Debug.LogError("No vignette!"); return;}
        vignette.color.value = color;
    }
    public void ActivateVignette(bool val)
    {
        if (vignette == null) { Debug.LogError("No vignette!"); return;}
        vignette.active = val;
    }


    public void ActivateLensDistortion(bool val)
    {
        if (lensDistortion == null) return;
        lensDistortion.active = val;
    }
    public void SetLensDistortion(float val)
    {
        if (lensDistortion == null) return;
        lensDistortion.intensity.value = val;
    }
}
