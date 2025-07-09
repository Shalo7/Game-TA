using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BattleUIManager : MonoBehaviour
{
    [Header("Main Systems")]
    public TypewritingManager typewritingManager;
    public TypingWordTimer typingTimerUI;

    [Header("UI References")]
    public GameObject UI_OptionSelector;
    public GameObject UI_TurnIndicator; // Banner "Your Turn"
    public GameObject UI_PlayerInfo;
    public GameObject UI_EnemyInfo;
    public GameObject UI_EnemyTurnIndicator;

    [Header("Component References")]
    public UIOptionSelector uiOptionSelectorScript;
    public TypingIntroAnimator typingIntroAnimator;

    [Header("Timing")]
    public float enemyTurnDuration = 2f;

    [Header("Tutorial Hook")]
    public TutorialController tutorialController; // 🟡 Assign jika ada tutorial

    private bool isInTutorial = false;

    public bool EnemyTurnFinished { get; private set; } = false;

    public void SetTutorialMode(bool active)
    {
        isInTutorial = active;
    }

    public void OnActionSelected(string action)
    {
        Debug.Log($"Player memilih: {action}");

        // Nonaktifkan UI yang tidak perlu
        UI_OptionSelector.SetActive(false);
        UI_TurnIndicator.SetActive(false);
        UI_PlayerInfo.SetActive(true);
        UI_EnemyInfo.SetActive(true);
        UI_EnemyTurnIndicator.SetActive(false);

        // Jalankan typing intro (Let's Begin!)
        typingIntroAnimator.onComplete = () =>
        {
            typewritingManager.BeginTypingSession();
            typingTimerUI.StartTimer();
        };

        typingIntroAnimator.PlayIntro();

        // Saat typing intro muncul, semua UI gameplay sebaiknya disembunyikan
        UI_OptionSelector.SetActive(false);
        UI_PlayerInfo.SetActive(false);
        UI_EnemyInfo.SetActive(false);
        UI_TurnIndicator.SetActive(false);
    }

    public void OnTypingSessionComplete()
    {
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("Giliran musuh dimulai...");

        UI_EnemyTurnIndicator.SetActive(true);
        UI_EnemyTurnIndicator.transform.localScale = Vector3.zero;
        UI_EnemyTurnIndicator.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

        UI_PlayerInfo.SetActive(false);
        UI_EnemyInfo.SetActive(false);
        UI_OptionSelector.SetActive(false);
        UI_TurnIndicator.SetActive(false);
        typingTimerUI.Hide();

        yield return new WaitForSeconds(enemyTurnDuration);

        EnemyTurnFinished = true;

        if (tutorialController != null && tutorialController.enabled && tutorialController.IsTutorialInStep5OrBelow())
        {
            tutorialController.TriggerStep6AfterEnemyTurn();
            yield break;
        }

        BackToPlayerSelection();
    }

    public void BackToPlayerSelection()
    {
        Debug.Log("Giliran pemain kembali");

        // Sembunyikan indikator giliran musuh
        UI_EnemyTurnIndicator.SetActive(false);

        // Fade in semua UI player
        UI_PlayerInfo.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_EnemyInfo.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_OptionSelector.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_TurnIndicator.GetComponent<UIFadeOut>()?.StartFadeIn();

        // Aktifkan opsi seleksi player
        uiOptionSelectorScript.EnableSelection();
    }
}
