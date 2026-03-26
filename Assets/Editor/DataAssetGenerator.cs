#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DataAssetGenerator
{
    // ============================================================
    // Step 1: EnemyData 5종
    // ============================================================
    [MenuItem("IdleRPG/Generate/Step1 - EnemyData (5종)")]
    public static void GenerateEnemyData()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Enemies");

        CreateEnemy("EnemyData_Slime",           "슬라임",     1.0f, 1.0f, 1, 1.5f, 10f, 5f);
        CreateEnemy("EnemyData_Goblin",           "고블린",     0.8f, 1.3f, 2, 1.8f, 12f, 6f);
        CreateEnemy("EnemyData_Golem",            "골렘",       2.5f, 0.7f, 3, 1.3f, 15f, 8f);
        CreateEnemy("EnemyData_Bat",              "박쥐",       0.6f, 0.9f, 1, 2.0f, 10f, 5f);
        CreateEnemy("EnemyData_SkeletonWarrior",  "해골전사",   1.8f, 1.5f, 3, 1.5f, 25f, 15f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Step 1 완료: EnemyData 5종 생성");
    }

    private static void CreateEnemy(string fileName, string enemyName,
        float hpMult, float atkMult, int baseDef, float atkInterval,
        float expMult, float goldMult)
    {
        string path = $"Assets/Data/Enemies/{fileName}.asset";
        if (AssetDatabase.LoadAssetAtPath<EnemyData>(path) != null)
        {
            Debug.Log($"이미 존재: {path}");
            return;
        }

        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.EnemyName = enemyName;
        data.BaseHP = 30f * hpMult;
        data.HPPerLevel = 15f * hpMult;
        data.BaseATK = 5f * atkMult;
        data.ATKPerLevel = 2f * atkMult;
        data.BaseDEF = baseDef;
        data.DEFPerLevel = 0.5f;
        data.AttackInterval = atkInterval;
        data.BaseExpReward = expMult;
        data.BaseGoldReward = goldMult;
        data.ExpRewardPerLevel = expMult * 0.5f;
        data.GoldRewardPerLevel = goldMult * 0.3f;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"생성: {path}");
    }

    // ============================================================
    // Step 2: LevelData 1개
    // ============================================================
    [MenuItem("IdleRPG/Generate/Step2 - LevelData")]
    public static void GenerateLevelData()
    {
        EnsureFolder("Assets/Data");

        string path = "Assets/Data/DefaultLevelData.asset";
        if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
        {
            Debug.Log($"이미 존재: {path}");
            return;
        }

        var data = ScriptableObject.CreateInstance<LevelData>();
        data.BaseHP = 100f;
        data.HPPerLevel = 20f;
        data.BaseATK = 10f;
        data.ATKPerLevel = 5.7f;
        data.BaseDEF = 5f;
        data.DEFPerLevel = 1.5f;
        data.BaseExpToNextLevel = 100f;
        data.ExpMultiplier = 1.45f;

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Step 2 완료: LevelData 생성");
    }

    // ============================================================
    // Step 3: StageData 1~10
    // ============================================================
    [MenuItem("IdleRPG/Generate/Step3 - StageData 01-10")]
    public static void GenerateStage01_10()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Stages");

        var slime  = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Slime.asset");
        var goblin = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Goblin.asset");
        var bat    = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Bat.asset");

        if (slime == null || goblin == null || bat == null)
        {
            Debug.LogError("EnemyData를 먼저 생성하세요 (Step 1)");
            return;
        }

        var sb = new[] { slime, bat };
        var sbg = new[] { slime, bat, goblin };
        var bg = new[] { bat, goblin };
        var g = new[] { goblin };

        CreateStage(1,  "초원 1",   sb,  1, 1, 4);
        CreateStage(2,  "초원 2",   sb,  1, 2, 4);
        CreateStage(3,  "초원 3",   sb,  2, 2, 5);
        CreateStage(4,  "초원 4",   sb,  2, 3, 5);
        CreateStage(5,  "초원 5",   sb,  3, 3, 5);
        CreateStage(6,  "숲 1",     sbg, 4, 4, 5);
        CreateStage(7,  "숲 2",     sbg, 4, 5, 6);
        CreateStage(8,  "숲 3",     bg,  5, 6, 6);   // 난이도 점프
        CreateStage(9,  "숲 4",     bg,  5, 6, 6);
        CreateStage(10, "숲 5",     g,   6, 6, 6);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Step 3 완료: StageData 1~10 생성");
    }

    // ============================================================
    // Step 4: StageData 11~20
    // ============================================================
    [MenuItem("IdleRPG/Generate/Step4 - StageData 11-20")]
    public static void GenerateStage11_20()
    {
        var goblin = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Goblin.asset");
        var golem  = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Golem.asset");
        var skel   = Load<EnemyData>("Assets/Data/Enemies/EnemyData_SkeletonWarrior.asset");

        if (goblin == null || golem == null || skel == null)
        {
            Debug.LogError("EnemyData를 먼저 생성하세요 (Step 1)");
            return;
        }

        var gg = new[] { goblin, golem };
        var gs = new[] { golem, skel };
        var ggs = new[] { goblin, golem, skel };
        var s = new[] { skel };

        CreateStage(11, "동굴 1",   gg,  7,  7,  6);
        CreateStage(12, "동굴 2",   gg,  7,  8,  6);
        CreateStage(13, "동굴 3",   gg,  8,  9,  7);
        CreateStage(14, "동굴 4",   gg,  9,  10, 7);
        CreateStage(15, "묘지 1",   gs,  10, 10, 7);   // 난이도 점프
        CreateStage(16, "묘지 2",   gs,  11, 11, 6);
        CreateStage(17, "묘지 3",   ggs, 11, 12, 7);
        CreateStage(18, "묘지 4",   ggs, 12, 13, 7);
        CreateStage(19, "묘지 5",   gs,  13, 14, 8);
        CreateStage(20, "폐허 입구", s,  14, 14, 8);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Step 4 완료: StageData 11~20 생성");
    }

    // ============================================================
    // Step 5: StageData 21~30
    // ============================================================
    [MenuItem("IdleRPG/Generate/Step5 - StageData 21-30")]
    public static void GenerateStage21_30()
    {
        var slime  = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Slime.asset");
        var goblin = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Goblin.asset");
        var golem  = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Golem.asset");
        var bat    = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Bat.asset");
        var skel   = Load<EnemyData>("Assets/Data/Enemies/EnemyData_SkeletonWarrior.asset");

        if (slime == null || goblin == null || golem == null || bat == null || skel == null)
        {
            Debug.LogError("EnemyData를 먼저 생성하세요 (Step 1)");
            return;
        }

        var all = new[] { slime, goblin, golem, bat, skel };
        var gs = new[] { golem, skel };
        var ggs = new[] { goblin, golem, skel };
        var s = new[] { skel };

        CreateStage(21, "폐허 1",     all, 15, 15, 7);
        CreateStage(22, "폐허 2",     all, 15, 16, 7);
        CreateStage(23, "폐허 3",     gs,  17, 18, 8);   // 난이도 점프
        CreateStage(24, "폐허 4",     all, 17, 18, 8);
        CreateStage(25, "폐허 5",     all, 18, 18, 8);
        CreateStage(26, "마탑 1",     ggs, 19, 19, 8);
        CreateStage(27, "마탑 2",     gs,  19, 20, 8);
        CreateStage(28, "마탑 3",     gs,  20, 21, 8);
        CreateStage(29, "마탑 4",     s,   21, 22, 8);
        CreateStage(30, "마탑 최상층", s,  22, 22, 8);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Step 5 완료: StageData 21~30 생성");
    }

    // ============================================================
    // 전체 실행
    // ============================================================
    [MenuItem("IdleRPG/Generate/ALL - 전체 생성 (Step 1~5)")]
    public static void GenerateAll()
    {
        GenerateEnemyData();
        GenerateLevelData();
        GenerateStage01_10();
        GenerateStage11_20();
        GenerateStage21_30();
        Debug.Log("전체 생성 완료! EnemyData 5 + LevelData 1 + StageData 30");
    }

    // ============================================================
    // 검증
    // ============================================================
    [MenuItem("IdleRPG/Generate/Verify - 에셋 개수 확인")]
    public static void VerifyAssets()
    {
        int enemies = AssetDatabase.FindAssets("t:EnemyData").Length;
        int levels = AssetDatabase.FindAssets("t:LevelData").Length;
        int stages = AssetDatabase.FindAssets("t:StageData").Length;

        Debug.Log($"검증 결과: EnemyData={enemies}/5, LevelData={levels}/1, StageData={stages}/30");

        if (enemies == 5 && levels == 1 && stages == 30)
            Debug.Log("✓ Phase A 완료!");
        else
            Debug.LogWarning("✗ 누락된 에셋이 있습니다.");
    }

    // ============================================================
    // 유틸리티
    // ============================================================
    private static void CreateStage(int num, string stageName,
        EnemyData[] pool, int minLv, int maxLv, int enemiesPerStage)
    {
        string path = $"Assets/Data/Stages/StageData_{num:D2}.asset";
        if (AssetDatabase.LoadAssetAtPath<StageData>(path) != null)
        {
            Debug.Log($"이미 존재: {path}");
            return;
        }

        var data = ScriptableObject.CreateInstance<StageData>();
        data.StageName = stageName;
        data.StageNumber = num;
        data.EnemyPool = pool;
        data.EnemiesPerStage = enemiesPerStage;
        data.MinEnemyLevel = minLv;
        data.MaxEnemyLevel = maxLv;
        data.PlayerAttackInterval = 1.0f;
        data.EnemySpawnDelay = 0.8f;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"생성: {path}");
    }

    private static T Load<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
