using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("기본 스탯")]
    public int Level = 1;
    public float HP;
    public float MaxHP = 100f;
    public float ATK = 10f;
    public float DEF = 5f;

    [Header("강화 보너스")]
    [HideInInspector] public float bonusHP;
    [HideInInspector] public float bonusATK;
    [HideInInspector] public float bonusDEF;

    [Header("경험치 / 재화")]
    public float Exp;
    public float ExpToNextLevel = 100f;
    public float Gold;

    [Header("데이터")]
    [SerializeField] private LevelData levelData;

    [Header("레벨업 성장 계수 (LevelData 없을 때 사용)")]
    [SerializeField] private float hpPerLevel = 20f;
    [SerializeField] private float hpQuadratic = 1.2f;
    [SerializeField] private float atkPerLevel = 5.7f;
    [SerializeField] private float defPerLevel = 2.9f;
    [SerializeField] private float expMultiplier = 1.45f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float attackRange = 1.5f;

    private ActorAnimationEvents animationEvents;
    private WaveSpawnManager waveManager;
    private SkillSystem skillSystemCache;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (levelData != null)
            ApplyLevelData();
        ApplyBonuses();
        HP = MaxHP;

        var renderer = GetComponent<SpriteRenderer>();
        var animator = GetComponent<Animator>();
        bool hasAnimationController = animator != null && animator.runtimeAnimatorController != null;
        if (renderer != null && !hasAnimationController)
            ProceduralSpriteLibrary.ApplyPlayerVisual(renderer);

        animationEvents = GetComponent<ActorAnimationEvents>();
        animationEvents?.ResetState();

        transform.localScale = Vector3.one * 1.3f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        MapBounds.Initialize();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveSpawnManager>();
        if (waveManager == null || waveManager.ActiveEnemies.Count == 0) return;

        Enemy closest = null;
        float closestDist = float.MaxValue;
        foreach (var enemy in waveManager.ActiveEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (!MapBounds.IsInside(enemy.transform.position)) continue;
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = enemy; }
        }

        if (closest == null) return;

        if (closestDist > attackRange)
        {
            Vector2 dir = ((Vector2)closest.transform.position - (Vector2)transform.position).normalized;

            float currentMoveSpeed = moveSpeed;
            if (skillSystemCache == null)
                skillSystemCache = FindAnyObjectByType<SkillSystem>();
            if (skillSystemCache != null && skillSystemCache.IsBuffActive)
                currentMoveSpeed *= 2f;

            Vector2 sep = ComputePlayerSeparation();
            Vector2 targetPos = rb.position + (dir * currentMoveSpeed + sep * 1.5f) * Time.fixedDeltaTime;
            rb.MovePosition(MapBounds.ClampPlayer(targetPos));

            // localScale flip (Animator-safe, flipX can be overwritten by Animator)
            float scaleX = Mathf.Abs(transform.localScale.x);
            if (dir.x < 0)
                scaleX = -scaleX;
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    private Vector2 ComputePlayerSeparation()
    {
        Vector2 sep = Vector2.zero;
        const float radius = 0.5f;
        if (waveManager == null) return sep;
        foreach (var enemy in waveManager.ActiveEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            Vector2 diff = rb.position - (Vector2)enemy.transform.position;
            float dist = diff.magnitude;
            if (dist > 0f && dist < radius)
                sep += diff.normalized * (1f - dist / radius);
        }
        return sep;
    }

    private void LateUpdate()
    {
        transform.position = MapBounds.ClampPlayer(transform.position);
    }

    private void ApplyLevelData()
    {
        MaxHP = levelData.GetMaxHP(Level);
        ATK = levelData.GetATK(Level);
        DEF = levelData.GetDEF(Level);
        ExpToNextLevel = levelData.GetExpToNextLevel(Level);
    }

    private void ApplyBonuses()
    {
        MaxHP += bonusHP;
        ATK += bonusATK;
        DEF += bonusDEF;
    }

    public void RefreshStats()
    {
        float hpRatio = MaxHP > 0 ? HP / MaxHP : 1f;

        if (levelData != null)
            ApplyLevelData();
        else
        {
            MaxHP = 100f + (Level - 1) * hpPerLevel + (Level - 1) * (Level - 1) * hpQuadratic;
            ATK = 10f + (Level - 1) * atkPerLevel;
            DEF = 5f + (Level - 1) * defPerLevel;
        }

        ApplyBonuses();
        HP = MaxHP * hpRatio;
    }

    public void AddExp(float amount)
    {
        Exp += amount;
        while (Exp >= ExpToNextLevel)
        {
            Exp -= ExpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLevelUp();

        if (levelData != null)
        {
            ApplyLevelData();
        }
        else
        {
            MaxHP = 100f + (Level - 1) * hpPerLevel + (Level - 1) * (Level - 1) * hpQuadratic;
            ATK = 10f + (Level - 1) * atkPerLevel;
            DEF = 5f + (Level - 1) * defPerLevel;
            ExpToNextLevel = Mathf.Floor(100f * Mathf.Pow(expMultiplier, Level - 1));
        }

        ApplyBonuses();
        HP = MaxHP;
        Debug.Log($"레벨 업! Lv.{Level} | HP:{MaxHP} ATK:{ATK} DEF:{DEF}");
    }

    public void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage - DEF, 1f);
        HP = Mathf.Max(HP - actualDamage, 0f);

        if (HP <= 0f)
            animationEvents?.PlayDie();
        else
            animationEvents?.PlayHit();
    }

    public bool IsDead => HP <= 0f;
}
