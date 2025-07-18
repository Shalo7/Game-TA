using System;
using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using UnityEngine;

public abstract class BaseAnimationController : MonoBehaviour
{
    protected Animator animator;
    public int currentAnimHash;
    public GenericAnimationEnums currentAnimEnum;
    protected bool currentLockStatus;
    protected Dictionary<GenericAnimationEnums, AnimationStateInstance> stateInstances = new Dictionary<GenericAnimationEnums, AnimationStateInstance>();

    protected abstract void AnimInitialization();

    public virtual void RequestPlayAnimation(AnimationLoadStruct data)
    {
        if (animator == null) return;
        if (stateInstances.Count < 1) return;
        Debug.LogWarning("Has animator!");

        int theHash = 0;
        if (!AnimationIndexes.GenericAnimationDict.TryGetValue(data.animEnum, out theHash)) return;
        Debug.LogWarning("Found hash!");
        if (currentAnimHash == theHash || currentAnimEnum == data.animEnum) return;
        Debug.LogWarning("Hash is not the same!");
        if (!data.canPass && currentLockStatus) return;
        currentAnimHash = theHash;
        currentAnimEnum = data.animEnum;
        currentLockStatus = data.isLock;
        animator.CrossFade(theHash, 0.2f, data.layer);
        Debug.LogWarning($"Will play {currentAnimEnum}!");
    }


    protected virtual void OnAnimationEvent(string parse)
    {
        string[] parsed = parse.Split(":");
        if (parsed.Length < 1) return;
    }


    public virtual void OnAnimationEnd(string parse)
    {
        //layer:animEnum:isLock:canPass
        string[] parsed = parse.Split(":");
        if (parsed.Length < 1 || parsed.Length > 4) return;
        int layer;
        GenericAnimationEnums animEnum;
        bool isLock;
        bool canPass;
        if (!int.TryParse(parsed[0], out layer)) { Debug.LogError("No layer!"); return; }
        if (!Enum.TryParse(parsed[1], out animEnum)) { Debug.LogError("No enum!"); return; }
        if (!bool.TryParse(parsed[2], out isLock)) { Debug.LogError("No isLock!"); return; }
        if (!bool.TryParse(parsed[3], out canPass)) { Debug.LogError("No canPass!"); return; }
        currentLockStatus = false;
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(layer, animEnum, isLock, canPass);
        RequestPlayAnimation(loadStruct);
        Debug.LogWarning($"{animEnum.ToString()}!");
    }
}
