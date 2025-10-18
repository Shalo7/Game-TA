using UnityEngine;
using UnityEngine.UI;

public class buttonTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Button btn = this?.GetComponent<Button>();
        if (btn == null) Debug.LogError("No button!!!");
        else Debug.LogError("Button!!!");
    }
}
