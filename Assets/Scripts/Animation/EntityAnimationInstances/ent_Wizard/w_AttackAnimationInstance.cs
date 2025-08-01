using ParticleData.SpawnData;
using UnityEngine;

public class w_AttackAnimationInstance : AnimationStateInstance
{
    public w_AttackAnimationInstance(BaseBodyPartHandler bodyParts, BaseAnimationController bac) : base(bodyParts, bac) { }
    
    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:

                break;
            case 0:
                ParticleSpawnData thunder = new ParticleSpawnData(null, animCtrl.GetCharaInstance().targetTransform.position, Vector3.zero, Vector3.one, ParticleEnum.W_ThunderAttack, false);
                ParticlePoolManager.instance.ActivateParticleFX(thunder);
                break;
            case 1:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
