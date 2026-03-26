using System;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [Header("강화 비용 데이터")]
    [SerializeField] private EnhanceCostData enhanceCostData;

    [Header("강화 레벨")]
    public int hpLevel = 0;
    public int atkLevel = 0;
    public int defLevel = 0;

    public event Action<string, int> OnUpgraded;

    public float GetBonusHP() => GetAccumulatedBonus(hpLevel, statType: "HP");
    public float GetBonusATK() => GetAccumulatedBonus(atkLevel, statType: "ATK");
    public float GetBonusDEF() => GetAccumulatedBonus(defLevel, statType: "DEF");

    public int GetUpgradeCost(int currentLevel)
    {
        if (enhanceCostData != null)
            return enhanceCostData.GetCost(currentLevel);

        return Mathf.FloorToInt(50 * Mathf.Pow(1.3f, currentLevel));
    }

    public bool TryUpgrade(string statType, CharacterStats player)
    {
        if (player == null) return false;

        int level;
        switch (statType)
        {
            case "HP":  level = hpLevel;  break;
            case "ATK": level = atkLevel; break;
            case "DEF": level = defLevel; break;
            default: return false;
        }

        int cost = GetUpgradeCost(level);
        if (player.Gold < cost) return false;

        player.Gold -= cost;

        switch (statType)
        {
            case "HP":  hpLevel++;  break;
            case "ATK": atkLevel++; break;
            case "DEF": defLevel++; break;
        }

        ApplyBonuses(player);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUpgradeSuccess();

        OnUpgraded?.Invoke(statType, level + 1);
        Debug.Log($"강화 완료! {statType} Lv.{level + 1} | 비용: {cost}G");
        return true;
    }

    public void ApplyBonuses(CharacterStats player)
    {
        if (player == null) return;
        player.bonusHP = GetBonusHP();
        player.bonusATK = GetBonusATK();
        player.bonusDEF = GetBonusDEF();
        player.RefreshStats();
    }

    private float GetAccumulatedBonus(int level, string statType)
    {
        float total = 0f;
        for (int i = 0; i < level; i++)
        {
            if (enhanceCostData == null)
            {
                total += statType == "HP" ? 10f : statType == "ATK" ? 3f : 1.5f;
                continue;
            }

            switch (statType)
            {
                case "HP":
                    total += enhanceCostData.GetHPBonusPerLevel(i);
                    break;
                case "ATK":
                    total += enhanceCostData.GetATKBonusPerLevel(i);
                    break;
                case "DEF":
                    total += enhanceCostData.GetDEFBonusPerLevel(i);
                    break;
            }
        }

        return total;
    }
}
