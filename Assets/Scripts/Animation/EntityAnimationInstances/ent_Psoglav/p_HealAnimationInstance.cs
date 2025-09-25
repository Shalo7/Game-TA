using UnityEngine;

public class p_HealAnimationInstance : AnimationStateInstance
{
    public p_HealAnimationInstance(BaseAnimationController bac) : base(bac) { }

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
