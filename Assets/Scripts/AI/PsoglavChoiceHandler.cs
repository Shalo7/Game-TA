using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PsoglavChoiceHandler : BaseEnemyChoiceHandler
{
    public override void DecideTurn()
    {
        if (BattleSystem.instance == null) return;
        if (charaInstance == null || targetInstance == null) return;
        Moves nextMove = null;
        int predictedDamage = PredictDamage(MoveType.Attack);
        int attackMoveTrackCount = CountMoveInMoveTracker(MoveType.Attack, StatType.None);
        int healMoveTrackCount = CountMoveInMoveTracker(MoveType.Heal, StatType.None);
        if (predictedDamage >= targetInstance.curHP && targetInstance.shieldHP <= 0)
        {
            nextMove = GetMoves(MoveType.Attack, StatType.None);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            UpdateMoveTracker(nextMove);
            return;
        }
        if (attackMoveTrackCount >= 3 && healMoveTrackCount < 1)
        {
            nextMove = GetMoves(MoveType.Heal, StatType.None);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            UpdateMoveTracker(nextMove);
            return;
        }

        nextMove = GetMoves(MoveType.Attack, StatType.None);
        if (nextMove == null) return;
        BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
        UpdateMoveTracker(nextMove);
    }

    public override void InitializeScript(CharaInstance cI)
    {
        charaInstance = cI;
        //moveSet = charaInstance.baseData.moves.ToDictionary(entry => entry.moveType , entry => entry);

        targetInstance = charaInstance?.targetTransform?.GetComponent<CharacterMarker>().GetCharacterInstance();
        if (targetInstance == null) Debug.LogError("No target instance!!!");
        targetMoveSet = targetInstance.baseData.moves.ToDictionary(entry => entry.moveType, entry => entry);
        moveTracker = new List<Moves>();
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

    protected override MoveType EnemyMovePrediction()
    {
        throw new System.NotImplementedException();
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

    protected override MoveType PredictMoveHighShieldHP()
    {
        throw new System.NotImplementedException();
    }

    protected override MoveType PredictMoveLowShieldHP()
    {
        throw new System.NotImplementedException();
    }

    protected override void UpdateMoveTracker(Moves m)
    {
        if (moveTracker.Count == maxMoveTracked)
        { moveTracker.RemoveAt(0); }

        moveTracker.Add(m);
    }

    protected override Moves GetOldestMoveTracked()
    {
        if (moveTracker.Count < 1) return null;
        return moveTracker[0];
    }
}