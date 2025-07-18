using UnityEngine;

namespace AnimationLoading.LoadStruct
{
    public struct AnimationLoadStruct
    {
        public int layer;
        public GenericAnimationEnums animEnum;
        public bool isLock;
        public bool canPass;

        public AnimationLoadStruct(int layer, GenericAnimationEnums animEnum, bool isLock, bool canPass)
        {
            this.layer = layer;
            this.animEnum = animEnum;
            this.isLock = isLock;
            this.canPass = canPass;
        }
    }
}
