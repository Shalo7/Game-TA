using System;
using System.Collections.Generic;
using AnimationLoading.LoadStruct;
using UnityEngine;

public abstract class BaseAnimationController : MonoBehaviour
{
    public Action AnimationEndsEvent;


    protected Animator animator;
    protected int currentAnimHash;
    public GenericAnimationStates currentAnimState;
    protected bool currentLockStatus;
    protected BaseBodyPartHandler bodyParts;
    protected Dictionary<GenericAnimationStates, AnimationStateInstance> stateInstances = new Dictionary<GenericAnimationStates, AnimationStateInstance>();
    protected AnimationStateInstance currentStateInstance;

    protected abstract void AnimInitialization();

    public virtual (BaseAnimationController animCtrl, AnimationStateInstance stateInstance) RequestPlayAnimation(AnimationLoadStruct data)
    {
        Debug.LogWarning($"{this.name} will try to play animations...");
        if (animator == null) return (null, null);
        if (stateInstances.Count < 1) return (null, null);
        //Debug.LogWarning($"this {this.name} Has animator!");

        int theHash = 0;
        AnimationStateInstance nextstateInstance;
        //semua animation state PERLU ANIMATIONSTATEINSTANCE walaupun tidak ada eventnya karena kalau tidak ada akan eror/animasi tidak play
        if (!AnimationIndexes.GenericAnimationDict.TryGetValue(data.animState, out theHash)) return (null, null);
        //Debug.LogWarning($" this {this.name} found hash!");
        if (!stateInstances.TryGetValue(data.animState, out nextstateInstance)) return (null, null);
        //Debug.LogError($"{currentAnimHash}, {theHash} || {currentAnimState}, {data.animState} || {nextstateInstance}, {currentStateInstance}");
        if (currentAnimHash == theHash || currentAnimState == data.animState || nextstateInstance == currentStateInstance) return (null, null);
        //Debug.LogWarning($"this {this.name} hash is not the same!");
        if (!data.canPass && currentLockStatus) return (null, null);

        currentAnimHash = theHash;
        currentAnimState = data.animState;
        currentLockStatus = data.isLock;
        currentStateInstance = nextstateInstance;
        animator.CrossFade(theHash, 0.2f, data.layer);
        Debug.LogWarning($"this {this.name} will play {currentAnimState}!");
        return (this, nextstateInstance);
    }


    protected virtual void OnAnimationEvent(string parse)
    {
        //animState:index
        string[] parsed = parse.Split(":");
        if (parsed.Length < 1) return;
        GenericAnimationStates animState;
        int eventIndex;
        if (!Enum.TryParse(parsed[0], out animState)) return;
        if (!int.TryParse(parsed[1], out eventIndex)) return;

        if (currentAnimState != animState) return;
        AnimationStateInstance eventAnimInstance;
        if (!stateInstances.TryGetValue(animState, out eventAnimInstance)) return;
        if (currentStateInstance != eventAnimInstance) return;
        eventAnimInstance.OnAnimationEvent(eventIndex);
        Debug.LogWarning($"{this.name} fired anim event with index {eventIndex}!");
    }


    public virtual void OnAnimationEnd(string parse)
    {
        //layer:animEnum:isLock:canPass
        string[] parsed = parse.Split(":");
        if (parsed.Length < 1 || parsed.Length > 4) return;
        int layer;
        GenericAnimationStates animState;
        bool isLock;
        bool canPass;
        if (!int.TryParse(parsed[0], out layer)) { Debug.LogError("No layer!"); return; }
        if (!Enum.TryParse(parsed[1], out animState)) { Debug.LogError("No enum!"); return; }
        if (!bool.TryParse(parsed[2], out isLock)) { Debug.LogError("No isLock!"); return; }
        if (!bool.TryParse(parsed[3], out canPass)) { Debug.LogError("No canPass!"); return; }
        currentLockStatus = false;
        AnimationLoadStruct loadStruct = new AnimationLoadStruct(layer, animState, isLock, canPass);
        AnimationEndsEvent?.Invoke();
        Debug.LogWarning($"Now {this.name} next animation will be {animState}!");
        RequestPlayAnimation(loadStruct);
    }
}
