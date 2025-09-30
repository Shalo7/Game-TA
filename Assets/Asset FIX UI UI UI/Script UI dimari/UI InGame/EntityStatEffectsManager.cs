using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatEffectsManager : MonoBehaviour
{
    [SerializeField] CharType charType;
    public CharType GetCharacterType() => charType;
    [SerializeField] List<EntityStatEffectHolder> statEffects = new List<EntityStatEffectHolder>();
    [SerializeField] Sprite attackBuffIMG;
    [SerializeField] Sprite attackDebuffIMG;
    [SerializeField] Sprite defBuffIMG;
    [SerializeField] Sprite defDebuffIMG;

    void Start()
    {
        foreach (Transform chld in transform)
        {
            if (!chld.TryGetComponent(out EntityStatEffectHolder eh)) continue;
            statEffects.Add(eh);
        }
    }

    public void UpdateStatEffectUI(Moves targetType, bool isXpired)
    {
        if (statEffects.Count < 1) return;
        //Debug.LogError($"{targetType.affectedStat} & {targetType.moveType}");
        if (!isXpired)
        {
            for (int i = 0; i < statEffects.Count; i++)
            {
                EntityStatEffectHolder index = statEffects[i];
                if (index.GetCurrentMove().affectedStat == targetType.affectedStat && index.GetCurrentMove().affectedStat != StatType.None) { Debug.LogError("Continued!"); continue; }
                if (index.GetCurrentMove().affectedStat == StatType.None)
                {
                    if (targetType.moveType == MoveType.Buff && targetType.affectedStat == StatType.Attack)
                    {
                        index.UpdateStatEffect(targetType, attackBuffIMG, isXpired);
                    }
                    else if (targetType.moveType == MoveType.Debuff && targetType.affectedStat == StatType.Attack)
                    {
                        index.UpdateStatEffect(targetType, attackDebuffIMG, isXpired);
                    }
                    else if (targetType.moveType == MoveType.Buff && targetType.affectedStat == StatType.Defense)
                    {
                        index.UpdateStatEffect(targetType, defBuffIMG, isXpired);
                    }
                    else if (targetType.moveType == MoveType.Debuff && targetType.affectedStat == StatType.Defense)
                    {
                        index.UpdateStatEffect(targetType, defDebuffIMG, isXpired);
                    }
                    //Debug.LogError($"Success at {index.gameObject.name} of parent {index.transform.parent.name}! With affected type {index.GetCurrentMove().affectedStat}");
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < statEffects.Count; i++)
            {
                EntityStatEffectHolder index = statEffects[i];
                if (index.GetCurrentMove().affectedStat == targetType.affectedStat && index.GetCurrentMove().affectedStat != StatType.None)
                {
                    index.UpdateStatEffect(targetType, null, isXpired);
                    break;
                }
            }
        }
        
    }
}
