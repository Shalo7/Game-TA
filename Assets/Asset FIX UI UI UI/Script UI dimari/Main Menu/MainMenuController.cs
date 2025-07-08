using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    public Button[] mainButtons;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;

    [Header("Options")]
    public GameObject optionsPanel;
    public Button[] optionsButtons;

    [Header("Selector")]
    public RectTransform selectorPointer;
    public float pointerOffsetX = -60f;

    [Header("Tutorial")]
    public TutorialBookController tutorialBook;

    private int mainIndex = 0;
    private int optionsIndex = 0;
    private bool inOptions = false;
    private bool inputLocked = false;

    void Start()
    {
        optionsPanel.SetActive(false);
        HighlightButtons(mainButtons, mainIndex);
        MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
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
            if (Input.GetKeyDown(KeyCode.Space)) ConfirmOptionsMenu();
        }
    }

    void HandleNavigation(Button[] buttons, ref int index)
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            index = (index - 1 + buttons.Length) % buttons.Length;
            HighlightButtons(buttons, index);
            MovePointer(buttons[index].GetComponent<RectTransform>());
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            index = (index + 1) % buttons.Length;
            HighlightButtons(buttons, index);
            MovePointer(buttons[index].GetComponent<RectTransform>());
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

        switch (mainIndex)
        {
            case 0: // Play
                if (SceneController.Instance != null)
                {
                    SceneController.Instance.NextLevel("LevelSelector");
                }
                else
                {
                    Debug.LogWarning("SceneController not found in scene.");
                }
                break;
            case 1: // Options
                inOptions = true;
                optionsPanel.SetActive(true);
                HighlightButtons(optionsButtons, optionsIndex = 0);
                MovePointer(optionsButtons[optionsIndex].GetComponent<RectTransform>());
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
            case 0: // Tutorial
                optionsPanel.SetActive(false);
                tutorialBook.StartTutorial(() =>
                {
                    inOptions = false;
                    inputLocked = false;
                    HighlightButtons(mainButtons, mainIndex = 0);
                    MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                });
                break;

            case 1: // Back
                optionsPanel.SetActive(false);
                inOptions = false;
                HighlightButtons(mainButtons, mainIndex = 0);
                MovePointer(mainButtons[mainIndex].GetComponent<RectTransform>());
                inputLocked = false;
                break;
        }
    }
}
