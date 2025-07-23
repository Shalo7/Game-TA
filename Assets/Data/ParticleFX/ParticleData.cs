using System;
using UnityEngine;
using UnityEngine.Animations;


public enum ParticleEnum
{
    EntityDamage,
    EntityHeal,
    OnTextTyped
}


namespace ParticleData.SpawnData
{
    public struct ParticleSpawnData
    {
        public Transform parent;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public ParticleEnum type;
        public bool isUI;

        public ParticleSpawnData(Transform parent, Vector3 position, Vector3 rotation, Vector3 scale, ParticleEnum type, bool isUI)
        {
            this.parent = parent;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.type = type;
            this.isUI = isUI;
        }
    }

    [Serializable]
    public struct CharInstanceParticleTransform
    {
        public Vector3 positionOffset;
        public Vector3 scale;

        public CharInstanceParticleTransform(Vector3 positionOffset, Vector3 scale)
        {
            this.positionOffset = positionOffset;
            this.scale = scale;
        }
    }
}
