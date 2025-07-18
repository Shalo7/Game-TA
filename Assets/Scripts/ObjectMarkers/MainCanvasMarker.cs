using UnityEngine;

public class MainCanvasMarker : MonoBehaviour
{
    public Transform GetTransform() => transform;
    public Canvas GetCanvas() => TryGetComponent(out Canvas cv) ? cv : null;
}
