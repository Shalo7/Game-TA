using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using AudioData;
using TMPro;
using System.Collections;
using DG.Tweening;
using System;

public class MainMenuController : MonoBehaviour
{
    // ... (Semua variabel tetap sama) ...
    [Header("Main Menu")]
    public Button[] mainButtons;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;
    [Header("Typing Mechanic")]
    public GameObject typingContainer;
    public TextMeshProUGUI ghostText;
    public TextMeshProUGUI typedText;
    public TextMeshProUGUI typeToPlayText;
    public string targetWord = "play";
    public Color typingErrorColor = Color.red;
    public Color ghostTextIdleColor = Color.grey;
    [Tooltip("Durasi animasi fade untuk teks 'Type to Play'")]
    public float typeToPlayFadeDuration = 0.5f;
    [Tooltip("Jeda abis typo salah ketik sebelum reset otomatis.")]
    public float typoResetDelay = 1f;
    private string currentTypedWord = "";
    private Color defaultTypedTextColor;
    private Coroutine _errorResetCoroutine;
    private Tween _ghostTextFadeTween;
    private Tween _typeToPlayBlinkTween;
    [Header("Options")]
    public CanvasGroup optionsPanel;
    public float optionsPanelFadeDuration = 0.3f;
    public Button[] optionsButtons;
    private Coroutine _optionsFadeCoroutine;
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
    public UIButtonEventComms buttonEventComms;

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

        if (typeToPlayText != null)
        {
            typeToPlayText.alpha = 0f;
            _typeToPlayBlinkTween = typeToPlayText.DOFade(0.5f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).Pause();
        }

        buttonEventComms.OnClickButton += OnClickButton;
        buttonEventComms.OnHoveredButton += OnHoverButton;
        buttonEventComms.OnExitButton += OnExitHoverButton;

