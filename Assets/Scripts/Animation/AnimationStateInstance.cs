using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationStateInstance
{
    public Action<AnimEventTypes> AnimationStateEvents;
    protected BaseAnimationController animationController;

    public AnimationStateInstance(BaseAnimationController bac)
    {
        this.animationController = bac;
    }

    public abstract void OnAnimationEvent(int index);
}
