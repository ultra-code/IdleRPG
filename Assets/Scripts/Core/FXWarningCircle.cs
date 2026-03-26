using System.Collections;
using UnityEngine;

/// <summary>
/// Warning circle that blinks alpha until destroyed externally.
/// </summary>
public class FXWarningCircle : MonoBehaviour
{
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float blinkHalfCycle = 0.3f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        if (sr.sprite == null)
        {
            sr.sprite = ProceduralSpriteLibrary.GetEffectCircleSprite();
            sr.color = new Color(1f, 0.15f, 0.1f, 0.35f);
        }

        sr.enabled = true;
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            // Fade up
            yield return LerpAlpha(minAlpha, maxAlpha, blinkHalfCycle);
            // Fade down
            yield return LerpAlpha(maxAlpha, minAlpha, blinkHalfCycle);
        }
    }

    private IEnumerator LerpAlpha(float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / dur);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
            yield return null;
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, to);
    }
}
