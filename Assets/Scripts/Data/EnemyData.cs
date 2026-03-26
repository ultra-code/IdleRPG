using UnityEngine;

public enum RoleType
{
    Basic,
    Rusher,
    Tanker,
    Swarm,
    Elite
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "IdleRPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string EnemyName = "Slime";
    public Sprite Sprite;
    public RuntimeAnimatorController AnimatorController;
    
    [Header("역할군 (기획서 8장)")]
    public RoleType roleType = RoleType.Basic;

    [Header("기본 스탯 (Lv.1 기준)")]
    public float BaseHP = 50f;
    public float BaseATK = 8f;
    public float BaseDEF = 2f;

    [Header("비주얼")]
    public float SpriteScale = 1.0f;

    [Header("이동/공격 범위")]
    public float MoveSpeed = 2f;
    public float AttackRange = 1.5f;
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

    public float GetHP(float hpMult) => BaseHP * hpMult;
    public float GetATK(float atkMult) => BaseATK * atkMult;
    public float GetDEF(float defMult) => BaseDEF * defMult;
    public float GetGoldReward(float goldMult) => BaseGoldReward * goldMult;
}
