using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using UnityEngine;

public class CultistAnimationController : BaseAnimationController
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
            {GenericAnimationStates.IDLE, new c_IdleAnimationInstance(this)},
            {GenericAnimationStates.ATTACK, new c_AttackAnimationInstance(this)},
            {GenericAnimationStates.BUFF, new c_BuffAnimationInstance(this)}

        };
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(0, GenericAnimationStates.IDLE, false, false, SameAnimActionEnum.None);
        RequestPlayAnimation(loadStruct);
    }
}
