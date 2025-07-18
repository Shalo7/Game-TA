using UnityEngine;

public enum CharType
{
    None,
    Player,
    Enemy
}

public class CharacterMarker : MonoBehaviour
{
    [SerializeField] CharType charType;

    public CharType GetCharType() => charType;
    public Transform GetTransform() => transform;
}
