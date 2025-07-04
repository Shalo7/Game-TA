using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class TypingWordTimer : MonoBehaviour
{
    [Header("Timer Components")]
    public Slider timerSlider;
    public CanvasGroup redFlashPanel;

    [Header("Timer Settings")]
    [Tooltip("Durasi maksimal waktu mengetik per kata")]
    public float maxTimePerWord = 10f;

    [Tooltip("Efek detak jantung saat mengetik")]
    public float heartbeatPulseScale = 1.05f;
    public float heartbeatSpeed = 0.4f;

    [Header("Critical Time Settings")]
    [Tooltip("Waktu tersisa yang memicu mode kritis")]
    public float criticalThreshold = 3f;

    public Color color1 = new Color(1, 0, 0, 0.2f);
    public Color color2 = new Color(1, 0, 0, 0.4f);
    public float blinkSpeed = 0.5f;

    private float currentTime;
    private bool isRunning = false;
    private bool isCritical = false;
    private Sequence heartbeatSeq;
    private Coroutine blinkRoutine;

    public System.Action OnTimerTimeout;

    void Start()
    {
        ResetTimer();
        Hide(); // ⛔ Timer tersembunyi saat start
    }

    public void StartTimer()
    {
        ResetTimer();
        isRunning = true;
        Show();

        heartbeatSeq = DOTween.Sequence().SetLoops(-1, LoopType.Yoyo);
        heartbeatSeq.Append(timerSlider.transform.DOScale(heartbeatPulseScale, heartbeatSpeed).SetEase(Ease.InOutSine));
        heartbeatSeq.Append(timerSlider.transform.DOScale(1f, heartbeatSpeed).SetEase(Ease.InOutSine));
    }

    public void StopTimer()
    {
        isRunning = false;

        heartbeatSeq?.Kill();
        heartbeatSeq = null;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        redFlashPanel.alpha = 0;
        isCritical = false;
        Hide();
    }

    public void ResetTimer()
    {
        currentTime = maxTimePerWord;
        timerSlider.maxValue = maxTimePerWord;
        timerSlider.value = maxTimePerWord;

        redFlashPanel.alpha = 0; // Pastikan tidak langsung muncul
        isCritical = false;
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        timerSlider.value = currentTime;

        if (!isCritical && currentTime <= criticalThreshold)
        {
            isCritical = true;
            blinkRoutine = StartCoroutine(FlashRedPanel());
        }

        if (currentTime <= 0)
        {
            StopTimer();
            if (OnTimerTimeout != null)
                OnTimerTimeout.Invoke();
        }
    }

    IEnumerator FlashRedPanel()
    {
        while (true)
        {
            redFlashPanel.DOFade(color1.a, blinkSpeed).SetEase(Ease.Linear);
            yield return new WaitForSeconds(blinkSpeed);
            redFlashPanel.DOFade(color2.a, blinkSpeed).SetEase(Ease.Linear);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    public void Show()
    {
        timerSlider.gameObject.SetActive(true);
        redFlashPanel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        timerSlider.gameObject.SetActive(false);
        redFlashPanel.gameObject.SetActive(false);
    }
}
