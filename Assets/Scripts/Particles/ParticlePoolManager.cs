using ParticleData.SpawnData;
using UnityEngine;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager instance;
    public Transform GetTransform() => transform;
    ParticleCamMarker particleCamMark;
    MainCanvasMarker mainCanvasMark;
    const string defaultLayer = "Default";
    const string particleUILayer = "ParticleUI";

    void Awake()
    {
        if (instance != null) return;
        instance = this;
    }


    void OnEnable()
    {
        Initialization();
    }


    void Initialization()
    {
        while (particleCamMark == null || mainCanvasMark == null)
        {
            particleCamMark = FindFirstObjectByType<ParticleCamMarker>();
            mainCanvasMark = FindFirstObjectByType<MainCanvasMarker>();
        }

        //Debug.Log("Found cam and canvas!!");
    }


    public Transform ActivateParticleFX(ParticleSpawnData datas)
    {
        if (transform.childCount < 1) return null;

        //Debug.Log(datas.type);
        ParticleFXController fXController = null;

        foreach (Transform tf in transform)
        {
            fXController = tf.TryGetComponent(out ParticleFXController pFX) ? pFX : null;
            if (fXController == null) continue;
            if (fXController.GetCurrentSystem() != null) continue;
            break;
        }

        //Debug.Log(fXController.transform);
        if (fXController == null) return null;

        if (fXController.GetTransform().childCount < (int)datas.type) return null;
        Transform particleObj = fXController.GetTransform().GetChild((int)datas.type);

        if (datas.isUI)
        {
            particleObj.gameObject.layer = LayerMask.NameToLayer(particleUILayer);
            foreach (Transform particles in particleObj)
            {
                particles.gameObject.layer = LayerMask.NameToLayer(particleUILayer);
            }
            datas.position = ConvertToUIScreenPos(datas.position);
        }
        else
        {
            particleObj.gameObject.layer = LayerMask.NameToLayer(defaultLayer);
            foreach (Transform particles in particleObj)
            {
                particles.gameObject.layer = LayerMask.NameToLayer(defaultLayer);
            }
        }

        fXController.InitializeParticleFX(datas);
        return fXController.transform;
    }


    Vector3 ConvertToUIScreenPos(Vector3 pos)
    {
        Camera particleCam = particleCamMark.GetCamera();
        Camera mainCam = mainCanvasMark.GetCanvas().worldCamera;

        Vector3 viewPortPos = mainCam.WorldToViewportPoint(pos);

        float depthFromMainCam = Vector3.Dot((pos - mainCam.transform.position), mainCam.transform.position);

        Vector3 finalWorldPos = particleCam.ViewportToWorldPoint(new Vector3(viewPortPos.x, viewPortPos.y, particleCam.nearClipPlane + 0.01f));

        finalWorldPos += particleCam.transform.forward * 10f;

        return finalWorldPos;
    }
}
