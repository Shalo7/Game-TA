using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class InGameOptionsMenuController : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pausePanel;
    public CanvasGroup pausePanelGroup;
    public Button[] optionButtons;
    public RectTransform selectorPointer;
    public float pointerOffsetX = -60f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;

    [Header("Tutorial Book")]
    public TutorialBookController tutorialBook;
    public CanvasGroup tutorialCanvasGroup;

    [Header("Gameplay Scripts")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("Fade Durations")]
    public float pauseFadeDuration = 0.25f;
    public float quitFadeDuration = 0.5f;

    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public float sliderStep = 0.05f;

    private int optionIndex = 0;
    private bool isPaused = false;
    private bool inputLocked = false;
    private bool isReadingBook = false;
    private bool suppressNextTab = false;

    [SerializeField] UIButtonEventComms UIButtonEventComms;

    void Start()
    {
        pausePanel.SetActive(true);
        pausePanelGroup.alpha = 0f;

        if (tutorialCanvasGroup != null)
            tutorialCanvasGroup.alpha = 0f;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var img = optionButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = false;
                img.maskable = false;
            }
        }
    }

    private void OnHoveredButtonEvent(Button button)
    {
        Debug.LogError("Hovered Button: " + button.name);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == button)
            {
                optionIndex = i;
                HighlightButtons(optionButtons, optionIndex);
                break;
            }
            
        }
    }

    private void OnExitButtonEvent(Button button)
    {
     
    }

    private void OnClickButtonEvent(Button button)
    {
        Debug.LogError("Clicked Button: " + button.name);
        if (optionIndex < optionButtons.Length)
        {
            if (optionButtons[optionIndex] != button) return;
            ConfirmOption();
        }
    }

  
    void Update()
    {
        if (!isReadingBook)
        {
            if (suppressNextTab && Input.GetKeyDown(KeyCode.Escape))
            {
                suppressNextTab = false;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    OpenPauseMenu();
                }
            }
        }

        if (isPaused && !inputLocked && !isReadingBook)
        {
            HandleNavigation();

            if (Input.GetKeyDown(KeyCode.Space))
                ConfirmOption();
        }
    }

    #region === Pause Control ===

    public void EscPressed()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    void OpenPauseMenu()
    {
        isPaused = true;
        inputLocked = false;


        pausePanel.SetActive(true);
        pausePanelGroup.DOFade(1f, pauseFadeDuration).SetUpdate(true);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var img = optionButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                img.maskable = true;
            }
        }
        UIButtonEventComms.OnClickButton += OnClickButtonEvent;
        UIButtonEventComms.OnExitButton += OnExitButtonEvent;
        UIButtonEventComms.OnHoveredButton += OnHoveredButtonEvent;

        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = false;

        optionIndex = 0;
        HighlightButtons(optionButtons, optionIndex);
        MovePointer(optionButtons[optionIndex].GetComponent<RectTransform>());
        Time.timeScale = 0f;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionButtons[optionIndex].gameObject);
    }

    void ResumeGame()
    {
        pausePanelGroup.DOFade(0f, pauseFadeDuration).SetUpdate(true).OnComplete(() =>
        {
            //pausePanel.SetActive(false);
        });
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var img = optionButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = false;
                img.maskable = false;
            }
        }

        UIButtonEventComms.OnClickButton -= OnClickButtonEvent;
        UIButtonEventComms.OnExitButton -= OnExitButtonEvent;
        UIButtonEventComms.OnHoveredButton -= OnHoveredButtonEvent;

        isPaused = false;
        inputLocked = false;
        Time.timeScale = 1f;

        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = true;
    }

    #endregion

    #region === Navigation & Selection ===

    void HandleNavigation()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.W))
        {
            optionIndex = (optionIndex - 1 + optionButtons.Length) % optionButtons.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            optionIndex = (optionIndex + 1) % optionButtons.Length;
            moved = true;
        }

        if (moved)
        {
            HighlightButtons(optionButtons, optionIndex);
            MovePointer(optionButtons[optionIndex].GetComponent<RectTransform>());
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionButtons[optionIndex].gameObject);
        }

        HandleSliderAdjustment();
    }

    void HandleSliderAdjustment()
    {
        GameObject current = optionButtons[optionIndex].gameObject;
        string currentName = current.name.ToLower();

        if (currentName.Contains("music") && musicSlider != null)
        {
            if (Input.GetKeyDown(KeyCode.D))
                musicSlider.value = Mathf.Clamp01(musicSlider.value + sliderStep);
            else if (Input.GetKeyDown(KeyCode.A))
                musicSlider.value = Mathf.Clamp01(musicSlider.value - sliderStep);
        }
        else if (currentName.Contains("sfx") && sfxSlider != null)
        {
            if (Input.GetKeyDown(KeyCode.D))
                sfxSlider.value = Mathf.Clamp01(sfxSlider.value + sliderStep);
            else if (Input.GetKeyDown(KeyCode.A))
                sfxSlider.value = Mathf.Clamp01(sfxSlider.value - sliderStep);
        }
    }

    void HighlightButtons(Button[] buttons, int selectedIndex)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var img = buttons[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    void SetConfirmed(Button button)
    {
        var img = button.GetComponent<Image>();
        if (img != null) img.color = confirmedColor;
    }

    void MovePointer(RectTransform target)
    {
        if (selectorPointer == null || target == null) return;
        Vector2 newPos = new Vector2(pointerOffsetX, target.anchoredPosition.y);
        selectorPointer.anchoredPosition = newPos;
    }

    void ConfirmOption()
    {
        GameObject current = optionButtons[optionIndex].gameObject;
        string name = current.name.ToLower();

        if (name.Contains("music") || name.Contains("sfx"))
            return;

        inputLocked = true;
        SetConfirmed(optionButtons[optionIndex]);

        switch (optionIndex)
        {
            case 2:
                OpenTutorialBook();
                break;
            case 3:
                ResumeGame();
                break;
            case 4:
                Time.timeScale = 1f;
                if (Director.instance == null) { StartCoroutine(QuitWithFade()); return; }
                Director.instance.DoTransition(SceneTransitionPairingsEnum.STP_LEFT2RIGHT, "LevelSelector");
                break;
        }
    }

    #endregion

    #region === Tutorial Integration ===

    void OpenTutorialBook()
    {
        Time.timeScale = 1f;
        isReadingBook = true;
        pausePanel.SetActive(false);

        if (tutorialBook != null)
        {
            if (!tutorialBook.gameObject.activeSelf)
                tutorialBook.gameObject.SetActive(true);

            if (tutorialBook.bukuTutorialGO != null && !tutorialBook.bukuTutorialGO.activeSelf)
                tutorialBook.bukuTutorialGO.SetActive(true);
        }

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 1f;
            tutorialCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
            tutorialCanvasGroup.transform.localScale = Vector3.one * 0.85f;
            tutorialCanvasGroup.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        tutorialBook.StartTutorial(() =>
        {
            isReadingBook = false;
            inputLocked = false;
            suppressNextTab = true;

            pausePanel.SetActive(true);
            pausePanelGroup.alpha = 1f;

            HighlightButtons(optionButtons, optionIndex = 0);
            MovePointer(optionButtons[optionIndex].GetComponent<RectTransform>());
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionButtons[optionIndex].gameObject);
        });
    }

    #endregion

    #region === Quit Logic ===

    IEnumerator QuitWithFade()
    {
        pausePanelGroup.DOFade(0f, quitFadeDuration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(quitFadeDuration);

        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = false;

        if (SceneController.Instance != null)
            SceneController.Instance.NextLevel("LevelSelector");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelector");
    }

    #endregion
}
