using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 정보")]
    public string EnemyName = "Slime";
    public int Level = 1;

    [Header("스탯")]
    public float MaxHP = 50f;
    public float ATK = 8f;
    public float DEF = 2f;

    [Header("보상")]
    public float ExpReward = 30f;
    public float GoldReward = 10f;

    [Header("이동")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float separationRadius = 0.8f;
    [SerializeField] private float separationStrength = 3f;

    [Header("전투")]
    [SerializeField] private float attackInterval = 1.5f;

    private float currentHP;
    private float attackTimer;
    private CharacterStats target;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private ActorAnimationEvents animationEvents;
    private Vector2 moveDir;
    private bool shouldMove;
    private bool isBoss;
    private bool isAttackInProgress;
    private bool deathHandled;

    public event Action<Enemy> OnDeath;

    public float CurrentHP => currentHP;
    public float HPRatio => MaxHP > 0 ? currentHP / MaxHP : 0f;
    public bool IsDead => currentHP <= 0f;
    public bool IsBoss => isBoss;

    private void Awake()
    {
        currentHP = MaxHP;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animationEvents = GetComponent<ActorAnimationEvents>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        animationEvents?.ResetState();

        EnemyWorldHPBar.Create(this);
    }

    public void SetTarget(CharacterStats player)
    {
        target = player;
    }

    private void Update()
    {
        if (IsDead || target == null || target.IsDead)
        {
            shouldMove = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            moveDir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
            shouldMove = true;
            attackTimer = 0f;
            return;
        }

        shouldMove = false;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            if (!isAttackInProgress)
                StartCoroutine(AttackRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (IsDead || rb == null) return;

        Vector2 separation = ComputeSeparation();

        if (shouldMove)
        {
            Vector2 move = moveDir * moveSpeed + separation * separationStrength;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
        }
        else if (separation.sqrMagnitude > 0.001f)
        {
            rb.MovePosition(rb.position + separation * separationStrength * Time.fixedDeltaTime);
        }
    }

    private Vector2 ComputeSeparation()
    {
        Vector2 sep = Vector2.zero;
        float radius = separationRadius * transform.localScale.x;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(rb.position, radius);
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            // Push away from other enemies
            Enemy other = col.GetComponent<Enemy>();
            if (other != null && !other.IsDead)
            {
                Vector2 diff = rb.position - (Vector2)other.transform.position;
                float dist = diff.magnitude;
                if (dist > 0f && dist < radius)
                    sep += diff.normalized * (1f - dist / radius);
                continue;
            }

            // Push away from player
            if (col.GetComponent<CharacterStats>() != null)
            {
                Vector2 diff = rb.position - (Vector2)col.transform.position;
                float dist = diff.magnitude;
                if (dist > 0f && dist < radius)
                    sep += diff.normalized * (1f - dist / radius);
            }
        }
        return sep;
    }

    public void TakeDamage(float damage, DamagePopupType popupType = DamagePopupType.Normal)
    {
        if (IsDead || deathHandled) return;

        float actualDamage = Mathf.Max(damage - DEF, 1f);
        bool isCritical = UnityEngine.Random.value < 0.1f;
        if (isCritical)
            actualDamage *= 1.5f;

        currentHP = Mathf.Max(currentHP - actualDamage, 0f);
        CombatFeedbackSystem.HitStop(isCritical ? 0.04f : 0.025f, isCritical ? 0.04f : 0.08f);

        if (AudioManager.Instance != null)
        {
            if (isCritical)
                AudioManager.Instance.PlayCriticalHit();
            else
                AudioManager.Instance.PlayEnemyHit();
        }

        DamagePopupType resolvedType = isBoss ? DamagePopupType.Boss : popupType;
        DamagePopup.Create(transform.position + Vector3.up * 0.7f, Mathf.CeilToInt(actualDamage), isCritical, resolvedType);

        if (IsDead)
        {
            Die();
        }
        else
        {
            animationEvents?.PlayHit();
        }
    }

    private void Die()
    {
        if (deathHandled)
            return;

        deathHandled = true;
        shouldMove = false;
        isAttackInProgress = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDeath();

        animationEvents?.PlayDie();
        CombatFeedbackSystem.BloodBurst(transform.position + Vector3.up * 0.2f, isBoss);
        if (isBoss)
            CombatFeedbackSystem.ShakeCamera(0.28f, 0.22f);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        if (target != null)
        {
            target.AddExp(ExpReward);
            target.Gold += GoldReward;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayGoldPickup();
        }

        Debug.Log($"{EnemyName} 처치! EXP +{ExpReward}, Gold +{GoldReward}");
        OnDeath?.Invoke(this);
        float destroyDelay = animationEvents != null ? animationEvents.GetDeathDestroyDelay() : 0.35f;
        Destroy(gameObject, destroyDelay);
    }

    public void Initialize(EnemyData data, int level)
    {
        EnemyName = data.EnemyName;
        Level = level;
        MaxHP = data.GetHP(level);
        ATK = data.GetATK(level);
        DEF = data.GetDEF(level);
        ExpReward = data.GetExpReward(level);
        GoldReward = data.GetGoldReward(level);
        moveSpeed = data.MoveSpeed;
        attackRange = data.AttackRange;
        attackInterval = data.AttackInterval;
        currentHP = MaxHP;
        isBoss = false;
        deathHandled = false;
        isAttackInProgress = false;
        animationEvents?.ResetState();

        transform.localScale = Vector3.one * data.SpriteScale;
        separationRadius = 0.5f + data.SpriteScale * 0.3f;
        ApplyVisual(data, false);
    }

    public void Initialize(EnemyData data, StageData stage)
    {
        EnemyName = data.EnemyName;
        MaxHP = data.BaseHP * stage.HPMult;
        ATK = data.BaseATK * stage.ATKMult;
        DEF = data.BaseDEF * stage.DEFMult;
        ExpReward = data.BaseExpReward;
        GoldReward = data.BaseGoldReward * stage.GoldMult;
        moveSpeed = data.MoveSpeed;
        attackRange = data.AttackRange;
        attackInterval = data.AttackInterval;
        currentHP = MaxHP;
        isBoss = false;
        deathHandled = false;
        isAttackInProgress = false;
        animationEvents?.ResetState();

        transform.localScale = Vector3.one * data.SpriteScale;
        separationRadius = 0.5f + data.SpriteScale * 0.3f;
        ApplyVisual(data, false);
    }

    public void InitializeAsBoss(BossData bossData, StageData stage)
    {
        EnemyData baseEnemy = bossData.BaseEnemy;
        EnemyName = bossData.DisplayName;
        MaxHP = baseEnemy.BaseHP * stage.HPMult * bossData.BossHPMult;
        ATK = baseEnemy.BaseATK * stage.ATKMult * bossData.BossATKMult;
        DEF = baseEnemy.BaseDEF * stage.DEFMult;
        ExpReward = baseEnemy.BaseExpReward * bossData.RewardMult;
        GoldReward = baseEnemy.BaseGoldReward * stage.GoldMult * bossData.RewardMult + bossData.FixedGoldBonus;
        moveSpeed = baseEnemy.MoveSpeed;
        attackRange = baseEnemy.AttackRange;
        attackInterval = baseEnemy.AttackInterval;
        currentHP = MaxHP;
        isBoss = true;
        deathHandled = false;
        isAttackInProgress = false;
        animationEvents?.ResetState();

        transform.localScale = Vector3.one * baseEnemy.SpriteScale * 1.5f;
        separationRadius = 0.5f + baseEnemy.SpriteScale * 1.5f * 0.3f;
        ApplyVisual(baseEnemy, true);

        Transform hpBarCanvas = transform.Find("HPBarCanvas");
        if (hpBarCanvas != null)
            Destroy(hpBarCanvas.gameObject);
    }

    public void SetLevel(int level)
    {
        Level = level;
        MaxHP = 50f + (level - 1) * 15f;
        ATK = 8f + (level - 1) * 2f;
        DEF = 2f + (level - 1) * 1f;
        ExpReward = 30f + (level - 1) * 10f;
        GoldReward = 10f + (level - 1) * 5f;
        currentHP = MaxHP;
        isBoss = false;
        deathHandled = false;
        isAttackInProgress = false;
        animationEvents?.ResetState();

        if (spriteRenderer != null && !HasAnimationController())
            ProceduralSpriteLibrary.ApplyEnemyVisual(spriteRenderer, EnemyName, false);
    }

    public void ConfigureAsBoss(float hpMultiplier, float atkMultiplier, float rewardMultiplier)
    {
        isBoss = true;
        MaxHP *= hpMultiplier;
        currentHP = MaxHP;
        ATK *= atkMultiplier;
        ExpReward *= rewardMultiplier;
        GoldReward *= rewardMultiplier;
        EnemyName = $"BOSS {EnemyName}";
        transform.localScale *= 1.5f;
        deathHandled = false;
        isAttackInProgress = false;
        animationEvents?.ResetState();

        if (spriteRenderer != null && !HasAnimationController())
            ProceduralSpriteLibrary.ApplyEnemyVisual(spriteRenderer, EnemyName, true);

        Transform hpBarCanvas = transform.Find("HPBarCanvas");
        if (hpBarCanvas != null)
            Destroy(hpBarCanvas.gameObject);
    }

    private void ApplyVisual(EnemyData data, bool boss)
    {
        if (data == null)
            return;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null && data.AnimatorController != null)
        {
            animator.runtimeAnimatorController = data.AnimatorController;

            if (spriteRenderer != null && data.Sprite != null && spriteRenderer.sprite == null)
                spriteRenderer.sprite = data.Sprite;

            return;
        }

        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = data.Sprite;
        ProceduralSpriteLibrary.ApplyEnemyVisual(spriteRenderer, EnemyName, boss);
    }

    private bool HasAnimationController()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        return animator != null && animator.runtimeAnimatorController != null;
    }

    private IEnumerator AttackRoutine()
    {
        isAttackInProgress = true;
        animationEvents?.PlayAttack();

        float impactDelay = animationEvents != null ? animationEvents.GetAttackImpactDelay() : 0.1f;
        yield return new WaitForSeconds(impactDelay);

        if (!deathHandled && !IsDead && target != null && !target.IsDead)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance <= attackRange + 0.1f)
                target.TakeDamage(ATK);
        }

        isAttackInProgress = false;
    }
}
