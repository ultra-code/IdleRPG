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
    public float ATKPerLevel = 3f;
    public float DEFPerLevel = 1.5f;
    public float ExpMultiplier = 1.5f;

    public float GetMaxHP(int level) => BaseHP + (level - 1) * HPPerLevel;
    public float GetATK(int level) => BaseATK + (level - 1) * ATKPerLevel;
    public float GetDEF(int level) => BaseDEF + (level - 1) * DEFPerLevel;
    public float GetExpToNextLevel(int level) => Mathf.Floor(BaseExpToNextLevel * Mathf.Pow(ExpMultiplier, level - 1));
}
