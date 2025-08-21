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

                break;
            case 1:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                Debug.LogWarning("Cultist buff!");
                break;
            case 2:
                
                break;
        }
    }
}
