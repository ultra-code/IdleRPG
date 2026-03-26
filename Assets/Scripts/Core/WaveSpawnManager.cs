using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WaveSpawnManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;

    [Header("스테이지 데이터")]
    [SerializeField] private StageData[] stages;

    [Header("웨이브 설정")]
    [SerializeField] private int enemiesPerWave = 15; // was 6
    [SerializeField] private int wavesPerStage = 3;
    [SerializeField] private float waveStartDelay = 1.0f; // was 1.5f

    [Header("전투 설정 (StageData 없을 때 사용)")]
    [SerializeField] private float playerAttackInterval = 1.0f;

    [Header("범위 공격")]
    [SerializeField] private float attackRadius = 2.0f;
    [SerializeField] private float attackAngle = 90f;

    [Header("보스")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float bossHPMultiplier = 5f;
    [SerializeField] private float bossATKMultiplier = 2f;
    [SerializeField] private float bossRewardMultiplier = 5f;

    [Header("보스 데이터 (기획서 8장)")]
    [SerializeField] private BossData[] bossDataList;

    [Header("스테이지")]
    public int CurrentStage = 1;

    [Header("스폰 오프셋")]
    [SerializeField] private float spawnOffsetRange = 0.5f;

    private readonly List<Enemy> activeEnemies = new List<Enemy>();
    // --- Spawn debug counters ---
    private readonly Dictionary<string, int> spawnCounts = new Dictionary<string, int>();
    private SkillSystem skillSystemCache;
    private ActorAnimationEvents playerAnimationEvents;
    private float playerAttackTimer;
    private float waveDelayTimer;
    private int currentWave;
    private bool isBattling;
    private bool isWaveDelaying;
    private bool isBossStage;
    private bool isPlayerAttackResolving;

    // 기존 이벤트
    public event Action<int> OnStageChanged;
    public event Action<Enemy> OnEnemySpawned;
    public event Action OnBattleStarted;
    public event Action OnBattlePaused;

    // 신규 이벤트
    public event Action<int> OnWaveStarted;
    public event Action OnWaveCleared;
    public event Action<List<Enemy>> OnPlayerAttack;
    public event Action<Enemy> OnBossSpawned;
    public event Action<Enemy> OnBossDefeated;

    public int CurrentWave => currentWave;
    public int WavesPerStage => GetWavesForCurrentStage();
    public List<Enemy> ActiveEnemies => activeEnemies;
    public bool IsBossStage => CurrentStage > 0 && CurrentStage % 10 == 0;
    public bool IsBossWave => IsBossStage && currentWave >= WavesPerStage;

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();

        if (player != null)
            playerAnimationEvents = player.GetComponent<ActorAnimationEvents>();

        BossUIController.EnsureExists(this);
        StartWave();
    }

    private void Update()
    {
        if (player == null || player.IsDead)
        {
            StopBattle();
            return;
        }

        CleanupDeadEnemies();

        if (activeEnemies.Count == 0)
        {
            if (isBattling)
            {
                isBattling = false;
                OnWaveCleared?.Invoke();
                // --- Spawn count log ---
                string countLog = $"Wave {currentWave} clear — Spawn count:";
                foreach (var kv in spawnCounts) countLog += $" {kv.Key}:{kv.Value}";
                Debug.Log(countLog);
                spawnCounts.Clear();

                if (currentWave >= WavesPerStage)
                {
                    AdvanceStage();
                }
                else
                {
                    isWaveDelaying = true;
                    waveDelayTimer = 0f;
                }
            }

            if (isWaveDelaying)
            {
                waveDelayTimer += Time.deltaTime;
                if (waveDelayTimer >= waveStartDelay)
                {
                    isWaveDelaying = false;
                    StartWave();
                }
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

        float interval = playerAttackInterval;
        StageData stageData = GetCurrentStageData();
        if (stageData != null)
            interval = stageData.PlayerAttackInterval;

        // 버프 활성 시 공격속도 x2
        SkillSystem skillSystem = skillSystemCache;
        if (skillSystem == null)
        {
            skillSystem = FindAnyObjectByType<SkillSystem>();
            skillSystemCache = skillSystem;
        }
        bool buffActive = skillSystem != null && skillSystem.IsBuffActive;
        if (buffActive)
            interval *= 0.5f;

        if (playerAttackTimer < interval) return;
        if (isPlayerAttackResolving) return;
        playerAttackTimer = 0f;

        Vector2 playerPos = player.transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(playerPos, attackRadius);
        if (hits.Length == 0) return;

        List<Enemy> hitEnemies = new List<Enemy>();

        if (buffActive)
        {
            // 버프 중: 원형 전체 공격
            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null || enemy.IsDead) continue;

                hitEnemies.Add(enemy);
            }
        }
        else
        {
            // 기본: 부채꼴 공격
            Vector2 facingDir = player.transform.right;
            float halfAngle = attackAngle * 0.5f;

            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null || enemy.IsDead) continue;

                Vector2 dirToEnemy = ((Vector2)enemy.transform.position - playerPos).normalized;
                float angle = Vector2.Angle(facingDir, dirToEnemy);

                if (angle <= halfAngle)
                    hitEnemies.Add(enemy);
            }
        }

        if (hitEnemies.Count > 0)
        {
            playerAnimationEvents?.PlayAttack();
            StartCoroutine(ResolvePlayerAttackAfterDelay(hitEnemies));
        }
    }

    private void StartWave()
    {
        currentWave++;
        isBossStage = IsBossWave;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWaveStart();

        if (isBossStage)
        {
            SpawnBoss();
        }
        else
        {
            int totalEnemies = enemiesPerWave;
            StageData stageData = GetCurrentStageData();
            if (stageData != null)
                totalEnemies = UnityEngine.Random.Range(stageData.MinEnemies, stageData.MaxEnemies + 1);

            if (spawnPoints != null && spawnPoints.Length >= 2)
            {
                int spawnGroupCount = Mathf.Min(UnityEngine.Random.Range(2, 4), spawnPoints.Length);

                // Shuffle spawn points and pick first spawnGroupCount
                List<Transform> selectedPoints = new List<Transform>(spawnPoints);
                for (int i = selectedPoints.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    var temp = selectedPoints[i];
                    selectedPoints[i] = selectedPoints[j];
                    selectedPoints[j] = temp;
                }

                for (int i = 0; i < totalEnemies; i++)
                {
                    Transform point = selectedPoints[i % spawnGroupCount];
                    SpawnEnemyAtPoint(point);
                }
            }
            else
            {
                for (int i = 0; i < totalEnemies; i++)
                    SpawnEnemyAtPoint(null);
            }
        }

        OnWaveStarted?.Invoke(currentWave);
        Debug.Log($"스테이지 {CurrentStage} - 웨이브 {currentWave}/{WavesPerStage} 시작! 적 {activeEnemies.Count}마리{(isBossStage ? " [BOSS]" : string.Empty)}");
    }

    private int GetWavesForCurrentStage()
    {
        StageData stageData = GetCurrentStageData();
        if (stageData != null && stageData.WavesPerStage > 0)
            return stageData.WavesPerStage;

        return wavesPerStage;
    }

    private void SpawnEnemyAtPoint(Transform point)
    {
        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy == null) return;
        Debug.Log($"[SPAWN] {obj.name} at {spawnPos}");

        StageData stageData = GetCurrentStageData();
        if (stageData != null)
        {
            EnemyData enemyData = stageData.GetRandomEnemy();
            if (enemyData != null)
            {
                enemy.Initialize(enemyData, stageData);
                // --- Track spawn count ---
                string key = enemyData.EnemyName;
                if (spawnCounts.ContainsKey(key)) spawnCounts[key]++;
                else spawnCounts[key] = 1;
            }
            else
                enemy.SetLevel(CurrentStage);
        }
        else
        {
            enemy.SetLevel(CurrentStage);
        }

        RegisterEnemy(enemy);
    }

    private void SpawnBoss()
    {
        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject prefab = bossPrefab != null ? bossPrefab : enemyPrefab;
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        Enemy boss = obj.GetComponent<Enemy>();
        if (boss == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossAppear();
            AudioManager.Instance.PlayBossBGM();
        }

        Debug.Log($"[SPAWN] BOSS {obj.name} at {spawnPos}");

        StageData stageData = GetCurrentStageData();
        BossData bossData = GetBossDataForStage(CurrentStage);

        if (bossData != null && stageData != null)
        {
            boss.InitializeAsBoss(bossData, stageData);
        }
        else
        {
            boss.SetLevel(CurrentStage);
            if (stageData != null)
            {
                EnemyData enemyData = stageData.GetRandomEnemy();
                if (enemyData != null)
                    boss.Initialize(enemyData, stageData);
            }

            if (boss.IsBoss)
            {
                // no-op
            }
            else
            {
                boss.ConfigureAsBoss(bossHPMultiplier, bossATKMultiplier, bossRewardMultiplier);
            }
        }

        RegisterEnemy(boss);
        OnBossSpawned?.Invoke(boss);
        Debug.Log($"보스 등장! {boss.EnemyName} HP:{boss.MaxHP}");
    }

    private BossData GetBossDataForStage(int stage)
    {
        if (bossDataList == null || bossDataList.Length == 0)
            return null;

        StageData stageData = GetCurrentStageData();
        foreach (BossData bd in bossDataList)
        {
            if (bd == null)
                continue;

            if (stageData != null && !string.IsNullOrEmpty(stageData.BossID) && bd.BossID == stageData.BossID)
                return bd;
        }

        int bossIndex = (stage / 10) - 1;
        if (bossIndex >= 0 && bossIndex < bossDataList.Length)
            return bossDataList[bossIndex];

        return null;
    }

    private void RegisterEnemy(Enemy enemy)
    {
        enemy.SetTarget(player);
        enemy.OnDeath += HandleEnemyDeath;
        activeEnemies.Add(enemy);
        OnEnemySpawned?.Invoke(enemy);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return MapBounds.RandomSpawnEdge();
    }

    private IEnumerator ResolvePlayerAttackAfterDelay(List<Enemy> hitEnemies)
    {
        isPlayerAttackResolving = true;
        float impactDelay = playerAnimationEvents != null ? playerAnimationEvents.GetAttackImpactDelay() : 0.08f;
        yield return new WaitForSeconds(impactDelay);

        List<Enemy> resolvedHits = new List<Enemy>();
        foreach (Enemy enemy in hitEnemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            enemy.TakeDamage(player.ATK);
            resolvedHits.Add(enemy);
        }

        if (resolvedHits.Count > 0)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPlayerAttack();
            OnPlayerAttack?.Invoke(resolvedHits);
        }

        isPlayerAttackResolving = false;
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        if (enemy == null) return;
        enemy.OnDeath -= HandleEnemyDeath;
        activeEnemies.Remove(enemy);

        if (enemy.IsBoss)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossDefeat();
                AudioManager.Instance.PlayBattleBGM();
            }

            OnBossDefeated?.Invoke(enemy);
        }
    }

    private void CleanupDeadEnemies()
    {
        activeEnemies.RemoveAll(e => e == null || e.IsDead);
    }

    private StageData GetCurrentStageData()
    {
        if (stages == null || stages.Length == 0) return null;
        int index = Mathf.Clamp(CurrentStage - 1, 0, stages.Length - 1);
        return stages[index];
    }

    private void AdvanceStage()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayStageClear();

        CurrentStage++;
        currentWave = 0;
        Debug.Log($"스테이지 {CurrentStage} 돌입!");
        OnStageChanged?.Invoke(CurrentStage);

        isWaveDelaying = true;
        waveDelayTimer = 0f;
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
