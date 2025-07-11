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
    public GameObject UI_TurnIndicator;
    public GameObject UI_PlayerInfo;
    public GameObject UI_EnemyInfo;
    public GameObject UI_EnemyTurnIndicator;

    [Header("Component References")]
    public UIOptionSelector uiOptionSelectorScript;
    public TypingIntroAnimator typingIntroAnimator;

    [Header("Timing")]
    public float enemyTurnDuration = 2f;

    [Header("Health")]
    public HealthBarAnimation playerHealthBarAnim;

    [Header("Tutorial Hook")]
    public TutorialController tutorialController;

    private bool isInTutorial = false;
    private bool hasDealtFirstEnemyDamage = false;

    public bool EnemyTurnFinished { get; private set; } = false;

    public void SetTutorialMode(bool active)
    {
        isInTutorial = active;
    }

    public void OnActionSelected(string action)
    {
        Debug.Log($"Player memilih: {action}");

        UI_OptionSelector.SetActive(false);
        UI_TurnIndicator.SetActive(false);
        UI_PlayerInfo.SetActive(true);
        UI_EnemyInfo.SetActive(true);
        UI_EnemyTurnIndicator.SetActive(false);

        typingIntroAnimator.onComplete = () =>
        {
            typewritingManager.BeginTypingSession();
            typingTimerUI.StartTimer();
        };

        typingIntroAnimator.PlayIntro();

        UI_OptionSelector.SetActive(false);
        UI_PlayerInfo.SetActive(true);
        UI_EnemyInfo.SetActive(true);
        Debug.Log("SetActive Health Bar Kedua");
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

        UI_PlayerInfo.SetActive(true);
        UI_EnemyInfo.SetActive(true);
        Debug.Log("SetActive Health Bar");
        UI_OptionSelector.SetActive(false);
        UI_TurnIndicator.SetActive(false);
        typingTimerUI.Hide();

        // ⚠️ Kurangi darah player hanya di giliran musuh pertama (flow 8)
        if (!hasDealtFirstEnemyDamage && playerHealthBarAnim != null)
        {
            yield return new WaitForSeconds(0.5f);
            playerHealthBarAnim.TakeDamage(50f);
            hasDealtFirstEnemyDamage = true;
        }

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

        UI_EnemyTurnIndicator.SetActive(false);

        UI_PlayerInfo.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_EnemyInfo.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_OptionSelector.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_TurnIndicator.GetComponent<UIFadeOut>()?.StartFadeIn();

        uiOptionSelectorScript.EnableSelection();
    }
}
