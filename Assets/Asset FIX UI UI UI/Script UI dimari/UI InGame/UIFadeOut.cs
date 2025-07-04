using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeOut : MonoBehaviour
{
    public float fadeDuration = 2f;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void StartFadeOut(Action onComplete = null)
    {
        StartCoroutine(FadeOut(onComplete));
    }

    public void StartFadeIn(Action onComplete = null)
    {
        gameObject.SetActive(true);
        ResetFade(); // pastikan alpha = 1
        StartCoroutine(FadeIn(onComplete));
    }

    public void ResetFade()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1;
        gameObject.SetActive(true);
    }

    IEnumerator FadeOut(Action onComplete)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1 - t / fadeDuration;
            yield return null;
        }

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    IEnumerator FadeIn(Action onComplete)
    {
        float t = 0;
        canvasGroup.alpha = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }

        canvasGroup.alpha = 1;
        onComplete?.Invoke();
    }
}
