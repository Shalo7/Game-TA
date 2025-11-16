using System.Collections;
using UnityEngine;

public class k_DebuffAbilityInstance : BaseAbilityInstance
{
    [SerializeField] AnimationCurve lensDistortionAnimCurve;
    [SerializeField] AnimationCurve vignetteAnimCurve;
    [SerializeField] Color vignetteColor;

    public override void ExecuteAbility()
    {
        if (PostProcessingManager.instance == null) return;
        if (CO_StartPostProEffect != null) return;
        PostProcessingManager.instance.ActivateLensDistortion(true);
        PostProcessingManager.instance.ActivateVignette(true);
        CO_StartPostProEffect = StartCoroutine(StartPostProEffect());
    }


    Coroutine CO_StartPostProEffect;
    IEnumerator StartPostProEffect()
    {
        var timer = 0f;
        var maxTimer = 0f;
        var vignetteTime = vignetteAnimCurve[vignetteAnimCurve.length - 1].time;
        var lensDistortTime = lensDistortionAnimCurve[lensDistortionAnimCurve.length - 1].time;
        if (lensDistortTime < vignetteTime || lensDistortTime > vignetteTime)
        {
            maxTimer = lensDistortTime + vignetteTime;
        }
        else if (lensDistortTime == vignetteTime) { maxTimer = lensDistortTime; }
        else { maxTimer = lensDistortTime > vignetteTime ? lensDistortTime : vignetteTime; }

        while (timer <= maxTimer)
        {
            var flt_tLens = timer / maxTimer;
            var flt_tVig = timer / maxTimer;
            var flt_EvalLens = lensDistortionAnimCurve.Evaluate(flt_tLens);
            var flt_EvalVig = vignetteAnimCurve.Evaluate(flt_tVig);

            PostProcessingManager.instance.SetLensDistortion(flt_EvalLens);
            PostProcessingManager.instance.SetVignette(flt_EvalVig);
            PostProcessingManager.instance.SetVignetteColor(vignetteColor);

            timer += Time.deltaTime;
            yield return null;
        }
        PostProcessingManager.instance.ActivateVignette(false);
        PostProcessingManager.instance.ActivateLensDistortion(false);
        CO_StartPostProEffect = null;
    }    
}
