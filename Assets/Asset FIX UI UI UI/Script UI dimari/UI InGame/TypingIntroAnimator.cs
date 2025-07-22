using UnityEngine;
using TMPro;
using DG.Tweening;
using ParticleData.SpawnData;

public class TypingIntroAnimator : MonoBehaviour
{
    public CanvasGroup panelGroup;
    public TMP_Text textLets;
    public TMP_Text textBegin;
    public float stompDuration = 0.6f;
    public Vector3 fromScale = Vector3.one * 2f;
    public Vector3 toScale = Vector3.one;
    public float delayBetweenTexts = 0.5f;

    public System.Action onComplete;

    public void PlayIntro()
    {
        gameObject.SetActive(true);
        panelGroup.alpha = 1;
        textLets.transform.localScale = fromScale;
        textBegin.transform.localScale = fromScale;
        textLets.gameObject.SetActive(true);
        textBegin.gameObject.SetActive(false);

        textLets.transform.DOScale(toScale, stompDuration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            textBegin.gameObject.SetActive(true);
            textBegin.transform.DOScale(toScale, stompDuration).SetEase(Ease.InExpo).SetDelay(delayBetweenTexts).OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    panelGroup.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
                });
            });
        });
    }

    private void ExecuteParticleEffects(ParticleSpawnData data)
    {
        if (ParticlePoolManager.instance == null) return;
        ParticlePoolManager.instance.ActivateParticleFX(data);
    }
}
