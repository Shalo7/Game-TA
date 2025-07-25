using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseManager : MonoBehaviour
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
            SceneManager.LoadScene("LevelSelector");
        }
    }
}
