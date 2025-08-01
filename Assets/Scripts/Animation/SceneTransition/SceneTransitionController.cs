using System;
using UnityEngine;

public class SceneTransitionController : MonoBehaviour
{
    [SerializeField] Director director;
    [SerializeField] Animator anim;

    void OnEnable()
    {
        if (anim == null) { GetComponent<Animator>(); }
        director.transitionHashEvent += PlayTransitionAnimation;
    }
    void OnDisable()
    {
        director.transitionHashEvent -= PlayTransitionAnimation;
    }


    private void PlayTransitionAnimation(int obj)
    {
        anim.CrossFade(obj, 0f, 0);
    }


    public void OnAnimationDone()
    {
        Director.instance.TransitionEvents();
    }
}
