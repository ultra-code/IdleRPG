using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill 3: Chain Lightning Ball — bouncing lightning orb with escalating damage.
/// </summary>
public class ChainSkill : MonoBehaviour
{
    [Header("Lightning Ball Visuals")]
    [SerializeField] private Sprite ballSprite;
    [SerializeField] private Sprite sparkSprite;
    [SerializeField] private Color ballColor = new Color(0.19f, 0.63f, 1f, 1f); // #30A0FF
    [SerializeField] private Color trailStart = new Color(0.19f, 0.63f, 1f, 1f);
    [SerializeField] private Color trailEnd = new Color(0.5f, 0.88f, 1f, 0f);

    [Header("Particle FX Prefab")]
    [SerializeField] private GameObject chainSparkFXPrefab;

    [Header("Settings")]
    [SerializeField] private float ballSpeed = 22f;
    [SerializeField] private int maxChains = 5;
    [SerializeField] private float baseDamageMultiplier = 1.0f;
    [SerializeField] private float chainDamageScale = 1.2f; // damage grows each bounce
    [SerializeField] private float sparkDuration = 0.15f;

    public void Execute(CharacterStats player, List<Enemy> allEnemies)
    {
        if (player == null) return;
        List<Enemy> pool = new List<Enemy>();
        foreach (var e in allEnemies)
        {
            if (e != null && !e.IsDead && MapBounds.IsInside(e.transform.position))
                pool.Add(e);
        }
        if (pool.Count == 0) return;

        float baseDamage = player.ATK * baseDamageMultiplier;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayChainLightning();

        StartCoroutine(ChainLightningRoutine(player.transform.position, pool, baseDamage));
    }

    private IEnumerator ChainLightningRoutine(Vector3 origin, List<Enemy> pool, float baseDamage)
    {
        GameObject ball = CreateBall(origin);
        Vector3 pos = origin;
        HashSet<Enemy> hit = new HashSet<Enemy>();
        float currentDamage = baseDamage;
        int chainCount = 0;

        // Sort by distance for first target
        Enemy current = FindClosest(pos, pool, hit);

        while (current != null && chainCount < maxChains)
        {
            Vector3 prevPos = pos;

            // Move ball to target
            while (current != null && !current.IsDead)
            {
                Vector3 targetPos = current.transform.position;
                float step = ballSpeed * Time.deltaTime;
                pos = Vector3.MoveTowards(pos, targetPos, step);
                ball.transform.position = pos;

                if (Vector3.Distance(pos, targetPos) < 0.15f) break;
                yield return null;
            }

            // Hit
            if (current != null && !current.IsDead)
            {
                current.TakeDamage(currentDamage, DamagePopupType.Skill);
                hit.Add(current);
                chainCount++;

                // Lightning line from prev to current
                SimpleSkillEffect.CreateChain(prevPos, current.transform.position, trailStart, 0.1f);

                // Spark effect at impact
                SpawnSpark(current.transform.position);

                // Escalate damage
                currentDamage *= chainDamageScale;
            }

            // Find next target
            current = FindClosest(pos, pool, hit);
        }

        Destroy(ball);
        Debug.Log($"[Chain Lightning] {chainCount} chains, final damage multiplier: {currentDamage / baseDamage:F2}x");
    }

    private Enemy FindClosest(Vector3 from, List<Enemy> pool, HashSet<Enemy> exclude)
    {
        Enemy best = null;
        float bestDist = float.MaxValue;
        foreach (var e in pool)
        {
            if (e == null || e.IsDead || exclude.Contains(e)) continue;
            float d = Vector3.Distance(from, e.transform.position);
            if (d < bestDist) { bestDist = d; best = e; }
        }
        return best;
    }

    private GameObject CreateBall(Vector3 pos)
    {
        GameObject obj = new GameObject("LightningBall");
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * 0.35f;

        var sr = obj.AddComponent<SpriteRenderer>();
        if (ballSprite != null)
        {
            sr.sprite = ballSprite;
            sr.color = ballColor;
        }
        else
        {
            sr.sprite = ProceduralSpriteLibrary.GetEffectCircleSprite();
            sr.color = new Color(0.588f, 0.863f, 1f, 1f); // #96DCFF
        }
        sr.sortingOrder = 30;

        var trail = obj.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.time = 0.15f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0.02f;
        trail.startColor = trailStart;
        trail.endColor = trailEnd;
        trail.sortingOrder = 29;
        trail.numCapVertices = 4;

        return obj;
    }

    private void SpawnSpark(Vector3 pos)
    {
        if (chainSparkFXPrefab != null)
        {
            Instantiate(chainSparkFXPrefab, pos, Quaternion.identity);
            return;
        }

        // Fallback: original sprite spark
        GameObject obj = new GameObject("LightningSpark");
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * 0.5f;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sparkSprite != null ? sparkSprite : ProceduralSpriteLibrary.GetEffectCircleSprite();
        sr.color = new Color(0.5f, 0.88f, 1f, 1f);
        sr.sortingOrder = 31;

        Destroy(obj, sparkDuration);
    }
}
