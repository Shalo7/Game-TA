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
        if (targetInstance.shieldHP > 0)
        {
            nextMove = GetMoves(MoveType.Attack);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            return;
        }
        if (predictedDamage >= targetInstance.curHP && targetInstance.shieldHP <= 0)
        {
            nextMove = GetMoves(MoveType.Attack);
            if (nextMove == null) return;
            BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
            return;
        }
        float choicePercentage = Random.Range(0f, 1f);
        nextMove = (choicePercentage > 0.5f) ? GetMoves(MoveType.Attack) : GetMoves(MoveType.Debuff);
        if (nextMove == null) return;
        BattleSystem.instance.ExecuteMove(charaInstance, targetInstance, nextMove, nextMove.power);
    }

    protected override Moves GetMoves(MoveType moveType)
    {
        if (!moveSet.TryGetValue(moveType, out Moves move)) return null;
        return move;
    }

    protected override int PredictDamage(MoveType moveType)
    {
        if (!moveSet.TryGetValue(moveType, out Moves move)) return 0;
        int modifiedPower = Mathf.RoundToInt(move.power);
        int hypotheticalDamage = Mathf.Max(1, modifiedPower + charaInstance.curAtt - targetInstance.curDef);
        return hypotheticalDamage;
    }
}
