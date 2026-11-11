using UnityEngine;
using TMPro;

public class StatusTab : MenuTabBase
{
    [Header("― ステータス表示用UI ―")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI stText;
    [SerializeField] private TextMeshProUGUI paramText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("― 参照 ―")]
    [SerializeField] private EntityStats playerStats;
    [SerializeField] private PlayerExpController expController;

    public override void RefreshTabContent()
    {
        if (playerStats != null)
        {
            // HPの表示 (ResourceGauge は MaxStat.Value を持つため、それを使用)
            if (hpText != null && playerStats.HP != null)
            {
                hpText.text = $"HP: {playerStats.HP.CurrentValue:F0} / {playerStats.HP.MaxStat.Value:F0}";
            }

            // STの表示
            if (stText != null && playerStats.ST != null)
            {
                stText.text = $"ST: {playerStats.ST.CurrentValue:F0} / {playerStats.ST.MaxStat.Value:F0}";
            }

            // 攻撃力の表示 (EntityStats の Attack ステータスを参照)
            if (paramText != null)
            {   
                
                paramText.text = $"ATK: {playerStats.BaseLevels.ATK}\nINT: {playerStats.BaseLevels.INT}\nDEF: {playerStats.BaseLevels.DEF}\nMND: {playerStats.BaseLevels.MND}\nVIT: {playerStats.BaseLevels.VIT}\nDEX: {playerStats.BaseLevels.DEX}";
            }
        }
        // EXPの表示
        if (expController != null && expController.ExpManager != null)
        {
            if (expText != null)
            {
                expText.text = $"所持EXP: {expController.ExpManager.CurrentExp}";
            }
        }

        Debug.Log("[StatusTab] ステータス画面の表示を更新しました。");
    }
}