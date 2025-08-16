using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DMGOutputController : MonoBehaviour
{
    [SerializeField] private TMP_Text dmgTxt;
    RectTransform rectTransform;
    public Transform GetText() => dmgTxt.transform;
    [SerializeField] Vector3 currentVelocity;
    [SerializeField] float spd;
    [Tooltip("Gaya gesekan yang memperlambat gerakan teks perlahan lahan sampai berhenti")]
    [SerializeField] float drag;

    Dictionary<AbilityOutputTypes, string> abilityColorOutput = new Dictionary<AbilityOutputTypes, string>
    {
        {AbilityOutputTypes.Damage, "#FF0000"},
        {AbilityOutputTypes.Heal, "#4AFF0B"},
        {AbilityOutputTypes.Shield, "#FFFFFF"}
    };

    void OnEnable()
    {
        if (dmgTxt == null)
        {
            dmgTxt = transform.GetComponentInChildren<TMP_Text>(true);
            if (dmgTxt == null) Debug.LogError("No damage text!");
        }
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }


    public void InitializeDMGOutput(int dmg, AbilityOutputTypes type)
    {
        if (dmgTxt == null) return;
        dmgTxt.transform.gameObject.SetActive(true);
        dmgTxt.text = dmg.ToString();
        string newColor = "";
        if (abilityColorOutput.TryGetValue(type, out string col)) { newColor = col; }
        if (ColorUtility.TryParseHtmlString(newColor, out Color colorHex)) { dmgTxt.color = colorHex; }

        currentVelocity = GetRandomDirection();
        Invoke("DisableDMGOutputText", 1.5f);
    }


    public void SetPosition(Vector3 pos)
    {
        if (rectTransform == null) return;
        rectTransform.anchoredPosition = pos;
    }


    private void DisableDMGOutputText()
    {
        if (dmgTxt == null) return;
        currentVelocity = Vector3.zero;
        dmgTxt.transform.gameObject.SetActive(false);
    }


    private Vector3 GetRandomDirection()
    {
        Vector3 potentialDir = Vector3.zero;
        do
        {
            float randomDirY = Random.Range(0f, 1f);
            float randomDirX = Random.Range(-1f, 1f);
            potentialDir = new Vector3(randomDirX, randomDirY, 0f).normalized;
        } while (Vector3.Dot(Vector3.up, potentialDir) > 0.8f || Vector3.Dot(Vector3.up, potentialDir) < 0.4f);
        return potentialDir;
    }


    void Update()
    {
        if (currentVelocity.sqrMagnitude < 0.0001f) return;
        transform.position += currentVelocity * spd * Time.deltaTime;
        currentVelocity += (-drag * currentVelocity) * Time.deltaTime;
        if (currentVelocity.sqrMagnitude < 0.0001f) currentVelocity = Vector3.zero; 
    }
}
