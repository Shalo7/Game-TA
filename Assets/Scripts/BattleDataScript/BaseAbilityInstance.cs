using UnityEngine;

public abstract class BaseAbilityInstance : MonoBehaviour
{
    [SerializeField] protected MoveType movetype;
    public MoveType GetMoveType() => movetype;
    [SerializeField] protected StatType affectedStat;
    public StatType GetAffectedStatType() => affectedStat;
    
    public abstract void ExecuteAbility();
}
