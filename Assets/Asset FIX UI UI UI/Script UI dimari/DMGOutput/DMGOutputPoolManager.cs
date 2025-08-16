using UnityEngine;

public class DMGOutputPoolManager : MonoBehaviour
{
    public static DMGOutputPoolManager instance;
    private Canvas mainCanvas;
    private RectTransform canvasRect;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
    }

    void OnEnable()
    {
        if (mainCanvas == null)
        {
            mainCanvas = transform.parent.TryGetComponent(out Canvas c) ? c : null;
            if (mainCanvas == null) Debug.LogError("No canvas!");
            else
            {
                canvasRect = mainCanvas.GetComponent<RectTransform>();
            }
        }
    }


    public void RequestActivateDMGOutput(int dmg, Vector3 position, AbilityOutputTypes type)
    {
        if (transform.childCount < 1) return;

        DMGOutputController chosenText = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            DMGOutputController doc = child.TryGetComponent(out DMGOutputController c) ? c : null;
            if (doc.GetText().gameObject.activeInHierarchy) continue;
            chosenText = doc;
            break;
        }
        if (chosenText == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);
        /*Vector2 screenPos = new Vector2(
            viewPortPos.x * canvasRect.sizeDelta.x,
            viewPortPos.y * canvasRect.sizeDelta.y
        );*/

        Vector2 uiPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,
            out uiPos
        );

        chosenText.SetPosition(uiPos);
        chosenText.InitializeDMGOutput(dmg, type);
    }
}
