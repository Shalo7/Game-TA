using AudioData;
using UnityEngine;

public class w_HealAnimationInstance : AnimationStateInstance
{
    public w_HealAnimationInstance(BaseAnimationController bac) : base(bac) { }

    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:

                break;
            case 0:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                if (AudioPoolManager.instance == null) return;
                if (animationController.GetCharaInstance().baseData.moveAudio.Length < 1) return;
                AudioClip healAudio = animationController.GetCharaInstance().baseData.moveAudio[2];
                AudioSpawnData spawnData = new AudioSpawnData(healAudio, false, Vector3.zero);
                AudioPoolManager.instance.RequestPlayAudio(spawnData);
                break;
        }
    }
}
