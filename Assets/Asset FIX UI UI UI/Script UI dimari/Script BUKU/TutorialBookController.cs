using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Pastikan aset DOTween sudah terinstall
using System.Collections;

public class TutorialBookController : MonoBehaviour
{
    [Header("Referensi BUKU")]
    public GameObject bukuTutorialGO;
    public RectTransform bookRootTransform;
    public CanvasGroup bookCanvasGroup;
    public CanvasGroup darkPanelGroup;

    [Header("Navigasi UI")]
    public CanvasGroup nextButtonGroup;
    public CanvasGroup prevButtonGroup;
    public CanvasGroup spaceContinueGroup;
    public CanvasGroup xButtonGroup; // Referensi tombol X

    [Header("Tombol Visual")]
    public Image nextButtonImage;
    public Image prevButtonImage;
    // Jika ada image untuk tombol X visual, tambahkan di sini:
    // public Image xButtonImage; 

    public Color normalButtonColor = Color.white;
    public Color pressedButtonColor = Color.gray;

    [Header("Dependensi Aset Buku")]
    // Pastikan skrip 'Book' dan 'AutoFlip' ada di project kamu
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

    // State Internal
    public int currentIndex = 1;
    private Tween blinkTween;
    private Tween xBlinkTween;
    private System.Action onComplete;
    private string originatingSceneName;
    public bool canClick = true;
    private float clickCooldown = 1f;

