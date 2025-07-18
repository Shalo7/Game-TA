using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ParticleData.SpawnData;
using AnimationLoading.LoadStruct;

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    public Charas playerChara;
    public Charas enemyChara;

    public Image playerImage;
    public Image enemyImage;

    public HealthBarAnimation playerHpBar;
    public HealthBarAnimation enemyHpBar;

    public Button[] moveButtons;
    public TMP_Text battleLog;

    public TMP_Text plrName;
    public TMP_Text enemyName;

    public TMP_Text plrStats;
    public TMP_Text enemyStats;

    public GameObject winScreen;
    public GameObject loseScreen;

    private CharaInstance player;
    private CharaInstance enemy;
    private Transform playerTransform;
    private Transform enemyTransform;

    private bool isPlayerTurn;

    void Start()
    {
        SetupBattle();
    }

    public void SetupBattle()
    {
        playerTransform = GetCharacterTransform(CharType.Player);
        enemyTransform = GetCharacterTransform(CharType.Enemy);
        player = new CharaInstance(playerChara, playerTransform);
        Debug.Log(playerTransform);
        enemy = new CharaInstance(enemyChara, enemyTransform);

        plrName.text = player.baseData.charaName;
        enemyName.text = enemy.baseData.charaName;

        playerImage.sprite = player.baseData.charaSprite;
        enemyImage.sprite = enemy.baseData.charaSprite;

        playerHpBar.maxHealth = player.baseData.maxHP;
        playerHpBar.SetHealth(player.curHP);
        
        enemyHpBar.maxHealth = enemy.baseData.maxHP;
        enemyHpBar.SetHealth(enemy.curHP);


        UpdateHPUI();

        SetupMoveButtons();

        StartCoroutine(BattleLoop());
    }

    private Transform GetCharacterTransform(CharType type)
    {
        CharacterMarker[] charMark = FindObjectsByType<CharacterMarker>(FindObjectsSortMode.None);
        CharacterMarker chosenCharMark = null;
        foreach (CharacterMarker marks in charMark)
        {
            chosenCharMark = marks;
            if (chosenCharMark.GetCharType() != type) continue;
            break;
        }

        if (chosenCharMark.GetCharType() != type) return null;
        else return chosenCharMark.GetTransform();
    }

    void SetupMoveButtons()
    {
        for (int i = 0; i < moveButtons.Length; i++)
        {
            int index = i;
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
        {
            battleLog.text = "Enemy Defeated!";
            Debug.Log("Enemy Defeated!");
            winScreen.SetActive(true);
        }
        else
        {
            battleLog.text = "You Lost!";
            Debug.Log("You Lost!");
            loseScreen.SetActive(true);
        }
    }

    [SerializeField] TypewritingManager typingManager;
    IEnumerator PlayerTurn()
    {
        Debug.Log("▶ PlayerTurn started");
        battleLog.text = "Your Turn!";
        EnableMoveButtons(true);

        yield return new WaitUntil(() => moveChosen);
        EnableMoveButtons(false);
        moveChosen = false;

        yield return new WaitForSeconds(2f);
        bool typingComplete = false;
        int correctWords = 0;

        typingManager.StartTyping((correctCount) =>
        {
            Debug.Log("✅ StartTyping() callback called with: " + correctCount);
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
                ParticleSpawnData data = new ParticleSpawnData(null, target.curTransform.position, Vector3.zero, Vector3.one, ParticleEnum.EntityDamage, true);
                ExecuteParticleEffects(data);
                if (source.GetCurrentAnimCtrl() != null)
                {
                    AnimationLoadStruct animStruct = new AnimationLoadStruct(0, GenericAnimationEnums.ATTACK, true, true);
                    source.GetCurrentAnimCtrl().RequestPlayAnimation(animStruct);
                }
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

    void ExecuteParticleEffects(ParticleSpawnData data)
    {
        if (ParticlePoolManager.instance == null) return;
        ParticlePoolManager.instance.ActivateParticleFX(data);
    }

    void UpdateHPUI()
    {
        //playerHpBar.mainHealthSlider.value = player.curHP;
        //enemyHpBar.mainHealthSlider.value = enemy.curHP;
        playerHpBar.TakeDamage(playerHpBar.CurrentHealth - player.curHP);
        enemyHpBar.TakeDamage(enemyHpBar.CurrentHealth - enemy.curHP);

        plrStats.text =
            $"HP: {player.curHP}/{player.baseData.maxHP}\n" +
            $"ATK: {player.curAtt}\n" +
            $"DEF: {player.curDef}";

        enemyStats.text =
            $"HP: {enemy.curHP}/{enemy.baseData.maxHP}\n" +
            $"ATK: {enemy.curAtt}\n" +
            $"DEF: {enemy.curDef}";
    }

    private bool moveChosen = false;
    private Moves selectedMove;

    void OnPlayerMoveChosen(int index)
    {
        //ebug.Log("Player selected move: " + index);
        selectedMove = player.baseData.moves[index];
        moveChosen = true;
    }

}
