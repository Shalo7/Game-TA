using System.Collections;
using UnityEngine;

namespace TrailRendererUtils
{
    public static class TrailRendererExtendedMethods
    {
        public static void EnableTrail(this TrailRenderer trail)
        {
            trail.Clear();
            trail.enabled = true;
        }

        public static void DisableTrail(this TrailRenderer trail)
        {
            trail.Clear();
            trail.enabled = false;
        }

        public static void DisableTrailFade(this TrailRenderer trail, MonoBehaviour sender, float duration = 0.2f)
        {
            sender.StartCoroutine(FadeOut(trail, duration));
        }

        private static IEnumerator FadeOut(TrailRenderer trail, float duration)
        {
            float initialTime = trail.time;
            float count = 0f;
            while (count <= duration)
            {
                count += Time.deltaTime;
                trail.time = Mathf.Lerp(initialTime, 0f, count / duration);
                yield return null;
            }
            trail.time = initialTime;
            trail.enabled = false;
        }
    }
}
