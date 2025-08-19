using UnityEngine;

public class c_AttackAnimationInstance : AnimationStateInstance
{
    public c_AttackAnimationInstance(BaseAnimationController controller) : base(controller) { }
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
