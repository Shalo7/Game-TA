using UnityEngine;

public class w_AttackAnimationStance : AnimationStateInstance
{
    public w_AttackAnimationStance() : base() { }
    
    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:

                break;
            case 0:
                Debug.Log("V'Ataku!");
                break;

        }
    }
}
