using UnityEngine;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    public AudioSource winOrLoseSFX;

    void Start()
    {
        winOrLoseSFX.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Director.instance == null) { SceneManager.LoadScene("LevelSelector"); return; }
            Director.instance?.DoTransition(SceneTransitionPairingsEnum.STP_LEFT2RIGHT, "LevelSelector");
        }
    }
}
