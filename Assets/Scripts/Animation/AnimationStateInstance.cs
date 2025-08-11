using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationStateInstance
{
    public Action<AnimEventTypes> AnimationStateEvents;
    protected BaseAnimationController animCtrl;

    public AnimationStateInstance(BaseAnimationController bac)
    {
        this.animCtrl = bac;
    }

    public abstract void OnAnimationEvent(int index);
}
