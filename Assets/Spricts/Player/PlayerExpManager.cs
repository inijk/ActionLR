using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerExpManager
{
    [Header("― EXPプール ―")]
    [SerializeField] private float currentExp = 0f;       // 休憩所で消費できる現在の所持EXP
    [SerializeField] private float totalEarnedExp = 0f;   // 累計獲得EXP

    [Header("― 必要EXPスケーリング設定 ―")]
    [SerializeField] private float baseRequiredExp = 50f;   // 最初のステータス強化に必要な基本EXP
    [SerializeField] private float expScalingFactor = 1.3f;   // ステータスを強化するごとに必要量が何倍になるか

    // 各ステータスの強化回数（例: "ATK" -> 2回強化, "VIT" -> 1回強化 など）
    private Dictionary<string, int> statUpgradeCounts = new Dictionary<string, int>();

    // 外部参照用
    public float CurrentExp => currentExp;
    public float TotalEarnedExp => totalEarnedExp;

    public event Action<float, float> OnExpChanged; // (currentExp, totalEarnedExp)

    public void Initialize()
    {
        OnExpChanged?.Invoke(currentExp, totalEarnedExp);
    }

    /// <summary>
    /// 敵撃破時などにEXPを獲得（プールに蓄積、ステータスには即時反映されない）
    /// </summary>
    public void AddExp(float amount)
    {
        if (amount <= 0) return;

        currentExp += amount;
        totalEarnedExp += amount;
        OnExpChanged?.Invoke(currentExp, totalEarnedExp);
        Debug.Log($"[EXP獲得] +{amount} (現在プール: {currentExp})");
    }

    /// <summary>
    /// 指定したステータスの現在の強化回数に基づく必要EXPを計算
    /// </summary>
    public float GetRequiredExpForStat(string statTypeName)
    {
        int count = GetStatUpgradeCount(statTypeName);
        return baseRequiredExp * Mathf.Pow(expScalingFactor, count);
    }

    public int GetStatUpgradeCount(string statTypeName)
    {
        if (statUpgradeCounts.TryGetValue(statTypeName, out int count))
        {
            return count;
        }
        return 0;
    }

    /// <summary>
    /// 休憩所などでEXPを消費してステータスを強化する
    /// </summary>
    public bool ConsumeExpForStat(string statTypeName)
    {
        float cost = GetRequiredExpForStat(statTypeName);

        if (currentExp < cost)
        {
            Debug.Log($"[EXP不足] 必要: {cost}, 所持: {currentExp}");
            return false;
        }

        currentExp -= cost;
        
        // 強化回数をインクリメント（次回から必要コストが増加）
        if (statUpgradeCounts.ContainsKey(statTypeName))
        {
            statUpgradeCounts[statTypeName]++;
        }
        else
        {
            statUpgradeCounts[statTypeName] = 1;
        }

        OnExpChanged?.Invoke(currentExp, totalEarnedExp);
        Debug.Log($"[ステータス強化成功] {statTypeName} (消費EXP: {cost}, 次回必要EXP: {GetRequiredExpForStat(statTypeName)})");
        return true;
    }
}