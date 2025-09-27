using AudioData;
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
                if (AudioPoolManager.instance == null) return;
                if (animationController.GetCharaInstance().baseData.moveAudio.Length < 1) return;
                AudioClip buffAudClip = animationController.GetCharaInstance().baseData.moveAudio[1];
                AudioSpawnData buffAudioSpawnData = new AudioSpawnData(buffAudClip, false, animationController.GetCharaInstance().curTransform.position);
                AudioPoolManager.instance.RequestPlayAudio(buffAudioSpawnData);
                break;
            case 2:
                
                break;
        }
    }
}
