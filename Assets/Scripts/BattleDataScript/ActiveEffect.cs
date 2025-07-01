using UnityEngine;

public class ActiveEffect
{
    public StatType stat;
    public int amount;
    public int duration;

    public ActiveEffect(StatType stat, int amount, int duration)
    {
        this.stat = stat;
        this.amount = amount;
        this.duration = duration;
    }
}
