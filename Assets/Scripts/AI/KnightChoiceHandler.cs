using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KnightChoiceHandler : BaseEnemyChoiceHandler
{
    public override void InitializeScript(CharaInstance cI)
    {
        charaInstance = cI;
        moveSet = charaInstance.baseData.moves.ToDictionary(entry => entry.moveType , entry => entry);

        targetInstance = charaInstance?.targetTransform?.GetComponent<CharacterMarker>().GetCharacterInstance();
        if (targetInstance == null) Debug.LogError("No target instance!!!");
    }

    public override void DecideTurn()
    {
        if (BattleSystem.instance == null) return;
        if (charaInstance == null || targetInstance == null) return;
        Moves nextMove = null;
        int predictedDamage = PredictDamage(MoveType.Attack);
        int debuffTrackedCount = CountMoveInMoveTracker(MoveType.Debuff, StatType.Defense);
        if (targetInstance.shieldHP > 0)
        {
            nextMove = GetMoves(MoveType.Attack, StatType.None);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            UpdateMoveTracker(nextMove);
            return;
        }
        if (predictedDamage >= targetInstance.curHP && targetInstance.shieldHP <= 0)
        {
            nextMove = GetMoves(MoveType.Attack, StatType.None);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            UpdateMoveTracker(nextMove);
            return;
        }

        if (debuffTrackedCount < 1)
        {
            float choicePercentage = Random.Range(0f, 1f);
            nextMove = (choicePercentage > 0.5f) ? GetMoves(MoveType.Attack, StatType.None) : GetMoves(MoveType.Debuff, StatType.Defense);    
        }
        else
        {
            nextMove = GetMoves(MoveType.Attack, StatType.None);
        }


        if (nextMove == null) return;
        BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
        UpdateMoveTracker(nextMove);
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
        return MoveType.None;
    }

    protected override MoveType PredictMoveHighShieldHP()
    {
        return MoveType.None;
    }

    protected override MoveType PredictMoveLowShieldHP()
    {
        return MoveType.None;
    }

    protected override int CountMoveInMoveTracker(MoveType mT, StatType sT)
    {
        if (moveTracker.Count < 1) return 0;
        int counter = 0;
        foreach(Moves m in moveTracker)
        {
            if (mT != m.moveType) continue;
            if (sT != m.affectedStat) continue;
            counter++;
        }
        return counter;
    }

    protected override void UpdateMoveTracker(Moves m)
    {
        if (moveTracker.Count == maxMoveTracked)
        { moveTracker.RemoveAt(0); }

        moveTracker.Add(m);
    }

    protected override Moves GetOldestMoveTracked()
    {
        throw new System.NotImplementedException();
    }
}
