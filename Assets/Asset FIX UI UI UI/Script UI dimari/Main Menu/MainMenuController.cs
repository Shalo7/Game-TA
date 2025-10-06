using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AudioData;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    public Button[] mainButtons;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;

    [Header("Options")]
    public CanvasGroup optionsPanel;
    public Button[] optionsButtons;

    [Header("Volume Sliders (embedded in button parents)")]
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
            if (Input.GetKeyDown(KeyCode.Space)) ConfirmMainMenu();
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
            if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(audioManager.sfxClips[1], false, Vector3.zero)); }

            // Set current selected object untuk EventSystem agar W/S terus bekerja
            EventSystem.current.SetSelectedGameObject(null); // optional reset
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }
    }

    void HandleSliderAdjustment()
    {
        GameObject current = optionsButtons[optionsIndex].gameObject;
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
        if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(audioManager.sfxClips[1], false, Vector3.zero)); }
        else {audioManager.PlaySFX(audioManager.sfxClips[1]);}
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
        inputLocked = true;
        SetConfirmed(mainButtons[mainIndex]);
        
        if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(audioManager.sfxClips[2], false, Vector3.zero)); }
        else { audioManager.PlaySFX(audioManager.sfxClips[2]); }

        switch (mainIndex)
        {
            case 0: // Play
                if (Director.instance == null) { SceneController.Instance?.NextLevel("LevelSelector"); return; }
                Director.instance?.DoTransition(SceneTransitionPairingsEnum.STP_RIGHT2LEFT, "LevelSelector");
                break;

            case 1: // Options
                inOptions = true;
                //optionsPanel.SetActive(true);
                optionsPanel.alpha = 1f;
                optionsIndex = 0;
                HighlightButtons(optionsButtons, optionsIndex);
                MovePointer(optionsButtons[optionsIndex].GetComponent<RectTransform>());

                // Reset EventSystem, pastikan tombol bisa dipilih pakai keyboard
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(optionsButtons[optionsIndex].gameObject);
                inputLocked = false;
                break;

            case 2: // Quit
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    void ConfirmOptionsMenu()
    {
        inputLocked = true;
        SetConfirmed(optionsButtons[optionsIndex]);

        switch (optionsIndex)
        {
            case 2: // Tutorial
                if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(audioManager.sfxClips[2], false, Vector3.zero)); }
                else {audioManager.PlaySFX(audioManager.sfxClips[2]);}

                // Pastikan GameObject TutorialBook aktif sebelum digunakan
                if (tutorialBook != null)
                {
                    if (!tutorialBook.gameObject.activeSelf)
                        tutorialBook.gameObject.SetActive(true);

                    // Jika ada GameObject khusus (seperti bukuTutorialGO), aktifkan juga
                    if (tutorialBook.bukuTutorialGO != null && !tutorialBook.bukuTutorialGO.activeSelf)
                        tutorialBook.bukuTutorialGO.SetActive(true);

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

            case 3: // Back
                if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(audioManager.sfxClips[2], false, Vector3.zero)); }
                else {audioManager.PlaySFX(audioManager.sfxClips[2]);}

                optionsPanel.alpha = 0f;
                inOptions = false;
                HighlightButtons(mainButtons, mainIndex = 0);
                MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                EventSystem.current.SetSelectedGameObject(mainButtons[mainIndex].gameObject);
                inputLocked = false;
                break;
        }
    }


}
