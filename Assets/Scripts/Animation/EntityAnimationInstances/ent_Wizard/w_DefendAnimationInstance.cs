using ParticleData.SpawnData;
using UnityEngine;

public class w_DefendAnimationInstance : AnimationStateInstance
{
    public w_DefendAnimationInstance(BaseAnimationController bac) : base(bac) { }

    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:
                Debug.LogError("No anim event index!");
                break;
            case 0:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                ParticleEnum shieldParticle = ParticleEnum.EntityShield;
                CharInstanceParticleTransform charInstanceParticleTransform = animationController.GetCharaInstance().GetCharInstanceParticleTransform((int)shieldParticle);

                Transform charTransform = animationController.GetCharaInstance().curTransform;
                Vector3 shieldOffsetPos = charTransform.position + charInstanceParticleTransform.positionOffset;
                Vector3 shieldScale = charInstanceParticleTransform.scale;
                ParticleSpawnData shieldData = new ParticleSpawnData(charTransform, shieldOffsetPos, Vector3.one, shieldScale, shieldParticle, false, true);

                ParticleFXController shieldFXController = ParticlePoolManager.instance.ActivateParticleFX(shieldData);
                animationController.AddActiveAnimationParticles(this, shieldFXController);
                break;
        }
    }
}
