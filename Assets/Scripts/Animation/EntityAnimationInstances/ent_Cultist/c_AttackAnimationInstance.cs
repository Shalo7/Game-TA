using AudioData;
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

                if (AudioPoolManager.instance == null) return;
                if (animationController.GetCharaInstance().baseData.moveAudio.Length < 1) return;
                AudioClip attackAudClip = animationController.GetCharaInstance().baseData.moveAudio[0];
                AudioSpawnData attackAudioSpawnData = new AudioSpawnData(attackAudClip, false, animationController.GetCharaInstance().curTransform.position);
                AudioPoolManager.instance.RequestPlayAudio(attackAudioSpawnData);
                break;
        }
    }
}