        HighlightButtons(mainButtons, mainIndex);
        MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
        EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
    }


    private void OnExitHoverButton(Button button)
    {
        
    }

    private void OnHoverButton(Button button)
    {
        if (!inOptions)
        {
            for (int i = 0; i < mainButtons.Length; i++)
            {
                if (mainButtons[i] == button)
                {
                    mainIndex = i;
                    HighlightButtons(mainButtons, mainIndex);
                    break;
                }

            }
        }
        else
        {
            for (int i = 0; i < optionsButtons.Length; i++)
            {
                if (optionsButtons[i] == button)
                {
                    optionsIndex = i;
                    HighlightButtons(optionsButtons, optionsIndex);
                    break;
                }
            }

        }
    }
    private void OnClickButton(Button obj)
    {

        if(!inOptions)
        {
            if (mainIndex < mainButtons.Length)
            {
                if (mainButtons[mainIndex] != obj) return;
                ConfirmMainMenu(); 
            }
        }
        else
        {
            if (optionsIndex < optionsButtons.Length)
            {
                if (optionsButtons[optionsIndex] != obj) return;
                ConfirmOptionsMenu();
            }
            
        }
    }

    void Update()
    {
        if (inputLocked) return;
        if (!inOptions)
        {
            HandleNavigation(mainButtons, ref mainIndex);
            if (mainIndex == 0) { HandleTypingInput(); }
            else { if (Input.GetKeyDown(KeyCode.Space)) { ConfirmMainMenu(); } }
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

    #region === Typing Mechanic (Tidak Berubah) ===
    void HandleTypingInput() { if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.S)) { string a = Input.inputString.ToLower(); if (a.Length > 0) { char b = a[0]; if (char.IsLetter(b)) { if (_errorResetCoroutine != null) { StopCoroutine(_errorResetCoroutine); _errorResetCoroutine = null; inputLocked = false; } ProcessTypedChar(b); } } } }
    void ProcessTypedChar(char a) { if (typedText.color == typingErrorColor) { ResetTyping(); } currentTypedWord += a; typedText.text = currentTypedWord; if (targetWord.StartsWith(currentTypedWord)) { if (currentTypedWord.Length == targetWord.Length) { StartCoroutine(OnTypingSuccess()); } } else { OnTypingError(); } }
    void OnTypingError() { if (_errorResetCoroutine == null) { _errorResetCoroutine = StartCoroutine(ErrorResetSequence()); } }
    IEnumerator ErrorResetSequence() { inputLocked = true; typedText.color = typingErrorColor; yield return new WaitForSeconds(typoResetDelay); ResetTyping(); inputLocked = false; _errorResetCoroutine = null; }
    IEnumerator OnTypingSuccess() { inputLocked = true; SetConfirmed(mainButtons[0]); if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]); yield return new WaitForSeconds(0.5f); if (Director.instance != null) { Director.instance.DoTransition(SceneTransitionPairingsEnum.STP_RIGHT2LEFT, "LevelSelector"); } else { SceneManager.LoadScene("LevelSelector"); } }
    void ResetTyping() { currentTypedWord = ""; if (typedText != null) { typedText.text = ""; typedText.color = defaultTypedTextColor; } }
    #endregion

    // --- FUNGSI INI DIUBAH SECARA SIGNIFIKAN ---
    void HighlightButtons(Button[] buttons, int selectedIndex)
    {
        bool isMainMenu = (buttons == mainButtons);

        // Atur visibilitas elemen typing secara individual
        if (typedText != null) typedText.gameObject.SetActive(isMainMenu);
        if (typeToPlayText != null) typeToPlayText.gameObject.SetActive(isMainMenu);

        // Warnai tombol yang dipilih
        for (int i = 0; i < buttons.Length; i++)
        {
            var img = buttons[i].GetComponent<Image>();
            if (img != null) { img.color = (i == selectedIndex) ? selectedColor : normalColor; }
        }

        // Kelola animasi teks HANYA jika ghostText ada
        if (ghostText != null)
        {
            if (isMainMenu)
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
                    ghostText.color = ghostTextIdleColor;
                    ghostText.text = "play";
                    _ghostTextFadeTween.Play();
                }

                // Kelola animasi typeToPlayText
                if (typeToPlayText != null)
                {
                    typeToPlayText.DOKill();
                    _typeToPlayBlinkTween.Pause();
                    if (isPlayButtonSelected)
                    {
                        typeToPlayText.DOFade(1f, typeToPlayFadeDuration).SetEase(Ease.OutQuad).OnComplete(() => _typeToPlayBlinkTween.Play());
                    }
                    else
                    {
                        typeToPlayText.DOFade(0f, typeToPlayFadeDuration).SetEase(Ease.InQuad);
                    }
                }
            }
            else // Jika kita di menu Opsi
            {
                // Pastikan ghostText tetap berkedip di latar belakang
                ghostText.color = ghostTextIdleColor;
                _ghostTextFadeTween.Play();
            }
        }
    }

    void SetConfirmed(Button button) { var img = button.GetComponent<Image>(); if (img != null) img.color = confirmedColor; }
    void MovePointer(RectTransform target) { if (selectorPointer == null || target == null) return; Vector2 newPos = new Vector2(pointerOffsetX, target.anchoredPosition.y); selectorPointer.anchoredPosition = newPos; }

    void ConfirmMainMenu()
    {
        if (mainIndex == 0) return;
        inputLocked = true;
        SetConfirmed(mainButtons[mainIndex]);
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);
        switch (mainIndex)
        {
            case 1:
                if (_optionsFadeCoroutine != null) StopCoroutine(_optionsFadeCoroutine);
                _optionsFadeCoroutine = StartCoroutine(FadeOptionsPanel(true));
                break;
            case 2:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    #region === Options Panel (Fokus kembali sudah benar) ===

    IEnumerator FadeOptionsPanel(bool fadeIn)
    {
        inputLocked = true;
        if (fadeIn)
        {
            inOptions = true;
            optionsIndex = 0;
            optionsPanel.interactable = true;
            optionsPanel.blocksRaycasts = true;
            HighlightButtons(optionsButtons, optionsIndex);
            MovePointer(optionsButtons[optionsIndex].GetComponent<RectTransform>());
            EventSystem.current.SetSelectedGameObject(optionsButtons[optionsIndex].gameObject);
        }
        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = optionsPanel.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < optionsPanelFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            optionsPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / optionsPanelFadeDuration);
            yield return null;
        }
        optionsPanel.alpha = targetAlpha;
        if (!fadeIn)
        {
            optionsPanel.interactable = false;
            optionsPanel.blocksRaycasts = false;
            inOptions = false;
            HighlightButtons(mainButtons, mainIndex);
            MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
            EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
        }
        inputLocked = false;
        _optionsFadeCoroutine = null;
    }

    void HandleSliderAdjustment() { GameObject a = optionsButtons[optionsIndex].gameObject; string b = a.name.ToLower(); if (b.Contains("music") && musicSlider != null) { if (Input.GetKeyDown(KeyCode.D)) musicSlider.value = Mathf.Clamp01(musicSlider.value + sliderStep); else if (Input.GetKeyDown(KeyCode.A)) musicSlider.value = Mathf.Clamp01(musicSlider.value - sliderStep); } else if (b.Contains("sfx") && sfxSlider != null) { if (Input.GetKeyDown(KeyCode.D)) sfxSlider.value = Mathf.Clamp01(sfxSlider.value + sliderStep); else if (Input.GetKeyDown(KeyCode.A)) sfxSlider.value = Mathf.Clamp01(sfxSlider.value - sliderStep); } }

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
                    optionsPanel.alpha = 0f;
                    optionsPanel.interactable = false;
                    optionsPanel.blocksRaycasts = false;
                    tutorialBook.gameObject.SetActive(true);
                    if (tutorialBook.bukuTutorialGO != null) tutorialBook.bukuTutorialGO.SetActive(true);

                    tutorialBook.StartTutorial(() =>
                    {
                        inOptions = false;
                        inputLocked = false;
                        HighlightButtons(mainButtons, mainIndex);
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
                if (_optionsFadeCoroutine != null) StopCoroutine(_optionsFadeCoroutine);
                _optionsFadeCoroutine = StartCoroutine(FadeOptionsPanel(false));
                break;
            default:
                inputLocked = false;
                break;
        }
    }
    #endregion
}