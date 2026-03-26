using System.Collections;
using UnityEngine;

public class SimpleSkillEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float duration;
    private float timer;
    private Color baseColor;

    // Aura sprite-sheet fields
    private bool isAura;
    private SpriteRenderer auraMain;
    private Sprite[] auraFrames;
    private float auraFPS;
    private float auraBaseScale;
    private const float AuraFadeDuration = 0.5f;
    private const float AuraPopDuration = 0.3f;

    public static void CreateFan(Vector3 origin, Vector2 direction, float radius, float angleDeg, Color color, float duration = 0.22f)
    {
        GameObject obj = new GameObject("FX_Fan");
        var effect = obj.AddComponent<SimpleSkillEffect>();
        effect.SetupLine(color, 0.12f, duration);

        int arcSegments = 12;
        Vector3[] points = new Vector3[arcSegments + 3];
        points[0] = origin;

        float startAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - angleDeg * 0.5f;
        for (int i = 0; i <= arcSegments; i++)
        {
            float currentAngle = startAngle + (angleDeg / arcSegments) * i;
            Vector3 dir = Quaternion.Euler(0f, 0f, currentAngle) * Vector3.right;
            points[i + 1] = origin + dir * radius;
        }

        points[points.Length - 1] = origin;
        effect.SetWorldPoints(points);
    }

    public static void CreateCircle(Vector3 center, float radius, Color color, float duration = 0.25f)
    {
        GameObject obj = new GameObject("FX_Circle");
        var effect = obj.AddComponent<SimpleSkillEffect>();
        effect.SetupLine(color, 0.1f, duration);

        int segments = 28;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = (Mathf.PI * 2f / segments) * i;
            points[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }

        effect.SetWorldPoints(points);
    }

    public static void CreateChain(Vector3 from, Vector3 to, Color color, float duration = 0.18f)
    {
        GameObject obj = new GameObject("FX_Chain");
        var effect = obj.AddComponent<SimpleSkillEffect>();
        effect.SetupLine(color, 0.16f, duration);
        effect.SetWorldPoints(new[] { from, to });
    }

    public static void CreateAura(Transform target, float radius, float duration, Color color)
    {
        if (target == null) return;

        // Load sprite sheet frames
        var allAssets = Resources.LoadAll<Sprite>(""); // fallback
        Sprite[] frames = LoadAuraFrames();
        if (frames == null || frames.Length == 0)
        {
            // Fallback to procedural circle if sprite sheet not found
            CreateAuraFallback(target, radius, duration);
            return;
        }

        GameObject obj = new GameObject("FX_Aura");
        obj.transform.SetParent(target, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.zero; // start at 0 for pop-in

        var effect = obj.AddComponent<SimpleSkillEffect>();
        effect.duration = duration;
        effect.isAura = true;
        effect.auraBaseScale = radius * 0.733f;
        effect.auraFrames = frames;
        effect.auraFPS = 10f;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = frames[0];
        sr.color = Color.white;
        var targetSR = target.GetComponent<SpriteRenderer>();
        sr.sortingOrder = targetSR != null ? targetSR.sortingOrder - 1 : 4;
        effect.auraMain = sr;

        effect.StartCoroutine(effect.AuraAnimLoop());
    }

    private static Sprite[] LoadAuraFrames()
    {
#if UNITY_EDITOR
        var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Sprites/Effects/Buff_Aura_SpriteSheet.png");
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var a in all)
        {
            if (a is Sprite s) list.Add(s);
        }
        list.Sort((a, b) => a.name.CompareTo(b.name));
        return list.Count > 0 ? list.ToArray() : null;
#else
        // Runtime: load from Resources or pre-assigned
        return null;
#endif
    }

    private static void CreateAuraFallback(Transform target, float radius, float duration)
    {
        GameObject obj = new GameObject("FX_Aura");
        obj.transform.SetParent(target, false);
        obj.transform.localPosition = Vector3.zero;

        var effect = obj.AddComponent<SimpleSkillEffect>();
        effect.duration = duration;
        effect.isAura = true;
        effect.auraBaseScale = radius * 0.733f;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteLibrary.GetEffectCircleSprite();
        sr.color = new Color(0.91f, 0.27f, 0.48f, 0.4f);
        var targetSR = target.GetComponent<SpriteRenderer>();
        sr.sortingOrder = targetSR != null ? targetSR.sortingOrder - 1 : 4;
        effect.auraMain = sr;

        float s = effect.auraBaseScale;
        obj.transform.localScale = new Vector3(s, s, 1f);
    }

    private IEnumerator AuraAnimLoop()
    {
        // Pop-in: scale 0 → 1 (EaseOut, 0.3s)
        float popElapsed = 0f;
        while (popElapsed < AuraPopDuration)
        {
            popElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(popElapsed / AuraPopDuration);
            float ease = 1f - (1f - t) * (1f - t);
            float s = auraBaseScale * ease;
            transform.localScale = new Vector3(s, s, 1f);
            if (auraMain != null)
                auraMain.color = new Color(1f, 1f, 1f, ease * 0.85f);
            yield return null;
        }
        transform.localScale = new Vector3(auraBaseScale, auraBaseScale, 1f);

        // Loop animation until duration expires
        float interval = 1f / Mathf.Max(auraFPS, 1f);
        int frameIndex = 0;
        float loopTimer = 0f;
        float totalLoop = duration - AuraPopDuration - AuraFadeDuration;
        if (totalLoop < 0f) totalLoop = 0f;

        while (loopTimer < totalLoop)
        {
            if (auraMain == null) yield break;
            auraMain.sprite = auraFrames[frameIndex];
            frameIndex = (frameIndex + 1) % auraFrames.Length;
            loopTimer += interval;
            yield return new WaitForSeconds(interval);
        }

        // Fade-out: alpha + scale → 0 (0.5s)
        float fadeElapsed = 0f;
        while (fadeElapsed < AuraFadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(fadeElapsed / AuraFadeDuration);
            float fadeMul = 1f - t;
            float s = auraBaseScale * fadeMul;
            transform.localScale = new Vector3(s, s, 1f);
            if (auraMain != null)
                auraMain.color = new Color(1f, 1f, 1f, fadeMul * 0.85f);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void SetupLine(Color color, float width, float effectDuration)
    {
        duration = effectDuration;
        baseColor = color;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width * 0.65f;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.sortingOrder = 20;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void SetWorldPoints(Vector3[] points)
    {
        if (lineRenderer == null || points == null) return;
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    private void Update()
    {
        if (isAura) return; // aura handled by coroutine

        timer += Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - (timer / Mathf.Max(duration, 0.01f)));

        if (lineRenderer != null)
        {
            Color faded = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            lineRenderer.startColor = faded;
            lineRenderer.endColor = faded;
        }

        if (timer >= duration)
            Destroy(gameObject);
    }
}
