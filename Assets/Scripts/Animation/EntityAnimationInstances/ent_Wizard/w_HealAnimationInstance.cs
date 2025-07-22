using UnityEngine;

public class w_HealAnimationInstance : AnimationStateInstance
{
    public w_HealAnimationInstance(BaseBodyPartHandler bodyParts) : base(bodyParts) { }

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
