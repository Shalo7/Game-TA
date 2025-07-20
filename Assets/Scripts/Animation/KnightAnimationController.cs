using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using UnityEngine;

public class KnightAnimationController : BaseAnimationController
{
    void OnEnable()
    {
        AnimInitialization();
    }

    protected override void AnimInitialization()
    {
        if (stateInstances.Count > 0) return;
        animator = GetComponent<Animator>();
        bodyParts = GetComponent<KnightBodyPartsHandler>();
        stateInstances = new Dictionary<GenericAnimationStates, AnimationStateInstance>()
        {
            {GenericAnimationStates.IDLE, new k_IdleAnimationInstance(bodyParts)},
            {GenericAnimationStates.ATTACK, new k_AttackAnimationInstance(bodyParts)},
            {GenericAnimationStates.DEF, new k_DefendAnimationInstance(bodyParts)},
            {GenericAnimationStates.HEAL, new k_HealAnimationInstance(bodyParts)}
        };
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(0, GenericAnimationStates.IDLE, false, false);
        RequestPlayAnimation(loadStruct);
    }
}
