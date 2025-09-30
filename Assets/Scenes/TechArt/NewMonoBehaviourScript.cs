using TMPro;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public TMP_Text text;
    public Color32 color = Color.green;

    void Start()
    {
        text.ForceMeshUpdate();
        var info = text.textInfo;
        var charInfo = info.characterInfo[0]; // huruf pertama
        int meshIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Color32[] vertexColors = info.meshInfo[meshIndex].colors32;

        for (int i = 0; i < 4; i++)
            vertexColors[vertexIndex + i] = color;


        /*Mesh mesh = info.meshInfo[meshIndex].mesh;
        mesh.colors32 = vertexColors;
        text.UpdateGeometry(mesh, meshIndex);8?*/
        text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}
