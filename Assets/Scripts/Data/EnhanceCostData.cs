using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnhanceCostData", menuName = "IdleRPG/Enhance Cost Data")]
public class EnhanceCostData : ScriptableObject
{
    [Serializable]
    public struct EnhanceCostRow
    {
        public int EnhanceLevel;
        public int CostGold;
        public float HPBonus;
        public float ATKBonus;
        public float DEFBonus;
    }

    public EnhanceCostRow[] Rows;

    public int GetCost(int currentLevel)
    {
        if (Rows != null)
        {
            for (int i = 0; i < Rows.Length; i++)
            {
                if (Rows[i].EnhanceLevel == currentLevel)
                    return Rows[i].CostGold;
            }
        }

        return Mathf.FloorToInt(50 * Mathf.Pow(1.3f, currentLevel));
    }

    public float GetHPBonusPerLevel(int currentLevel)
    {
        return GetRowValue(currentLevel, row => row.HPBonus, 10f);
    }

    public float GetATKBonusPerLevel(int currentLevel)
    {
        return GetRowValue(currentLevel, row => row.ATKBonus, 3f);
    }

    public float GetDEFBonusPerLevel(int currentLevel)
    {
        return GetRowValue(currentLevel, row => row.DEFBonus, 1.5f);
    }

    private float GetRowValue(int currentLevel, Func<EnhanceCostRow, float> selector, float fallback)
    {
        if (Rows != null)
        {
            for (int i = 0; i < Rows.Length; i++)
            {
                if (Rows[i].EnhanceLevel == currentLevel)
                    return selector(Rows[i]);
            }
        }

        return fallback;
    }
}
