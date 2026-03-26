using UnityEngine;

public class CombatFeedbackSystem : MonoBehaviour
{
    private static CombatFeedbackSystem instance;
    private const float DefaultFixedDeltaTime = 0.02f;

    private float hitStopTimer;
    private float hitStopScale = 0.06f;
    private float restoreTimeScale = 1f;
    private bool hitStopActive;

    private Transform shakeTarget;
    private Vector3 shakeOrigin;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeStrength;

    public static void HitStop(float duration = 0.04f, float slowScale = 0.06f)
    {
        EnsureInstance().BeginHitStop(duration, slowScale);
    }

    public static void BloodBurst(Vector3 position, bool bossBurst = false)
    {
        EnsureInstance().SpawnBloodBurst(position, bossBurst);
    }

    public static void ShakeCamera(float duration = 0.25f, float strength = 0.18f)
    {
        EnsureInstance().BeginShake(duration, strength);
    }

    private static CombatFeedbackSystem EnsureInstance()
    {
        if (instance != null)
            return instance;

        GameObject obj = new GameObject("CombatFeedbackSystem");
        instance = obj.AddComponent<CombatFeedbackSystem>();
        DontDestroyOnLoad(obj);
        return instance;
    }

    private void Update()
    {
        UpdateHitStop();
        UpdateCameraShake();
    }

    private void BeginHitStop(float duration, float slowScale)
    {
        if (duration <= 0f)
            return;

        if (!hitStopActive)
        {
            restoreTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
            hitStopActive = true;
        }

        hitStopTimer = Mathf.Max(hitStopTimer, duration);
        hitStopScale = Mathf.Clamp(slowScale, 0.01f, 1f);
        Time.timeScale = Mathf.Min(Time.timeScale, hitStopScale);
        Time.fixedDeltaTime = DefaultFixedDeltaTime * Time.timeScale;
    }

    private void UpdateHitStop()
    {
        if (hitStopTimer <= 0f)
            return;

        hitStopTimer -= Time.unscaledDeltaTime;
        if (hitStopTimer > 0f)
            return;

        hitStopTimer = 0f;
        hitStopActive = false;
        Time.timeScale = restoreTimeScale;
        Time.fixedDeltaTime = DefaultFixedDeltaTime * Time.timeScale;
    }

    private void BeginShake(float duration, float strength)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        shakeTarget = cam.transform;
        shakeOrigin = shakeTarget.position;
        shakeDuration = duration;
        shakeTimer = duration;
        shakeStrength = strength;
    }

    private void UpdateCameraShake()
    {
        if (shakeTarget == null || shakeTimer <= 0f)
            return;

        shakeTimer -= Time.unscaledDeltaTime;
        if (shakeTimer <= 0f)
        {
            shakeTarget.position = shakeOrigin;
            shakeTarget = null;
            return;
        }

        float damping = shakeDuration > 0f ? shakeTimer / shakeDuration : 0f;
        Vector2 offset = Random.insideUnitCircle * (shakeStrength * damping);
        shakeTarget.position = shakeOrigin + new Vector3(offset.x, offset.y, 0f);
    }

    private void SpawnBloodBurst(Vector3 position, bool bossBurst)
    {
        int count = bossBurst ? 14 : 8;
        float baseScale = bossBurst ? 0.35f : 0.2f;
        float life = bossBurst ? 0.55f : 0.35f;

        for (int i = 0; i < count; i++)
        {
            GameObject piece = new GameObject(bossBurst ? "BloodBurst_Boss" : "BloodBurst");
            piece.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.12f);

            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteLibrary.GetEffectCircleSprite();
            sr.sortingOrder = 18;
            sr.color = Color.Lerp(
                new Color(0.45f, 0.05f, 0.05f, 0.9f),
                new Color(0.85f, 0.15f, 0.15f, 0.95f),
                Random.value
            );

            var fx = piece.AddComponent<BloodBurstPiece>();
            fx.Initialize(Random.insideUnitCircle.normalized * Random.Range(1.3f, bossBurst ? 3.4f : 2.2f), baseScale * Random.Range(0.8f, 1.6f), life);
        }
    }

    private void OnDisable()
    {
        ResetTimeScaleIfNeeded();
    }

    private void OnDestroy()
    {
        ResetTimeScaleIfNeeded();
    }

    private void ResetTimeScaleIfNeeded()
    {
        if (!hitStopActive && Mathf.Approximately(Time.timeScale, restoreTimeScale))
            return;

        hitStopTimer = 0f;
        hitStopActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = DefaultFixedDeltaTime;
    }

    private sealed class BloodBurstPiece : MonoBehaviour
    {
        private Vector2 velocity;
        private float life;
        private float timer;
        private SpriteRenderer sr;

        public void Initialize(Vector2 initialVelocity, float scale, float duration)
        {
            velocity = initialVelocity;
            life = duration;
            transform.localScale = Vector3.one * scale;
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity *= 0.93f;

            if (sr != null)
            {
                float alpha = Mathf.Clamp01(1f - (timer / Mathf.Max(life, 0.01f)));
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, alpha);
            }

            if (timer >= life)
                Destroy(gameObject);
        }
    }
}
