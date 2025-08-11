using UnityEngine;

public class k_DebuffAnimationInstance : AnimationStateInstance
{
    public k_DebuffAnimationInstance(BaseBodyPartHandler bodyParts, BaseAnimationController bac) : base(bodyParts, bac) { }
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
