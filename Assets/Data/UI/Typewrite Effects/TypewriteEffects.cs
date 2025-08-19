using System;
using TMPro;
using UnityEngine;

[Serializable]
public class TypewriteEffects
{
    public TMP_Text txt;
    public int charIndex;
    public AnimationCurve animCurve;
    public float timer;
    public float duration;
    public Vector3[] baseVerts;

    public TypewriteEffects(TMP_Text txt, int charIndex, AnimationCurve animCurve)
    {
        this.txt = txt;
        this.charIndex = charIndex;
        this.animCurve = animCurve;
        timer = 0f;
        duration = animCurve[animCurve.length - 1].time;
        
        TMP_TextInfo textInfo = txt.textInfo;
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (charInfo.isVisible)
        {
            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] verts = textInfo.meshInfo[meshIndex].vertices;

            baseVerts = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                baseVerts[i] = verts[vertexIndex + i];
            }
        }
    }
}
