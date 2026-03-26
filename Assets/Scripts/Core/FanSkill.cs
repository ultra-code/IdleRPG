using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill 1: Staff Rush (Yondu Style) — staff projectile pierces enemies one by one then returns.
/// </summary>
public class FanSkill : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Sprite staffSprite;
    [SerializeField] private Sprite[] staffSpriteFrames;
    [SerializeField] private float staffAnimFPS = 12f;
    [SerializeField] private Material additiveMaterial;
    [SerializeField] private Color trailStartColor = new Color(0.83f, 0.64f, 0.30f, 1f); // #D4A44C
    [SerializeField] private Color trailEndColor = new Color(0.83f, 0.64f, 0.30f, 0f);

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private int maxTargets = 8;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private float redirectDelay = 0.03f;

    public void Execute(CharacterStats player, List<Enemy> allEnemies)
    {
        if (player == null) return;
        List<Enemy> targets = CollectTargets(player.transform.position, allEnemies, maxTargets);
        if (targets.Count == 0) return;

        float damage = player.ATK * damageMultiplier;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayStaffRush();

        StartCoroutine(StaffRushRoutine(player.transform, targets, damage));
    }

    private List<Enemy> CollectTargets(Vector2 origin, List<Enemy> allEnemies, int max)
    {
        List<Enemy> pool = new List<Enemy>();
        foreach (var e in allEnemies)
        {
            if (e != null && !e.IsDead && MapBounds.IsInside(e.transform.position))
                pool.Add(e);
        }
        pool.Sort((a, b) =>
            Vector2.Distance(origin, a.transform.position)
            .CompareTo(Vector2.Distance(origin, b.transform.position)));
        if (pool.Count > max) pool.RemoveRange(max, pool.Count - max);
        return pool;
    }

    private IEnumerator StaffRushRoutine(Transform playerTransform, List<Enemy> targets, float damage)
    {
        GameObject proj = CreateProjectile(playerTransform.position);
        Vector3 pos = playerTransform.position;

        for (int i = 0; i < targets.Count; i++)
        {
            Enemy target = targets[i];
            if (target == null || target.IsDead) continue;

            // Move toward target
            while (target != null && !target.IsDead)
            {
                Vector3 targetPos = target.transform.position;
                Vector3 dir = (targetPos - pos).normalized;
                float step = projectileSpeed * Time.deltaTime;
                pos = Vector3.MoveTowards(pos, targetPos, step);
                proj.transform.position = pos;

                // Rotate to face direction
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

                if (Vector3.Distance(pos, targetPos) < 0.15f) break;
                yield return null;
            }

            // Hit
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage, DamagePopupType.Skill);
                SimpleSkillEffect.CreateChain(pos, target.transform.position, new Color(1f, 0.82f, 0.3f, 1f), 0.12f);
            }

            if (i < targets.Count - 1)
                yield return new WaitForSeconds(redirectDelay);
        }

        // Return to player
        float returnSpeed = projectileSpeed * 1.2f;
        while (playerTransform != null && Vector3.Distance(pos, playerTransform.position) > 0.2f)
        {
            Vector3 dir = (playerTransform.position - pos).normalized;
            pos = Vector3.MoveTowards(pos, playerTransform.position, returnSpeed * Time.deltaTime);
            proj.transform.position = pos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        Destroy(proj);
        Debug.Log($"[Staff Rush] Complete! {targets.Count} targets pierced");
    }

    private GameObject CreateProjectile(Vector3 startPos)
    {
        GameObject obj = new GameObject("StaffProjectile");
        obj.transform.position = startPos;

        var sr = obj.AddComponent<SpriteRenderer>();
        bool useFrames = staffSpriteFrames != null && staffSpriteFrames.Length > 0;

        if (useFrames)
        {
            sr.sprite = staffSpriteFrames[0];
            sr.color = Color.white;
            obj.transform.localScale = Vector3.one * 2f;
            StartCoroutine(AnimateSprite(sr, staffSpriteFrames, staffAnimFPS));
        }
        else
        {
            sr.sprite = staffSprite != null ? staffSprite : ProceduralSpriteLibrary.GetEffectCircleSprite();
            sr.color = new Color(0.83f, 0.64f, 0.30f, 1f);
            if (staffSprite == null)
                obj.transform.localScale = Vector3.one * 0.6f;
            else
                obj.transform.localScale = Vector3.one * 2f;
        }

        sr.sortingOrder = 30;

        var trail = obj.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.time = 0.2f;
        trail.startWidth = 0.5f;
        trail.endWidth = 0.04f;
        trail.startColor = trailStartColor;
        trail.endColor = trailEndColor;
        trail.sortingOrder = 29;
        trail.numCapVertices = 4;

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
}
