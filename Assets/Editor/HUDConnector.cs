#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class HUDConnector
{
    [MenuItem("IdleRPG/Setup/Connect PlayerHUD References")]
    public static void ConnectHUD()
    {
        var hudObj = GameObject.Find("PlayerHUD");
        if (hudObj == null) { Debug.LogError("PlayerHUD 오브젝트를 찾을 수 없음"); return; }

        var hud = hudObj.GetComponent<PlayerHUD>();
        if (hud == null) { Debug.LogError("PlayerHUD 컴포넌트를 찾을 수 없음"); return; }

        var playerObj = GameObject.Find("Player");
        var gmObj = GameObject.Find("GameManager");

        var so = new SerializedObject(hud);

        if (playerObj != null)
        {
            var stats = playerObj.GetComponent<CharacterStats>();
            if (stats != null)
            {
                so.FindProperty("player").objectReferenceValue = stats;
                Debug.Log("player ← Player(CharacterStats) 연결 완료");
            }
        }
        else Debug.LogWarning("Player 오브젝트를 찾을 수 없음");

        if (gmObj != null)
        {
            var wm = gmObj.GetComponent<WaveSpawnManager>();
            if (wm != null)
            {
                so.FindProperty("waveManager").objectReferenceValue = wm;
                Debug.Log("waveManager ← GameManager(WaveSpawnManager) 연결 완료");
            }
        }
        else Debug.LogWarning("GameManager 오브젝트를 찾을 수 없음");

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(hud);
        Debug.Log("PlayerHUD 참조 연결 완료!");
    }

    [MenuItem("IdleRPG/Setup/Add UpgradeSystem to GameManager")]
    public static void AddUpgradeSystem()
    {
        var gmObj = GameObject.Find("GameManager");
        if (gmObj == null) { Debug.LogError("GameManager 오브젝트를 찾을 수 없음"); return; }

        if (gmObj.GetComponent<UpgradeSystem>() != null)
        {
            Debug.Log("UpgradeSystem이 이미 존재합니다");
            return;
        }

        gmObj.AddComponent<UpgradeSystem>();
        EditorUtility.SetDirty(gmObj);
        Debug.Log("GameManager에 UpgradeSystem 추가 완료!");
    }
}
#endif
