using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialController : MonoBehaviour
{
    [Header("Tutorial UI")]
    public CanvasGroup blackPanel;
    public Image[] tutorialSteps;
    public GameObject spaceSkipPrompt;

    [Header("Zona Tutorial")]
    public GameObject zonaTutorial;

    [Header("GameObjects Tutorial")]
    public GameObject playerHealthBar;
    public GameObject enemyHealthBar;
    public GameObject bannerYourTurn;
    public GameObject bannerEnemyTurn;
    public GameObject optionsGroup;
    public GameObject typingIntroPanel;

    [Header("Controller")]
    public UIOptionSelector uiOptionSelector;
    public BattleUIManager battleUIManager;
    public TypingIntroAnimator typingIntroAnimator;

    private int currentStepIndex = 0;
    private bool tutorialActive = true;
    private bool skipAllowed = true;
    private bool waitingForFinalStep = false;
    private bool flow11Triggered = false;
    private bool typingIntroStarted = false; // Flag untuk mencegah typing intro berjalan berulang

    private Transform originalParent_PlayerHB;
    private Transform originalParent_EnemyHB;
    private Transform originalParent_BannerYourTurn;
    private Transform originalParent_OptionsGroup;

    void Start()
    {
        originalParent_PlayerHB = playerHealthBar.transform.parent;
        originalParent_EnemyHB = enemyHealthBar.transform.parent;
        originalParent_BannerYourTurn = bannerYourTurn.transform.parent;
        originalParent_OptionsGroup = optionsGroup.transform.parent;

        blackPanel.alpha = 1;
        blackPanel.gameObject.SetActive(true);
        spaceSkipPrompt.SetActive(true);
        zonaTutorial.SetActive(true);

        foreach (var step in tutorialSteps)
        {
            step.transform.SetParent(zonaTutorial.transform);
            step.gameObject.SetActive(false);
        }

        ShowStep(0);
    }

    void Update()
    {
        if (!tutorialActive) return;

        if (waitingForFinalStep && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Flow10ToFlow11());
            return;
        }

        if (skipAllowed && Input.GetKeyDown(KeyCode.Space))
        {
            ProceedToNextStep();
        }
    }

    void ProceedToNextStep()
    {
        currentStepIndex++;
        ShowStep(currentStepIndex);
    }

    void ShowStep(int index)
    {
        foreach (var img in tutorialSteps)
            img.gameObject.SetActive(false);

        if (index < tutorialSteps.Length)
        {
            tutorialSteps[index].gameObject.SetActive(true);
        }

        switch (index)
        {
            case 0:
                uiOptionSelector.DisableSelection();
                break;

            case 1:
                playerHealthBar.transform.SetParent(zonaTutorial.transform);
                enemyHealthBar.transform.SetParent(zonaTutorial.transform);
                break;

            case 2:
                playerHealthBar.transform.SetParent(originalParent_PlayerHB);
                enemyHealthBar.transform.SetParent(originalParent_EnemyHB);
                bannerYourTurn.transform.SetParent(zonaTutorial.transform);
                break;

            case 3:
                bannerYourTurn.transform.SetParent(originalParent_BannerYourTurn);
                optionsGroup.transform.SetParent(zonaTutorial.transform);
                uiOptionSelector.SetAvailableOptions(new[] { "Attack" });
                uiOptionSelector.DisableSelection();
                break;

            case 4:
                optionsGroup.transform.SetParent(originalParent_OptionsGroup);
                break;

            case 5:
                StartCoroutine(StartTypingPhase());
                break;

            case 6:
                break; // handled externally

            case 7:
                StartCoroutine(ShowFinalStep7Image());
                break;
        }
    }

    IEnumerator ShowFinalStep7Image()
    {
        foreach (var img in tutorialSteps)
            img.gameObject.SetActive(false);

        yield return null;

        if (tutorialSteps.Length > 6)
        {
            tutorialSteps[6].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Step 7 (index 6) tidak ditemukan di array tutorialSteps.");
        }

        // Pastikan UI tetap aktif dan di posisi yang benar
        playerHealthBar.transform.SetParent(originalParent_PlayerHB);
        enemyHealthBar.transform.SetParent(originalParent_EnemyHB);
        bannerYourTurn.transform.SetParent(originalParent_BannerYourTurn);
        optionsGroup.transform.SetParent(originalParent_OptionsGroup);

        playerHealthBar.SetActive(true);
        enemyHealthBar.SetActive(true);
        bannerYourTurn.SetActive(true);
        optionsGroup.SetActive(true);
        bannerEnemyTurn.SetActive(false);

        uiOptionSelector.SetAvailableOptions(new[] { "Attack", "Defend", "Heal" });
        uiOptionSelector.EnableSelection();

        skipAllowed = false;
        waitingForFinalStep = true;
        spaceSkipPrompt.SetActive(true);
    }

    IEnumerator Flow10ToFlow11()
    {
        if (flow11Triggered) yield break;
        flow11Triggered = true;

        waitingForFinalStep = false;
        spaceSkipPrompt.SetActive(false);

        if (tutorialSteps.Length > 6)
            tutorialSteps[6].gameObject.SetActive(false);

        blackPanel.DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.6f);

        zonaTutorial.SetActive(false);
        tutorialActive = false;

        HideAllTutorialUI();
        optionsGroup.SetActive(false);

        // Hanya jalankan typing intro jika belum pernah dimulai
        if (!typingIntroStarted)
        {
            typingIntroStarted = true;
            typingIntroAnimator.onComplete = () =>
            {
                playerHealthBar.SetActive(false);
                enemyHealthBar.SetActive(false);
                bannerYourTurn.SetActive(false);
                optionsGroup.SetActive(false);

                battleUIManager.typewritingManager.BeginTypingSession();
                battleUIManager.typingTimerUI.StartTimer();
            };

            typingIntroAnimator.PlayIntro();
        }
    }

    IEnumerator StartTypingPhase()
    {
        skipAllowed = false;
        tutorialSteps[5].gameObject.SetActive(false);
        blackPanel.DOFade(0, 0.4f);
        spaceSkipPrompt.SetActive(false);

        HideAllTutorialUI();

        // Mulai typing intro part 2 dan set flag
        typingIntroStarted = true;
        typingIntroAnimator.onComplete = () =>
        {
            battleUIManager.typewritingManager.BeginTypingSession();
            battleUIManager.typingTimerUI.StartTimer();
        };

        typingIntroAnimator.PlayIntro();
        yield return null;
    }

    public void TriggerStep6AfterEnemyTurn()
    {
        if (currentStepIndex >= 6) return;
        StartCoroutine(ShowStep6AfterEnemyTurn());
    }

    IEnumerator ShowStep6AfterEnemyTurn()
    {
        currentStepIndex = 6;

        zonaTutorial.SetActive(true);
        blackPanel.alpha = 1f;
        blackPanel.gameObject.SetActive(true);

        foreach (var img in tutorialSteps)
            img.gameObject.SetActive(false);

        if (tutorialSteps.Length > 5)
            tutorialSteps[5].gameObject.SetActive(true);
        else
            Debug.LogError("Step 6 (index 5) tidak ditemukan di array tutorialSteps.");

        // Pindahkan UI ke zona tutorial tanpa fade out
        optionsGroup.transform.SetParent(zonaTutorial.transform, false);
        optionsGroup.SetActive(true);

        // Pastikan UI lain tetap aktif dan visible
        playerHealthBar.SetActive(true);
        enemyHealthBar.SetActive(true);
        bannerYourTurn.SetActive(true);

        if (uiOptionSelector != null)
        {
            uiOptionSelector.SetAvailableOptions(new string[] { "Defend", "Heal" });
            uiOptionSelector.ForceSelectOption("Defend");
            uiOptionSelector.EnableSelection();
        }

        spaceSkipPrompt.SetActive(true);
        skipAllowed = true;

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        // Setelah skip flow 9, jangan fade out UI dulu
        // Langsung proceed ke step 7 tanpa menyembunyikan UI
        ProceedToNextStep();
    }

    void HideAllTutorialUI()
    {
        playerHealthBar.SetActive(false);
        enemyHealthBar.SetActive(false);
        bannerYourTurn.SetActive(false);
        optionsGroup.SetActive(false);
        bannerEnemyTurn.SetActive(false);
    }

    public bool IsTutorialInStep5OrBelow()
    {
        return currentStepIndex <= 5;
    }

    public bool IsTutorialBlockingInput()
    {
        return tutorialActive && !skipAllowed;
    }

    // Method untuk reset typing intro jika diperlukan
    public void ResetTypingIntroFlag()
    {
        typingIntroStarted = false;
    }
}