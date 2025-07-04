using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BattleUIManager : MonoBehaviour
{
    public TypewritingManager typewritingManager;
    public TypingWordTimer typingTimerUI;

    public GameObject UI_OptionSelector;
    public GameObject UI_TurnIndicator; // ✅ Ini adalah UI_PLAYERTurnIndicator
    public GameObject UI_PlayerInfo;
    public GameObject UI_EnemyInfo;
    public GameObject UI_EnemyTurnIndicator;

    public UIOptionSelector uiOptionSelectorScript;
    public TypingIntroAnimator typingIntroAnimator;

    public float enemyTurnDuration = 2f;

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

        typingIntroAnimator.PlayIntro(); // ⬅️ Tambahkan ini
    }

    public void OnTypingSessionComplete()
    {
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("Giliran musuh dimulai...");

        // Show indikator musuh
        UI_EnemyTurnIndicator.SetActive(true);
        UI_EnemyTurnIndicator.transform.localScale = Vector3.zero;
        UI_EnemyTurnIndicator.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

        // Hide player-related UI
        UI_PlayerInfo.SetActive(false);
        UI_EnemyInfo.SetActive(false);
        UI_OptionSelector.SetActive(false);
        UI_TurnIndicator.SetActive(false);
        typingTimerUI.Hide();

        yield return new WaitForSeconds(enemyTurnDuration);

        // Kembali ke giliran pemain
        BackToPlayerSelection();
    }

    public void BackToPlayerSelection()
    {
        Debug.Log("Giliran pemain kembali");

        // ✅ Sembunyikan UI_ENEMYTurnIndicator
        UI_EnemyTurnIndicator.SetActive(false);

        // ✅ Fade-in player UI
        UI_PlayerInfo.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_EnemyInfo.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_OptionSelector.GetComponent<UIFadeOut>()?.StartFadeIn();
        UI_TurnIndicator.GetComponent<UIFadeOut>()?.StartFadeIn();

        // Aktifkan kembali pemilihan aksi
        uiOptionSelectorScript.EnableSelection();
    }
}
