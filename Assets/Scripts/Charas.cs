using ParticleData.SpawnData;
using UnityEngine;

[CreateAssetMenu(fileName = "Charas", menuName = "Scriptable Objects/Charas")]
public class Charas : ScriptableObject
{
    public string charaName;
    public Sprite charaSprite;

    public int maxHP;
    public int attack;
    public int defense;
    public int speed;

    public Moves[] moves;

    public CharInstanceParticleTransform[] charParticleTransformArray;
}
