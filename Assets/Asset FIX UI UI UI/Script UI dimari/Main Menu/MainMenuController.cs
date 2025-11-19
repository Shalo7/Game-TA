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
    [Header("--- CORE UI REFERENCES ---")]
    [Tooltip("Masukkan semua tombol menu utama (Play, Options, Quit)")]
    public Button[] mainButtons;

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow; // Warna tombol saat di-hover (termasuk Play)
    public Color confirmedColor = Color.green;

    [Header("--- TYPING MECHANIC SETUP ---")]
    public GameObject typingContainer;
    public TextMeshProUGUI ghostText;
    public TextMeshProUGUI typedText;
    public TextMeshProUGUI typeToPlayText;

    [Header("Typing Visuals")]
    public string targetWord = "play";
    public Color typingErrorColor = Color.red;

    [Tooltip("Warna Ghost Text saat tombol Play TIDAK di-hover (Idle/Kedip)")]
    public Color ghostTextIdleColor = Color.grey;

    [Tooltip("Warna Ghost Text saat tombol Play DI-HOVER (Misal: Putih/Cyan - Biar kontras dengan tombol kuning)")]
    public Color ghostTextHoverColor = Color.white;

    [Header("Play Button Feedback")]
    public float shakeStrength = 10f;
    public float shakeDuration = 0.5f;

    [Header("Highlight Effects")]
    public Image typeToPlayHighlight;
    public Color highlightActiveColor = new Color(1, 1, 1, 0.5f);
    public float highlightFadeDuration = 0.3f;

    [Header("Timings")]
    public float typeToPlayFadeDuration = 0.5f;
    public float typoResetDelay = 1f;

    // --- Private State Variables ---
    private string currentTypedWord = "";
    private Color defaultTypedTextColor; // Menyimpan warna asli TypedText dari Inspector
    private Coroutine _errorResetCoroutine;

    // --- Tweens ---
    private Tween _ghostTextFadeTween;
    private Tween _typeToPlayBlinkTween;

    [Header("--- OPTIONS & AUDIO ---")]
    public CanvasGroup optionsPanel;
    public float optionsPanelFadeDuration = 0.3f;
    public Button[] optionsButtons;
    private Coroutine _optionsFadeCoroutine;

    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public float sliderStep = 0.05f;

    [Header("Selector & Tutorial")]
    public RectTransform selectorPointer;
    public float pointerOffsetX = -60f;
    public TutorialBookController tutorialBook;

    // Event System custom (Opsional, jika kamu pakai script terpisah)
    public UIButtonEventComms buttonEventComms;

    private int mainIndex = 0;
    private int optionsIndex = 0;
    private bool inOptions = false;
    private bool inputLocked = false;

    AudioManager audioManager;

    private void Awake()
    {
        // Mencari AudioManager (Pastikan tag 'Audio' ada di scene)
        GameObject audioObj = GameObject.FindWithTag("Audio");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();
    }

    void Start()
    {
        // 1. Setup Options Panel (Sembunyi di awal)
        optionsPanel.gameObject.SetActive(true);
        optionsPanel.alpha = 0f;
        optionsPanel.interactable = false;
        optionsPanel.blocksRaycasts = false;

        // 2. Setup Typed Text (Simpan warna asli dari Inspector)
        if (typedText != null)
        {
            // PENTING: Ambil warna dari inspector agar tidak transparan
            defaultTypedTextColor = typedText.color;
            typedText.text = "";
        }
        targetWord = targetWord.ToLower();

        // 3. Setup Ghost Text Animation (Idle Blink)
        if (ghostText != null)
        {
            _ghostTextFadeTween = ghostText.DOFade(0.3f, 1.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        // 4. Setup TypeToPlay Animation
        if (typeToPlayText != null)
        {
            typeToPlayText.alpha = 0f;
            _typeToPlayBlinkTween = typeToPlayText.DOFade(0.5f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .Pause();
        }

        // 5. Setup Highlight Image
        if (typeToPlayHighlight != null)
        {
            // Mulai transparan
            typeToPlayHighlight.color = new Color(highlightActiveColor.r, highlightActiveColor.g, highlightActiveColor.b, 0f);
        }

        // 6. Setup Custom Event System (Jika ada)
        if (buttonEventComms != null)
        {
            buttonEventComms.OnClickButton += OnClickButton;
            buttonEventComms.OnHoveredButton += OnHoverButton;
            buttonEventComms.OnExitButton += OnExitHoverButton;
        }

        // 7. Inisialisasi Seleksi Awal
        HighlightButtons(mainButtons, mainIndex);
        MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
        EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
    }

    // --- EVENT HANDLERS (Untuk Mouse Hover/Click) ---
    private void OnExitHoverButton(Button button) { }

    private void OnHoverButton(Button button)
    {
        if (inputLocked) return;

        if (!inOptions)
        {
            for (int i = 0; i < mainButtons.Length; i++)
            {
                if (mainButtons[i] == button)
                {
                    mainIndex = i;
                    HighlightButtons(mainButtons, mainIndex);
                    PlayHoverSound();
                    MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                    break;
                }
            }
            // Jika pindah ke tombol play, reset ketikan
            if (button == mainButtons[0]) ResetTyping();
        }
        else
        {
            for (int i = 0; i < optionsButtons.Length; i++)
            {
                if (optionsButtons[i] == button)
                {
                    optionsIndex = i;
                    HighlightButtons(optionsButtons, optionsIndex);
                    PlayHoverSound();
                    MovePointer(optionsButtons[optionsIndex].GetComponent<RectTransform>());
                    break;
                }
            }
        }
    }

    private void OnClickButton(Button obj)
    {
        if (inputLocked) return;

        if (!inOptions)
        {
            if (mainIndex < mainButtons.Length)
            {
                if (mainButtons[mainIndex] != obj) return;

                if (mainIndex == 0) TriggerPlayButtonError(); // Tombol Play -> Error Effect
                else ConfirmMainMenu(); // Tombol lain -> Confirm normal
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

    // --- MAIN UPDATE LOOP ---
    void Update()
    {
        if (inputLocked) return;

        if (!inOptions)
        {
            HandleNavigation(mainButtons, ref mainIndex);

            if (mainIndex == 0)
            {
                HandleTypingInput(); // Ketik "play"
                if (Input.GetKeyDown(KeyCode.Space)) TriggerPlayButtonError(); // Spasi = Error
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space)) ConfirmMainMenu(); // Spasi = Confirm
            }
        }
        else
        {
            HandleNavigation(optionsButtons, ref optionsIndex);
            HandleSliderAdjustment();
            if (Input.GetKeyDown(KeyCode.Space)) ConfirmOptionsMenu();
        }
    }

    // --- CORE NAVIGATION ---
    void HandleNavigation(Button[] buttons, ref int index)
    {
        bool moved = false;
        if (Input.GetKeyDown(KeyCode.W)) { index = (index - 1 + buttons.Length) % buttons.Length; moved = true; }
        else if (Input.GetKeyDown(KeyCode.S)) { index = (index + 1) % buttons.Length; moved = true; }

        if (moved)
        {
            HighlightButtons(buttons, index);
            MovePointer(buttons[index].GetComponent<RectTransform>());
            PlayHoverSound();
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
            if (buttons == mainButtons) ResetTyping();
        }
    }

    // --- VISUAL: HIGHLIGHT BUTTONS (Logika Utama Warna & Animasi) ---
    void HighlightButtons(Button[] buttons, int selectedIndex)
    {
        bool isMainMenu = (buttons == mainButtons);

        // 1. Atur Visibilitas Container Typing
        if (typingContainer != null) typingContainer.SetActive(isMainMenu);
        if (typedText != null) typedText.gameObject.SetActive(isMainMenu);
        if (typeToPlayText != null) typeToPlayText.gameObject.SetActive(isMainMenu);

        // 2. LOGIKA WARNA TOMBOL (BACKGROUND)
        for (int i = 0; i < buttons.Length; i++)
        {
            var img = buttons[i].GetComponent<Image>();
            if (img != null)
            {
                // PERBAIKAN: Semua tombol (termasuk Play) jadi kuning jika dipilih
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }

        // 3. LOGIKA WARNA & ANIMASI TEKS (Ghost, Typed, Highlight)
        if (isMainMenu)
        {
            bool isPlayButtonSelected = (selectedIndex == 0);

            // --- GHOST TEXT ---
            if (ghostText != null)
            {
                if (isPlayButtonSelected)
                {
                    _ghostTextFadeTween.Pause();
                    // Gunakan warna HOVER KHUSUS (misal Putih) agar terlihat di atas tombol Kuning
                    ghostText.color = ghostTextHoverColor;
                    ghostText.text = targetWord;
                }
                else
                {
                    ghostText.color = ghostTextIdleColor; // Kembali ke Abu-abu
                    ghostText.text = "play";
                    _ghostTextFadeTween.Play(); // Lanjut kedip-kedip
                }
            }

            // --- TYPE TO PLAY TEXT ---
            if (typeToPlayText != null)
            {
                typeToPlayText.DOKill();
                _typeToPlayBlinkTween.Pause();
                if (isPlayButtonSelected)
                {
                    typeToPlayText.DOFade(1f, typeToPlayFadeDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => _typeToPlayBlinkTween.Play());
                }
                else
                {
                    typeToPlayText.DOFade(0f, typeToPlayFadeDuration).SetEase(Ease.InQuad);
                }
            }

            // --- HIGHLIGHT GLOW IMAGE ---
            if (typeToPlayHighlight != null)
            {
                typeToPlayHighlight.DOKill();
                if (isPlayButtonSelected)
                {
                    typeToPlayHighlight.DOColor(highlightActiveColor, highlightFadeDuration);
                }
                else
                {
                    Color transparent = new Color(highlightActiveColor.r, highlightActiveColor.g, highlightActiveColor.b, 0f);
                    typeToPlayHighlight.DOColor(transparent, highlightFadeDuration);
                }
            }
        }
        else // SAAT DI MENU OPTIONS
        {
            // Tetap jalankan animasi idle di background
            if (ghostText != null)
            {
                ghostText.color = ghostTextIdleColor;
                _ghostTextFadeTween.Play();
            }
            // Matikan highlight
            if (typeToPlayHighlight != null)
            {
                typeToPlayHighlight.color = new Color(highlightActiveColor.r, highlightActiveColor.g, highlightActiveColor.b, 0f);
            }
        }
    }

    // --- ERROR EFFECT (Shake & Color) ---
    void TriggerPlayButtonError()
    {
        // Shake Tombol Play
        if (mainButtons.Length > 0)
        {
            Transform playBtnTransform = mainButtons[0].transform;
            playBtnTransform.DOKill(true);
            playBtnTransform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0, 0), 10, 90, false, true);
        }
        // Merahkan Ghost Text sejenak
        if (ghostText != null)
        {
            ghostText.DOKill();
            ghostText.color = typingErrorColor;
            ghostText.DOColor(ghostTextHoverColor, 0.5f).SetDelay(0.2f);
        }
    }

    // --- TYPING LOGIC ---
    #region === Typing Mechanic Implementation ===
    void HandleTypingInput()
    {
        // Pastikan input bukan tombol navigasi
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.Space))
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
        if (typedText.color == typingErrorColor) ResetTyping();

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

        // Shake effect on typo
        if (mainButtons.Length > 0) mainButtons[0].transform.DOShakePosition(shakeDuration * 0.5f, new Vector3(shakeStrength * 0.5f, 0, 0));

        yield return new WaitForSeconds(typoResetDelay);
        ResetTyping();
        inputLocked = false;
        _errorResetCoroutine = null;
    }

    IEnumerator OnTypingSuccess()
    {
        inputLocked = true;
        SetConfirmed(mainButtons[0]);
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]); // Success SFX

        yield return new WaitForSeconds(0.5f);

        // Logic pindah scene
        if (Director.instance != null) Director.instance.DoTransition(SceneTransitionPairingsEnum.STP_RIGHT2LEFT, "LevelSelector");
        else SceneManager.LoadScene("LevelSelector");
    }

    void ResetTyping()
    {
        currentTypedWord = "";
        if (typedText != null)
        {
            typedText.text = "";
            // PENTING: Kembalikan warna ke default (bukan merah error)
            typedText.color = defaultTypedTextColor;
        }
    }
    #endregion

    // --- CONFIRMATION & HELPERS ---
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

    void PlayHoverSound()
    {
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[1]);
    }

    void ConfirmMainMenu()
    {
        if (mainIndex == 0) return; // Play button handled elsewhere

        inputLocked = true;
        SetConfirmed(mainButtons[mainIndex]);
        if (audioManager != null) audioManager.PlaySFX(audioManager.sfxClips[2]);

        switch (mainIndex)
        {
            case 1: // Options
                if (_optionsFadeCoroutine != null) StopCoroutine(_optionsFadeCoroutine);
                _optionsFadeCoroutine = StartCoroutine(FadeOptionsPanel(true));
                break;
            case 2: // Quit
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    // --- OPTIONS PANEL LOGIC (COROUTINE) ---
    #region === Options Panel ===
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

            // KEMBALI DARI OPTIONS: Reset ke indeks tombol yang aktif (bisa Play atau Options)
            // Disini kita set kembali ke tombol Play (index 0) atau Options (index 1) tergantung desainmu
            // Sesuai request sebelumnya, kita kembalikan fokus ke Options (index 1) agar lebih natural
            HighlightButtons(mainButtons, mainIndex);
            MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
            EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
        }
        inputLocked = false;
        _optionsFadeCoroutine = null;
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

    void ConfirmOptionsMenu()
    {
        inputLocked = true;
        SetConfirmed(optionsButtons[optionsIndex]);
        switch (optionsIndex)
        {
            case 2: // Tutorial
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
                        // Kembali dari Tutorial
                        HighlightButtons(mainButtons, mainIndex);
                        MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                        EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
                    });
                }
                else inputLocked = false;
                break;
            case 3: // Back
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