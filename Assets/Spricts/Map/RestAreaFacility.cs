using UnityEngine;

public class RestAreaFacility : MonoBehaviour
{
    /// <summary>
    /// 休憩所で特定のステータスを強化する
    /// </summary>
    /// <param name="playerStats">プレイヤーのEntityStats</param>
    /// <param name="expController">プレイヤーのPlayerExpController</param>
    /// <param name="statName">強化する対象 ("ATK" など)</param>
    public void TryUpgradeStat(EntityStats playerStats, PlayerExpController expController, string statName)
    {
        if (playerStats == null || expController == null) return;

        // 1. EXPマネージャー経由でコストを支払い、強化回数を進める
        if (expController.ExpManager.ConsumeExpForStat(statName))
        {
            // 2. EntityStats 側の数値を上昇させる
            ApplyStatIncrease(playerStats, statName);
        }
    }

    private void ApplyStatIncrease(EntityStats stats, string statName)
    {
        switch (statName)
        {
            case "ATK":
                // 例: 攻撃力ステータスの基礎値やバフを増やす
                // stats.Attack.AddModifier(new StatModifier(5f, StatModType.Flat));
                Debug.Log("プレイヤーの ATK が上昇しました！");
                break;
            case "VIT":
                // 例: 物理防御力やHP上限を増やす
                // stats.Defense.AddModifier(new StatModifier(3f, StatModType.Flat));
                Debug.Log("プレイヤーの VIT が上昇しました！");
                break;
            default:
                Debug.LogWarning($"未知のステータス名です: {statName}");
                break;
        }
        
        // 必要に応じてステータスの再初期化を呼び出す
        // stats.InitializeStats();
    }
}