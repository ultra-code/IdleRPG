using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;
    [SerializeField] private WaveSpawnManager waveManager;

    [Header("스킬 목록")]
    [SerializeField] private SkillData[] skills;

    [Header("New Skill Scripts")]
    [SerializeField] private FanSkill fanSkill;
    [SerializeField] private MeteorSkill meteorSkill;
    [SerializeField] private ChainSkill chainSkill;
    [SerializeField] private DashSkill dashSkill;

    private float[] cooldownTimers;
    private bool isBuffActive;
    private float buffTimer;
    private AudioSource audioSource;
    private ActorAnimationEvents animationEvents;
    private bool isSkillCasting;

    public bool IsBuffActive => isBuffActive;
    public event Action<SkillData> OnSkillUsed;

    // Priority: Buff(Dash) > Circle(Meteor) > Chain > FanShape(Staff)
    private static readonly SkillType[] priorityOrder =
    {
        SkillType.Buff,
        SkillType.Circle,
        SkillType.Chain,
        SkillType.FanShape
    };

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();
        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveSpawnManager>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        animationEvents = GetComponent<ActorAnimationEvents>();

        // Auto-find skill scripts on same GameObject if not assigned
        if (fanSkill == null) fanSkill = GetComponent<FanSkill>();
        if (meteorSkill == null) meteorSkill = GetComponent<MeteorSkill>();
        if (chainSkill == null) chainSkill = GetComponent<ChainSkill>();
        if (dashSkill == null) dashSkill = GetComponent<DashSkill>();

        // Auto-add if missing
        if (fanSkill == null) fanSkill = gameObject.AddComponent<FanSkill>();
        if (meteorSkill == null) meteorSkill = gameObject.AddComponent<MeteorSkill>();
        if (chainSkill == null) chainSkill = gameObject.AddComponent<ChainSkill>();
        if (dashSkill == null) dashSkill = gameObject.AddComponent<DashSkill>();

        if (skills != null)
            cooldownTimers = new float[skills.Length];
    }

    private void Update()
    {
        if (player == null || player.IsDead) return;
        if (skills == null || skills.Length == 0) return;

        UpdateCooldowns();
        UpdateBuff();
        if (isSkillCasting) return;
        TryUseSkillByPriority();
    }

    private void UpdateCooldowns()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
                cooldownTimers[i] -= Time.deltaTime;
        }
    }

    private void UpdateBuff()
    {
        if (!isBuffActive) return;

        buffTimer -= Time.deltaTime;
        if (buffTimer <= 0f)
        {
            isBuffActive = false;
            Debug.Log("Buff ended");
        }
    }

    private void TryUseSkillByPriority()
    {
        foreach (SkillType type in priorityOrder)
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] == null) continue;
                if (skills[i].skillType != type) continue;
                if (cooldownTimers[i] > 0f) continue;

                if (TryExecuteSkill(skills[i], i))
                    return;
            }
        }
    }

    private bool TryExecuteSkill(SkillData skill, int index)
    {
        List<Enemy> allEnemies = waveManager != null ? waveManager.ActiveEnemies : new List<Enemy>();

        // For non-buff skills, need at least one enemy alive
        if (skill.skillType != SkillType.Buff)
        {
            bool hasAlive = false;
            foreach (var e in allEnemies)
            {
                if (e != null && !e.IsDead) { hasAlive = true; break; }
            }
            if (!hasAlive) return false;
        }

        if (skill.skillType == SkillType.Buff)
        {
            if (isBuffActive) return false;
            if (dashSkill != null && dashSkill.IsDashing) return false;
        }

        isSkillCasting = true;
        animationEvents?.PlaySkill();
        cooldownTimers[index] = skill.cooldown;
        OnSkillUsed?.Invoke(skill);
        PlayEffect(skill, player.transform.position);

        StartCoroutine(ExecuteNewSkillAfterDelay(skill, allEnemies));
        return true;
    }

    private IEnumerator ExecuteNewSkillAfterDelay(SkillData skill, List<Enemy> allEnemies)
    {
        float impactDelay = animationEvents != null ? animationEvents.GetSkillImpactDelay() : 0.12f;
        yield return new WaitForSeconds(impactDelay);

        switch (skill.skillType)
        {
            case SkillType.FanShape:
                fanSkill.Execute(player, allEnemies);
                break;

            case SkillType.Circle:
                meteorSkill.Execute(player, allEnemies);
                break;

            case SkillType.Chain:
                chainSkill.Execute(player, allEnemies);
                break;

            case SkillType.Buff:
                // Activate buff state + execute dash
                isBuffActive = true;
                buffTimer = skill.buffDuration;
                SimpleSkillEffect.CreateAura(player.transform, 1.6f, skill.buffDuration, Color.white);
                dashSkill.Execute(player, allEnemies);
                Debug.Log($"[Berserker Dash] Buff active for {skill.buffDuration}s + dash attack");
                break;
        }

        isSkillCasting = false;
    }

    private void PlayEffect(SkillData skill, Vector2 position)
    {
        if (skill.effectPrefab != null)
            Instantiate(skill.effectPrefab, position, Quaternion.identity);

        if (skill.soundClip != null && audioSource != null)
            audioSource.PlayOneShot(skill.soundClip);
    }

    public float GetCooldownRemaining(int index)
    {
        if (cooldownTimers == null || index < 0 || index >= cooldownTimers.Length)
            return 0f;
        return Mathf.Max(cooldownTimers[index], 0f);
    }

    public float GetCooldownRatio(int index)
    {
        if (skills == null || index < 0 || index >= skills.Length) return 0f;
        if (skills[index] == null || skills[index].cooldown <= 0f) return 0f;
        return Mathf.Clamp01(GetCooldownRemaining(index) / skills[index].cooldown);
    }
}
