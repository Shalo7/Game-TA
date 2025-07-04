using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public UIFadeOut uiOptionsGroupFade;
    public BattleUIManager battleUIManager;

    private string storedChoice; // ⬅️ Simpan sementara pilihan player

    public void OnPlayerChoice(string choice)
    {
        Debug.Log("Player chose: " + choice);

        storedChoice = choice;

        uiOptionsGroupFade.StartFadeOut(() =>
        {
            if (battleUIManager != null)
            {
                battleUIManager.OnActionSelected(storedChoice); // Pastikan giliran baru bersih
            }
            else
            {
                Debug.LogWarning("BattleUIManager tidak ditemukan!");
            }
        });
    }
}
