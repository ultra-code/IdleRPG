using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill 4: Berserker Dash — heroine dashes through all enemies at ultra-high speed.
/// </summary>
public class DashSkill : MonoBehaviour
{
    [Header("Afterimage Visuals")]
    [SerializeField] private Color afterImageColor = new Color(0.91f, 0.27f, 0.48f, 0.6f); // #E8457B
    [SerializeField] private float afterImageInterval = 0.05f;
    [SerializeField] private float afterImageFadeDuration = 0.2f;

    [Header("Settings")]
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float damageMultiplier = 2.0f;
    [SerializeField] private float cameraZoomScale = 0.95f;
    [SerializeField] private float maxDashDuration = 5f;

    private bool isDashing;
    public bool IsDashing => isDashing;

    public void Execute(CharacterStats player, List<Enemy> allEnemies)
    {
        if (player == null || isDashing) return;

        List<Enemy> targets = new List<Enemy>();
        foreach (var e in allEnemies)
        {
            if (e != null && !e.IsDead && MapBounds.IsInside(e.transform.position))
                targets.Add(e);
        }
        if (targets.Count == 0) return;

        // Sort by distance from player
        Vector2 origin = player.transform.position;
        targets.Sort((a, b) =>
            Vector2.Distance(origin, a.transform.position)
            .CompareTo(Vector2.Distance(origin, b.transform.position)));

        float damage = player.ATK * damageMultiplier;
        StartCoroutine(DashRoutine(player, targets, damage));
    }

    private IEnumerator DashRoutine(CharacterStats player, List<Enemy> targets, float damage)
    {
        isDashing = true;
        player.IsInvincible = true;

        float dashStartTime = Time.time;
        Camera cam = Camera.main;
        float originalOrthoSize = cam != null ? cam.orthographicSize : 5f;

        try
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBerserkerDash();
                AudioManager.Instance.PlayBuffActivate();
            }

            Vector3 originalPos = player.transform.position;
            SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();

            // Camera zoom in
            if (cam != null)
                cam.orthographicSize = originalOrthoSize * cameraZoomScale;

            // Afterimage spawning timer
            float afterImageTimer = 0f;

            // Dash through each target
            foreach (var target in targets)
            {
                if (Time.time - dashStartTime >= maxDashDuration)
                {
                    Debug.LogWarning("[Berserker Dash] Timed out during target pass-through");
                    break;
                }
                if (target == null || target.IsDead) continue;

                while (target != null && !target.IsDead)
                {
                    if (Time.time - dashStartTime >= maxDashDuration)
                    {
                        Debug.LogWarning("[Berserker Dash] Timed out while moving to target");
                        break;
                    }

                    Vector3 targetPos = target.transform.position;
                    float step = dashSpeed * Time.deltaTime;
                    player.transform.position = MapBounds.ClampPlayer(
                        Vector3.MoveTowards(player.transform.position, targetPos, step));

                    // Flip sprite
                    Vector3 dir = targetPos - player.transform.position;
                    if (dir.x != 0f)
                    {
                        float scaleX = Mathf.Abs(player.transform.localScale.x);
                        player.transform.localScale = new Vector3(
                            dir.x < 0 ? -scaleX : scaleX,
                            player.transform.localScale.y,
                            player.transform.localScale.z);
                    }

                    // Spawn afterimage
                    afterImageTimer += Time.deltaTime;
                    if (afterImageTimer >= afterImageInterval && playerSR != null)
                    {
                        afterImageTimer = 0f;
                        SpawnAfterImage(player.transform, playerSR);
                    }

                    if (Vector3.Distance(player.transform.position, targetPos) < 0.15f) break;
                    yield return null;
                }

                // Hit on pass-through
                if (target != null && !target.IsDead)
                {
                    target.TakeDamage(damage, DamagePopupType.Skill);
                    CombatFeedbackSystem.HitStop(0.02f, 0.04f);
                }
            }

            // Return to original position
            while (Vector3.Distance(player.transform.position, originalPos) > 0.2f)
            {
                if (Time.time - dashStartTime >= maxDashDuration)
                {
                    Debug.LogWarning("[Berserker Dash] Timed out while returning to origin");
                    break;
                }

                player.transform.position = MapBounds.ClampPlayer(
                    Vector3.MoveTowards(player.transform.position, originalPos, dashSpeed * 0.8f * Time.deltaTime));

                afterImageTimer += Time.deltaTime;
                if (afterImageTimer >= afterImageInterval && playerSR != null)
                {
                    afterImageTimer = 0f;
                    SpawnAfterImage(player.transform, playerSR);
                }

                yield return null;
            }

            player.transform.position = MapBounds.ClampPlayer(originalPos);

            Debug.Log($"[Berserker Dash] Complete! {targets.Count} enemies hit");
        }
        finally
        {
            player.IsInvincible = false;
            if (cam != null)
                cam.orthographicSize = originalOrthoSize;
            isDashing = false;
        }
    }

    private void SpawnAfterImage(Transform source, SpriteRenderer sourceSR)
    {
        GameObject obj = new GameObject("DashAfterImage");
        obj.transform.position = source.position;
        obj.transform.localScale = source.localScale;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sourceSR.sprite;
        sr.flipX = sourceSR.flipX;
        sr.color = afterImageColor;
        sr.sortingOrder = sourceSR.sortingOrder - 1;

        StartCoroutine(FadeAndDestroy(obj, sr));
    }

    private IEnumerator FadeAndDestroy(GameObject obj, SpriteRenderer sr)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < afterImageFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / afterImageFadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(obj);
    }
}
