using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using AudioData;
using TMPro;
using System.Collections;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    public Button[] mainButtons;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;

    [Header("Typing Mechanic")]
    public GameObject typingContainer;
    public TextMeshProUGUI ghostText;
    public TextMeshProUGUI typedText;
    public string targetWord = "play";
    public Color typingErrorColor = Color.red;
    public Color ghostTextIdleColor = Color.grey; // Warna ghostText saat tidak di-hover
    [Tooltip("Jeda abis typo salah ketik sebelum reset otomatis.")]
    public float typoResetDelay = 1f;
    private string currentTypedWord = "";
    private Color defaultTypedTextColor;
    private Coroutine _errorResetCoroutine;
    private Tween _ghostTextFadeTween;

    [Header("Options")]
    public CanvasGroup optionsPanel;
    public Button[] optionsButtons;

    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public float sliderStep = 0.05f;

    [Header("Selector")]
    public RectTransform selectorPointer;
    public float pointerOffsetX = -60f;

    [Header("Tutorial")]
    public TutorialBookController tutorialBook;

    private int mainIndex = 0;
    private int optionsIndex = 0;
    private bool inOptions = false;
    private bool inputLocked = false;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        optionsPanel.gameObject.SetActive(true);
        optionsPanel.alpha = 0f;
        optionsPanel.interactable = false;
        optionsPanel.blocksRaycasts = false;

        if (typedText != null) defaultTypedTextColor = typedText.color;
        targetWord = targetWord.ToLower();
        ResetTyping();

        if (ghostText != null)
        {
            _ghostTextFadeTween = ghostText.DOFade(0.3f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        HighlightButtons(mainButtons, mainIndex);
        MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
        EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
    }

    void Update()
    {
        if (inputLocked) return;

        if (!inOptions)
        {
            HandleNavigation(mainButtons, ref mainIndex);

            if (mainIndex == 0)
            {
                HandleTypingInput();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ConfirmMainMenu();
                }
            }
        }
        else
        {
            HandleNavigation(optionsButtons, ref optionsIndex);
            HandleSliderAdjustment();
            if (Input.GetKeyDown(KeyCode.Space)) ConfirmOptionsMenu();
        }
    }

    void HandleNavigation(Button[] buttons, ref int index)
    {
        bool moved = false;
        if (Input.GetKeyDown(KeyCode.W))
        {
            index = (index - 1 + buttons.Length) % buttons.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            index = (index + 1) % buttons.Length;
            moved = true;
        }

        if (moved)
        {
            HighlightButtons(buttons, index);
            MovePointer(buttons[index].GetComponent<RectTransform>());
            if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[1]);
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);

            if (buttons == mainButtons) ResetTyping();
        }
    }

    #region === Typing Mechanic Implementation ===

    void HandleTypingInput()
    {
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.S))
        {
            string inputString = Input.inputString.ToLower();
            if (inputString.Length > 0)
            {
                char pressedChar = inputString[0];
                if (char.IsLetter(pressedChar))
                {
                    if (_errorResetCoroutine != null)
                    {
                        StopCoroutine(_errorResetCoroutine);
                        _errorResetCoroutine = null;
                        inputLocked = false;
                    }
                    ProcessTypedChar(pressedChar);
                }
            }
        }
    }

    void ProcessTypedChar(char typedChar)
    {
        if (typedText.color == typingErrorColor)
        {
            ResetTyping();
        }

        currentTypedWord += typedChar;
        typedText.text = currentTypedWord;

        if (targetWord.StartsWith(currentTypedWord))
        {
            if (currentTypedWord.Length == targetWord.Length)
            {
                StartCoroutine(OnTypingSuccess());
            }
        }
        else
        {
            OnTypingError();
        }
    }

    void OnTypingError()
    {
        if (_errorResetCoroutine == null)
        {
            _errorResetCoroutine = StartCoroutine(ErrorResetSequence());
        }
    }

    IEnumerator ErrorResetSequence()
    {
        inputLocked = true;
        typedText.color = typingErrorColor;

        yield return new WaitForSeconds(typoResetDelay);

        ResetTyping();
        inputLocked = false;
        _errorResetCoroutine = null;
    }

    IEnumerator OnTypingSuccess()
    {
        inputLocked = true;
        SetConfirmed(mainButtons[0]);
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);

        yield return new WaitForSeconds(0.5f);

        if (Director.instance != null)
        {
            Director.instance.DoTransition(SceneTransitionPairingsEnum.STP_RIGHT2LEFT, "LevelSelector");
        }
        else
        {
            SceneManager.LoadScene("LevelSelector");
        }
    }

    void ResetTyping()
    {
        currentTypedWord = "";
        if (typedText != null)
        {
            typedText.text = "";
            typedText.color = defaultTypedTextColor;
        }
    }

    #endregion

    void HighlightButtons(Button[] buttons, int selectedIndex)
    {
        bool isMainMenu = (buttons == mainButtons);
        if (typingContainer != null) typingContainer.SetActive(isMainMenu);

        for (int i = 0; i < buttons.Length; i++)
        {
            var img = buttons[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }

        if (isMainMenu && ghostText != null)
        {
            bool isPlayButtonSelected = (selectedIndex == 0);
            if (isPlayButtonSelected)
            {
                _ghostTextFadeTween.Pause();
                ghostText.color = selectedColor;
                ghostText.text = targetWord;
            }
            else
            {
                // Menggunakan warna idle baru saat tidak di-hover
                ghostText.color = ghostTextIdleColor;
                ghostText.text = "play";
                _ghostTextFadeTween.Play();
            }
        }
    }

    void SetConfirmed(Button button)
    {
        var img = button.GetComponent<Image>();
        if (img != null)
            img.color = confirmedColor;
    }

    void MovePointer(RectTransform target)
    {
        if (selectorPointer == null || target == null) return;
        Vector2 newPos = new Vector2(pointerOffsetX, target.anchoredPosition.y);
        selectorPointer.anchoredPosition = newPos;
    }

    void ConfirmMainMenu()
    {
        if (mainIndex == 0) return;

        inputLocked = true;
        SetConfirmed(mainButtons[mainIndex]);
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);

        switch (mainIndex)
        {
            case 1:
                inOptions = true;
                optionsPanel.alpha = 1f;
                optionsPanel.interactable = true;
                optionsPanel.blocksRaycasts = true;
                optionsIndex = 0;
                HighlightButtons(optionsButtons, optionsIndex);
                MovePointer(optionsButtons[optionsIndex].GetComponent<RectTransform>());
                EventSystem.current.SetSelectedGameObject(optionsButtons[optionsIndex].gameObject);
                inputLocked = false;
                break;
            case 2:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    #region === Options Panel (Tidak diubah) ===

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

    void ConfirmOptionsMenu()
    {
        inputLocked = true;
        SetConfirmed(optionsButtons[optionsIndex]);
        switch (optionsIndex)
        {
            case 2:
                if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);
                if (tutorialBook != null)
                {
                    tutorialBook.gameObject.SetActive(true);
                    if (tutorialBook.bukuTutorialGO != null) tutorialBook.bukuTutorialGO.SetActive(true);
                    optionsPanel.alpha = 0f;
                    tutorialBook.StartTutorial(() =>
                    {
                        inOptions = false;
                        inputLocked = false;
                        HighlightButtons(mainButtons, mainIndex = 0);
                        MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                        EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
                    });
                }
                else
                {
                    Debug.LogWarning("TutorialBook reference is missing!");
                    inputLocked = false;
                }
                break;
            case 3:
                if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);
                optionsPanel.alpha = 0f;
                optionsPanel.interactable = false;
                optionsPanel.blocksRaycasts = false;
                inOptions = false;
                HighlightButtons(mainButtons, mainIndex = 0);
                MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
                inputLocked = false;
                break;
            default:
                inputLocked = false;
                break;
        }
    }
    #endregion
}