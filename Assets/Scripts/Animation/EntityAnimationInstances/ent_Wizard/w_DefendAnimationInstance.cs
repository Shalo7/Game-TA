using UnityEngine;

public class w_DefendAnimationInstance : AnimationStateInstance
{
    public w_DefendAnimationInstance(BaseBodyPartHandler bodyParts, BaseAnimationController bac) : base(bodyParts, bac) { }

    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:
                Debug.LogError("No anim event index!");
                break;
            case 0:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
