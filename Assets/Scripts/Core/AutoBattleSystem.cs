using System;
using UnityEngine;

public class AutoBattleSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject enemyPrefab;

    [Header("스테이지 데이터")]
    [SerializeField] private StageData[] stages;

    [Header("전투 설정 (StageData 없을 때 사용)")]
    [SerializeField] private float playerAttackInterval = 1.0f;
    [SerializeField] private float enemySpawnDelay = 1.0f;

    [Header("스테이지")]
    public int CurrentStage = 1;
    [SerializeField] private int enemiesPerStage = 5;

    private Enemy currentEnemy;
    private float playerAttackTimer;
    private float spawnTimer;
    private int enemiesDefeated;
    private bool isBattling;

    public event Action<int> OnStageChanged;
    public event Action<Enemy> OnEnemySpawned;
    public event Action OnBattleStarted;
    public event Action OnBattlePaused;

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();

        SpawnEnemy();
    }

    private void Update()
    {
        if (player == null || player.IsDead)
        {
            StopBattle();
            return;
        }

        if (currentEnemy == null || currentEnemy.IsDead)
        {
            isBattling = false;
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= enemySpawnDelay)
            {
                spawnTimer = 0f;
                SpawnEnemy();
            }
            return;
        }

        if (!isBattling)
        {
            isBattling = true;
            playerAttackTimer = 0f;
            OnBattleStarted?.Invoke();
        }

        PlayerAutoAttack();
    }

    private void PlayerAutoAttack()
    {
        playerAttackTimer += Time.deltaTime;
        if (playerAttackTimer >= playerAttackInterval)
        {
            playerAttackTimer = 0f;
            currentEnemy.TakeDamage(player.ATK);
        }
    }

    private StageData GetCurrentStageData()
    {
        if (stages == null || stages.Length == 0) return null;
        int index = Mathf.Clamp(CurrentStage - 1, 0, stages.Length - 1);
        return stages[index];
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos = enemySpawnPoint != null
            ? enemySpawnPoint.position
            : transform.position + Vector3.right * 3f;

        GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemy = obj.GetComponent<Enemy>();

        StageData stageData = GetCurrentStageData();
        if (stageData != null)
        {
            EnemyData enemyData = stageData.GetRandomEnemy();
            int enemyLevel = stageData.GetRandomEnemyLevel();
            if (enemyData != null)
                currentEnemy.Initialize(enemyData, enemyLevel);
            else
                currentEnemy.SetLevel(CurrentStage);
        }
        else
        {
            currentEnemy.SetLevel(CurrentStage);
        }

        currentEnemy.SetTarget(player);
        currentEnemy.OnDeath += HandleEnemyDeath;

        OnEnemySpawned?.Invoke(currentEnemy);
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;
        enemiesDefeated++;

        if (enemiesDefeated >= enemiesPerStage)
        {
            AdvanceStage();
        }
    }

    private void AdvanceStage()
    {
        CurrentStage++;
        enemiesDefeated = 0;
        Debug.Log($"스테이지 {CurrentStage} 돌입!");
        OnStageChanged?.Invoke(CurrentStage);
    }

    private void StopBattle()
    {
        if (isBattling)
        {
            isBattling = false;
            OnBattlePaused?.Invoke();
        }
    }
}
