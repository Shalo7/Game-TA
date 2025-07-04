using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    public Charas playerChara;
    public Charas enemyChara;

    public Image playerImage;
    public Image enemyImage;

    public Slider playerHpBar;
    public Slider enemyHpBar;

    public Button[] moveButtons;
    public TMP_Text battleLog;

    public TMP_Text plrStats;
    public TMP_Text enemyStats;

    private CharaInstance player;
    private CharaInstance enemy;

    private bool isPlayerTurn;

    void Start()
    {
        SetupBattle();
    }

    void SetupBattle()
    {
        player = new CharaInstance(playerChara);
        enemy = new CharaInstance(enemyChara);

        playerImage.sprite = player.baseData.charaSprite;
        enemyImage.sprite = enemy.baseData.charaSprite;

        playerHpBar.maxValue = player.baseData.maxHP;
        enemyHpBar.maxValue = enemy.baseData.maxHP;

        UpdateHPUI();

        SetupMoveButtons();

        StartCoroutine(BattleLoop());
    }

    void SetupMoveButtons()
    {
        for (int i = 0; i < moveButtons.Length; i++)
        {
            int index = i;
            moveButtons[i].GetComponentInChildren<TMP_Text>().text = player.baseData.moves[i].moveName;
            moveButtons[i].onClick.AddListener(() => OnPlayerMoveChosen(index));
        }
    }

    void EnableMoveButtons(bool enable)
    {
        foreach (var btn in moveButtons)
            btn.interactable = enable;
    }

    IEnumerator BattleLoop()
    {
        battleLog.text = "Battle Start!";
        yield return new WaitForSeconds(1f);

        while (true)
        {
            isPlayerTurn = player.curSpd >= enemy.curSpd;
            //yield return isPlayerTurn ? PlayerTurn() : EnemyTurn();

            if (isPlayerTurn)
            {
                yield return PlayerTurn();
                if (enemy.IsFainted()) break;

                yield return EnemyTurn();
                if (player.IsFainted()) break;
            }
            else
            {
                yield return EnemyTurn();
                if (player.IsFainted()) break;

                yield return PlayerTurn();
                if (enemy.IsFainted()) break;
            }

            player.OnTurnEnd();
            enemy.OnTurnEnd();

            yield return new WaitForSeconds(0.5f);
        }

        if (enemy.IsFainted())
            battleLog.text = "Enemy Defeated!";
        else
            battleLog.text = "You Lost!";
    }

    [SerializeField] TypingManager typingManager;
    IEnumerator PlayerTurn()
    {
        battleLog.text = "Your Turn!";
        EnableMoveButtons(true);

        yield return new WaitUntil(() => moveChosen);
        EnableMoveButtons(false);
        moveChosen = false;

        yield return new WaitForSeconds(3f);
        bool typingComplete = false;
        int correctWords = 0;

        typingManager.StartTyping((correctCount) =>
        {
            correctWords = correctCount;
            typingComplete = true;
        });

        yield return new WaitUntil(() => typingComplete);

        if (correctWords == 0)
        {
            battleLog.text = "Failed";
            yield return new WaitForSeconds(1f);
            yield break;
        }

        float finalPower = selectedMove.power * correctWords;

        ExecuteMove(player, enemy, selectedMove, finalPower);

        UpdateHPUI();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn");
        battleLog.text = "Enemy's turn!";
        yield return new WaitForSeconds(1f);

        int moveIndex = Random.Range(0, enemy.baseData.moves.Length); //Placeholder AI
        int basePower = enemy.baseData.moves[moveIndex].power;
        ExecuteMove(enemy, player, enemy.baseData.moves[moveIndex], basePower);
        Debug.Log(moveIndex);

        UpdateHPUI();
        yield return new WaitForSeconds(1f);
    }

    void ExecuteMove(CharaInstance source, CharaInstance target, Moves move, float modifiedPower)
    {
        int finalPower = Mathf.RoundToInt(modifiedPower);

        //Attack Damage Math
        if (move.moveType == MoveType.Attack)
        {
            int damage = Mathf.Max(1, finalPower + source.curAtt - target.curDef);

            //Block Mechanic
            if (target.isBlocking)
            {
                damage = 0;
                target.isBlocking = false;
            }
            else
            {
                target.curHP -= damage;
            }
        }
        else if (move.moveType == MoveType.Debuff || move.moveType == MoveType.Heal)
        {
            source.ApplyMoveEffect(move, false, target, finalPower); //Debuff
        }
        else
        {
            source.ApplyMoveEffect(move, false, null, finalPower);
        }

        target.curHP = Mathf.Clamp(target.curHP, 0, target.baseData.maxHP);
        source.curHP = Mathf.Clamp(source.curHP, 0, source.baseData.maxHP);
    }

    void UpdateHPUI()
    {
        playerHpBar.value = player.curHP;
        enemyHpBar.value = enemy.curHP;

        plrStats.text =
            $"HP: {player.curHP}/{player.baseData.maxHP}\n" +
            $"ATK: {player.curAtt}\n" +
            $"DEF: {player.curDef}\n" +
            $"SPD: {player.curSpd}";

        enemyStats.text =
            $"HP: {enemy.curHP}/{enemy.baseData.maxHP}\n" +
            $"ATK: {enemy.curAtt}\n" +
            $"DEF: {enemy.curDef}\n" +
            $"SPD: {enemy.curSpd}";
    }

    private bool moveChosen = false;
    private Moves selectedMove;

    void OnPlayerMoveChosen(int index)
    {
        Debug.Log("Player selected move: " + index);
        selectedMove = player.baseData.moves[index];
        moveChosen = true;
    }

}
