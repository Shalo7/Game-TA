using System;
using UnityEngine;

[Serializable]
public struct SceneTransitioneerPair
{
    public SceneTransitionEnums firstAct;
    public SceneTransitionEnums secondAct;

    public SceneTransitioneerPair(SceneTransitionEnums first, SceneTransitionEnums second)
    {
        firstAct = first;
        secondAct = second;
    }
}