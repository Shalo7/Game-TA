using System;
using System.Collections.Generic;
using ParticleData.SpawnData;
using AudioData;
using UnityEngine;

public class w_AttackAnimationInstance : AnimationStateInstance
{
    public w_AttackAnimationInstance(BaseAnimationController bac) : base(bac) { }


    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:
                Debug.LogError("No anim event index");
                break;
            case 0:
                Transform stickFrontTip = animationController.GetBodyParts().GetPart(BodyParts.WEAPON_FRONTTIP);
                Vector3 stickFrontTipParticleSize = new Vector3(0.5f, 0.5f, 0.5f);
                ParticleSpawnData newData = new ParticleSpawnData(stickFrontTip, stickFrontTip.position, Vector3.zero, stickFrontTipParticleSize, ParticleEnum.W_StickGlow, false, true);

                ParticleFXController enter_glowStickPFXC = ParticlePoolManager.instance.ActivateParticleFX(newData);
                animationController.AddActiveAnimationParticles(this, enter_glowStickPFXC);
                break;
            case 1:
                if (AudioPoolManager.instance == null) return;
                if (animationController.GetCharaInstance().baseData.moveAudio.Length < 1) return;
                AudioClip attackAudClip = animationController.GetCharaInstance().baseData.moveAudio[0];
                AudioSpawnData attackAudioSpawnData = new AudioSpawnData(attackAudClip, false, animationController.GetCharaInstance().curTransform.position);
                AudioPoolManager.instance.RequestPlayAudio(attackAudioSpawnData);
                break;
            case 2:
                ParticleSpawnData thunder = new ParticleSpawnData(null, animationController.GetCharaInstance().targetTransform.position, Vector3.zero, Vector3.one, ParticleEnum.W_ThunderAttack, false, false);
                ParticlePoolManager.instance.ActivateParticleFX(thunder);
                break;
            case 3:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                ParticleFXController exit_glowStickPFXC = animationController.GetSpecificActiveAnimationParticle(this, ParticleEnum.W_StickGlow);
                if (exit_glowStickPFXC == null) return;
                exit_glowStickPFXC.ChangeLoop(false);
                break;
        }
    }
}
