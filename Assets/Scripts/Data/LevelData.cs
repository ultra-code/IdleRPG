using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "IdleRPG/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("초기 스탯")]
    public float BaseHP = 100f;
    public float BaseATK = 10f;
    public float BaseDEF = 5f;
    public float BaseExpToNextLevel = 100f;

    [Header("레벨당 성장치")]
    public float HPPerLevel = 20f;
public float ATKPerLevel = 5.7f;
public float DEFPerLevel = 2.9f;
public float ExpMultiplier = 1.45f;

[Header("HP 2차 성장 계수")]
public float HPQuadratic = 1.2f;

public float GetMaxHP(int level)
{
    float linear = (level - 1) * HPPerLevel;
    float quadratic = (level - 1) * (level - 1) * HPQuadratic;
    return BaseHP + linear + quadratic;
}
    public float GetATK(int level) => BaseATK + (level - 1) * ATKPerLevel;
    public float GetDEF(int level) => BaseDEF + (level - 1) * DEFPerLevel;
    public float GetExpToNextLevel(int level) => Mathf.Floor(BaseExpToNextLevel * Mathf.Pow(ExpMultiplier, level - 1));
}
