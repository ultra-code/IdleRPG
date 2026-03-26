using UnityEngine;

[CreateAssetMenu(fileName = "NewBossData", menuName = "IdleRPG/Boss Data")]
public class BossData : ScriptableObject
{
    public string BossID = "BOSS_STOMACH";
    public string DisplayName = "Stomach Beast";
    public EnemyData BaseEnemy;

    [Header("보스 배율")]
    public float BossHPMult = 5.0f;
    public float BossATKMult = 2.0f;
    public float RewardMult = 5.0f;

    [Header("고정 골드 보너스 (기획서 7장 개선안)")]
    public int FixedGoldBonus = 500;
}