    void Update()
    {
        // Jika buku tidak aktif, jangan jalankan logika input
        if (bukuTutorialGO == null || !bukuTutorialGO.activeSelf) return;

        // Navigasi Next (D)
        if (Input.GetKeyDown(KeyCode.D) && currentIndex < 4)
        {
            AnimateButtonPress(nextButtonImage);
            OnClickNext();
        }

        // Navigasi Prev (A)
        if (Input.GetKeyDown(KeyCode.A) && currentIndex > 1)
        {
            AnimateButtonPress(prevButtonImage);
            OnClickPrevious();
        }

        // Tutup Buku dengan SPACE (Hanya jika sudah di halaman akhir)
        if (Input.GetKeyDown(KeyCode.Space) && currentIndex >= 4)
        {
            StopBlinking();
            HideBook();
        }

        // Tutup Buku dengan X atau ESC (Bisa kapan saja)
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            // Jika kamu punya referensi image tombol X, bisa animasi disini:
            // AnimateButtonPress(xButtonImage); 

            StopBlinking();
            HideBook();
        }
    }

    public void StartTutorial(System.Action onFinish)
    {
        onComplete = onFinish;
        originatingSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Setup awal visibilitas
        if (bukuTutorialGO != null) bukuTutorialGO.SetActive(true);

        SetAlpha(bookCanvasGroup, 0);
        SetAlpha(darkPanelGroup, 0);
        SetAlpha(spaceContinueGroup, 0);
        SetAlpha(xButtonGroup, 0);

        if (bookRootTransform != null)
        {
            bookRootTransform.anchoredPosition = startPos;
            bookRootTransform.localScale = startScale;
        }

        // Setup tombol navigasi
        SetAlpha(nextButtonGroup, 0);
        SetAlpha(prevButtonGroup, 0);
        SetGroupInteractable(nextButtonGroup, true);
        SetGroupInteractable(prevButtonGroup, true);

        // Reset Halaman Buku (Membutuhkan Script Book.cs)
        if (book != null)
        {
            book.currentPage = 1;
            book.UpdateSprites();
        }

        currentIndex = 1;

        ShowBook();
    }

    void ShowBook()
    {
        if (darkPanelGroup != null) darkPanelGroup.DOFade(0.5f, fadeDuration);
        if (bookCanvasGroup != null) bookCanvasGroup.DOFade(1f, fadeDuration);

        // Munculkan tombol X
        if (xButtonGroup != null) xButtonGroup.DOFade(1f, fadeDuration);

        if (bookRootTransform != null)
        {
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
    }

    public void HideBook()
    {
        if (darkPanelGroup != null) darkPanelGroup.DOFade(0f, fadeDuration).SetUpdate(true);
        if (bookCanvasGroup != null) bookCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);
        if (xButtonGroup != null) xButtonGroup.DOFade(0f, fadeDuration).SetUpdate(true);

        if (bookRootTransform != null)
        {
            bookRootTransform.DOAnchorPos(startPos, moveDuration).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    if (bukuTutorialGO != null) bukuTutorialGO.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }

    // --- LOGIKA NAVIGASI ---

    public void OnClickNext()
    {
        if (nextButtonGroup != null && nextButtonGroup.alpha < 1f) return;
        if (!canClick) return;

        if (currentIndex < 4)
        {
            currentIndex++;
            if (autoFlip != null) autoFlip.FlipRightPage();
            UpdateNavigationButtons();
            UpdateSpecialElements();
            StartCoroutine(ClickCooldownRoutine());
        }
    }

    public void OnClickPrevious()
    {
        if (prevButtonGroup != null && prevButtonGroup.alpha < 1f) return;
        if (!canClick) return;

        if (currentIndex > 1)
        {
            currentIndex--;
            if (autoFlip != null) autoFlip.FlipLeftPage();
            UpdateNavigationButtons();
            UpdateSpecialElements();
            StartCoroutine(ClickCooldownRoutine());
        }
    }

    private IEnumerator ClickCooldownRoutine()
    {
        canClick = false;
        yield return new WaitForSeconds(clickCooldown);
        canClick = true;
    }

    void UpdateNavigationButtons()
    {
        bool showNext = currentIndex >= 1 && currentIndex <= 3;
        if (nextButtonGroup != null)
        {
            nextButtonGroup.DOFade(showNext ? 1f : 0f, 0.3f);
            SetGroupInteractable(nextButtonGroup, true);
        }

        bool showPrev = currentIndex >= 2 && currentIndex <= 5;
        if (prevButtonGroup != null)
        {
            prevButtonGroup.DOFade(showPrev ? 1f : 0f, 0.3f);
            SetGroupInteractable(prevButtonGroup, true);
        }
    }

    void UpdateSpecialElements()
    {
        // Tombol Space (Continue) hanya muncul di akhir
        if (currentIndex >= 4 && currentIndex <= 5)
        {
            if (spaceContinueGroup != null) spaceContinueGroup.DOFade(1f, 0.5f);
            StartBlinking();
        }
        else
        {
            if (spaceContinueGroup != null) spaceContinueGroup.DOFade(0f, 0.3f);
            StopBlinking();
        }
    }

    // --- ANIMASI ---

    void StartBlinking()
    {
        if (blinkTween != null) blinkTween.Kill();
        if (spaceContinueGroup != null)
        {
            blinkTween = spaceContinueGroup.DOFade(0.2f, spaceBlinkSpeed).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
    }

    public void StopBlinking()
    {
        if (blinkTween != null) blinkTween.Kill();
        blinkTween = null;

        if (spaceContinueGroup != null) spaceContinueGroup.alpha = 0;

        if (xBlinkTween != null) xBlinkTween.Kill();
        xBlinkTween = null;
    }

    void AnimateButtonPress(Image buttonImg)
    {
        if (buttonImg == null) return;
        buttonImg.color = pressedButtonColor;
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (buttonImg != null) buttonImg.color = normalButtonColor;
        });
    }

    // --- UTILITIES HELPER (Untuk mencegah error Null Reference) ---

    void SetAlpha(CanvasGroup cg, float val)
    {
        if (cg != null) cg.alpha = val;
    }

    void SetGroupInteractable(CanvasGroup cg, bool val)
    {
        if (cg != null)
        {
            cg.interactable = val;
            cg.blocksRaycasts = val;
        }
    }
}