using System;
using UnityEngine;

public abstract class AnimationStateInstance
{
    public event EventHandler OnAnimationDoneEvent;
    public AnimationStateInstance() { }
    public abstract void OnAnimationEvent(int index);
}
