using UnityEngine;

public class ActiveEffect
{
    public Moves move;
    public int amount;
    public int duration;

    public ActiveEffect(Moves move, int amount, int duration)
    {
        this.move = move;
        this.amount = amount;
        this.duration = duration;
    }
}
