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
        bodyParts = GetComponent<BaseBodyPartHandler>();
        stateInstances = new Dictionary<GenericAnimationStates, AnimationStateInstance>()
        {
            {GenericAnimationStates.IDLE, new k_IdleAnimationInstance(this)},
            {GenericAnimationStates.ATTACK, new k_AttackAnimationInstance(this)},
            {GenericAnimationStates.DEF, new k_DefendAnimationInstance(this)},
            {GenericAnimationStates.DEBUFF, new k_DebuffAnimationInstance(this)},
            { GenericAnimationStates.HEAL, new k_HealAnimationInstance(this)}

        };
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(0, GenericAnimationStates.IDLE, false, false, SameAnimActionEnum.None);
        RequestPlayAnimation(loadStruct);
    }
}
