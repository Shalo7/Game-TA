using UnityEngine;
using TrailRendererUtils;
using AudioData;

public class p_AttackAnimationInstance : AnimationStateInstance
{
    public p_AttackAnimationInstance(BaseAnimationController bac) : base(bac) { }

    TrailRenderer rClawTrail;
    TrailRenderer lClawTrail;
    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:
                Debug.LogError("No anim index!");
                break;
            case 0:
                if (AudioPoolManager.instance == null) return;
                if (animationController.GetCharaInstance().baseData.moveAudio.Length < 1) return;
                AudioClip attackAudClip = animationController.GetCharaInstance().baseData.moveAudio[0];
                AudioSpawnData attackAudioSpawnData = new AudioSpawnData(attackAudClip, false, animationController.GetCharaInstance().curTransform.position);
                AudioPoolManager.instance.RequestPlayAudio(attackAudioSpawnData);
                break;
            case 1:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
