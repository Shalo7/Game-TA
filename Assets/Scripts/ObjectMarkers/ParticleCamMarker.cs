using UnityEngine;

public class ParticleCamMarker : MonoBehaviour
{
    public Transform GetTransform() => transform;
    public Camera GetCamera() => transform.GetComponent<Camera>();
}
