using System;
using System.Collections;
using ParticleData.SpawnData;
using UnityEngine;

public class ParticleFXController : MonoBehaviour
{
    public Action<ParticleFXController> OnDoneEvent;
    public Transform GetTransform() => transform;
    ParticleSystem currentSystem;
    int currentChild;
    public int GetCurrentActiveChild() => currentChild;
    public ParticleSystem GetCurrentSystem() => currentSystem;

    public void InitializeParticleFX(ParticleSpawnData data)
    {
        if (data.parent != null) { transform.SetParent(data.parent); }

        transform.position = data.position;
        transform.eulerAngles = data.rotation;

        currentChild = (int)data.type;

        if (transform.childCount < currentChild) return;
        currentSystem = transform.GetChild(currentChild).TryGetComponent(out ParticleSystem ps) ? ps : null;

        if (currentSystem == null) return;
        ChangeLoop(data.isLoop);
        if (data.scale != Vector3.zero) ChangeSize(data.scale);
        if (CO_ParticleLifetime != null) { StopCoroutine(CO_ParticleLifetime); CO_ParticleLifetime = null; }

        transform.GetChild(currentChild).gameObject.SetActive(true);
        currentSystem.Play();
        if (!currentSystem.main.loop) CO_ParticleLifetime = StartCoroutine(ParticleLifetime());
    }


    Coroutine CO_ParticleLifetime;
    IEnumerator ParticleLifetime()
    {
        if (currentSystem == null) { CO_ParticleLifetime = null; yield break; }
        //var flt_counter = 0f;
        //var flt_duration = currentSystem.main.duration + currentSystem.main.startLifetime.constantMax;

        /*while (flt_counter <= flt_duration)
        {
            flt_counter += Time.deltaTime;
            yield return null;
        }*/

        while (currentSystem.isPlaying)
        {
            yield return null;
        }

        if (transform.childCount < currentChild) { CO_ParticleLifetime = null; currentSystem = null; yield break; }
        currentSystem.Stop();
        transform.SetParent(ParticlePoolManager.instance.GetTransform(), false);
        transform.GetChild(currentChild).gameObject.SetActive(false);
        currentSystem.transform.gameObject.SetActive(false);
        currentSystem = null;
        //Debug.LogWarning(currentSystem);
        transform.localScale = Vector3.one;
        OnDoneEvent?.Invoke(this);
        CO_ParticleLifetime = null;
    }

    public void ChangeLoop(bool isLoop)
    {
        if (currentSystem == null) return;
        var psMain = currentSystem.main;
        psMain.loop = isLoop;
        foreach (Transform chld in currentSystem.transform)
        {
            if (!chld.TryGetComponent(out ParticleSystem ps)) continue;
            var pChilSysMain = ps.main;
            pChilSysMain.loop = isLoop;
        }

        if (isLoop == false)
        {
            if (!currentSystem.IsAlive()) return;
            if (CO_ParticleLifetime != null) { StopCoroutine(CO_ParticleLifetime); CO_ParticleLifetime = null; }
            CO_ParticleLifetime = StartCoroutine(ParticleLifetime());
        }
        else
        {
            if (CO_ParticleLifetime != null) { StopCoroutine(CO_ParticleLifetime); CO_ParticleLifetime = null; }
        }
    }

    public void ChangeSize(Vector3 size)
    {
        if (currentSystem == null) return;
        currentSystem.transform.localScale = size;
        /*foreach (Transform child in currentSystem.transform)
        {
            child.localScale = size;
        }*/
    }

    public void ForceStop()
    {
        if (currentSystem == null) return;
        if (currentSystem.IsAlive() && currentSystem.isPlaying)
        {
            currentSystem.Stop();
            transform.SetParent(ParticlePoolManager.instance.GetTransform(), false);
            transform.GetChild(currentChild).gameObject.SetActive(false);
            currentSystem.transform.gameObject.SetActive(false);
            currentSystem = null;
            transform.localScale = Vector3.one;
            OnDoneEvent?.Invoke(this);
        }
    }
}
