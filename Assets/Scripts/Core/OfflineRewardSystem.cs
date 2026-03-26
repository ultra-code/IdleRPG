using System;
using UnityEngine;

public class OfflineRewardSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;
    [SerializeField] private SaveSystem saveSystem;

    [Header("보상 설정")]
    [SerializeField] private float goldPerSecond = 1f;
    [SerializeField] private float maxOfflineHours = 8f;

    private const string KEY_QUIT_TIME = "Save_QuitTime";

    private float pendingGold;
    private float offlineSeconds;
    private bool rewardReady;

    public event Action<float, float> OnRewardReady;

    public float PendingGold => pendingGold;
    public float OfflineSeconds => offlineSeconds;
    public bool HasPendingReward => rewardReady;

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();
        if (saveSystem == null)
            saveSystem = FindAnyObjectByType<SaveSystem>();

        OfflineRewardPopupUI.EnsureExists(this);
        CheckOfflineReward();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveQuitTime();
    }

    private void OnApplicationQuit()
    {
        SaveQuitTime();
    }

    private void SaveQuitTime()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(KEY_QUIT_TIME, now.ToString());
        PlayerPrefs.Save();
    }

    private void CheckOfflineReward()
    {
        if (!PlayerPrefs.HasKey(KEY_QUIT_TIME)) return;

        string saved = PlayerPrefs.GetString(KEY_QUIT_TIME);
        if (!long.TryParse(saved, out long quitTime)) return;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        offlineSeconds = Mathf.Max(now - quitTime, 0);

        float maxSeconds = maxOfflineHours * 3600f;
        offlineSeconds = Mathf.Min(offlineSeconds, maxSeconds);

        if (offlineSeconds < 60f) return;

        pendingGold = offlineSeconds * goldPerSecond;
        rewardReady = true;

        string timeStr = FormatTime(offlineSeconds);
        Debug.Log($"오프라인 보상 준비 | {timeStr} 경과, Gold +{pendingGold:F0}");

        OnRewardReady?.Invoke(offlineSeconds, pendingGold);
    }

    public void ClaimReward()
    {
        if (!rewardReady || player == null) return;

        player.Gold += pendingGold;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOfflineReward();

        Debug.Log($"오프라인 보상 수령! Gold +{pendingGold:F0}");

        pendingGold = 0f;
        offlineSeconds = 0f;
        rewardReady = false;

        if (saveSystem != null)
            saveSystem.SaveGame();
    }

    private string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600f);
        int m = (int)((seconds % 3600f) / 60f);
        if (h > 0) return $"{h}h {m}m";
        return $"{m}m";
    }
}
