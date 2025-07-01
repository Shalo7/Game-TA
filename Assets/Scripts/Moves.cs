using UnityEngine;

public enum MoveType
{
    Attack,
    Defend,
    Heal,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "Moves", menuName = "Scriptable Objects/Moves")]
public class Moves : ScriptableObject
{
    public string moveName;
    public MoveType moveType;

    public int power; // for damage, healing, or stat amount
    public string desc;

    public StatType affectedStat;
    public bool isPercentageChange = false;
    public int statChangeAmount;
    public int duration;
}

public enum StatType
{
    Attack,
    Defense
}
