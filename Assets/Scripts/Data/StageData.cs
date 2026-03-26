using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "IdleRPG/Stage Data")]
public class StageData : ScriptableObject
{
    public string StageName = "Plains";
    public int StageNumber = 1;

    [Header("적 구성")]
    public EnemyData[] EnemyPool;
    public int EnemiesPerStage = 5;

    [Header("적 레벨 범위")]
    public int MinEnemyLevel = 1;
    public int MaxEnemyLevel = 1;

    [Header("전투 설정")]
    public float PlayerAttackInterval = 1.0f;
    public float EnemySpawnDelay = 1.0f;

    [Header("스탯 배율 (기획서 8장)")]
    public float HPMult = 1.0f;
    public float ATKMult = 1.0f;
    public float DEFMult = 1.0f;
    public float GoldMult = 1.0f;

    [Header("웨이브 설정")]
    public int WavesPerStage = 3;
    public int MinEnemies = 2;
    public int MaxEnemies = 3;

    [Header("보스")]
    public bool IsBossStage = false;
    public string BossID = "";

    public EnemyData GetRandomEnemy()
    {
        if (EnemyPool == null || EnemyPool.Length == 0) return null;
        return EnemyPool[Random.Range(0, EnemyPool.Length)];
    }

    public int GetRandomEnemyLevel()
    {
        return Random.Range(MinEnemyLevel, MaxEnemyLevel + 1);
    }
}
