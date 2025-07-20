using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;

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

    void Start()
    {
        pausePanel.SetActive(false);
        pausePanelGroup.alpha = 0f;

        if (tutorialCanvasGroup != null)
            tutorialCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (!isReadingBook)
        {
            if (suppressNextTab && Input.GetKeyDown(KeyCode.Tab))
            {
                suppressNextTab = false;
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (isPaused)
                    ResumeGame();
                else
                    OpenPauseMenu();
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

    void OpenPauseMenu()
    {
        isPaused = true;
        inputLocked = false;

        pausePanel.SetActive(true);
        pausePanelGroup.DOFade(1f, pauseFadeDuration).SetUpdate(true);

        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = false;

        optionIndex = 0;
        HighlightButtons(optionButtons, optionIndex);
        MovePointer(optionButtons[optionIndex].GetComponent<RectTransform>());
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionButtons[optionIndex].gameObject);
    }

    void ResumeGame()
    {
        pausePanelGroup.DOFade(0f, pauseFadeDuration).SetUpdate(true).OnComplete(() =>
        {
            pausePanel.SetActive(false);
        });

        isPaused = false;
        inputLocked = false;

        foreach (var script in scriptsToDisable)
            if (script != null) script.enabled = true;
    }

    #endregion

    #region === Navigation & Selection ===

    void HandleNavigation()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            optionIndex = (optionIndex - 1 + optionButtons.Length) % optionButtons.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
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
                StartCoroutine(QuitWithFade());
                break;
        }
    }

    #endregion

    #region === Tutorial Integration ===

    void OpenTutorialBook()
    {
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
