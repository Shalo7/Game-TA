using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Director : MonoBehaviour
{
    public static Director instance;
    private SceneTransitionStateEnum currentState;
    private SceneTransitioneerPair currentPair;
    private string nextScene;
    public Action<int> transitionHashEvent;
    [SerializeField] private int currentLevel = 0;
    public int GetCurrentLevel() => currentLevel;
    Dictionary<SceneTransitionPairingsEnum, SceneTransitioneerPair> transitionPairing = new Dictionary<SceneTransitionPairingsEnum, SceneTransitioneerPair>
    {
        {SceneTransitionPairingsEnum.STP_FILLED2LEFT, new SceneTransitioneerPair(SceneTransitionEnums.ST_FILLED, SceneTransitionEnums.ST_LEFTEXIT)},
        {SceneTransitionPairingsEnum.STP_FILLED2RIGHT, new SceneTransitioneerPair(SceneTransitionEnums.ST_FILLED, SceneTransitionEnums.ST_RIGHTEXIT)},
        {SceneTransitionPairingsEnum.STP_LEFT2RIGHT, new SceneTransitioneerPair(SceneTransitionEnums.ST_LEFTENTER, SceneTransitionEnums.ST_RIGHTEXIT)},
        {SceneTransitionPairingsEnum.STP_RIGHT2LEFT, new SceneTransitioneerPair(SceneTransitionEnums.ST_RIGHTENTER, SceneTransitionEnums.ST_LEFTEXIT)}
    };



    void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    void Start()
    {
        Cursor.visible = false;
        DoTransition(SceneTransitionPairingsEnum.STP_FILLED2RIGHT, "MainMenu");
    }

    public void UpdateCurrentLevel(int nextLevel)
    {
        if (currentLevel > nextLevel) return;
        currentLevel = nextLevel;
    }

    public void DoTransition(SceneTransitionPairingsEnum wantedPair, string sceneName)
    {
        if (!transitionPairing.TryGetValue(wantedPair, out currentPair)) return;
        nextScene = sceneName;
        currentState = SceneTransitionStateEnum.StartLoading;
        TransitionEvents();
    }

    public void TransitionEvents()
    {
        if (currentState == SceneTransitionStateEnum.StartLoading)
        {
            int wantedHash;
            if (!AnimationIndexes.TransitionHashes.TryGetValue(currentPair.firstAct, out wantedHash)) return;
            transitionHashEvent?.Invoke(wantedHash);
            currentState = SceneTransitionStateEnum.Loading;
            return;
        }
        if (currentState == SceneTransitionStateEnum.Loading)
        {
            if (CO_IsLoadingScene != null) { StopCoroutine(CO_IsLoadingScene);  CO_IsLoadingScene = null; }
            CO_IsLoadingScene = StartCoroutine(IsLoadingScene());
            return;
        }
        if (currentState == SceneTransitionStateEnum.StartFinishing)
        {
            currentState = SceneTransitionStateEnum.Finish;
            return;
        }
    }

    Coroutine CO_IsLoadingScene;
    IEnumerator IsLoadingScene()
    {
        if (nextScene == "") { CO_IsLoadingScene = null; yield break; }
        int activeSceneCount = SceneManager.sceneCount;
        for (int i = 0; i < activeSceneCount; i++)
        {
            Scene index = SceneManager.GetSceneAt(i);
            if (index.name == "Director") continue;
            SceneManager.UnloadSceneAsync(index);
        }

        yield return SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        yield return new WaitForSeconds(1f);
        int wantedHash;
        if (!AnimationIndexes.TransitionHashes.TryGetValue(currentPair.secondAct, out wantedHash)) { CO_IsLoadingScene = null; yield break; }
        transitionHashEvent?.Invoke(wantedHash);
        currentState = SceneTransitionStateEnum.StartFinishing;
    }
}
