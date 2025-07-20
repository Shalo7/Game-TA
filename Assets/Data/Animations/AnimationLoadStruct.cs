using UnityEngine;

namespace AnimationLoading.LoadStruct
{
    public struct AnimationLoadStruct
    {
        public int layer;
        public GenericAnimationStates animState;
        public bool isLock;
        public bool canPass;

        public AnimationLoadStruct(int layer, GenericAnimationStates animEnum, bool isLock, bool canPass)
        {
            this.layer = layer;
            this.animState = animEnum;
            this.isLock = isLock;
            this.canPass = canPass;
        }
    }
}
