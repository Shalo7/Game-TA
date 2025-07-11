using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIOptionSelector : MonoBehaviour
{
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

    private int currentIndex = 0;
    private bool selectionMade = false;
    private bool inputEnabled = false;
    private string[] availableOptions = new string[] { "Attack", "Defend", "Heal" };

    void Start()
    {
        UpdatePointer();
        ResetButtonColors();
    }

    void Update()
    {
        if (!inputEnabled || selectionMade || !gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            string selectedOption = optionButtons[currentIndex].name.Replace("Button_", "");
            if (availableOptions.Contains(selectedOption))
            {
                ConfirmSelection();
            }

            if (moveButtons != null && currentIndex >= 0 && currentIndex < moveButtons.Length)
            {
                Button selectedBtn = moveButtons[currentIndex].GetComponent<Button>();
                if (selectedBtn != null && selectedBtn.interactable)
                {
                    Debug.Log("Triggered" + selectedBtn.name);
                    selectedBtn.onClick.Invoke();
                }
            }
        }
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
                break;
            }
        }
    }

    void ConfirmSelection()
    {
        selectionMade = true;
        ResetButtonColors();

        if (optionImages != null && currentIndex < optionImages.Length)
        {
            optionImages[currentIndex].color = selectedColor;
        }

        string choice = optionButtons[currentIndex].name.Replace("Button_", "");
        FindFirstObjectByType<TurnManager>()?.OnPlayerChoice(choice);

        fadeUIOptionsGroup?.StartFadeOut();
        fadeTurnIndicator?.StartFadeOut();
        fadePlayerInfo?.StartFadeOut();
        fadeEnemyInfo?.StartFadeOut();
    }

    void UpdatePointer()
    {
        selectorPointer.position = optionButtons[currentIndex].position + Vector3.left * pointerOffsetX;
    }

    void ResetButtonColors()
    {
        for (int i = 0; i < optionImages.Length; i++)
        {
            string key = optionButtons[i].name.Replace("Button_", "");
            optionImages[i].color = availableOptions.Contains(key) ? normalColor : disabledColor;
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
        gameObject.SetActive(true);

        fadeUIOptionsGroup?.ResetFade();
        fadeTurnIndicator?.ResetFade();
        fadePlayerInfo?.ResetFade();
        fadeEnemyInfo?.ResetFade();
    }

    public void DisableSelection()
    {
        inputEnabled = false;
    }

    public void SetAvailableOptions(string[] allowedOptions)
    {
        availableOptions = allowedOptions;
        ResetButtonColors();
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
