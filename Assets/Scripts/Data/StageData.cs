using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "IdleRPG/Stage Data")]
public class StageData : ScriptableObject
{
    public string StageName = "초원";
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
