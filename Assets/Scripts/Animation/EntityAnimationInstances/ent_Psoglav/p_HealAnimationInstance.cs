using AudioData;
using UnityEngine;

public class p_HealAnimationInstance : AnimationStateInstance
{
    public p_HealAnimationInstance(BaseAnimationController bac) : base(bac) { }

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
                AudioClip healAudClip = animationController.GetCharaInstance().baseData.moveAudio[1];
                AudioSpawnData healAudioSpawnData = new AudioSpawnData(healAudClip, false, animationController.GetCharaInstance().curTransform.position);
                AudioPoolManager.instance.RequestPlayAudio(healAudioSpawnData);
                break;
            case 1:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
