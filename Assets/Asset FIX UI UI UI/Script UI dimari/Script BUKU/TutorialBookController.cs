using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialBookController : MonoBehaviour
{
    [Header("Referensi BUKU")]
    public GameObject bukuTutorialGO;
    public RectTransform bookRootTransform;
    public CanvasGroup bookCanvasGroup;
    public CanvasGroup darkPanelGroup;
    public CanvasGroup nextButtonGroup;
    public CanvasGroup prevButtonGroup;
    public CanvasGroup spaceContinueGroup;

    public Image nextButtonImage;
    public Image prevButtonImage;
    public Color normalButtonColor = Color.white;
    public Color pressedButtonColor = Color.gray;

    public Book book;
    public AutoFlip autoFlip;

    [Header("Animasi")]
    public Vector2 startPos;
    public Vector2 targetPos;
    public float moveDuration = 0.5f;
    public float fadeDuration = 0.5f;
    public Vector3 startScale = Vector3.one * 0.9f;
    public Vector3 targetScale = Vector3.one;
    public float spaceBlinkSpeed = 1.5f;

    private int currentIndex = 1; // Start dari element 1
    private Tween blinkTween;
    private System.Action onComplete;
    private string originatingSceneName;

    void Update()
    {
        if (!bukuTutorialGO.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.D) && currentIndex < 5)
        {
            AnimateButtonPress(nextButtonImage);
            OnClickNext();
        }

        if (Input.GetKeyDown(KeyCode.A) && currentIndex > 1)
        {
            AnimateButtonPress(prevButtonImage);
            OnClickPrevious();
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentIndex >= 4)
        {
            StopBlinking();
            HideBook();
        }
    }

    public void StartTutorial(System.Action onFinish)
    {
        onComplete = onFinish;
        originatingSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Setup awal
        bukuTutorialGO.SetActive(true);
        bookCanvasGroup.alpha = 0;
        darkPanelGroup.alpha = 0;
        spaceContinueGroup.alpha = 0;

        bookRootTransform.anchoredPosition = startPos;
        bookRootTransform.localScale = startScale;

        nextButtonGroup.alpha = 0;
        prevButtonGroup.alpha = 0;
        nextButtonGroup.interactable = false;
        prevButtonGroup.interactable = false;
        nextButtonGroup.blocksRaycasts = false;
        prevButtonGroup.blocksRaycasts = false;

        book.currentPage = 1;
        currentIndex = 1;

        ShowBook();
    }

    void ShowBook()
    {
        darkPanelGroup.DOFade(0.5f, fadeDuration);
        bookCanvasGroup.DOFade(1f, fadeDuration);

        bookRootTransform.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                bookRootTransform.DOScale(targetScale, 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    UpdateNavigationButtons();
                    UpdateSpecialElements();
                });
            });
    }

    void HideBook()
    {
        darkPanelGroup.DOFade(0f, fadeDuration).SetUpdate(true);
        bookCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);

        bookRootTransform.DOAnchorPos(startPos, moveDuration).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() =>
            {
                bukuTutorialGO.SetActive(false);

                // ❌ Hapus Scene Reset, kita tidak perlu ini:
                // UnityEngine.SceneManagement.SceneManager.LoadScene(originatingSceneName);

                // ✅ Cukup panggil callback
                onComplete?.Invoke();
            });
    }

    public void OnClickNext()
    {
        if (currentIndex < 5)
        {
            currentIndex++;
            autoFlip.FlipRightPage();
            UpdateNavigationButtons();
            UpdateSpecialElements();
        }
    }

    public void OnClickPrevious()
    {
        if (currentIndex > 1)
        {
            currentIndex--;
            autoFlip.FlipLeftPage();
            UpdateNavigationButtons();
            UpdateSpecialElements();
        }
    }

    void UpdateNavigationButtons()
    {
        bool showNext = currentIndex >= 1 && currentIndex <= 3;
        nextButtonGroup.DOFade(showNext ? 1f : 0f, 0.3f);
        nextButtonGroup.interactable = false;
        nextButtonGroup.blocksRaycasts = false;

        bool showPrev = currentIndex >= 2 && currentIndex <= 5;
        prevButtonGroup.DOFade(showPrev ? 1f : 0f, 0.3f);
        prevButtonGroup.interactable = false;
        prevButtonGroup.blocksRaycasts = false;
    }

    void UpdateSpecialElements()
    {
        if (currentIndex >= 4 && currentIndex <= 5)
        {
            spaceContinueGroup.DOFade(1f, 0.5f);
            StartBlinking();
        }
        else
        {
            spaceContinueGroup.DOFade(0f, 0.3f);
            StopBlinking();
        }
    }

    void StartBlinking()
    {
        if (blinkTween != null) blinkTween.Kill();
        blinkTween = spaceContinueGroup.DOFade(0.2f, spaceBlinkSpeed).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    void StopBlinking()
    {
        if (blinkTween != null) blinkTween.Kill();
        blinkTween = null;
        spaceContinueGroup.alpha = 0;
    }

    void AnimateButtonPress(Image buttonImg)
    {
        if (buttonImg == null) return;
        buttonImg.color = pressedButtonColor;
        DOVirtual.DelayedCall(0.2f, () => buttonImg.color = normalButtonColor);
    }
}
