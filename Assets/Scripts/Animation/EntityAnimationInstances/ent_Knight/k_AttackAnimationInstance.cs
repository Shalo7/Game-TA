using TrailRendererUtils;
using UnityEngine;

public class k_AttackAnimationInstance : AnimationStateInstance
{
    public k_AttackAnimationInstance(BaseAnimationController bac) : base(bac) { }

    TrailRenderer swordTrail;

    public override void OnAnimationEvent(int index)
    {
        switch (index)
        {
            default:
                Debug.LogError("No anim index!");
                break;
            case 0:
                swordTrail = FindTrailRendererInObject(animationController.GetBodyParts().GetPart(BodyParts.WEAPON_MIDDLE));
                if (swordTrail == null) break;
                swordTrail.EnableTrail();
                break;
            case 1:
                AnimationStateEvents?.Invoke(AnimEventTypes.GENERICMOVE);
                //Debug.LogWarning("V Ataku!");
                break;
            case 2:
                if (swordTrail == null ) {swordTrail = FindTrailRendererInObject(animationController.GetBodyParts().GetPart(BodyParts.WEAPON_MIDDLE));}
                swordTrail.DisableTrailFade(BattleSystem.instance, 0.75f);
                break;
        }
    }

    TrailRenderer FindTrailRendererInObject(Transform bodyPart)
    {
        TrailRenderer trail;
        if (!bodyPart.TryGetComponent(out trail)) return null;
        return trail;
    }
}
