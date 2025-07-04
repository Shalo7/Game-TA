using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [SerializeField] private Animator transitionAnim;
    [SerializeField] private float transitionTime = 1f;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // hindari duplikat
        }
    }

    public void NextLevel(string sceneName)
    {
        StartCoroutine(LoadLevel(sceneName));
    }

    private IEnumerator LoadLevel(string sceneName)
    {
        if (transitionAnim != null)
        {
            transitionAnim.SetTrigger("End");
            yield return new WaitForSeconds(transitionTime);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (transitionAnim != null)
        {
            transitionAnim.SetTrigger("Start");
        }
    }
}
