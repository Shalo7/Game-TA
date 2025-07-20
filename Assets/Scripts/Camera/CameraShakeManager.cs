using UnityEngine;
using UnityEngine.Rendering;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;
    [SerializeField] Vector3 axisMultiplier = Vector3.zero;
    [SerializeField, Range(0, 1f)] float duration;
    [SerializeField, Range(0, 5f)] float shakeAmount;
    [SerializeField, Range(0, 2f)] float decreaseFactor;
    Vector3 originalPosition;

    void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    void OnEnable()
    {
        originalPosition = transform.position;
    }

    public void ActivateCamShake(Vector3 axis, float duration = 0.3f, float shakeAmount = 0.3f, float decreaseFactor = 1f)
    {
        axisMultiplier = axis;
        this.duration = duration;
        this.decreaseFactor = decreaseFactor;
        this.shakeAmount = shakeAmount;
    }

    void Update()
    {
        if (duration > 0)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            Vector3 filteredOffset = Vector3.Scale(randomOffset, axisMultiplier.normalized);
            if (float.IsNaN(filteredOffset.x) || float.IsNaN(filteredOffset.y) || float.IsNaN(filteredOffset.z)) { duration = 0f; return;}
            
            transform.localPosition = originalPosition + filteredOffset;

            duration -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPosition;
            axisMultiplier = Vector3.zero;
        }
    }
}
