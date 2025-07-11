using System.Collections.Generic;
using UnityEngine;

public class CharaInstance
{
    public Charas baseData;

    public int curHP;
    public int curAtt;
    public int curDef;
    public int curSpd;

    public bool isBlocking = false;

    private List<ActiveEffect> activeEffects = new();

    public CharaInstance(Charas baseData)
    {
        this.baseData = baseData;
        ResetStats();
    }

    public void ResetStats()
    {
        curHP = baseData.maxHP;
        curAtt = baseData.attack;
        curDef = baseData.defense;
        curSpd = baseData.speed;
        activeEffects.Clear();
    }

    public void ApplyMoveEffect(Moves move, bool isFromEnemy, CharaInstance target = null, int overridePower = -1)
    {
        int finalPower = overridePower > -1 ? overridePower : move.power;

        switch (move.moveType)
        {
            case MoveType.Attack:
                // handled in battle logic
                break;

            case MoveType.Defend:
                isBlocking = true;
                break;
            case MoveType.Buff:
                ApplyStatEffect(move.affectedStat, finalPower, move.duration, move);
                break;
            case MoveType.Debuff:
                if (target != null)
                    target.ApplyStatEffect(move.affectedStat, finalPower, move.duration, move);
                break;

            case MoveType.Heal:
                curHP += finalPower;
                curHP = Mathf.Min(curHP, baseData.maxHP);
                break;
        }
    }

    private void ApplyStatEffect(StatType stat, int amount, int duration, Moves move)
    {
        int effectiveAmount = amount;

        if (move.isPercentageChange)
        {
            int baseStat = 0;
            switch (stat)
            {
                case StatType.Attack: baseStat = baseData.attack; break;
                case StatType.Defense: baseStat = baseData.defense; break;
            }

            effectiveAmount = Mathf.RoundToInt(baseStat * (amount / 100f));
        }

        //Apply stat change
        switch (stat)
        {
            case StatType.Attack: curAtt += effectiveAmount; break;
            case StatType.Defense: curDef += effectiveAmount; break;
        }


        if (duration != -1)
        {
            activeEffects.Add(new ActiveEffect(stat, effectiveAmount, duration));
        }
    }

    public void OnTurnEnd()
    {
        List<ActiveEffect> expiredEffects = new();

        foreach (var effect in activeEffects)
        {
            effect.duration--;

            if (effect.duration <= 0)
            {
                switch (effect.stat)
                {
                    case StatType.Attack: curAtt -= effect.amount; break;
                    case StatType.Defense: curDef -= effect.amount; break;
                }

                expiredEffects.Add(effect);
            }
        }

        foreach (var effect in expiredEffects)
        {
            activeEffects.Remove(effect);
        }
    }

    public bool IsFainted()
    {
        return curHP <= 0;
    }
}
