using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

public class LevelSelectorController : MonoBehaviour
{
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
    public GameObject optionsPanel;
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

    void Start()
    {
        StartRotatingIndicator();
        UpdateUI(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
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

    void HandleStageInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
            AnimateIndicatorTransition();
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = Mathf.Min(stagePositions.Length - 1, currentIndex + 1);
            AnimateIndicatorTransition();
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowVisualFeedback(settingsIcon, settingsSelectedColor, settingsNormalColor);
            OpenOptionsPanel();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
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

    void StartRotatingIndicator()
    {
        levelIndicator.DORotate(new Vector3(0, 0, -360), 360f / indicatorRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    void AnimateIndicatorTransition()
    {
        isChanging = true;
        indicatorCanvasGroup.DOFade(0, indicatorFadeDuration / 2).OnComplete(() =>
        {
            levelIndicator.position = stagePositions[currentIndex].position + indicatorOffset;
            indicatorCanvasGroup.DOFade(1, indicatorFadeDuration / 2).OnComplete(() =>
            {
                isChanging = false;
            });
        });
    }

    void UpdateUI(bool initial = false)
    {
        for (int i = 0; i < roomNameGroups.Length; i++)
        {
            bool isActive = (i == currentIndex);
            if (isActive)
                AnimateRoomName(roomNameGroups[i], initial);
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

    void SelectLevel()
    {
        if (currentIndex == 0)
        {
            SceneManager.LoadScene("InGame UI");
        }
        else
        {
            Debug.Log($"Stage {currentIndex + 1} belum tersedia.");
        }
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    #endregion

    #region === Options Panel ===

    void OpenOptionsPanel()
    {
        inOptions = true;
        optionsPanel.SetActive(true);
        optionsIndex = 0;
        HighlightOptions();
        MovePointer();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsButtons[optionsIndex].gameObject);
    }

    void CloseOptionsPanel()
    {
        inOptions = false;
        optionsPanel.SetActive(false);
        inputLocked = false;
    }

    void HandleOptionsInput()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            optionsIndex = (optionsIndex - 1 + optionsButtons.Length) % optionsButtons.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            optionsIndex = (optionsIndex + 1) % optionsButtons.Length;
            moved = true;
        }

        if (moved)
        {
            HighlightOptions();
            MovePointer();
            EventSystem.current.SetSelectedGameObject(null);
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
            if (Input.GetKeyDown(KeyCode.RightArrow))
                musicSlider.value = Mathf.Clamp01(musicSlider.value + sliderStep);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                musicSlider.value = Mathf.Clamp01(musicSlider.value - sliderStep);
        }
        else if (currentName.Contains("sfx") && sfxSlider != null)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                sfxSlider.value = Mathf.Clamp01(sfxSlider.value + sliderStep);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                sfxSlider.value = Mathf.Clamp01(sfxSlider.value - sliderStep);
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

        if (optionsIndex == 2) // Tutorial
        {
            optionsPanel.SetActive(false);
            if (tutorialBook != null)
            {
                if (!tutorialBook.gameObject.activeSelf)
                    tutorialBook.gameObject.SetActive(true);

                if (tutorialBook.bukuTutorialGO != null && !tutorialBook.bukuTutorialGO.activeSelf)
                    tutorialBook.bukuTutorialGO.SetActive(true);

                tutorialBook.StartTutorial(() =>
                {
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
        else if (optionsIndex == 3) // Back
        {
            CloseOptionsPanel();
        }
    }

    void SetConfirmed(Button btn)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = confirmedColor;
    }

    #endregion
}
