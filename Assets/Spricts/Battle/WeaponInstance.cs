using UnityEngine;  
  
public class WeaponInstance  
{  
    public WeaponData Data { get; private set; }  
    public ResourceGauge WPGauge { get; private set; }  
  
    public SkillData Fire1Skill => Data != null ? Data.fire1Skill : null;  
    public SkillData Fire2Skill => Data != null ? Data.fire2Skill : null;  
  
    public WeaponInstance(WeaponData data, Stat maxStat = null)  
    {  
        Data = data;  
        if (Data != null)  
        {  
            Stat wpStat = maxStat != null ? maxStat : new Stat(100f);  
            WPGauge = new ResourceGauge(wpStat);  
            WPGauge.Initialize(); // ゲージの初期化を確実に実行  
  
            InitializeDefaultSkills();  
        }  
    }  
  
    private void InitializeDefaultSkills()  
    {  
        if (Data.settableSkills == null || Data.settableSkills.Count == 0) return;  
  
        if (Data.fire1Skill == null && Data.settableSkills.Count > 0)  
        {  
            Data.fire1Skill = Data.settableSkills[0];  
        }  
  
        if (Data.fire2Skill == null)  
        {  
            if (Data.settableSkills.Count > 1) Data.fire2Skill = Data.settableSkills[1];  
            else if (Data.settableSkills.Count == 1) Data.fire2Skill = Data.settableSkills[0];  
        }  
    }  
  
    /// <summary>  
    /// スキルコスト分の WP を消費する処理  
    /// </summary>  
    public bool ConsumeWP(float amount)  
    {  
        if (WPGauge == null) return false;  
  
        if (WPGauge.CurrentValue >= amount)  
        {  
            bool success = WPGauge.Consume(amount);   
            if (success)  
            {  
                //Debug.Log($"[WeaponInstance] WPを消費しました: -{amount}");  
                return true;  
            }  
        }  
  
        //Debug.Log($"[WeaponInstance] WPが不足しています (必要: {amount})");  
        return false;  
    }  
  
    /// <summary>  
    /// 指定量の WP を回復する処理  
    /// </summary>  
    public void RecoverWP(float amount)
    {
        if (WPGauge == null) return;

        // ResourceGauge が持つ Recover メソッドを呼び出して実際に数値を増やす
        WPGauge.Recover(amount);
    }
  
    /// <summary>  
    /// 毎フレームの WP 自動回復や自然減少の更新  
    /// </summary>  
    public void UpdateWP(float deltaTime, float recoveryRatePerSecond)  
    {  
        if (WPGauge == null) return;  
  
        if (WPGauge.CurrentValue < WPGauge.MaxStat.Value)  
        {  
            float recoverAmount = recoveryRatePerSecond * deltaTime;  
            RecoverWP(recoverAmount);  
        }  
    }  
}