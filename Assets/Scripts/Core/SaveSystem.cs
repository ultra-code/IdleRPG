using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;
    [SerializeField] private WaveSpawnManager battleSystem;
    [SerializeField] private UpgradeSystem upgradeSystem;

    [Header("자동 저장")]
    [SerializeField] private float autoSaveInterval = 30f;

    private float autoSaveTimer;

    private const string KEY_LEVEL = "Save_Level";
    private const string KEY_HP = "Save_HP";
    private const string KEY_MAX_HP = "Save_MaxHP";
    private const string KEY_ATK = "Save_ATK";
    private const string KEY_DEF = "Save_DEF";
    private const string KEY_EXP = "Save_Exp";
    private const string KEY_EXP_NEXT = "Save_ExpToNextLevel";
    private const string KEY_GOLD = "Save_Gold";
    private const string KEY_STAGE = "Save_CurrentStage";
    private const string KEY_UPG_HP = "Save_UpgradeHP";
    private const string KEY_UPG_ATK = "Save_UpgradeATK";
    private const string KEY_UPG_DEF = "Save_UpgradeDEF";

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();
        if (battleSystem == null)
            battleSystem = FindAnyObjectByType<WaveSpawnManager>();
        if (upgradeSystem == null)
            upgradeSystem = FindAnyObjectByType<UpgradeSystem>();

        LoadGame();
    }

    private void Update()
    {
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveGame();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        if (player == null) return;

        PlayerPrefs.SetInt(KEY_LEVEL, player.Level);
        PlayerPrefs.SetFloat(KEY_HP, player.HP);
        PlayerPrefs.SetFloat(KEY_MAX_HP, player.MaxHP);
        PlayerPrefs.SetFloat(KEY_ATK, player.ATK);
        PlayerPrefs.SetFloat(KEY_DEF, player.DEF);
        PlayerPrefs.SetFloat(KEY_EXP, player.Exp);
        PlayerPrefs.SetFloat(KEY_EXP_NEXT, player.ExpToNextLevel);
        PlayerPrefs.SetFloat(KEY_GOLD, player.Gold);

        if (battleSystem != null)
            PlayerPrefs.SetInt(KEY_STAGE, battleSystem.CurrentStage);

        if (upgradeSystem != null)
        {
            PlayerPrefs.SetInt(KEY_UPG_HP, upgradeSystem.hpLevel);
            PlayerPrefs.SetInt(KEY_UPG_ATK, upgradeSystem.atkLevel);
            PlayerPrefs.SetInt(KEY_UPG_DEF, upgradeSystem.defLevel);
        }

        PlayerPrefs.Save();
        Debug.Log($"게임 저장 완료 | Lv.{player.Level} Stage {battleSystem?.CurrentStage}");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(KEY_LEVEL))
        {
            Debug.Log("저장 데이터 없음 — 새 게임 시작");
            return;
        }

        if (player != null)
        {
            player.Level = PlayerPrefs.GetInt(KEY_LEVEL, 1);
            player.MaxHP = PlayerPrefs.GetFloat(KEY_MAX_HP, 100f);
            player.HP = PlayerPrefs.GetFloat(KEY_HP, player.MaxHP);
            if (player.HP <= 0f)
                player.HP = player.MaxHP;
            player.ATK = PlayerPrefs.GetFloat(KEY_ATK, 10f);
            player.DEF = PlayerPrefs.GetFloat(KEY_DEF, 5f);
            player.Exp = PlayerPrefs.GetFloat(KEY_EXP, 0f);
            player.ExpToNextLevel = PlayerPrefs.GetFloat(KEY_EXP_NEXT, 100f);
            player.Gold = PlayerPrefs.GetFloat(KEY_GOLD, 0f);
        }

        if (battleSystem != null)
            battleSystem.CurrentStage = PlayerPrefs.GetInt(KEY_STAGE, 1);

        if (upgradeSystem != null)
        {
            upgradeSystem.hpLevel = PlayerPrefs.GetInt(KEY_UPG_HP, 0);
            upgradeSystem.atkLevel = PlayerPrefs.GetInt(KEY_UPG_ATK, 0);
            upgradeSystem.defLevel = PlayerPrefs.GetInt(KEY_UPG_DEF, 0);
            upgradeSystem.ApplyBonuses(player);
        }

        Debug.Log($"게임 로드 완료 | Lv.{player?.Level} Stage {battleSystem?.CurrentStage}");
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(KEY_LEVEL);
        PlayerPrefs.DeleteKey(KEY_HP);
        PlayerPrefs.DeleteKey(KEY_MAX_HP);
        PlayerPrefs.DeleteKey(KEY_ATK);
        PlayerPrefs.DeleteKey(KEY_DEF);
        PlayerPrefs.DeleteKey(KEY_EXP);
        PlayerPrefs.DeleteKey(KEY_EXP_NEXT);
        PlayerPrefs.DeleteKey(KEY_GOLD);
        PlayerPrefs.DeleteKey(KEY_STAGE);
        PlayerPrefs.DeleteKey("Save_QuitTime");
        PlayerPrefs.DeleteKey(KEY_UPG_HP);
        PlayerPrefs.DeleteKey(KEY_UPG_ATK);
        PlayerPrefs.DeleteKey(KEY_UPG_DEF);
        PlayerPrefs.Save();
        Debug.Log("저장 데이터 삭제 완료");
    }
}
