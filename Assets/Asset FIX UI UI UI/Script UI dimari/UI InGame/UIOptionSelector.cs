using System.Collections.Generic;
using System.Linq;
using AudioData;
using UnityEngine;
using UnityEngine.UI;

public class UIOptionSelector : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] UIButtonEventComms buttonEventComms;
    [Header("Option Navigation")]
    public Button[] moveButtons;
    public RectTransform[] optionButtons;
    public RectTransform selectorPointer;
    public float pointerOffsetX = -30f;

    [Header("Visuals")]
    public Image[] optionImages;
    public Color selectedColor = Color.black;
    public Color normalColor = Color.white;
    public Color disabledColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("UI Faders")]
    public UIFadeOut fadeUIOptionsGroup;
    public UIFadeOut fadeTurnIndicator;
    public UIFadeOut fadePlayerInfo;
    public UIFadeOut fadeEnemyInfo;
    [SerializeField] UIFadeOut fadeStatEffects;

    [Header("Sound")]
    [SerializeField] AudioClip optSelect;
    [SerializeField] AudioClip optHover;

    private int currentIndex = 0;
    private bool selectionMade = false;
    public bool inputEnabled = false;
    private string[] availableOptions = new string[] { "Attack", "Defend", "Heal" };

    void Start()
    {
        UpdatePointer();
        ResetButtonColors();
        ChangedColorOnSelect();

        buttonEventComms.OnHoveredButton += OnHoveredButtonEvent;
        buttonEventComms.OnClickButton += OnClickButtonEvent;
    }


    void Update()
    {
        if (!inputEnabled || selectionMade || !gameObject.activeSelf || transform.gameObject.GetComponent<CanvasGroup>()?.alpha < 1f) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && inputEnabled)
        {
            string selectedOption = optionButtons[currentIndex].name.Replace("Button_", "");
            Button selectedBtn = moveButtons[currentIndex].GetComponent<Button>();
            if (availableOptions.Contains(selectedOption))
            {
                ConfirmSelection();
            }

            if (moveButtons != null && currentIndex >= 0 && currentIndex < moveButtons.Length)
            {
                if (selectedBtn != null && selectedBtn.interactable)
                {
                    selectedBtn.onClick.Invoke();
                }
            }
        }
    }


    private void OnHoveredButtonEvent(Button button)
    {
        if (!inputEnabled) return;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (button == moveButtons[i])
            {
                var dir = 0;
                string opt = optionButtons[i].name.Replace("Button_", "");
                if (!availableOptions.Contains(opt)) continue;
                if (i > currentIndex)
                {
                    dir = 1;
                    Debug.Log(dir);
                    MoveSelection(dir);
                }
                else if (i < currentIndex)
                {
                    dir = -1;
                    Debug.Log(dir);
                    MoveSelection(dir);
                }
                break;
            }
        }
    }

    private void OnClickButtonEvent(Button button)
    {
        if (!inputEnabled) return;
        if (button != moveButtons[currentIndex]) return;
        /*Image btnIMG = button.transform?.GetComponent<Image>();
        if (btnIMG.color.a < 1f) return;*/
        ConfirmSelection();
    }


    void MoveSelection(int direction)
    {
        int start = currentIndex;
        for (int i = 0; i < optionButtons.Length; i++)
        {
            currentIndex = (currentIndex + direction + optionButtons.Length) % optionButtons.Length;
            string opt = optionButtons[currentIndex].name.Replace("Button_", "");
            if (availableOptions.Contains(opt))
            {
                UpdatePointer();
                ChangedColorOnSelect();
                break;
            }
        }
        if (AudioPoolManager.instance == null || optHover == null) return;
        AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(optHover, false, Vector3.zero));
    }

    void ChangedColorOnSelect()
    {
        for (int i = 0; i < optionImages.Length; i++)
        {
            string key = optionButtons[i].name.Replace("Button_", "");
            if (i == currentIndex && availableOptions.Contains(key))
            {
                optionImages[i].color = selectedColor;
            }
            else if(availableOptions.Contains(key))
            {
                optionImages[i].color = normalColor;
            }
            else
            {
                optionImages[i].color = disabledColor;
            }
        }
    }

    void ConfirmSelection()
    {
        selectionMade = true;
        //ResetButtonColors();

        string opt = optionButtons[currentIndex].name.Replace("Button_", "");
        if (!availableOptions.Contains(opt)) return;

        for (int i = 0; i < optionImages.Length; i++)
        {
            if (i != currentIndex)
            {
                optionImages[i].color = new Color(optionImages[i].color.r, optionImages[i].color.g, optionImages[i].color.b, 0.15f);
            }
        }

        string choice = optionButtons[currentIndex].name.Replace("Button_", "");
        FindFirstObjectByType<TurnManager>()?.OnPlayerChoice(choice);

        fadeUIOptionsGroup?.StartFadeOut();
        fadeTurnIndicator?.StartFadeOut();
        fadePlayerInfo?.StartFadeOut();
        fadeEnemyInfo?.StartFadeOut();
        fadeStatEffects?.StartFadeOut();

        if (AudioPoolManager.instance == null || optSelect == null) return;
        AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(optSelect, false, Vector3.zero));
    }

    void UpdatePointer()
    {
        selectorPointer.position = optionButtons[currentIndex].position + Vector3.right * pointerOffsetX;
    }

    void ResetButtonColors()
    {
        for (int i = 0; i < optionImages.Length; i++)
        {
            string key = optionButtons[i].name.Replace("Button_", "");
            if (moveButtons.Length < i) continue;
            
            optionImages[i].color = availableOptions.Contains(key) ? normalColor : disabledColor;
            if (optionImages[i].color != disabledColor)
            { moveButtons[i].interactable = true; }
            else
            { moveButtons[i].interactable = false; }
        }
    }

    public void EnableSelection()
    {
        inputEnabled = true;
        selectionMade = false;

        // Temukan index opsi pertama yang tersedia
        for (int i = 0; i < optionButtons.Length; i++)
        {
            string opt = optionButtons[i].name.Replace("Button_", "");
            if (availableOptions.Contains(opt))
            {
                currentIndex = i;
                break;
            }
        }

        UpdatePointer();
        ResetButtonColors();
        ChangedColorOnSelect();
        gameObject.SetActive(true);

        fadeUIOptionsGroup?.ResetFade();
        fadeTurnIndicator?.ResetFade();
        fadePlayerInfo?.ResetFade();
        fadeEnemyInfo?.ResetFade();
        fadeStatEffects?.ResetFade();
    }

    public void DisableSelection()
    {
        inputEnabled = false;
    }

    public void SetAvailableOptions(string[] allowedOptions)
    {
        availableOptions = allowedOptions;
        ResetButtonColors();
        ChangedColorOnSelect();
    }

    public void ForceSelectOption(string optionName)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i].name.Replace("Button_", "") == optionName)
            {
                currentIndex = i;
                UpdatePointer();
                break;
            }
        }
    }
}
