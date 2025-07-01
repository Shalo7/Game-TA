using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordGlossary", menuName = "Scriptable Objects/WordGlossary")]
public class WordGlossary : ScriptableObject
{
    public List<string> words = new List<string>();
}
