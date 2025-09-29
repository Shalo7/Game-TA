using UnityEngine;
using UnityEngine.UI;

public class EntityStatEffectHolder : MonoBehaviour
{
    [SerializeField] Image imgComp;
    public Image GetImageComponent() => imgComp;
    [SerializeField] Moves currentMove;
    public Moves GetCurrentMove() => currentMove;

    void Start()
    {
        currentMove = new Moves();
        imgComp = GetComponent<Image>();
    }

    public void UpdateStatEffect(Moves type, Sprite statEffectIMG, bool isXpired)
    {
        if (imgComp == null) return;
        if (statEffectIMG == imgComp.sprite) return;
        //Debug.LogError("Can continue!");
        if (isXpired)
        {
            imgComp.sprite = null;
            imgComp.color = new Color(imgComp.color.r, imgComp.color.g, imgComp.color.b, 0f);
            currentMove.affectedStat = StatType.None;
        }
        else
        {
            imgComp.sprite = statEffectIMG;
            imgComp.color = new Color(imgComp.color.r, imgComp.color.g, imgComp.color.b, 1f);
            currentMove.affectedStat = type.affectedStat;
        }

        //Debug.LogError($"stat effect has expired? {isXpired}!");
    }
}
