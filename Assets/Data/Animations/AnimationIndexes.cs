using System.Collections.Generic;
using UnityEngine;


public enum GenericAnimationStates
{
    IDLE,
    ATTACK,
    DEF,
    HEAL,
    BUFF,
    DEBUFF,
    DEATH,
    NONE
}

public enum AnimEventTypes
{
    GENERICMOVE,
    POWERUP
}

public static class AnimationIndexes
{
    public static readonly Dictionary<GenericAnimationStates, int> GenericAnimationDict = new Dictionary<GenericAnimationStates, int>
    {
        {GenericAnimationStates.IDLE, Animator.StringToHash("IDLE")},
        {GenericAnimationStates.ATTACK, Animator.StringToHash("ATTACK")},
        {GenericAnimationStates.DEF, Animator.StringToHash("DEF")},
        {GenericAnimationStates.HEAL, Animator.StringToHash("HEAL")},
        {GenericAnimationStates.BUFF, Animator.StringToHash("BUFF")},
        {GenericAnimationStates.DEBUFF, Animator.StringToHash("DEBUFF")},
        {GenericAnimationStates.DEATH, Animator.StringToHash("DEATH")},
        {GenericAnimationStates.NONE, Animator.StringToHash("IDLE")}
    };
}
