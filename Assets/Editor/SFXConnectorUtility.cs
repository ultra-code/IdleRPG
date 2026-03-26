#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

/// <summary>
/// SFX 파일을 AudioManager Inspector 슬롯에 자동 연결하고,
/// 볼륨 밸런스를 조정하는 에디터 유틸리티.
///
/// 메뉴: Tools/IdleRPG/Connect SFX to AudioManager
/// </summary>
public static class SFXConnectorUtility
{
    [MenuItem("Tools/IdleRPG/Connect SFX to AudioManager")]
    public static void ConnectSFX()
    {
        // ── 1. AudioManager 찾기 ──────────────────────────
        var gm = GameObject.Find("GameManager");
        if (gm == null)
        {
            Debug.LogError("[SFXConnector] GameManager not found in scene!");
            return;
        }

        var am = gm.GetComponent<AudioManager>();
        if (am == null)
        {
            Debug.LogError("[SFXConnector] AudioManager component not found on GameManager!");
            return;
        }

        var so = new SerializedObject(am);
        int linked = 0;

        // ── 2. SFX 파일 → SerializeField 매핑 ─────────────
        // key: 파일명(확장자 제외), value: SerializeField 이름
        var mapping = new System.Collections.Generic.Dictionary<string, string>
        {
            // Combat
            { "sfx_player_attack",     "sfxPlayerAttack" },
            { "sfx_enemy_hit",         "sfxEnemyHit" },
            { "sfx_enemy_death",       "sfxEnemyDeath" },
            { "sfx_critical_hit",      "sfxCriticalHit" },
            // sfxBossAppear — 제외 (사용자 판단: 듣기 싫은 소리)
            { "sfx_boss_defeat",       "sfxBossDefeat" },

            // Skills
            { "sfx_staff_rush",        "sfxStaffRush" },
            // sfxMeteorRain — 제외 (불필요)
            { "sfx_meteor_explosion",  "sfxMeteorExplosion" },
            { "sfx_chain_lightning",   "sfxChainLightning" },
            { "sfx_berserker_dash",    "sfxBerserkerDash" },
            { "sfx_manual_attack",     "sfxManualAttack" },
            { "sfx_buff_activate",     "sfxBuffActivate" },

            // System
            { "sfx_gold_pickup",       "sfxGoldPickup" },
            { "sfx_level_up",          "sfxLevelUp" },
            { "sfx_upgrade_success",   "sfxUpgradeSuccess" },
            // sfxWaveStart — 제외
            // sfxStageClear — 제외
            { "sfx_offline_reward",    "sfxOfflineReward" },
        };

        // ── 3. Assets/SFX/ 에서 파일 찾아서 연결 ───────────
        string sfxFolder = "Assets/SFX";
        if (!AssetDatabase.IsValidFolder(sfxFolder))
        {
            Debug.LogError($"[SFXConnector] Folder not found: {sfxFolder}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { sfxFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (mapping.TryGetValue(fileName, out string fieldName))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                var prop = so.FindProperty(fieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = clip;
                    linked++;
                    Debug.Log($"[SFXConnector] {fileName} → {fieldName}");
                }
                else
                {
                    Debug.LogWarning($"[SFXConnector] Field '{fieldName}' not found in AudioManager");
                }
            }
        }

        // ── 4. 제외된 사운드 슬롯 비우기 ───────────────────
        // 비워두면 프로시저럴 fallback이 자동으로 채움
        // 프로시저럴도 안 울리게 하려면 null 유지 + AudioManager에서 처리
        string[] excludedFields = {
            "sfxBossAppear",    // 듣기 싫은 소리
            "sfxMeteorRain",    // 불필요
            "sfxWaveStart",     // 제외
            "sfxStageClear",    // 제외
        };

        // 주의: 이 필드들은 null로 두면 프로시저럴이 채움
        // 완전 무음 처리는 AudioManager 코드 수정으로 대응

        // ── 5. 볼륨 설정 조정 ──────────────────────────────
        // 전체 SFX 볼륨을 낮춰서 기본 크기 줄임
        var sfxVolProp = so.FindProperty("sfxVolume");
        if (sfxVolProp != null)
        {
            sfxVolProp.floatValue = 0.45f;  // 0.7 → 0.45 (전체적으로 소리가 크다는 피드백)
            Debug.Log("[SFXConnector] sfxVolume: 0.7 → 0.45");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(am);

        Debug.Log($"[SFXConnector] Complete! {linked} clips linked.");
        Debug.Log("[SFXConnector] 제외된 슬롯: BossAppear, MeteorRain, WaveStart, StageClear");
        Debug.Log("[SFXConnector] → 이 슬롯들은 프로시저럴 fallback 또는 무음 처리됨");
    }
}
#endif
