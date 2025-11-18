using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemyChoiceHandler : MonoBehaviour
{
    [SerializeField] protected CharaInstance charaInstance;
    [SerializeField] protected Dictionary<MoveType, Moves> moveSet;
    [SerializeField] protected Dictionary<MoveType, Moves> targetMoveSet;
    protected CharaInstance targetInstance;

    public abstract void InitializeScript(CharaInstance cI);
    public abstract void DecideTurn();
    protected abstract Moves GetMoves(MoveType moveType, StatType statType);
    protected abstract int PredictDamage(MoveType moveType); 
    protected abstract MoveType EnemyMovePrediction();
    protected abstract MoveType PredictMoveHighShieldHP();
    protected abstract MoveType PredictMoveLowShieldHP();
}