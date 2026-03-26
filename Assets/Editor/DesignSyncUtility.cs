using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DesignSyncUtility
{
    private sealed class StageAnchor
    {
        public int Stage;
        public float Hp;
        public float Atk;
        public float Def;
        public float Gold;
        public int Waves;
        public int Min;
        public int Max;

        public StageAnchor(int stage, float hp, float atk, float def, float gold, int waves, int min, int max)
        {
            Stage = stage;
            Hp = hp;
            Atk = atk;
            Def = def;
            Gold = gold;
            Waves = waves;
            Min = min;
            Max = max;
        }
    }

    private static readonly StageAnchor[] anchors =
    {
        new StageAnchor(1, 1.0f, 1.0f, 1.0f, 1.0f, 3, 2, 3),
        new StageAnchor(2, 1.1f, 1.05f, 1.0f, 1.0f, 3, 2, 4),
        new StageAnchor(3, 1.2f, 1.1f, 1.0f, 1.0f, 3, 3, 4),
        new StageAnchor(5, 1.5f, 1.25f, 1.1f, 1.1f, 4, 3, 5),
        new StageAnchor(10, 2.0f, 1.5f, 1.3f, 1.2f, 5, 3, 5),
        new StageAnchor(15, 3.0f, 2.0f, 1.5f, 1.3f, 5, 4, 5),
        new StageAnchor(20, 4.0f, 2.5f, 1.8f, 1.5f, 6, 4, 6),
        new StageAnchor(30, 6.0f, 3.5f, 2.5f, 2.0f, 7, 5, 7),
    };

    [MenuItem("Tools/IdleRPG/Sync Design Data")]
    public static void SyncAll()
    {
        AssetDatabase.Refresh();
        SyncStageAssets();
        SyncSampleSceneReferences();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static string RunFromCli()
    {
        SyncAll();
        return "Design sync complete";
    }

    private static void SyncStageAssets()
    {
        EnemyData slime = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Slime.asset");
        EnemyData stalker = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Bat.asset");
        EnemyData skeleton = Load<EnemyData>("Assets/Data/Enemies/EnemyData_SkeletonWarrior.asset");
        EnemyData hemocyte = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Goblin.asset");
        EnemyData twinhead = Load<EnemyData>("Assets/Data/Enemies/EnemyData_Golem.asset");

        for (int stage = 1; stage <= 30; stage++)
        {
            string path = $"Assets/Data/Stages/StageData_{stage:00}.asset";
            StageData asset = Load<StageData>(path);
            if (asset == null)
                continue;

            StageAnchor values = EvaluateStage(stage);
            asset.WavesPerStage = values.Waves;
            asset.MinEnemies = values.Min;
            asset.MaxEnemies = values.Max;
            asset.HPMult = values.Hp;
            asset.ATKMult = values.Atk;
            asset.DEFMult = values.Def;
            asset.GoldMult = values.Gold;

            if (stage <= 2)
            {
                asset.EnemyPool = new[] { slime };
            }
            else if (stage <= 4)
            {
                asset.EnemyPool = new[] { slime, stalker };
            }
            else if (stage <= 9)
            {
                asset.EnemyPool = new[] { slime, stalker, skeleton };
            }
            else if (stage <= 14)
            {
                asset.EnemyPool = new[] { slime, stalker, skeleton, hemocyte };
            }
            else if (stage <= 19)
            {
                asset.EnemyPool = new[] { slime, stalker, skeleton, hemocyte, twinhead };
            }
            else if (stage <= 29)
            {
                asset.EnemyPool = new[] { stalker, skeleton, hemocyte, twinhead };
            }
            else
            {
                asset.EnemyPool = new[] { skeleton, hemocyte, twinhead };
            }

            asset.IsBossStage = stage == 10 || stage == 20 || stage == 30;
            asset.BossID = stage == 10 ? "BOSS_STOMACH" : stage == 20 ? "BOSS_SPINE" : stage == 30 ? "BOSS_HEART" : "";

            // Keep legacy fields coherent so older fallback paths remain predictable.
            asset.EnemiesPerStage = values.Max;
            asset.MinEnemyLevel = Mathf.Max(1, stage);
            asset.MaxEnemyLevel = Mathf.Max(asset.MinEnemyLevel, stage);

            EditorUtility.SetDirty(asset);
        }
    }

    private static void SyncSampleSceneReferences()
    {
        string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject gameManager = EnsureGameManager();
        GameObject player = EnsurePlayer();
        EnsureSpawnPoints();
        EnsureCanvasAndUi();

        WaveSpawnManager wave = gameManager.GetComponent<WaveSpawnManager>();
        CharacterStats stats = player.GetComponent<CharacterStats>();
        SkillSystem skillSystem = player.GetComponent<SkillSystem>();
        if (wave == null || stats == null || skillSystem == null)
            throw new InvalidOperationException("Required components not found in SampleScene.");

        StageData[] stages = LoadArray<StageData>(
            "Assets/Data/Stages/StageData_01.asset",
            "Assets/Data/Stages/StageData_02.asset",
            "Assets/Data/Stages/StageData_03.asset",
            "Assets/Data/Stages/StageData_04.asset",
            "Assets/Data/Stages/StageData_05.asset",
            "Assets/Data/Stages/StageData_06.asset",
            "Assets/Data/Stages/StageData_07.asset",
            "Assets/Data/Stages/StageData_08.asset",
            "Assets/Data/Stages/StageData_09.asset",
            "Assets/Data/Stages/StageData_10.asset",
            "Assets/Data/Stages/StageData_11.asset",
            "Assets/Data/Stages/StageData_12.asset",
            "Assets/Data/Stages/StageData_13.asset",
            "Assets/Data/Stages/StageData_14.asset",
            "Assets/Data/Stages/StageData_15.asset",
            "Assets/Data/Stages/StageData_16.asset",
            "Assets/Data/Stages/StageData_17.asset",
            "Assets/Data/Stages/StageData_18.asset",
            "Assets/Data/Stages/StageData_19.asset",
            "Assets/Data/Stages/StageData_20.asset",
            "Assets/Data/Stages/StageData_21.asset",
            "Assets/Data/Stages/StageData_22.asset",
            "Assets/Data/Stages/StageData_23.asset",
            "Assets/Data/Stages/StageData_24.asset",
            "Assets/Data/Stages/StageData_25.asset",
            "Assets/Data/Stages/StageData_26.asset",
            "Assets/Data/Stages/StageData_27.asset",
            "Assets/Data/Stages/StageData_28.asset",
            "Assets/Data/Stages/StageData_29.asset",
            "Assets/Data/Stages/StageData_30.asset"
        );
        BossData[] bosses = LoadArray<BossData>(
            "Assets/Data/Bosses/BossData_Stomach.asset",
            "Assets/Data/Bosses/BossData_Spine.asset",
            "Assets/Data/Bosses/BossData_Heart.asset"
        );
        SkillData[] skills = LoadArray<SkillData>(
            "Assets/Skills/Skill_Buff.asset",
            "Assets/Skills/Skill_Circle.asset",
            "Assets/Skills/Skill_Chain.asset",
            "Assets/Skills/Skill_FanShape.asset"
        );
        LevelData levelData = Load<LevelData>("Assets/Data/DefaultLevelData.asset");
        EnhanceCostData enhanceCost = EnsureEnhanceCostData();
        GameObject enemyPrefab = Load<GameObject>("Assets/Prefabs/Enemy.prefab");
        Transform[] spawnPoints = new[]
        {
            GameObject.Find("SpawnPoint_3").transform,
            GameObject.Find("SpawnPoint_4").transform,
            GameObject.Find("SpawnPoint_5").transform,
            GameObject.Find("SpawnPoint_6").transform,
            GameObject.Find("SpawnPoint_7").transform,
        };

        SetSerializedArray(wave, "stages", stages);
        SetSerializedArray(wave, "bossDataList", bosses);
        SetSerializedArray(wave, "spawnPoints", spawnPoints);
        SetSerializedReference(wave, "player", stats);
        SetSerializedReference(wave, "enemyPrefab", enemyPrefab);
        SetSerializedReference(stats, "levelData", levelData);
        SetSerializedArray(skillSystem, "skills", skills);
        SetSerializedReference(skillSystem, "player", stats);
        SetSerializedReference(skillSystem, "waveManager", wave);

        SaveSystem save = gameManager.GetComponent<SaveSystem>();
        OfflineRewardSystem offline = gameManager.GetComponent<OfflineRewardSystem>();
        UpgradeSystem upgrade = gameManager.GetComponent<UpgradeSystem>();
        SetSerializedReference(save, "player", stats);
        SetSerializedReference(save, "battleSystem", wave);
        SetSerializedReference(save, "upgradeSystem", upgrade);
        SetSerializedReference(offline, "player", stats);
        SetSerializedReference(offline, "saveSystem", save);
        SetSerializedReference(upgrade, "enhanceCostData", enhanceCost);

        EditorUtility.SetDirty(wave);
        EditorUtility.SetDirty(stats);
        EditorUtility.SetDirty(skillSystem);
        EditorUtility.SetDirty(save);
        EditorUtility.SetDirty(offline);
        EditorUtility.SetDirty(upgrade);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject EnsureGameManager()
    {
        GameObject obj = GameObject.Find("GameManager");
        if (obj != null)
            return obj;

        obj = new GameObject("GameManager");
        obj.AddComponent<WaveSpawnManager>();
        obj.AddComponent<SaveSystem>();
        obj.AddComponent<OfflineRewardSystem>();
        obj.AddComponent<UpgradeSystem>();
        return obj;
    }

    private static GameObject EnsurePlayer()
    {
        GameObject obj = GameObject.Find("Player");
        if (obj != null)
            return obj;

        obj = new GameObject("Player");
        obj.transform.position = new Vector3(-2f, 0f, 0f);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<CharacterStats>();
        obj.AddComponent<SkillSystem>();
        obj.AddComponent<AudioSource>();
        return obj;
    }

    private static void EnsureSpawnPoints()
    {
        EnsureSpawnPoint("SpawnPoint_3", new Vector3(3f, 1.5f, 0f));
        EnsureSpawnPoint("SpawnPoint_4", new Vector3(3.5f, 0f, 0f));
        EnsureSpawnPoint("SpawnPoint_5", new Vector3(3f, -1.5f, 0f));
        EnsureSpawnPoint("SpawnPoint_6", new Vector3(4.5f, 1f, 0f));
        EnsureSpawnPoint("SpawnPoint_7", new Vector3(4.5f, -1f, 0f));
    }

    private static void EnsureSpawnPoint(string name, Vector3 position)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
            obj = new GameObject(name);
        obj.transform.position = position;
    }

    private static void EnsureCanvasAndUi()
    {
        if (GameObject.Find("Canvas") == null)
            CanvasSetupGenerator.CreateCanvasAndHUD();

        if (GameObject.Find("ShopButton") == null || GameObject.Find("ShopPanel") == null)
            ShopSetupGenerator.RebuildShopUI();

        if (GameObject.Find("PlayerHUD") != null)
            HUDConnector.ConnectHUD();
    }

    private static StageAnchor EvaluateStage(int stage)
    {
        for (int i = 0; i < anchors.Length - 1; i++)
        {
            StageAnchor a = anchors[i];
            StageAnchor b = anchors[i + 1];
            if (stage < a.Stage || stage > b.Stage)
                continue;

            if (stage == a.Stage)
                return new StageAnchor(stage, a.Hp, a.Atk, a.Def, a.Gold, a.Waves, a.Min, a.Max);
            if (stage == b.Stage)
                return new StageAnchor(stage, b.Hp, b.Atk, b.Def, b.Gold, b.Waves, b.Min, b.Max);

            float t = (stage - a.Stage) / (float)(b.Stage - a.Stage);
            return new StageAnchor(
                stage,
                Round1(Mathf.Lerp(a.Hp, b.Hp, t)),
                Round1(Mathf.Lerp(a.Atk, b.Atk, t)),
                Round1(Mathf.Lerp(a.Def, b.Def, t)),
                Round1(Mathf.Lerp(a.Gold, b.Gold, t)),
                Mathf.RoundToInt(Mathf.Lerp(a.Waves, b.Waves, t)),
                Mathf.RoundToInt(Mathf.Lerp(a.Min, b.Min, t)),
                Mathf.RoundToInt(Mathf.Lerp(a.Max, b.Max, t))
            );
        }

        StageAnchor last = anchors[anchors.Length - 1];
        return new StageAnchor(stage, last.Hp, last.Atk, last.Def, last.Gold, last.Waves, last.Min, last.Max);
    }

    private static float Round1(float value)
    {
        return Mathf.Round(value * 10f) / 10f;
    }

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
            throw new InvalidOperationException($"Asset not found: {path}");
        return asset;
    }

    private static T[] LoadArray<T>(params string[] paths) where T : UnityEngine.Object
    {
        var result = new List<T>(paths.Length);
        foreach (string path in paths)
            result.Add(Load<T>(path));
        return result.ToArray();
    }

    private static void SetSerializedReference(UnityEngine.Object target, string propertyName, UnityEngine.Object reference)
    {
        var so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
            throw new InvalidOperationException($"Property not found: {propertyName}");
        prop.objectReferenceValue = reference;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedArray<T>(UnityEngine.Object target, string propertyName, T[] values) where T : UnityEngine.Object
    {
        var so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
            throw new InvalidOperationException($"Property not found: {propertyName}");
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static EnhanceCostData EnsureEnhanceCostData()
    {
        const string path = "Assets/Data/EnhanceCostData.asset";
        EnhanceCostData asset = AssetDatabase.LoadAssetAtPath<EnhanceCostData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<EnhanceCostData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.Rows = new EnhanceCostData.EnhanceCostRow[26];
        for (int i = 0; i <= 25; i++)
        {
            asset.Rows[i] = new EnhanceCostData.EnhanceCostRow
            {
                EnhanceLevel = i,
                CostGold = Mathf.FloorToInt(50f * Mathf.Pow(1.3f, i)),
                HPBonus = 10f,
                ATKBonus = 3f,
                DEFBonus = 1.5f
            };
        }

        EditorUtility.SetDirty(asset);
        return asset;
    }
}
