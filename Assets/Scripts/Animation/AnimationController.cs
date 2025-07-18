using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using UnityEngine;

public class AnimationController : BaseAnimationController
{
    void OnEnable()
    {
        AnimInitialization();
    }

    protected override void AnimInitialization()
    {
        if (stateInstances.Count > 0) return;
        animator = GetComponent<Animator>();
        stateInstances = new Dictionary<GenericAnimationEnums, AnimationStateInstance>()
        {
            {GenericAnimationEnums.IDLE, new w_IdleAnimationInstance()},
            { GenericAnimationEnums.ATTACK, new w_AttackAnimationStance()}
        };
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(0, GenericAnimationEnums.IDLE, false, false);
        RequestPlayAnimation(loadStruct);
    }
}
