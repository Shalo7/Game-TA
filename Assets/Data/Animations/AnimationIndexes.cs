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

public enum SameAnimActionEnum
{
    Override,
    Unpause,
    None
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

    public static readonly Dictionary<SceneTransitionEnums, int> TransitionHashes = new Dictionary<SceneTransitionEnums, int>
    {
        {SceneTransitionEnums.ST_EMPTY, Animator.StringToHash("Empty")},
        {SceneTransitionEnums.ST_FILLED, Animator.StringToHash("Filled")},
        {SceneTransitionEnums.ST_LEFTENTER, Animator.StringToHash("Left_Enter")},
        {SceneTransitionEnums.ST_RIGHTEXIT, Animator.StringToHash("Right_Exit")},
        {SceneTransitionEnums.ST_RIGHTENTER, Animator.StringToHash("Right_Enter")},
        {SceneTransitionEnums.ST_LEFTEXIT, Animator.StringToHash("Left_Exit")}
    };
}
