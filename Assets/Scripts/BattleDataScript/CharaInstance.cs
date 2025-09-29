using System;
using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using ParticleData.SpawnData;
using UnityEngine;


public class CharaInstance
{
    public Charas baseData;

    public int curHP;
    public int curAtt;
    public int curDef;
    public int curSpd;
    public float curHeight;
    public int shieldHP;
    public float shieldDmgReduc = 0.05f; //5%
    public Transform curTransform;
    public Transform targetTransform;
    public Transform GetTargetTransform() => targetTransform;
    public CharInstanceParticleTransform[] charParticleTransformArray;
    private BaseAnimationController animCtrl;
    public BaseAnimationController GetCurrentAnimCtrl() => animCtrl;
    CharacterMarker characterMarker;
    EntityStatEffectsManager entityStatEffectsManager;

    public bool isBlocking = false;

    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    public CharaInstance(Charas baseData, Transform transform, Transform target)
    {
        this.baseData = baseData;
        this.curTransform = transform;
        this.targetTransform = target;
        if (curTransform.TryGetComponent(out characterMarker)) { AssignCharaInstance(this);}
        this.animCtrl = GetAnimationController();
        ResetStats();
    }

    private void AssignCharaInstance(CharaInstance charaInstance)
    {
        characterMarker.InitializeCharacterInstance(charaInstance);
    }

    public void AssignStatEffectUI(EntityStatEffectsManager chosenManager)
    {
        entityStatEffectsManager = chosenManager;
    }

    public void ResetStats()
    {
        curHP = baseData.maxHP;
        curAtt = baseData.attack;
        curDef = baseData.defense;
        curSpd = baseData.speed;
        curHeight = baseData.height;
        this.charParticleTransformArray = baseData.charParticleTransformArray;
        activeEffects.Clear();
    }

    private BaseAnimationController GetAnimationController()
    {
        if (curTransform == null) return null;
        animCtrl = curTransform.GetComponentInChildren<BaseAnimationController>();
        if (animCtrl == null) return null;
        return animCtrl;
    }

    public CharInstanceParticleTransform GetCharInstanceParticleTransform(int index)
    {
        if (charParticleTransformArray.Length < index) return new CharInstanceParticleTransform(Vector3.zero, Vector3.zero);
        else
        {
            return charParticleTransformArray[index];
        }
    }

    public void ApplyMoveEffect(Moves move, bool isFromEnemy, CharaInstance target = null, int overridePower = -1)
    {
        int finalPower = overridePower > -1 ? overridePower : move.power;

        switch (move.moveType)
        {
            case MoveType.Attack:
                // handled in battle logic
                break;

            case MoveType.Defend:
                // isBlocking = true;
                // int shieldAmount = Mathf.RoundToInt(finalPower * 0.2f);
                // shieldHP += shieldAmount;

                // Debug.Log($"{this} shield {shieldAmount} HP!");

                //handled in battle logic
                break;
            case MoveType.Buff:
                ApplyStatEffect(move.affectedStat, finalPower, move.duration, move);
                break;
            case MoveType.Debuff:
                if (target != null)
                    target.ApplyStatEffect(move.affectedStat, finalPower, move.duration, move);
                break;

            case MoveType.Heal:
                curHP += finalPower;
                curHP = Mathf.Min(curHP, baseData.maxHP);

                Vector3 targetCenter = Vector3.zero;
                if (curHeight > 0)
                {
                        float h = curHeight / 2f;
                        targetCenter += curTransform.position + new Vector3(0, h, 0);
                }
                ExecuteAbilityOutput(finalPower, targetCenter, AbilityOutputTypes.Heal);

                ParticleEnum healparticleType = ParticleEnum.EntityHeal;
                CharInstanceParticleTransform cipTransform = charParticleTransformArray[(int)healparticleType];
                Vector3 particlePos = curTransform.position + (curTransform.up * cipTransform.positionOffset.y);

                ParticleSpawnData healData = new ParticleSpawnData(null, particlePos, Vector3.zero, cipTransform.scale, healparticleType, false, false);

                ExecuteParticleEffects(healData);
                break;
        }
    }

    void ExecuteParticleEffects(ParticleSpawnData data)
    {
        if (ParticlePoolManager.instance == null) return;
        ParticlePoolManager.instance.ActivateParticleFX(data);
    }

    void ExecuteAbilityOutput(int dmg, Vector3 pos, AbilityOutputTypes colorType)
    {
        if (DMGOutputPoolManager.instance == null) return;
        DMGOutputPoolManager.instance.RequestActivateDMGOutput(dmg, pos, colorType);
    }

    private void ApplyStatEffect(StatType stat, int amount, int duration, Moves move)
    {
        int effectiveAmount = amount;

        if (move.isPercentageChange)
        {
            int baseStat = 0;
            switch (stat)
            {
                case StatType.Attack: baseStat = baseData.attack; break;
                case StatType.Defense: baseStat = baseData.defense; break;
            }

            effectiveAmount = Mathf.RoundToInt(baseStat * (amount / 100f));

            if (entityStatEffectsManager != null)
            {
                entityStatEffectsManager.UpdateStatEffectUI(move, false);
                Debug.LogError("entity exist and has status effect!");
            }
        }

        //Apply stat change
        switch (stat)
        {
            case StatType.Attack: curAtt += effectiveAmount; break;
            case StatType.Defense: curDef += effectiveAmount; break;
        }


        if (duration != -1)
        {
            activeEffects.Add(new ActiveEffect(move, effectiveAmount, duration));
        }
    }

    public void OnTurnEnd()
    {
        List<ActiveEffect> expiredEffects = new();

        foreach (var effect in activeEffects)
        {
            effect.duration--;

            if (effect.duration <= 0)
            {
                switch (effect.move.affectedStat)
                {
                    case StatType.Attack: curAtt -= effect.amount; break;
                    case StatType.Defense: curDef -= effect.amount; break;
                }

                expiredEffects.Add(effect);

                if (entityStatEffectsManager != null)
                {
                    entityStatEffectsManager.UpdateStatEffectUI(effect.move, true);
                }

            }
        }

        foreach (var effect in expiredEffects)
        {
            activeEffects.Remove(effect);
        }


    }

    public bool IsFainted()
    {
        return curHP <= 0;
    }
}
