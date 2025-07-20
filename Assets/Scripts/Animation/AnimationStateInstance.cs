using System;
using UnityEngine;

public abstract class AnimationStateInstance
{
    public Action<AnimEventTypes> AnimationStateEvents;
    protected BaseBodyPartHandler bodyParts;
    public AnimationStateInstance(BaseBodyPartHandler bodyParts)
    { this.bodyParts = bodyParts; }
    public abstract void OnAnimationEvent(int index);
}
