using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill 2: Meteor Rain — 15-20 meteors rain down on enemy positions across the map.
/// </summary>
public class MeteorSkill : MonoBehaviour
{
    [Header("Meteor Visuals")]
    [SerializeField] private Sprite meteorSprite;
    [SerializeField] private Sprite[] meteorSpriteFrames;
    [SerializeField] private float meteorAnimFPS = 10f;
    [SerializeField] private Material additiveMaterial;
    [SerializeField] private Color meteorColor = new Color(1f, 0.38f, 0.18f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.1f, 0.35f);

    [Header("Explosion Sprite Animation")]
    [SerializeField] private Sprite[] explosionFrames;
    [SerializeField] private float explosionFPS = 20f;
    [SerializeField] private float explosionScale = 0.5f;

    [Header("Particle FX Prefabs")]
    [SerializeField] private GameObject meteorExplosionFXPrefab;
    [SerializeField] private GameObject warningCircleFXPrefab;

    [Header("Settings")]
    [SerializeField] private int minMeteors = 15;
    [SerializeField] private int maxMeteors = 20;
    [SerializeField] private float damageMultiplier = 3.0f;
    [SerializeField] private float blastRadius = 1.5f;
    [SerializeField] private float warningRadius = 0.8f;
    [SerializeField] private float fallSpeedMin = 18f;
    [SerializeField] private float fallSpeedMax = 22f;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float spawnDelayMin = 0.05f;
    [SerializeField] private float spawnDelayMax = 0.15f;
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private float meteorRotationOffset = 180f;

    private void Start()
    {
        StartCoroutine(ScanForMagenta());
    }

