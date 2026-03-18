using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "IdleRPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string EnemyName = "슬라임";
    public Sprite Sprite;

    [Header("기본 스탯 (Lv.1 기준)")]
    public float BaseHP = 50f;
    public float BaseATK = 8f;
    public float BaseDEF = 2f;
    public float AttackInterval = 1.5f;

    [Header("레벨당 성장치")]
    public float HPPerLevel = 15f;
    public float ATKPerLevel = 2f;
    public float DEFPerLevel = 1f;

    [Header("보상 (Lv.1 기준)")]
    public float BaseExpReward = 30f;
    public float BaseGoldReward = 10f;
    public float ExpRewardPerLevel = 10f;
    public float GoldRewardPerLevel = 5f;

    public float GetHP(int level) => BaseHP + (level - 1) * HPPerLevel;
    public float GetATK(int level) => BaseATK + (level - 1) * ATKPerLevel;
    public float GetDEF(int level) => BaseDEF + (level - 1) * DEFPerLevel;
    public float GetExpReward(int level) => BaseExpReward + (level - 1) * ExpRewardPerLevel;
    public float GetGoldReward(int level) => BaseGoldReward + (level - 1) * GoldRewardPerLevel;
}
