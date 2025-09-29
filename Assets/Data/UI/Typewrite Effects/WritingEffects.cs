using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class ActiveWritingFX
{
    public TMP_Text txt;
    public int charIndex;
    public AnimationCurve animCurve;
    public Color newColor;
    public float timer;
    public float duration;
    public Vector3[] baseVerts;
    TMP_TextInfo textInfo;
    public TMP_TextInfo GetTextInfo() => textInfo;
    TMP_CharacterInfo charInfo;
    public TMP_CharacterInfo GetCharacterInfo() => charInfo;
    int meshIndex;
    public int GetMeshIndex() => meshIndex;
    int vertexIndex;
    public int GetVertexIndex() => vertexIndex;
    public bool IsDoneFX;


    public ActiveWritingFX(TMP_Text txt, int charIndex, AnimationCurve animCurve, Color newColor)
    {
        IsDoneFX = false;
        this.txt = txt;
        this.charIndex = charIndex;
        this.animCurve = animCurve;
        this.newColor = newColor;
        this.timer = 0f;
        this.duration = animCurve[animCurve.length - 1].time;

        textInfo = txt.textInfo;
        charInfo = textInfo.characterInfo[charIndex];
        if (charInfo.isVisible)
        {
            baseVerts = new Vector3[4];
            meshIndex = charInfo.materialReferenceIndex;
            vertexIndex = charInfo.vertexIndex;
            Vector3[] verts = textInfo.meshInfo[meshIndex].vertices;

            for (int i = 0; i < 4; i++)
            {
                baseVerts[i] = verts[vertexIndex + i];
            }
        }
    }
}
