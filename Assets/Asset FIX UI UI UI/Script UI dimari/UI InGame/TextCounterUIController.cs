using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TextCounterUIController : MonoBehaviour
{
    [SerializeField] RectTransform rectTrans;
    [SerializeField] TMP_Text counterText;

    [Header("Animation Setting")]
    [SerializeField] AnimationCurve resizeCounterCurve;
    [SerializeField] Vector3 baseScale;
    [SerializeField] Gradient currentColorGradient;

    void OnEnable()
    {
        if (rectTrans == null)
        {
            rectTrans = TryGetComponent(out RectTransform rt) ? rt : null;
            if (rectTrans == null) { Debug.LogError("No rect transform!"); return; }
        }
        if (counterText == null)
        {
            counterText = TryGetComponent(out TMP_Text tmpT) ? tmpT : null;
            if (counterText == null) { Debug.LogError("No Counter Text!"); return; }
        }
        baseScale = rectTrans.localScale;
        StartTextAnimation = null;
    }


    public void UpdateTextCounter(int counter, Color c)
    {
        if (counter >= 0)
        { counterText.text = counter.ToString(); counterText.color = c; }

        if (StartTextAnimation != null) return;
        StartTextAnimation = StartCoroutine(DoStartTextAnimation());
    }

    public void EmptyText()
    {
        counterText.text = "";
    }


    Coroutine StartTextAnimation;
    IEnumerator DoStartTextAnimation()
    {
        var flt_Time = 0f;
        var flt_MaxCounter = resizeCounterCurve[resizeCounterCurve.length - 1].time;

        while (flt_Time < flt_MaxCounter)
        {
            flt_Time += Time.deltaTime;
            var flt_t = flt_Time / flt_MaxCounter;
            var flt_CurveEval = resizeCounterCurve.Evaluate(flt_t);

            rectTrans.localScale = baseScale * flt_CurveEval;
            yield return null;
        }
        rectTrans.localScale = baseScale;
        StartTextAnimation = null;
    }
}