    private IEnumerator ScanForMagenta()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("=== [MAGENTA SCAN] SpriteRenderers ===");
        var srs = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var sr in srs)
        {
            if (!sr.enabled) continue;
            string parentName = sr.transform.parent != null ? sr.transform.parent.name : "(root)";
            bool spriteNull = sr.sprite == null;
            string matName = sr.sharedMaterial != null ? sr.sharedMaterial.name : "NULL";
            string shaderName = sr.sharedMaterial != null ? sr.sharedMaterial.shader.name : "NULL";
            bool isError = shaderName.Contains("Error") || shaderName.Contains("Hidden/InternalErrorShader");
            if (spriteNull || isError)
            {
                Debug.LogWarning($"[MAGENTA] SR: '{sr.gameObject.name}' parent='{parentName}' sprite={(spriteNull ? "NULL" : sr.sprite.name)} mat={matName} shader={shaderName}");
            }
        }

        Debug.Log("=== [MAGENTA SCAN] ParticleSystemRenderers ===");
        var psrs = FindObjectsByType<ParticleSystemRenderer>(FindObjectsSortMode.None);
        foreach (var psr in psrs)
        {
            string parentName = psr.transform.parent != null ? psr.transform.parent.name : "(root)";
            string matName = psr.sharedMaterial != null ? psr.sharedMaterial.name : "NULL";
            string shaderName = psr.sharedMaterial != null ? psr.sharedMaterial.shader.name : "NULL";
            bool isError = matName == "NULL" || shaderName.Contains("Error") || shaderName.Contains("Hidden");
            if (isError)
            {
                Debug.LogWarning($"[MAGENTA] PSR: '{psr.gameObject.name}' parent='{parentName}' mat={matName} shader={shaderName}");
            }
            else
            {
                Debug.Log($"[SCAN] PSR: '{psr.gameObject.name}' parent='{parentName}' mat={matName} shader={shaderName}");
            }
        }

        Debug.Log("=== [MAGENTA SCAN] Renderers with error shader ===");
        var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in allRenderers)
        {
            if (r is SpriteRenderer || r is ParticleSystemRenderer) continue;
            foreach (var m in r.sharedMaterials)
            {
                if (m == null)
                {
                    string pn = r.transform.parent != null ? r.transform.parent.name : "(root)";
                    Debug.LogWarning($"[MAGENTA] Renderer: '{r.gameObject.name}' type={r.GetType().Name} parent='{pn}' mat=NULL");
                    continue;
                }
                if (m.shader.name.Contains("Error") || m.shader.name.Contains("Hidden/Internal"))
                {
                    string pn = r.transform.parent != null ? r.transform.parent.name : "(root)";
                    Debug.LogWarning($"[MAGENTA] Renderer: '{r.gameObject.name}' type={r.GetType().Name} parent='{pn}' mat={m.name} shader={m.shader.name}");
                }
            }
        }
        Debug.Log("=== [MAGENTA SCAN] Complete ===");
    }

    public void Execute(CharacterStats player, List<Enemy> allEnemies)
    {
        if (player == null) return;
        float damage = player.ATK * damageMultiplier;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMeteorRain();

        StartCoroutine(MeteorRainRoutine(allEnemies, damage));
    }

    private IEnumerator MeteorRainRoutine(List<Enemy> allEnemies, float damage)
    {
        int count = Random.Range(minMeteors, maxMeteors + 1);
        List<Vector2> targets = BuildTargetList(allEnemies, count);

        int launched = 0;
        foreach (Vector2 target in targets)
        {
            StartCoroutine(SingleMeteor(target, damage));
            launched++;
            yield return new WaitForSeconds(Random.Range(spawnDelayMin, spawnDelayMax));
        }

        Debug.Log($"[Meteor Rain] Launched {launched} meteors");
    }

    private List<Vector2> BuildTargetList(List<Enemy> enemies, int count)
    {
        List<Vector2> targets = new List<Vector2>();

        // Add enemy positions (only inside map)
        foreach (var e in enemies)
        {
            if (e != null && !e.IsDead && MapBounds.IsInside(e.transform.position))
                targets.Add((Vector2)e.transform.position + Random.insideUnitCircle * 0.3f);
        }

        // Fill remaining with random positions inside camera view
        while (targets.Count < count)
        {
            targets.Add(MapBounds.RandomInsideView());
        }

        // Shuffle
        for (int i = targets.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = targets[i]; targets[i] = targets[j]; targets[j] = tmp;
        }

        if (targets.Count > count) targets.RemoveRange(count, targets.Count - count);
        return targets;
    }

    private IEnumerator SingleMeteor(Vector2 groundTarget, float damage)
    {
        // Warning circle on ground
        GameObject warning = CreateWarningCircle(groundTarget);

        // Meteor start position
        float offsetX = Random.Range(-1f, 1f);
        Vector2 startPos = new Vector2(groundTarget.x + offsetX, groundTarget.y + spawnHeight);
        float fallSpeed = Random.Range(fallSpeedMin, fallSpeedMax);
        float fallDist = Vector2.Distance(startPos, groundTarget);
        float fallTime = fallDist / fallSpeed;

        // Show warning for a brief time before meteor arrives
        float warningDuration = Mathf.Min(fallTime, 0.5f);

        // Create meteor
        GameObject meteor = CreateMeteorObject(startPos);

        // Fall
        float elapsed = 0f;
        while (elapsed < fallTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallTime;
            Vector2 pos = Vector2.Lerp(startPos, groundTarget, t);
            meteor.transform.position = pos;

            // Rotate meteor along travel direction
            Vector2 dir = (groundTarget - startPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            meteor.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f + meteorRotationOffset);

            // Scale warning circle alpha
            if (warning != null)
            {
                float warningAlpha = Mathf.Lerp(0.1f, 0.5f, t);
                var wsr = warning.GetComponent<SpriteRenderer>();
                if (wsr != null)
                    wsr.color = new Color(warningColor.r, warningColor.g, warningColor.b, warningAlpha);
            }

            yield return null;
        }

        // Destroy warning & meteor
        if (warning != null) Destroy(warning);
        Destroy(meteor);

        // Explosion
        Explode(groundTarget, damage);
    }

    private void Explode(Vector2 center, float damage)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMeteorExplosion();

        // Damage enemies in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, blastRadius);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
                enemy.TakeDamage(damage, DamagePopupType.Skill);
        }

        // Sprite sheet explosion animation (preferred) → particle FX → fallback
        if (explosionFrames != null && explosionFrames.Length > 0)
        {
            Debug.Log($"[Meteor] Explode → PlayExplosionAnim at {center}");
            StartCoroutine(PlayExplosionAnim(center));
        }
        else if (meteorExplosionFXPrefab != null)
        {
            Debug.Log($"[Meteor] Explode → Instantiate meteorExplosionFXPrefab at {center}");
            Instantiate(meteorExplosionFXPrefab, center, Quaternion.identity);
        }
        else
        {
            Debug.Log($"[Meteor] Explode → SimpleSkillEffect.CreateCircle at {center}");
            SimpleSkillEffect.CreateCircle(center, blastRadius, new Color(1f, 0.45f, 0.15f, 1f), 0.3f);
        }

        // Camera shake
        CombatFeedbackSystem.ShakeCamera(0.12f, shakeStrength);
    }

    private GameObject CreateMeteorObject(Vector2 pos)
    {
        GameObject obj = new GameObject("Meteor");
        obj.transform.position = pos;

        var sr = obj.AddComponent<SpriteRenderer>();
        bool useFrames = meteorSpriteFrames != null && meteorSpriteFrames.Length > 0;

        if (useFrames)
        {
            sr.sprite = meteorSpriteFrames[0];
            sr.color = Color.white;
            obj.transform.localScale = Vector3.one * 3.6f;
            StartCoroutine(AnimateSprite(sr, meteorSpriteFrames, meteorAnimFPS));
        }
        else
        {
            obj.transform.localScale = Vector3.one * 1.2f;
            sr.sprite = meteorSprite != null ? meteorSprite : ProceduralSpriteLibrary.GetEffectCircleSprite();
            sr.color = meteorColor;
        }

        sr.sortingOrder = 28;

        var trail = obj.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.time = 0.3f;
        trail.startWidth = 0.9f;
        trail.endWidth = 0.06f;
        trail.startColor = new Color(1f, 0.5f, 0.1f, 1f);
        trail.endColor = new Color(1f, 0.2f, 0f, 0f);
        trail.sortingOrder = 27;
        trail.numCapVertices = 3;

        return obj;
    }

    private IEnumerator AnimateSprite(SpriteRenderer sr, Sprite[] frames, float fps)
    {
        float interval = 1f / Mathf.Max(fps, 1f);
        int index = 0;
        while (sr != null)
        {
            sr.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(interval);
        }
    }

    private GameObject CreateWarningCircle(Vector2 pos)
    {
        GameObject obj = new GameObject("MeteorWarning");
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * warningRadius * 2.5f;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteLibrary.GetEffectCircleSprite();
        sr.color = new Color(1f, 0.15f, 0.1f, 0.35f);
        sr.sortingOrder = 1;

        obj.AddComponent<FXWarningCircle>();

        return obj;
    }

    private IEnumerator PlayExplosionAnim(Vector2 center)
    {
        GameObject obj = new GameObject("MeteorExplosionAnim");
        obj.transform.position = center;
        obj.transform.localScale = Vector3.one * explosionScale;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = explosionFrames[0];
        sr.color = new Color(1f, 1f, 1f, 0.7f);
        sr.sortingOrder = 25;

        int total = explosionFrames.Length;
        int fadeStart = Mathf.Max(total - 2, 0);
        float interval = 1f / Mathf.Max(explosionFPS, 1f);

        for (int i = 0; i < total; i++)
        {
            if (sr == null) yield break;
            sr.sprite = explosionFrames[i];

            if (i >= fadeStart)
            {
                float t = (float)(i - fadeStart + 1) / (total - fadeStart);
                sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.7f, 0f, t));
            }

            yield return new WaitForSeconds(interval);
        }

        if (obj != null) Destroy(obj);
    }
}
