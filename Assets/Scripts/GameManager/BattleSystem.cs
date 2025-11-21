using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ParticleData.SpawnData;
using AnimationLoading.LoadStruct;
using System;
using Random = UnityEngine.Random;
using AudioData;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem instance;
    [Header("References")]
    [SerializeField] Charas playerChara;
    [SerializeField] Charas enemyChara;

    [SerializeField] Image playerImage;
    [SerializeField] Image enemyImage;

    [SerializeField] HealthBarAnimation playerHpBar;
    [SerializeField] HealthBarAnimation enemyHpBar;

    [SerializeField] Button[] moveButtons;
    [SerializeField] TMP_Text battleLog;

    [SerializeField] TMP_Text plrName;
    [SerializeField] TMP_Text enemyName;

    [SerializeField] TMP_Text plrStats;
    [SerializeField] TMP_Text enemyStats;

    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] Slider shieldSlider;
    [SerializeField] int maxShieldHP = 50;
    public int GetMaxShieldHP() => maxShieldHP;

    [SerializeField] UIOptionSelector selector;
    [SerializeField] BattleUIManager battleUIManager;
    [SerializeField] TypewritingManager typingManager;
    [SerializeField] List<AudioClip> gameplaySounds = new List<AudioClip>();

    private CharaInstance player;
    private CharaInstance enemy;
    private Transform playerTransform;
    private Transform enemyTransform;

    private CharaInstance currentAttacker;
    private CharaInstance currentTarget;
    private Moves currentMove;
    private int currentFinalPower;
    private int damagePower;
    int debuffTurnCount = 1;
    int buffTurnCount = 1;
    [SerializeField] int nextLevel = 0;
    [SerializeField] BaseEnemyChoiceHandler choiceHandler;

    BaseAnimationController currentAnimMonitored;
    AnimationStateInstance currentAnimStateMonitored;
    bool isAnimationDone = false;
    public bool isRunningGame = false;
    public bool GetIsRunningGame() => isRunningGame;

    private bool isPlayerTurn;

    void Awake()
    {
        if (instance != null) return;
        instance = this;
        isRunningGame = false;   
    }

    void Start()
    {
        SetupBattle();
        isAnimationDone = true;
    }

    public void SetupBattle()
    {
        playerTransform = GetCharacterTransform(CharType.Player);
        enemyTransform = GetCharacterTransform(CharType.Enemy);
        player = new CharaInstance(playerChara, playerTransform, enemyTransform);
        enemy = new CharaInstance(enemyChara, enemyTransform, playerTransform);
        AssignStatEffectUI(player, CharType.Player);
        AssignStatEffectUI(enemy, CharType.Enemy);

        plrName.text = player.baseData.charaName;
        enemyName.text = enemy.baseData.charaName;

        playerImage.sprite = player.baseData.charaSprite;
        enemyImage.sprite = enemy.baseData.charaSprite;

        playerHpBar.maxHealth = player.baseData.maxHP;
        playerHpBar.SetHealth(player.curHP);
        
        enemyHpBar.maxHealth = enemy.baseData.maxHP;
        enemyHpBar.SetHealth(enemy.curHP);

        shieldSlider.maxValue = maxShieldHP;
        shieldSlider.value = player.shieldHP;

        if (choiceHandler != null) choiceHandler.InitializeScript(enemy);

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

    public void AssignStatEffectUI(CharaInstance instance, CharType charType)
    {
        EntityStatEffectsManager[] entityStatEffectsManagers = FindObjectsByType<EntityStatEffectsManager>(FindObjectsSortMode.None);

        EntityStatEffectsManager chosenManager = null;
        foreach (EntityStatEffectsManager marks in entityStatEffectsManagers)
        {
            chosenManager = marks;
            if (chosenManager.GetCharacterType() != charType) continue;
            instance.AssignStatEffectUI(chosenManager);
            break;
        }
    }

    void SetupMoveButtons()
    {
        for (int i = 0; i < moveButtons.Length; i++)
        {
            int index = i;
            moveButtons[index].onClick.AddListener(() => OnPlayerMoveChosen(index));
        }
    }

    void EnableMoveButtons(bool enable)
    {
        foreach (var btn in moveButtons)
            btn.interactable = enable;
    }

    IEnumerator BattleLoop()
    {
        yield return new WaitForSeconds(1f);
        isRunningGame = true;

        while (true)
        {
            isPlayerTurn = player.curSpd >= enemy.curSpd;
            //yield return isPlayerTurn ? PlayerTurn() : EnemyTurn();

            if (isPlayerTurn)
            {
                if (isDebuffing)
                    debuffTurnCounter();
                // battleLog.text = $"Turn {turnCount}";
                // turnCount++;
                if (isBuffing)
                    buffTurnCounter();
                yield return PlayerTurn();
                if (enemy.IsFainted()) break;
                //yield return WaitTurnDone();

                yield return EnemyTurn();
                if (player.IsFainted()) break;
                //yield return WaitTurnDone();
            }
            else
            {
                yield return EnemyTurn();
                if (player.IsFainted()) break;
                //yield return WaitTurnDone();

                yield return PlayerTurn();
                if (enemy.IsFainted()) break;
                //yield return WaitTurnDone();
            }

            player.OnTurnEnd();
            enemy.OnTurnEnd();

            yield return new WaitForSeconds(0.5f);
        }

        if (enemy.IsFainted())
        {
            Debug.Log("Enemy Defeated!");
            AudioManager.Instance.StopBGM();
            battleUIManager.playerNotDone = false;
            winScreen.SetActive(true);
            Director.instance.UpdateCurrentLevel(nextLevel);
        }
        else
        {
            Debug.Log("You Lost!");
            AudioManager.Instance.StopBGM();
            battleUIManager.playerNotDone = false;
            loseScreen.SetActive(true);
        }
    }

    IEnumerator PlayerTurn()
    {
        if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(gameplaySounds[0], false, Vector3.zero)); }
        Debug.Log("▶ PlayerTurn started");
        selector.SetAvailableOptions(new[] {"Attack", "Defend", "Heal"} );
        EnableMoveButtons(true);
        //selector.EnableSelection();

        yield return new WaitUntil(() => moveChosen);
        EnableMoveButtons(false);
        moveChosen = false;
        typingManager.AddColorGradient(selectedMove.counterColor);

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
            yield return new WaitForSeconds(1f);
            yield break;
        }

        float finalPower = selectedMove.power * correctWords;

        ExecuteMove(player, enemy, selectedMove, finalPower);

        yield return WaitTurnDone();
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn");
        typingManager.EmptyCounterText();
        yield return new WaitForSeconds(1f);

        if (choiceHandler == null)
        {
            int moveIndex = Random.Range(0, enemy.baseData.moves.Length); //Placeholder AI
            int basePower = enemy.baseData.moves[moveIndex].power;
            ExecuteMove(enemy, player, enemy.baseData.moves[moveIndex], basePower);
            Debug.LogError($"Enemy movement is {enemy.baseData.moves[moveIndex]}!");
        }
        else
        {
            choiceHandler.DecideTurn();   
        }
        yield return WaitTurnDone();
    }

    public IEnumerator WaitTurnDone()
    {
        while (!isAnimationDone)
        {
            yield return null;
        }
    }

    public void ExecuteMove(CharaInstance source, CharaInstance target, Moves move, float modifiedPower)
    {
        isAnimationDone = false;
        currentAttacker = source;
        currentTarget = target;
        currentMove = move;
        currentFinalPower = Mathf.RoundToInt(modifiedPower);

        switch (move.moveType)
        {
            default:
                Debug.LogError("No move!");
                break;

            case MoveType.Attack:
                AnimationLoadStruct attackAnimStruct = new AnimationLoadStruct(0, GenericAnimationStates.ATTACK, true, true, SameAnimActionEnum.None);
                (this.currentAnimMonitored, this.currentAnimStateMonitored) = source.GetCurrentAnimCtrl().RequestPlayAnimation(attackAnimStruct);
                //this.currentAnimMonitored = currentAnimMonitored;
                //this.currentAnimStateMonitored = currentAnimStateMonitored;
                //currentAnimMonitored.AnimationEndsEvent = OnTurnAnimationEnds;
                currentAnimStateMonitored.AnimationStateEvents = OnAnimationStateEvents;
                break;

            case MoveType.Heal:
                AnimationLoadStruct healAnimStruct = new AnimationLoadStruct(0, GenericAnimationStates.HEAL, true, true, SameAnimActionEnum.None);
                (this.currentAnimMonitored, this.currentAnimStateMonitored) = source.GetCurrentAnimCtrl().RequestPlayAnimation(healAnimStruct);
                //this.currentAnimMonitored = currentAnimMonitored;
                //this.currentAnimStateMonitored = currentAnimStateMonitored;
                //currentAnimMonitored.AnimationEndsEvent = OnTurnAnimationEnds;
                currentAnimStateMonitored.AnimationStateEvents = OnAnimationStateEvents;
                break;

            case MoveType.Defend:
                AnimationLoadStruct defAnimStruct = new AnimationLoadStruct(0, GenericAnimationStates.DEF, true, true, SameAnimActionEnum.None);
                (this.currentAnimMonitored, this.currentAnimStateMonitored) = source.GetCurrentAnimCtrl().RequestPlayAnimation(defAnimStruct);
                //this.currentAnimMonitored = currentAnimMonitored;
                //this.currentAnimStateMonitored = currentAnimStateMonitored;
                //currentAnimMonitored.AnimationEndsEvent = OnTurnAnimationEnds;
                currentAnimStateMonitored.AnimationStateEvents = OnAnimationStateEvents;
                break;

            case MoveType.Buff:
                AnimationLoadStruct buffAnimStruct = new AnimationLoadStruct(0, GenericAnimationStates.BUFF, true, true, SameAnimActionEnum.None);
                (this.currentAnimMonitored, this.currentAnimStateMonitored) = source.GetCurrentAnimCtrl().RequestPlayAnimation(buffAnimStruct);
                //this.currentAnimMonitored = currentAnimMonitored;
                //this.currentAnimStateMonitored = currentAnimStateMonitored;
                //currentAnimMonitored.AnimationEndsEvent = OnTurnAnimationEnds;
                currentAnimStateMonitored.AnimationStateEvents = OnAnimationStateEvents;
                break;

            case MoveType.Debuff:
                if (currentAttacker == enemy) { Debug.LogError("Enemy debuffs!"); }
                AnimationLoadStruct debuffAnimStruct = new AnimationLoadStruct(0, GenericAnimationStates.DEBUFF, true, true, SameAnimActionEnum.None);
                (this.currentAnimMonitored, this.currentAnimStateMonitored) = source.GetCurrentAnimCtrl().RequestPlayAnimation(debuffAnimStruct);
                //this.currentAnimMonitored = currentAnimMonitored;
                //this.currentAnimStateMonitored = currentAnimStateMonitored;
                //currentAnimMonitored.AnimationEndsEvent = OnTurnAnimationEnds;
                currentAnimStateMonitored.AnimationStateEvents = OnAnimationStateEvents;
                break;
        }


        target.curHP = Mathf.Clamp(target.curHP, 0, target.baseData.maxHP);
        source.curHP = Mathf.Clamp(source.curHP, 0, source.baseData.maxHP);
    }

    private bool isDebuffing = false;
    private bool isBuffing = false;
    private void OnAnimationStateEvents(AnimEventTypes types)
    {
        if (types == AnimEventTypes.GENERICMOVE)
        {
            if (currentMove.moveType == MoveType.Attack)
            {
                Vector3 targetCenter = Vector3.zero;
                if (currentTarget.curHeight > 0)
                {
                    float h = currentTarget.curHeight / 2f;
                    targetCenter += currentTarget.curTransform.position + new Vector3(0, h, 0);
                }

                damagePower = Mathf.Max(1, currentFinalPower + currentAttacker.curAtt - currentTarget.curDef);
                ParticleEnum particleType = ParticleEnum.EntityDamage;
                CharInstanceParticleTransform cipTransform = currentTarget.charParticleTransformArray[(int)particleType];
                Vector3 particlePos = currentTarget.curTransform.position + (currentTarget.curTransform.up * cipTransform.positionOffset.y);
                if (currentTarget.shieldHP > 0)
                {
                    int reducedDmg = Mathf.RoundToInt(damagePower * (1f - currentTarget.shieldDmgReduc));

                    //Debug.LogError($"{currentTarget.curTransform.name} shields {currentAttacker.curTransform.name}!");
                    if (currentTarget.shieldHP >= reducedDmg)
                    {
                        currentTarget.shieldHP -= reducedDmg;
                        shieldSlider.value = currentTarget.shieldHP;
                        CameraShakeManager.instance.ActivateCamShake(new Vector3(0.25f, 0f, 0f), 0.3f, 0.75f);
                        if (currentTarget.shieldHP < Mathf.Abs(0.001f))
                        {
                            currentTarget.isBlocking = false;
                            Debug.LogError(currentTarget.isBlocking);
                            ParticleFXController shieldParticle = currentTarget.GetCurrentAnimCtrl().GetSpecificActiveAnimationParticle(null, ParticleEnum.EntityShield);
                            //Debug.LogError(shieldParticle);

                            if (shieldParticle != null) { shieldParticle.ForceStop(); }
                            ParticleEnum shieldBreakType = ParticleEnum.EntityShieldHit;
                            CharInstanceParticleTransform particleTransform = currentTarget.charParticleTransformArray[(int)shieldBreakType];
                            Vector3 shieldBreakPos = currentTarget.curTransform.position + (currentTarget.curTransform.up * particleTransform.positionOffset.y);

                            ParticleSpawnData shieldBreakData = new ParticleSpawnData(null, shieldBreakPos, Vector3.zero, cipTransform.scale, shieldBreakType, false, false);
                            ExecuteParticleEffects(shieldBreakData);

                            CameraShakeManager.instance.ActivateCamShake(new Vector3(1f, 0f, 0f), 0.3f, 0.75f);
                        }
                        ExecuteDMGOutput(reducedDmg, targetCenter, AbilityOutputTypes.ShieldDMG);

                    }
                    else
                    {
                        int leftover = reducedDmg - currentTarget.shieldHP;
                        currentTarget.shieldHP = 0;
                        shieldSlider.value = currentTarget.shieldHP;
                        currentTarget.curHP -= leftover;
                        //Debug.LogError($"shield broke! Took {leftover} dmg!");
                        currentTarget.isBlocking = false;
                        //Debug.LogError(currentTarget.isBlocking);
                        ParticleFXController shieldParticle = currentTarget.GetCurrentAnimCtrl().GetSpecificActiveAnimationParticle(null, ParticleEnum.EntityShield);
                        //Debug.LogError(shieldParticle);

                        if (shieldParticle != null) { shieldParticle.ForceStop(); }
                        ParticleEnum shieldBreakType = ParticleEnum.EntityShieldHit;
                        CharInstanceParticleTransform particleTransform = currentTarget.charParticleTransformArray[(int)shieldBreakType];
                        Vector3 shieldBreakPos = currentTarget.curTransform.position + (currentTarget.curTransform.up * particleTransform.positionOffset.y);

                        ParticleSpawnData shieldBreakData = new ParticleSpawnData(null, shieldBreakPos, Vector3.zero, cipTransform.scale, shieldBreakType, false, false);
                        ExecuteParticleEffects(shieldBreakData);

                        ExecuteDMGOutput(reducedDmg, targetCenter, AbilityOutputTypes.ShieldDMG);
                        ExecuteDMGOutput(leftover, targetCenter, AbilityOutputTypes.Damage);

                        CameraShakeManager.instance.ActivateCamShake(new Vector3(1f, 0f, 0f), 0.3f, 0.75f);

                        if (shieldParticle != null) { shieldParticle.ForceStop(); }
                    }
                }
                else
                {
                    currentTarget.curHP -= damagePower;
                    ParticleSpawnData data = new ParticleSpawnData(null, particlePos, Vector3.zero, cipTransform.scale, particleType, false, false);

                    ExecuteDMGOutput(damagePower, targetCenter, AbilityOutputTypes.Damage);

                    ExecuteParticleEffects(data);
                }
                ExecuteAbility();
                //currentTarget.curHP -= damagePower;
                //Debug.LogError($"{currentAttacker.curTransform.name} is attacking {currentTarget.curTransform.name}!");


                if (currentAttacker == player) { CameraShakeManager.instance.ActivateCamShake(new Vector3(0f, 1f, 0f)); }
                else { CameraShakeManager.instance.ActivateCamShake(new Vector3(1f, 1.5f, 0f), 0.3f, 0.75f); }


                if (currentTarget.curHP < Mathf.Abs(0.001f))
                {
                    if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(gameplaySounds[2], false, Vector3.zero)); }
                    currentTarget.curTransform.gameObject.SetActive(false);
                }
                else
                {
                    if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(gameplaySounds[1], false, Vector3.zero)); }
                }
            }

            else if (currentMove.moveType == MoveType.Defend)
            {
                currentAttacker.isBlocking = true;
                int shieldAmount = Mathf.RoundToInt(currentFinalPower * 0.2f);
                currentAttacker.shieldHP += shieldAmount;
                currentAttacker.shieldHP = Mathf.Clamp(currentAttacker.shieldHP, 0, maxShieldHP);

                shieldSlider.value = currentAttacker.shieldHP;

                Vector3 targetCenter = Vector3.zero;
                if (currentAttacker.curHeight > 0)
                {
                    float h = currentAttacker.curHeight / 2f;
                    targetCenter += currentAttacker.curTransform.position + new Vector3(0, h, 0);
                }

                ExecuteDMGOutput(shieldAmount, targetCenter, AbilityOutputTypes.Shield);
                ExecuteAbility();
                //Debug.Log($"{this} shield {shieldAmount} HP!");
                //Debug.Log(currentAttacker.shieldHP);
                /*if (currentAttacker.isBlocking)
                {
                    ParticleFXController shieldParticle = currentTarget.GetCurrentAnimCtrl().GetSpecificActiveAnimationParticle(null, ParticleEnum.EntityShield);
                    //Debug.LogError(shieldParticle);

                    if (shieldParticle != null) { shieldParticle.ForceStop(); }
                    ParticleEnum particleType = ParticleEnum.EntityShieldHit;
                    CharInstanceParticleTransform cipTransform = currentTarget.charParticleTransformArray[(int)particleType];
                    Vector3 particlePos = currentTarget.curTransform.position + (currentTarget.curTransform.up * cipTransform.positionOffset.y);

                    ParticleSpawnData data = new ParticleSpawnData(null, particlePos, Vector3.zero, cipTransform.scale, particleType, false, false);
                    ExecuteParticleEffects(data);

                    CameraShakeManager.instance.ActivateCamShake(new Vector3(1f, 0f, 0f), 0.3f, 0.75f);
                }*/
            }
            else if (currentMove.moveType == MoveType.Debuff)
            {
                //currentAttacker.ApplyMoveEffect(currentMove, false, currentTarget, currentFinalPower);
                if (!currentTarget.isBlocking)
                {
                    currentAttacker.ApplyMoveEffect(currentMove, false, currentTarget, currentFinalPower);
                    isDebuffing = true;
                    ExecuteAbility();
                    //debuffIndicator.gameObject.SetActive(true);
                }
                /*if (debuffTurnCount == 3)
                {
                    debuffIndicator.gameObject.SetActive(false);
                    //isDebuffing = false;
                }*/

            }
            else if (currentMove.moveType == MoveType.Heal)
            {
                currentAttacker.ApplyMoveEffect(currentMove, false, currentTarget, currentFinalPower);
            }
            else if (currentMove.moveType == MoveType.Buff)
            {
                ExecuteAbility();
                currentAttacker.ApplyMoveEffect(currentMove, false, null, currentFinalPower);
                isBuffing = true;
                //buffIndicator.gameObject.SetActive(true);
                if (buffTurnCount == 3)
                {
                    //buffIndicator.gameObject.SetActive(false);
                    isBuffing = false;
                }
            }
        }

        OnTurnAnimationEnds();
        UpdateHPUI();
    }


    void ExecuteAbility()
    {
        if (currentMove == null || currentAttacker == null) return;
        if (currentAttacker.curTransform.childCount < 1) return;
        Transform casterTransform = currentAttacker.curTransform;
        BaseAbilityInstance baseAbilityInstance = null;
        for (int i = 0; i < casterTransform.childCount; i++)
        {
            casterTransform.GetChild(i).transform.TryGetComponent(out baseAbilityInstance);
            if (baseAbilityInstance == null) continue;
            if (baseAbilityInstance.GetMoveType() != currentMove.moveType) continue;
            if (baseAbilityInstance.GetAffectedStatType() != currentMove.affectedStat) continue;
            baseAbilityInstance.ExecuteAbility();
        }
    }
    
    private void debuffTurnCounter()
    {
        battleLog.text = $"Turn {debuffTurnCount}";
        debuffTurnCount++;
    }

    private void buffTurnCounter()
    {
        battleLog.text = $"Turn {buffTurnCount}";
        buffTurnCount++;
    }

    private void OnTurnAnimationEnds()
    {
        currentAnimMonitored.AnimationEndsEvent -= OnTurnAnimationEnds;
        currentAnimStateMonitored.AnimationStateEvents -= OnAnimationStateEvents;
        currentAnimMonitored = null;
        currentAnimStateMonitored = null;
        isAnimationDone = true;
        Debug.LogWarning("Animation ended!");
    }

    void ExecuteParticleEffects(ParticleSpawnData data)
    {
        if (ParticlePoolManager.instance == null) return;
        ParticlePoolManager.instance.ActivateParticleFX(data);
    }

    void ExecuteDMGOutput(int dmg, Vector3 pos, AbilityOutputTypes colorType)
    {
        if (DMGOutputPoolManager.instance == null) return;
        DMGOutputPoolManager.instance.RequestActivateDMGOutput(dmg, pos, colorType);
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
            $"DEF: {player.curDef}\n" +
            $"SHD: {player.shieldHP}";

        enemyStats.text =
            $"HP: {enemy.curHP}/{enemy.baseData.maxHP}\n" +
            $"ATK: {enemy.curAtt}\n" +
            $"DEF: {enemy.curDef}\n" +
            $"SHD: {enemy.shieldHP}";
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
