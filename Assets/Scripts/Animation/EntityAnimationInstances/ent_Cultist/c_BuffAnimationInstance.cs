using UnityEngine;

public class c_BuffAnimationInstance : AnimationStateInstance
{
    public c_BuffAnimationInstance(BaseAnimationController animationController) : base(animationController) {}
    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:
                Debug.LogError("No anim index!");
                break;
            case 0:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
