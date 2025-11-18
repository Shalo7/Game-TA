using System.Linq;
using UnityEngine;

public class CultistChoiceHandler : BaseEnemyChoiceHandler
{
    //cultist bergerak berdasarkan prediksi gerakan pemain. Prediksi ini berdasarkan health pemain, jika pemain akan attack karena healthnya aman maka dia akan buff defense. Jika pemain akan heal/shield dia akan buff attack. Namun, akan ada percentage di mana dia akan attack setelah memilih buff.
    public override void DecideTurn()
    {
        if (BattleSystem.instance == null) return;
        if (charaInstance == null || targetInstance == null) return;
        
        Moves nextMove = null;
        MoveType targetPredictedMove = EnemyMovePrediction();
        Debug.LogError(targetPredictedMove);
        int predictedDamage = PredictDamage(MoveType.Attack);
        if (predictedDamage >= targetInstance.curHP && targetInstance.shieldHP <= 0)
        {
            nextMove = GetMoves(MoveType.Attack, StatType.None);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            return;
        }

        switch(targetPredictedMove)
        {
            case MoveType.None:
                nextMove = GetMoves(MoveType.Attack, StatType.None);
                break;
            case MoveType.Attack:
                nextMove = GetMoves(MoveType.Buff, StatType.Defense);
                break;
            case MoveType.Heal:
                nextMove = GetMoves(MoveType.Attack, StatType.None);
                break;
            case MoveType.Defend:
                nextMove = GetMoves(MoveType.Buff, StatType.Attack);
                break;
        }

        if(nextMove.moveType != MoveType.Attack)
        {
            float randomChangeAttack = Random.Range(0f, 1f);
            if (randomChangeAttack < 0.3f)
            {
                nextMove = GetMoves(MoveType.Attack, StatType.None);
                if (nextMove == null) return;
                BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
                return;
            } 
        }

        if (nextMove == null) return;
        BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
    }

    public override void InitializeScript(CharaInstance cI)
    {
        charaInstance = cI;
        //moveSet = charaInstance.baseData.moves.ToDictionary(entry => entry.moveType , entry => entry);

        targetInstance = charaInstance?.targetTransform?.GetComponent<CharacterMarker>().GetCharacterInstance();
        if (targetInstance == null) Debug.LogError("No target instance!!!");
        targetMoveSet = targetInstance.baseData.moves.ToDictionary(entry => entry.moveType, entry => entry);
    }

    protected override Moves GetMoves(MoveType moveType, StatType statType)
    {
        Moves nextMove = null;
        foreach(Moves m in charaInstance.baseData.moves)
        {
            if (m.moveType != moveType) continue;
            if (m.affectedStat != statType) continue;
            nextMove = m;
            break;
        }
        return nextMove;
    }

    protected override int PredictDamage(MoveType moveType)
    {
        Moves move = null;
        foreach (Moves m in charaInstance.baseData.moves)
        {
            if (m.moveType != moveType) continue;
            move = m;
        }
        int modifiedPower = Mathf.RoundToInt(move.power);
        int hypotheticalDamage = Mathf.Max(1, modifiedPower + charaInstance.curAtt - targetInstance.curDef);
        return hypotheticalDamage;
    }

    protected override MoveType EnemyMovePrediction()
    {
        if (targetInstance == null) return MoveType.None;
        //if (targetMoveSet.Count < 1) return MoveType.None;

        MoveType predictedNextMove;

        float maxTargetHP = targetInstance.baseData.maxHP;
        float currentTargetHealthPercent = targetInstance.curHP/maxTargetHP;
        
        //Health value ketika player mulai heal/shield
        float highHPThreshold = 0.65f;
        float lowHPThreshold = 0.35f;

        //Probabilitas heal/shield di high HP (0.65)
        float probShieldHighHP = 0.65f;
        float probHealHighHP = 0.35f;
        
        //Probabilitas heal/shield di low HP (0.35)
        float probShieldLowHP = 0.35f;
        float probHealLowHP = 0.65f;

        float currentShieldProb = 0f;
        float currentHealProb = 0f;

        if (currentTargetHealthPercent > highHPThreshold)
        {
            float randomValue = Random.Range(0f,1f);

            return (randomValue < 0.5f) ? MoveType.Attack : MoveType.None;
        }
        else if (currentTargetHealthPercent == highHPThreshold)
        {
            currentShieldProb = probShieldHighHP;
            currentHealProb = probHealHighHP;
        }
        else if (currentTargetHealthPercent <= lowHPThreshold)
        {
            currentShieldProb = probShieldLowHP;
            currentHealProb = probHealLowHP;
        }
        else
        {
            float t = Mathf.InverseLerp(highHPThreshold, lowHPThreshold, currentTargetHealthPercent);
            currentShieldProb = Mathf.Lerp(probShieldHighHP, probShieldLowHP, t);
            currentHealProb = Mathf.Lerp(probHealHighHP, probHealLowHP, t);
        }
        Debug.LogError($"Shield probability is {currentShieldProb} and Heal probability is {currentHealProb}!");
        
        float attackOverrideChance = 0.3f;
        float randomValOverride = Random.Range(0f, 1f);
        if (randomValOverride < attackOverrideChance) return MoveType.Attack;

        float randomVal = Random.Range(0f, 1f);
        if (randomVal < currentShieldProb)
        {
            predictedNextMove = MoveType.Defend;
        }
        else
        {
            predictedNextMove = MoveType.Heal;
        }

        return predictedNextMove;
    }

    protected override MoveType PredictMoveHighShieldHP()
    {
        return MoveType.None;
    }

    protected override MoveType PredictMoveLowShieldHP()
    {
        return MoveType.None;
    }
}
