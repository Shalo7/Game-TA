using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
            AnimateIndicatorTransition();
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.D))
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
    }

    void CloseOptionsPanel()
    {
        inOptions = false;
        optionsPanel.SetActive(false);
        inputLocked = false;
    }

    void HandleOptionsInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            optionsIndex = (optionsIndex - 1 + optionsButtons.Length) % optionsButtons.Length;
            HighlightOptions();
            MovePointer();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            optionsIndex = (optionsIndex + 1) % optionsButtons.Length;
            HighlightOptions();
            MovePointer();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmOptionsMenu();
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

        switch (optionsIndex)
        {
            case 0: // Tutorial
                optionsPanel.SetActive(false);
                tutorialBook.StartTutorial(() =>
                {
                    inOptions = false;
                    inputLocked = false;
                    optionsIndex = 0;
                    HighlightOptions();
                    MovePointer();
                });
                break;

            case 1: // Back
                CloseOptionsPanel();
                break;
        }
    }

    void SetConfirmed(Button btn)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = confirmedColor;
    }

    #endregion
}
