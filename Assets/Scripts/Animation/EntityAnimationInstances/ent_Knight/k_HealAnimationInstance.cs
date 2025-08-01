using UnityEngine;

public class k_HealAnimationInstance : AnimationStateInstance
{
    public k_HealAnimationInstance(BaseBodyPartHandler bodyParts, BaseAnimationController bac) : base(bodyParts, bac) { }

    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:

                break;
            case 0:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
