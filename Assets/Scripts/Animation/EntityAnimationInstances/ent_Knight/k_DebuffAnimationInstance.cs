using AudioData;
using UnityEngine;

public class k_DebuffAnimationInstance : AnimationStateInstance
{
    public k_DebuffAnimationInstance( BaseAnimationController bac) : base(bac) { }
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
                AudioClip debuffAudio = animationController.GetCharaInstance().baseData.moveAudio[1];
                AudioSpawnData spawnData = new AudioSpawnData(debuffAudio, false, Vector3.zero);
                AudioPoolManager.instance.RequestPlayAudio(spawnData);
                break;
            case 1:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                break;
        }
    }
}
