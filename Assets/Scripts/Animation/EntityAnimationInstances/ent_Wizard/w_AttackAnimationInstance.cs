using UnityEngine;

public class w_AttackAnimationInstance : AnimationStateInstance
{
    public w_AttackAnimationInstance(BaseBodyPartHandler bodyParts) : base(bodyParts) { }
    
    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:

                break;
            case 0:
                Debug.Log("V'Ataku!");
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;

        }
    }
}
