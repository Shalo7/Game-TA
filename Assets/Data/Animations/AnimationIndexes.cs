using System.Collections.Generic;
using UnityEngine;


public enum GenericAnimationEnums
{
    IDLE,
    ATTACK,
    DEF,
    HEAL,
    DEATH,
    NONE
}

public static class AnimationIndexes
{
    public static readonly Dictionary<GenericAnimationEnums, int> GenericAnimationDict = new Dictionary<GenericAnimationEnums, int>
    {
        {GenericAnimationEnums.IDLE, Animator.StringToHash("IDLE")},
        {GenericAnimationEnums.ATTACK, Animator.StringToHash("ATTACK")},
        {GenericAnimationEnums.DEF, Animator.StringToHash("DEF")},
        {GenericAnimationEnums.HEAL, Animator.StringToHash("HEAL")},
        {GenericAnimationEnums.DEATH, Animator.StringToHash("DEATH")},
        {GenericAnimationEnums.NONE, Animator.StringToHash("IDLE")}
    };
}
