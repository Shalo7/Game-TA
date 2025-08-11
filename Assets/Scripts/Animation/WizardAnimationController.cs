using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using UnityEngine;

public class WizardAnimationController : BaseAnimationController
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
            { GenericAnimationStates.IDLE, new w_IdleAnimationInstance(this) },
            { GenericAnimationStates.ATTACK, new w_AttackAnimationInstance(this) },
            { GenericAnimationStates.DEF, new w_DefendAnimationInstance(this) },
            { GenericAnimationStates.HEAL, new w_HealAnimationInstance(this) }
        };
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(0, GenericAnimationStates.IDLE, false, false, SameAnimActionEnum.None);
        RequestPlayAnimation(loadStruct);
    }
}
