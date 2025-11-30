using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EntityStatEffectHolder : MonoBehaviour
{
    RectTransform rectTrans;
    Vector3 baseScale;
    Vector3 currentScale;
    [SerializeField] AnimationCurve resizeAnimCurve;
    [SerializeField] Image imgComp;
    public Image GetImageComponent() => imgComp;
    [SerializeField] Moves currentMove;
    public Moves GetCurrentMove() => currentMove;

    void Start()
    {
        currentMove = new Moves();
        imgComp = GetComponent<Image>();
        rectTrans = transform?.GetComponent<RectTransform>();
        currentScale = rectTrans.localScale;
    }

    public void UpdateStatEffect(Moves type, Sprite statEffectIMG, bool isXpired)
    {
        if (imgComp == null) return;
        if (statEffectIMG == imgComp.sprite) return;
        //Debug.LogError("Can continue!");
        if (isXpired)
        {
            imgComp.sprite = null;
            imgComp.color = new Color(imgComp.color.r, imgComp.color.g, imgComp.color.b, 0f);
            currentMove.affectedStat = StatType.None;
        }
        else
        {
            imgComp.sprite = statEffectIMG;
            imgComp.color = new Color(imgComp.color.r, imgComp.color.g, imgComp.color.b, 1f);
            currentMove.affectedStat = type.affectedStat;
            if (CO_ResizeUI != null) return;
            CO_ResizeUI = StartCoroutine(StartResizeUI());
        }

    }

    Coroutine CO_ResizeUI;
    IEnumerator StartResizeUI()
    {
        float timer = 0f;
        float maxTimer = resizeAnimCurve[resizeAnimCurve.length - 1].time;
        while (timer <= maxTimer)
        {
            timer += Time.deltaTime;
            var flt_T = timer/maxTimer;
            var flt_CurveEval = resizeAnimCurve.Evaluate(flt_T);
            rectTrans.localScale = currentScale * flt_CurveEval;
            yield return null;
        }
        currentScale = rectTrans.localScale;
        CO_ResizeUI = null;
    }
}
