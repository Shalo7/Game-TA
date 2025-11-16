using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemyChoiceHandler : MonoBehaviour
{
    [SerializeField] protected CharaInstance charaInstance;
    [SerializeField] protected Dictionary<MoveType, Moves> moveSet;
    protected CharaInstance targetInstance;

    public abstract void InitializeScript(CharaInstance cI);
    public abstract void DecideTurn();
    protected abstract Moves GetMoves(MoveType moveType);
    protected abstract int PredictDamage(MoveType moveType); 
}