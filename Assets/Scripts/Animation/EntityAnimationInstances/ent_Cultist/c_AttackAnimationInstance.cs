using ParticleData.SpawnData;
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
            case 1:
                Transform backTip = animationController.GetBodyParts().GetPart(BodyParts.WEAPON_BACKTIP);
                if (backTip == null) return;
                ParticleSpawnData groundImpact = new ParticleSpawnData(null, backTip.transform.position, Vector3.zero, Vector3.one, ParticleEnum.C_GroundHit, false, false);
                if (ParticlePoolManager.instance != null) { ParticlePoolManager.instance.ActivateParticleFX(groundImpact); }
                break;
        }
    }
}
