using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class WinNLosePanel : MonoBehaviour
{
    [Header("Black Panel")]
    public Image blackPanel;
    public float blackFadeDuration = 1f;
    public Vector3 blackStartScale = Vector3.one;
    public Vector3 blackTargetScale = Vector3.one * 1.2f;
    [Range(0f, 1f)]
    public float blackTargetAlpha = 0.6f;  // <--- adjustable alpha

    [Header("Text Foreground (TMP)")]
    public TextMeshProUGUI tmpFront;
    public float tmpFrontFadeDuration = 1f;

    [Header("Space To Continue")]
    public CanvasGroup spaceContinue;
    public float delayBeforeSpaceContinue = 3f;
    public float fadeSpaceContinueDuration = 1f;

    [Header("Debug / Preview")]
    public bool autoPreviewOnStart = false;

    private bool canContinue = false;

    void Start()
    {
        ResetPanel();

#if UNITY_EDITOR
        if (autoPreviewOnStart)
        {
            ShowPanel();
        }
#endif
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        ResetPanel();

        // BLACK PANEL fade in + scale
        blackPanel.transform.localScale = blackStartScale;
        blackPanel.color = new Color(0, 0, 0, 0);
        blackPanel.DOFade(blackTargetAlpha, blackFadeDuration);
        blackPanel.transform.DOScale(blackTargetScale, blackFadeDuration).SetEase(Ease.OutQuad);

        // TMP FRONT: fade in (langsung tanpa delay)
        tmpFront.alpha = 0;
        tmpFront.DOFade(1f, tmpFrontFadeDuration);

        // SPACE TO CONTINUE
        spaceContinue.alpha = 0;
        spaceContinue.gameObject.SetActive(false);
        DOVirtual.DelayedCall(delayBeforeSpaceContinue, () =>
        {
            spaceContinue.gameObject.SetActive(true);
            spaceContinue.DOFade(1f, fadeSpaceContinueDuration);
            canContinue = true;
        });
    }

    void Update()
    {
        if (canContinue && Input.GetKeyDown(KeyCode.Space))
        {
            HidePanel();
        }
    }

    public void HidePanel()
    {
        canContinue = false;
        DOTween.KillAll();
        gameObject.SetActive(false);
    }

    private void ResetPanel()
    {
        blackPanel.color = new Color(0, 0, 0, 0);
        blackPanel.transform.localScale = blackStartScale;

        tmpFront.alpha = 0;

        spaceContinue.alpha = 0;
        spaceContinue.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    [ContextMenu("🔁 Preview Panel (Editor Only)")]
    private void EditorPreview()
    {
        ShowPanel();
    }
#endif
}
