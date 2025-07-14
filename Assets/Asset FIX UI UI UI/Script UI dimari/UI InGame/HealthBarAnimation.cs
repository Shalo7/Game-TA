using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarAnimation : MonoBehaviour
{
    [Header("Sliders")]
    public Slider mainHealthSlider;   // Darah merah
    public Slider easeHealthSlider;   // Darah kuning

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Animation Settings")]
    public float easeDelay = 0.3f;
    public float easeDuration = 0.6f;

    private Coroutine easeCoroutine;

    public float CurrentHealth => currentHealth;

    // void Start()
    // {
    //     currentHealth = maxHealth;
    //     Debug.Log(maxHealth);

    //     if (mainHealthSlider) mainHealthSlider.maxValue = maxHealth;
    //     if (easeHealthSlider) easeHealthSlider.maxValue = maxHealth;

    //     UpdateSlidersInstant(currentHealth);
    // }

    public void TakeDamage(float damage)
    {
        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (mainHealthSlider) mainHealthSlider.value = currentHealth;

        if (easeCoroutine != null)
            StopCoroutine(easeCoroutine);

        if (easeHealthSlider)
            easeCoroutine = StartCoroutine(AnimateEaseBar(easeHealthSlider.value, currentHealth));
    }

    IEnumerator AnimateEaseBar(float from, float to)
    {
        yield return new WaitForSeconds(easeDelay);

        float elapsed = 0f;
        while (elapsed < easeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / easeDuration;
            easeHealthSlider.value = Mathf.Lerp(from, to, t);
            yield return null;
        }

        easeHealthSlider.value = to;
    }

    public void UpdateSlidersInstant(float value)
    {
        if (mainHealthSlider) mainHealthSlider.value = value;
        if (easeHealthSlider) easeHealthSlider.value = value;
    }

    public void SetHealth(float value)
    {
        currentHealth = maxHealth;
        Debug.Log(maxHealth);

        if (mainHealthSlider) mainHealthSlider.maxValue = maxHealth;
        if (easeHealthSlider) easeHealthSlider.maxValue = maxHealth;

        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateSlidersInstant(currentHealth);
    }
}
