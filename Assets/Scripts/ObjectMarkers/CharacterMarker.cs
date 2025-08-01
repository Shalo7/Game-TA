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
    CharaInstance thisCharaInstance;

    public CharType GetCharType() => charType;
    public Transform GetTransform() => transform;
    public CharaInstance GetCharacterInstance() => thisCharaInstance;
    public void InitializeCharacterInstance(CharaInstance chara)
    {
        thisCharaInstance = chara;
        BaseAnimationController baseAnimationController = GetComponentInChildren<BaseAnimationController>();
        if (baseAnimationController == null) return;
        baseAnimationController.InitializeCharacterInstance(thisCharaInstance);
    }
}
