using System;
using UnityEngine;

public abstract class AnimationStateInstance
{
    public Action<AnimEventTypes> AnimationStateEvents;
    protected BaseBodyPartHandler bodyParts;
    protected BaseAnimationController animCtrl;

    public AnimationStateInstance(BaseBodyPartHandler bodyParts, BaseAnimationController bac)
    {
        this.bodyParts = bodyParts;
        this.animCtrl = bac;
    }

    public abstract void OnAnimationEvent(int index);
}
