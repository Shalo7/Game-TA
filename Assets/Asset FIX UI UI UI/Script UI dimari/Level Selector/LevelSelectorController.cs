using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using System.Collections;
using AudioData;

public class LevelSelectorController : MonoBehaviour
{
    // ... (Semua variabel tetap sama) ...
    public static LevelSelectorController instance;
    [SerializeField] AudioManager audioManager;
    [Header("Stages")]
    public RectTransform[] stagePositions;
    public CanvasGroup[] roomNameGroups;
    [Header("Indicator")]
    public RectTransform levelIndicator;
    public CanvasGroup indicatorCanvasGroup;
    public Vector3 indicatorOffset;
    public float indicatorFadeDuration = 0.3f;
    public float indicatorRotationSpeed = 30f;
    [Header("Room Name Animation")]
    public float roomFadeDuration = 0.5f;
    public float roomScaleDuration = 0.5f;
    public float roomMoveYAmount = 30f;
    [Header("Settings Panel")]
    public CanvasGroup optionsPanel;
    public float panelAnimDuration = 0.3f;
    public Button[] optionsButtons;
    public RectTransform selectorPointer;
    public float pointerOffsetX = -60f;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;
    [Header("Settings Icon")]
    public Image settingsIcon;
    public Color settingsNormalColor = Color.white;
    public Color settingsSelectedColor = Color.green;
    [Header("Back Arrow")]
    public Image backArrowIcon;
    public Color backNormalColor = Color.white;
    public Color backSelectedColor = Color.red;
    [Header("Tutorial System")]
    public TutorialBookController tutorialBook;
    [Header("Visual Feedback")]
    public float feedbackDuration = 0.25f;
    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public float sliderStep = 0.05f;
    private int currentIndex = 0;
    private int optionsIndex = 0;
    private bool isChanging = false;
    private bool inOptions = false;
    private bool inputLocked = false;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (instance != this && instance != null) return;
        instance = this;
    }

    void Start()
    {
        UpdateUI(true);
        if (optionsPanel != null)
        {
            optionsPanel.gameObject.SetActive(true);
            optionsPanel.alpha = 0f;
            optionsPanel.interactable = false;
            optionsPanel.blocksRaycasts = false;
        }
        settingsIcon.color = settingsNormalColor;
        backArrowIcon.color = backNormalColor;
    }

    void Update()
    {
        if (inputLocked) return;
        if (inOptions)
        {
            HandleOptionsInput();
        }
        else
        {
            HandleStageInput();
        }
    }

    #region === Stage Navigation ===

    // --- PERBAIKAN STRUKTUR INPUT ---
    void HandleStageInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
            AnimateIndicatorTransition();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            currentIndex = Mathf.Min(stagePositions.Length - 1, currentIndex + 1);
            AnimateIndicatorTransition();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectLevel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowVisualFeedback(settingsIcon, settingsSelectedColor, settingsNormalColor);
            OpenOptionsPanel();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowVisualFeedback(backArrowIcon, backSelectedColor, backNormalColor);
            ReturnToMainMenu();
        }
    }

    void ShowVisualFeedback(Image icon, Color highlightColor, Color returnColor)
    {
        icon.color = highlightColor;
        DOVirtual.DelayedCall(feedbackDuration, () =>
        {
            if (icon != null) icon.color = returnColor;
        });
    }

    void AnimateIndicatorTransition()
    {
        isChanging = true;
        indicatorCanvasGroup.DOFade(0, indicatorFadeDuration / 2).OnComplete(() =>
        {
            levelIndicator.position = stagePositions[currentIndex].position + indicatorOffset;
            indicatorCanvasGroup.DOFade(1, indicatorFadeDuration / 2).OnComplete(() => { isChanging = false; });
        });
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[1]);
    }

    void UpdateUI(bool initial = false)
    {
        for (int i = 0; i < roomNameGroups.Length; i++)
        {
            bool isActive = (i == currentIndex);
            if (isActive) AnimateRoomName(roomNameGroups[i], initial);
            else
            {
                roomNameGroups[i].DOFade(0, 0.2f);
                roomNameGroups[i].transform.localScale = Vector3.one;
            }
        }
        if (initial)
            levelIndicator.position = stagePositions[currentIndex].position + indicatorOffset;
    }

    void AnimateRoomName(CanvasGroup cg, bool immediate = false)
    {
        cg.alpha = 0;
        RectTransform rt = cg.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;
        rt.anchoredPosition = originalPos - new Vector2(0, roomMoveYAmount);
        rt.localScale = Vector3.one * 0.85f;
        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1, roomFadeDuration));
        seq.Join(rt.DOAnchorPos(originalPos, roomFadeDuration).SetEase(Ease.OutCubic));
        seq.Join(rt.DOScale(1f, roomScaleDuration).SetEase(Ease.OutBack));
    }

    // --- PERBAIKAN LOGIKA TRANSISI ---
    void SelectLevel()
    {
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[0]);
        int currentUnlockedLevel = Director.instance != null ? Director.instance.GetCurrentLevel() : 3;

        string sceneToLoad = "";

        if (currentIndex == 0 && currentUnlockedLevel >= 0)
        {
            sceneToLoad = "MainBattle";
        }
        else if (currentIndex == 1 && currentUnlockedLevel >= 1)
        {
            sceneToLoad = "MainBattle_TechArt1";
        }
        else if (currentIndex == 2 && currentUnlockedLevel >= 2)
        {
            sceneToLoad = "MainBattle_TechArt2";
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (Director.instance != null)
            {
                Director.instance.DoTransition(SceneTransitionPairingsEnum.STP_RIGHT2LEFT, sceneToLoad);
            }
            else
            {
                // Fallback jika Director tidak ada
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            Debug.Log($"Stage {currentIndex + 1} belum tersedia.");
        }
    }

    // --- PERBAIKAN LOGIKA TRANSISI ---
    void ReturnToMainMenu()
    {
        if (Director.instance != null)
        {
            Director.instance.DoTransition(SceneTransitionPairingsEnum.STP_LEFT2RIGHT, "MainMenu");
        }
        else
        {
            // Fallback jika Director tidak ada
            SceneManager.LoadScene("MainMenu");
        }
    }
    #endregion

    #region === Options Panel (Tidak Perlu Diubah) ===
    void OpenOptionsPanel()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        inOptions = true;
        optionsIndex = 0;
        HighlightOptions();
        MovePointer();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsButtons[optionsIndex].gameObject);

        _fadeCoroutine = StartCoroutine(FadePanel(true));
    }

    void CloseOptionsPanel()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);
        _fadeCoroutine = StartCoroutine(FadePanel(false));
    }

    IEnumerator FadePanel(bool fadeIn)
    {
        inputLocked = true;
        if (fadeIn)
        {
            optionsPanel.interactable = true;
            optionsPanel.blocksRaycasts = true;
        }
        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = optionsPanel.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < panelAnimDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / panelAnimDuration);
            optionsPanel.alpha = newAlpha;
            yield return null;
        }
        optionsPanel.alpha = targetAlpha;
        if (!fadeIn)
        {
            optionsPanel.interactable = false;
            optionsPanel.blocksRaycasts = false;
            inOptions = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        inputLocked = false;
        _fadeCoroutine = null;
    }

    void HandleOptionsInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseOptionsPanel();
            return;
        }
        bool moved = false;
        if (Input.GetKeyDown(KeyCode.W))
        {
            optionsIndex = (optionsIndex - 1 + optionsButtons.Length) % optionsButtons.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            optionsIndex = (optionsIndex + 1) % optionsButtons.Length;
            moved = true;
        }
        if (moved)
        {
            HighlightOptions();
            MovePointer();
            if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[3]);
            EventSystem.current.SetSelectedGameObject(optionsButtons[optionsIndex].gameObject);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmOptionsMenu();
        }
        HandleSliderAdjustment();
    }

    void HandleSliderAdjustment()
    {
        GameObject current = optionsButtons[optionsIndex].gameObject;
        string currentName = current.name.ToLower();
        if (currentName.Contains("music") && musicSlider != null)
        {
            if (Input.GetKeyDown(KeyCode.D)) musicSlider.value = Mathf.Clamp01(musicSlider.value + sliderStep);
            else if (Input.GetKeyDown(KeyCode.A)) musicSlider.value = Mathf.Clamp01(musicSlider.value - sliderStep);
        }
        else if (currentName.Contains("sfx") && sfxSlider != null)
        {
            if (Input.GetKeyDown(KeyCode.D)) sfxSlider.value = Mathf.Clamp01(sfxSlider.value + sliderStep);
            else if (Input.GetKeyDown(KeyCode.A)) sfxSlider.value = Mathf.Clamp01(sfxSlider.value - sliderStep);
        }
    }

    void HighlightOptions()
    {
        for (int i = 0; i < optionsButtons.Length; i++)
        {
            var img = optionsButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == optionsIndex) ? selectedColor : normalColor;
        }
    }

    void MovePointer()
    {
        if (selectorPointer == null || optionsButtons[optionsIndex] == null) return;
        RectTransform target = optionsButtons[optionsIndex].GetComponent<RectTransform>();
        Vector2 newPos = new Vector2(pointerOffsetX, target.anchoredPosition.y);
        selectorPointer.anchoredPosition = newPos;
    }

    void ConfirmOptionsMenu()
    {
        inputLocked = true;
        SetConfirmed(optionsButtons[optionsIndex]);
        GameObject current = optionsButtons[optionsIndex].gameObject;
        string name = current.name.ToLower();
        if (name.Contains("music") || name.Contains("sfx"))
        {
            inputLocked = false;
            return;
        }
        if (optionsIndex == 2)
        {
            optionsPanel.alpha = 0f;
            if (tutorialBook != null)
            {
                tutorialBook.gameObject.SetActive(true);
                if (tutorialBook.bukuTutorialGO != null) tutorialBook.bukuTutorialGO.SetActive(true);
                tutorialBook.StartTutorial(() => {
                    inOptions = false;
                    inputLocked = false;
                    optionsIndex = 0;
                    HighlightOptions();
                    MovePointer();
                });
            }
            else
            {
                Debug.LogWarning("TutorialBook reference is missing!");
                inputLocked = false;
            }
        }
        else if (optionsIndex == 3)
        {
            CloseOptionsPanel();
        }
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);
    }

    void SetConfirmed(Button btn)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = confirmedColor;
    }
    #endregion
}