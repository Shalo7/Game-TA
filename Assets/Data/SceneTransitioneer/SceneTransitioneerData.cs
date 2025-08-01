using System;
using UnityEngine;

public enum SceneTransitionEnums
{
    ST_RIGHTENTER,
    ST_RIGHTEXIT,
    ST_LEFTENTER,
    ST_LEFTEXIT,
    ST_EMPTY,
    ST_FILLED
}

public enum SceneTransitionPairingsEnum
{
    STP_LEFT2RIGHT,
    STP_RIGHT2LEFT,
    STP_FILLED2RIGHT,
    STP_FILLED2LEFT
}

public enum SceneTransitionStateEnum
{
    StartLoading,
    Loading,
    StartFinishing,
    Finish
}
