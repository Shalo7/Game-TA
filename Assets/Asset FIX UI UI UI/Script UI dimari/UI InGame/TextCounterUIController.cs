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
    [SerializeField] AnimationCurve repositionCenterCurve;
    [SerializeField] Vector3 baseScale;
    Vector3 currentScale;
    [SerializeField] RectTransform basePos;
    [SerializeField] RectTransform middlePos;
    private bool isActive = true;
    public bool GetActiveStatus() => isActive;
    public void SetActive(bool val) { isActive = val; }


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
        currentScale = rectTrans.localScale;
        StartTextAnimation = null;
    }


    public void UpdateTextCounter(int counter, Color c)
    {
        if (!isActive) return;
        if (counter >= 0)
        { counterText.text = counter.ToString(); counterText.color = c; }

        if (StartTextAnimation != null) return;
        StartTextAnimation = StartCoroutine(DoStartTextAnimation());
    }

    public void EmptyText()
    {
        counterText.text = "";
        rectTrans.localScale = baseScale;
        currentScale = rectTrans.localScale;
        //rectTrans.localPosition = basePos.localPosition;
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

            rectTrans.localScale = currentScale * flt_CurveEval;
            yield return null;
        }
        currentScale = rectTrans.localScale;
        StartTextAnimation = null;
    }

    public void TextCenterReposition()
    {
        if (StartMovingToCenter != null) return;
        //StartMovingToCenter = StartCoroutine(DoStartMovingToCenter());
    }

    Coroutine StartMovingToCenter;
    IEnumerator DoStartMovingToCenter()
    {
        var flt_Time = 0f;
        var flt_MaxTime = repositionCenterCurve[repositionCenterCurve.length - 1].time;
        var rt_StartPos = rectTrans.localPosition;
        while (flt_Time <= flt_MaxTime)
        {
            flt_Time += Time.deltaTime;
            var flt_T = flt_Time / flt_MaxTime;
            var flt_CurveEval = resizeCounterCurve.Evaluate(flt_T);
            rectTrans.localPosition = Vector3.Lerp(rt_StartPos, middlePos.localPosition, flt_CurveEval);
            yield return null;
        }
        rectTrans.localPosition = middlePos.localPosition;
        StartMovingToCenter = null;
    }
}
