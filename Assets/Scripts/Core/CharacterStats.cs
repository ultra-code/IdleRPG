using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("기본 스탯")]
    public int Level = 1;
    public float HP;
    public float MaxHP = 100f;
    public float ATK = 10f;
    public float DEF = 5f;

    [Header("경험치 / 재화")]
    public float Exp;
    public float ExpToNextLevel = 100f;
    public float Gold;

    [Header("데이터")]
    [SerializeField] private LevelData levelData;

    [Header("레벨업 성장 계수 (LevelData 없을 때 사용)")]
    [SerializeField] private float hpPerLevel = 20f;
    [SerializeField] private float atkPerLevel = 3f;
    [SerializeField] private float defPerLevel = 1.5f;
    [SerializeField] private float expMultiplier = 1.5f;

    private void Awake()
    {
        if (levelData != null)
            ApplyLevelData();
        HP = MaxHP;
    }

    private void ApplyLevelData()
    {
        MaxHP = levelData.GetMaxHP(Level);
        ATK = levelData.GetATK(Level);
        DEF = levelData.GetDEF(Level);
        ExpToNextLevel = levelData.GetExpToNextLevel(Level);
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

        if (levelData != null)
        {
            ApplyLevelData();
        }
        else
        {
            MaxHP += hpPerLevel;
            ATK += atkPerLevel;
            DEF += defPerLevel;
            ExpToNextLevel = Mathf.Floor(ExpToNextLevel * expMultiplier);
        }

        HP = MaxHP;
        Debug.Log($"레벨 업! Lv.{Level} | HP:{MaxHP} ATK:{ATK} DEF:{DEF}");
    }

    public void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage - DEF, 1f);
        HP = Mathf.Max(HP - actualDamage, 0f);
    }

    public bool IsDead => HP <= 0f;
}
