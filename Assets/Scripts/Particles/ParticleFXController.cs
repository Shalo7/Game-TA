using System.Collections;
using ParticleData.SpawnData;
using UnityEngine;

public class ParticleFXController : MonoBehaviour
{
    public Transform GetTransform() => transform;
    ParticleSystem currentSystem;
    int currentChild;
    public ParticleSystem GetCurrentSystem() => currentSystem;

    public void InitializeParticleFX(ParticleSpawnData data)
    {
        if (data.parent != null) { transform.SetParent(data.parent, true); }

        transform.position = data.position;
        transform.eulerAngles = data.rotation;
        if (data.scale != Vector3.zero) transform.localScale = data.scale;

        currentChild = (int)data.type;
        if (transform.childCount < currentChild) return;
        currentSystem = transform.GetChild(currentChild).TryGetComponent(out ParticleSystem ps) ? ps : null;
        if (CO_ParticleLifetime != null) { StopCoroutine(CO_ParticleLifetime); CO_ParticleLifetime = null; }
        transform.GetChild(currentChild).gameObject.SetActive(true);
        CO_ParticleLifetime = StartCoroutine(ParticleLifetime());
    }


    Coroutine CO_ParticleLifetime;
    IEnumerator ParticleLifetime()
    {
        if (currentSystem == null) { CO_ParticleLifetime = null; yield break; }
        var flt_counter = 0f;
        var flt_duration = currentSystem.main.duration + currentSystem.main.startLifetime.constantMax;

        currentSystem.Play();
        while (flt_counter <= flt_duration)
        {
            flt_counter += Time.deltaTime;
            yield return null;
        }

        if (transform.childCount < currentChild) { CO_ParticleLifetime = null; currentSystem = null; yield break; }
        currentSystem.Stop();
        transform.SetParent(ParticlePoolManager.instance.GetTransform(), false);
        transform.GetChild(currentChild).gameObject.SetActive(false);
        currentSystem.transform.gameObject.SetActive(false);
        currentSystem = null;
        //Debug.LogWarning(currentSystem);
        CO_ParticleLifetime = null;
    }
}
