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
        bodyParts = GetComponent<WizardBodyPartsHandler>();
        stateInstances = new Dictionary<GenericAnimationStates, AnimationStateInstance>()
        {
            { GenericAnimationStates.IDLE, new w_IdleAnimationInstance(bodyParts) },
            { GenericAnimationStates.ATTACK, new w_AttackAnimationInstance(bodyParts) },
            { GenericAnimationStates.DEF, new w_DefendAnimationInstance(bodyParts) },
            { GenericAnimationStates.HEAL, new w_HealAnimationInstance(bodyParts) }
        };
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(0, GenericAnimationStates.IDLE, false, false);
        RequestPlayAnimation(loadStruct);
    }
}
