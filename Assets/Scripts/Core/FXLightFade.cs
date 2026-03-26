using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Fades a Light2D intensity to 0 over duration, then destroys the component.
/// </summary>
public class FXLightFade : MonoBehaviour
{
    public float duration = 0.3f;

    private Light2D light2D;
    private float startIntensity;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        if (light2D != null)
        {
            startIntensity = light2D.intensity;
            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light2D.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / duration);
            yield return null;
        }
        light2D.intensity = 0f;
    }
}
