using UnityEngine;
using System;
using System.Collections.Generic;

public class ActorAnimationEvents : MonoBehaviour
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int SkillHash = Animator.StringToHash("Skill");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Animator animator;
    private bool isDead;
    private readonly Dictionary<string, float> clipLengths = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        animator = GetComponent<Animator>();
        CacheClipLengths();
    }

    public void PlayAttack()
    {
        Trigger(AttackHash);
    }

    public void PlayHit()
    {
        Trigger(HitHash);
    }

    public void PlaySkill()
    {
        Trigger(SkillHash);
    }

    public void PlayDie()
    {
        if (animator == null || isDead)
            return;

        isDead = true;
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(HitHash);
        animator.ResetTrigger(SkillHash);
        animator.SetTrigger(DieHash);
    }

    public void ResetState()
    {
        isDead = false;
        if (animator == null)
            animator = GetComponent<Animator>();
        CacheClipLengths();
    }

    public float GetAttackImpactDelay()
    {
        return Mathf.Clamp(GetClipLength("attack", 0.18f) * 0.35f, 0.06f, 0.18f);
    }

    public float GetSkillImpactDelay()
    {
        return Mathf.Clamp(GetClipLength("skill", 0.22f) * 0.4f, 0.08f, 0.22f);
    }

    public float GetDeathDestroyDelay()
    {
        return Mathf.Clamp(GetClipLength("die", 0.45f) * 0.95f, 0.2f, 0.8f);
    }

    private void Trigger(int triggerHash)
    {
        if (animator == null || isDead || animator.runtimeAnimatorController == null)
            return;

        animator.SetTrigger(triggerHash);
    }

    private float GetClipLength(string keyword, float fallback)
    {
        if (clipLengths.TryGetValue(keyword, out float value) && value > 0f)
            return value;

        return fallback;
    }

    private void CacheClipLengths()
    {
        clipLengths.Clear();

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip == null)
                continue;

            string name = clip.name.ToLowerInvariant();
            if (name.Contains("attack"))
                clipLengths["attack"] = clip.length;
            else if (name.Contains("hit"))
                clipLengths["hit"] = clip.length;
            else if (name.Contains("skill"))
                clipLengths["skill"] = clip.length;
            else if (name.Contains("die"))
                clipLengths["die"] = clip.length;
        }
    }
}
