using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public abstract class BaseBodyPartHandler : MonoBehaviour
{
    [SerializeField] protected List<BodyPartElement> bodyParts = new();

    protected Dictionary<BodyParts, Transform> partDict;

    protected virtual void Awake()
    {
        partDict = bodyParts.ToDictionary(entry => entry.partKey, entry => entry.partTransform);
    }

    public Transform GetPart(BodyParts key)
    {
        if (partDict.TryGetValue(key, out Transform part))
            return part;

        Debug.LogWarning($"Part with key '{key}' not found in {gameObject.name}");
        return null;
    }
}
