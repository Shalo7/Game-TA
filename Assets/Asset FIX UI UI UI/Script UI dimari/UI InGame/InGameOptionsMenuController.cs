using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class InGameOptionsMenuController : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pausePanel;
    public CanvasGroup pausePanelGroup; // Untuk animasi fade out quit
    public Button[] optionButtons;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color confirmedColor = Color.green;

    [Header("Tutorial Book")]
    public TutorialBookController tutorialBook;

    [Header("Gameplay Scripts")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("Quit Fade Settings")]
    public float fadeOutDuration = 0.5f;

    private int optionIndex = 0;
    private bool isPaused = false;
    private bool inputLocked = false;
    private bool isReadingBook = false;
    private bool suppressNextTab = false;

    void Start()
    {
        pausePanel.SetActive(false);
        pausePanelGroup.alpha = 0f;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!isReadingBook)
        {
            if (suppressNextTab && Input.GetKeyDown(KeyCode.Tab))
            {
                suppressNextTab = false; // Konsumsi input Tab agar tidak lanjut ke resume
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

    void OpenPauseMenu()
    {
        isPaused = true;
        inputLocked = false;
        pausePanel.SetActive(true);
        pausePanelGroup.alpha = 1f;
        Time.timeScale = 0f;

        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        optionIndex = 0;
        HighlightButtons(optionButtons, optionIndex);
    }

    void ResumeGame()
    {
        pausePanel.SetActive(false);
        isPaused = false;
        inputLocked = false;
        Time.timeScale = 1f;

        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = true;
        }
    }

    void HandleNavigation()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            optionIndex = (optionIndex - 1 + optionButtons.Length) % optionButtons.Length;
            HighlightButtons(optionButtons, optionIndex);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            optionIndex = (optionIndex + 1) % optionButtons.Length;
            HighlightButtons(optionButtons, optionIndex);
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

    void ConfirmOption()
    {
        inputLocked = true;
        SetConfirmed(optionButtons[optionIndex]);

        switch (optionIndex)
        {
            case 0: // TUTORIAL
                OpenTutorialBookFromPause();
                break;

            case 1: // QUIT
                StartCoroutine(QuitWithFade());
                break;
        }
    }

    void OpenTutorialBookFromPause()
    {
        isReadingBook = true;

        pausePanel.SetActive(false);

        tutorialBook.StartTutorial(() =>
        {
            isReadingBook = false;
            inputLocked = false;
            suppressNextTab = true; // ✅ Tambahan penting agar Tab tidak langsung menutup panel

            pausePanel.SetActive(true);
            pausePanelGroup.alpha = 1f;

            HighlightButtons(optionButtons, optionIndex = 0);
        });
    }


    IEnumerator QuitWithFade()
    {
        if (pausePanelGroup != null)
        {
            pausePanelGroup.DOFade(0f, fadeOutDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(fadeOutDuration);
        }

        Time.timeScale = 1f;

        // Pastikan semua gameplay script tidak menyala saat pindah scene
        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        if (SceneController.Instance != null)
            SceneController.Instance.NextLevel("LevelSelector");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelector");
    }
}
