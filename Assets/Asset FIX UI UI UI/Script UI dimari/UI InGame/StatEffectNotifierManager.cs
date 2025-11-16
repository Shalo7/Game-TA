using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class StatEffectNotifierManager : MonoBehaviour
{
    public static StatEffectNotifierManager instance;
    public void Awake()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
    }

    [SerializeField] RectTransform canvasRect;
    [SerializeField] List<StatEffectNotifierController> statEffectNotifiers = new();
    [SerializeField] Sprite attackBuffIMG;
    [SerializeField] Sprite attackDebuffIMG;
    [SerializeField] Sprite defBuffIMG;
    [SerializeField] Sprite defDebuffIMG;

    void Start()
    {
        foreach (Transform childTrans in transform)
        {
            if (!childTrans.TryGetComponent(out StatEffectNotifierController sec)) continue;
            statEffectNotifiers.Add(sec);
        }
    }

    public void StartNotifying(Moves targetType, Transform victim, Transform perp)
    {
        CharaInstance victimChar = victim?.GetComponent<CharacterMarker>().GetCharacterInstance();
        CharaInstance perpChar = perp?.GetComponent<CharacterMarker>().GetCharacterInstance();

        var v3_victimScreenPos = Camera.main.WorldToScreenPoint(victim.position + Vector3.up * (victimChar.curHeight / 2f));
        var v3_perpScreenPos = Camera.main.WorldToScreenPoint(perp.position + Vector3.up * (perpChar.curHeight / 2f));

        Vector2 victimPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            v3_victimScreenPos,
            Camera.main,
            out victimPos
        );

        Vector2 perpPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            v3_perpScreenPos,
            Camera.main,
            out perpPos
        );

        var v2_direction = (perpPos - victimPos).normalized;
        for (int i = 0; i < statEffectNotifiers.Count; i++)
        {
            StatEffectNotifierController index = statEffectNotifiers[i];
            Image img = statEffectNotifiers[i].GetImage();
            if (index.GetCO_StartFlying() != null) continue;
            if (img.color != new Color(img.color.r, img.color.g, img.color.b, 0)) continue;
            if (targetType.moveType == MoveType.Buff && targetType.affectedStat == StatType.Attack)
            {
                index.StartNotif(attackBuffIMG, victimPos, v2_direction);
                Debug.LogError($"{targetType.moveType} {targetType.affectedStat}!" );
            }
            else if (targetType.moveType == MoveType.Buff && targetType.affectedStat == StatType.Defense)
            {
                index.StartNotif(defBuffIMG, victimPos, v2_direction);
                Debug.LogError($"{targetType.moveType} {targetType.affectedStat}!" );
            }
            else if (targetType.moveType == MoveType.Debuff && targetType.affectedStat == StatType.Attack)
            {
                index.StartNotif(attackDebuffIMG, victimPos, v2_direction);
                Debug.LogError($"{targetType.moveType} {targetType.affectedStat}!" );
            }
            else if (targetType.moveType == MoveType.Debuff && targetType.affectedStat == StatType.Defense)
            {
                index.StartNotif(defDebuffIMG, victimPos, v2_direction);
                Debug.LogError($"{targetType.moveType} {targetType.affectedStat}!" );
            }
            break;
        }
    }
}
