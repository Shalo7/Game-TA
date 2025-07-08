using UnityEngine;
using UnityEngine.UI;

public class UIOptionSelector : MonoBehaviour
{
    public RectTransform[] optionButtons;
    public RectTransform selectorPointer;
    public float pointerOffsetX = -30f;

    public Image[] optionImages;
    public Color selectedColor = Color.black;
    public Color normalColor = Color.white;

    public UIFadeOut fadeUIOptionsGroup;
    public UIFadeOut fadeTurnIndicator;
    public UIFadeOut fadePlayerInfo;
    public UIFadeOut fadeEnemyInfo;

    private int currentIndex = 0;
    private bool selectionMade = false;

    void Start()
    {
        UpdatePointer();
        ResetButtonColors();
    }

    void Update()
    {
        if (selectionMade) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + optionButtons.Length) % optionButtons.Length;
            UpdatePointer();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % optionButtons.Length;
            UpdatePointer();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }

    void UpdatePointer()
    {
        selectorPointer.position = optionButtons[currentIndex].position + Vector3.left * pointerOffsetX;
    }

    void ConfirmSelection()
    {
        selectionMade = true;

        // 1. Hitamkan pilihan
        ResetButtonColors();
        if (optionImages != null && currentIndex < optionImages.Length)
        {
            optionImages[currentIndex].color = selectedColor;
        }

        // 2. Ambil nama button -> kirim ke TurnManager
        string choice = optionButtons[currentIndex].name.Replace("Button_", ""); // lebih clean
        Debug.Log("Player chose: " + choice);
        FindFirstObjectByType<TurnManager>().OnPlayerChoice(choice);

        // 3. Fade semua UI
        fadeUIOptionsGroup?.StartFadeOut();
        fadeTurnIndicator?.StartFadeOut();
        fadePlayerInfo?.StartFadeOut();
        fadeEnemyInfo?.StartFadeOut();
    }

    public void EnableSelection()
    {
        selectionMade = false;
        currentIndex = 0;
        UpdatePointer();
        ResetButtonColors();
        gameObject.SetActive(true);

        fadeUIOptionsGroup?.ResetFade();
        fadeTurnIndicator?.ResetFade();
        fadePlayerInfo?.ResetFade();
        fadeEnemyInfo?.ResetFade();
    }

    void ResetButtonColors()
    {
        foreach (var img in optionImages)
        {
            img.color = normalColor;
        }
    }
}
