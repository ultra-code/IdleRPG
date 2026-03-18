using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 정보")]
    public string EnemyName = "슬라임";
    public int Level = 1;

    [Header("스탯")]
    public float MaxHP = 50f;
    public float ATK = 8f;
    public float DEF = 2f;

    [Header("보상")]
    public float ExpReward = 30f;
    public float GoldReward = 10f;

    [Header("전투")]
    [SerializeField] private float attackInterval = 1.5f;

    private float currentHP;
    private float attackTimer;
    private CharacterStats target;

    public event Action<Enemy> OnDeath;

    public float CurrentHP => currentHP;
    public float HPRatio => MaxHP > 0 ? currentHP / MaxHP : 0f;
    public bool IsDead => currentHP <= 0f;

    private void Awake()
    {
        currentHP = MaxHP;
    }

    public void SetTarget(CharacterStats player)
    {
        target = player;
    }

    private void Update()
    {
        if (IsDead || target == null || target.IsDead) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            target.TakeDamage(ATK);
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        float actualDamage = Mathf.Max(damage - DEF, 1f);
        currentHP = Mathf.Max(currentHP - actualDamage, 0f);

        if (IsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        if (target != null)
        {
            target.AddExp(ExpReward);
            target.Gold += GoldReward;
        }

        Debug.Log($"{EnemyName} 처치! EXP +{ExpReward}, Gold +{GoldReward}");
        OnDeath?.Invoke(this);
        Destroy(gameObject);
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
        attackInterval = data.AttackInterval;
        currentHP = MaxHP;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.Sprite != null)
            sr.sprite = data.Sprite;
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
    }
}
