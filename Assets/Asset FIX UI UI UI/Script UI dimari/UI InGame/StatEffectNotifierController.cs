using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatEffectNotifierController : MonoBehaviour
{
    RectTransform rectTransform;
    public Image GetImage() => transform?.GetComponent<Image>();
    Coroutine CO_StartFlying;
    public Coroutine GetCO_StartFlying() => CO_StartFlying;
    [SerializeField] AnimationCurve notifSpeedAnimCurve;
    [SerializeField] Vector3 direction;

    void Start()
    {
        rectTransform = transform?.GetComponent<RectTransform>();
        GetImage().color = new Color(GetImage().color.r, GetImage().color.g, GetImage().color.b, 0f);
    }

    public void StartNotif(Sprite sprite, Vector3 position, Vector3 dir)
    {
        GetImage().sprite = sprite;
        direction = dir;
        GetImage().color = new Color(GetImage().color.r, GetImage().color.g, GetImage().color.b, 1f);
        rectTransform.anchoredPosition = position;
        CO_StartFlying = StartCoroutine(StartFlying());
    }

    IEnumerator StartFlying()
    {
        var timer = 0f;
        var maxTimer = notifSpeedAnimCurve[notifSpeedAnimCurve.length - 1].time;
        while (timer <= maxTimer)
        {
            var flt_t = timer / maxTimer;
            var flt_Scale = notifSpeedAnimCurve.Evaluate(flt_t);
            rectTransform.position += direction * flt_Scale * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        CO_StartFlying = null;
        GetImage().color = new Color(GetImage().color.r, GetImage().color.g, GetImage().color.b, 0f);
    }

}
