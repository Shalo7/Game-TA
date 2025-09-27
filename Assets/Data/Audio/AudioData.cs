using UnityEngine;

namespace AudioData
{
    public struct AudioSpawnData
    {
        public AudioClip audClip;
        public bool is3D;
        public Vector3 position;

        public AudioSpawnData(AudioClip audClip, bool is3D, Vector3 position)
        {
            this.audClip = audClip;
            this.is3D = is3D;
            this.position = position;
        }
    }
}
