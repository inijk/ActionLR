using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private EntityStats stats;
    
    // 現在付与されているすべての効果（状態異常・バフ・デバフ）
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
    }

    /// <summary>
    /// 効果（状態異常・バフ・デバフ）を付与またはレベル更新
    /// </summary>
    public void ApplyEffect(StatusEffectData data, int level, float duration)
    {
        ActiveEffect existing = activeEffects.Find(e => e.data.effectType == data.effectType);

        if (existing != null)
        {
            // 既に存在する場合はLvと時間を上書き・更新（例：高い方のLvを採用）
            OnEffectRemove(existing); // 一旦旧レベルの効果を破棄
            existing.currentLevel = Mathf.Min(existing.data.maxLevel, Mathf.Max(existing.currentLevel, level));
            existing.duration = Mathf.Max(existing.duration, duration);
            OnEffectApply(existing); // 新レベルの効果を適用
        }
        else
        {
            ActiveEffect newEffect = new ActiveEffect(data, level, duration);
            activeEffects.Add(newEffect);
            OnEffectApply(newEffect);
        }
    }

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.duration -= Time.deltaTime;

            // 継続処理 (炎上などのDot処理)
            ProcessContinuousEffect(effect);

            // 時間切れで解除
            if (effect.duration <= 0)
            {
                OnEffectRemove(effect);
                activeEffects.RemoveAt(i);
            }
        }
    }

    // 効果開始時（ステータス変化の加算など）
    private void OnEffectApply(ActiveEffect effect)
    {
        float val = effect.GetCurrentValue();

        switch (effect.data.effectType)
        {
            // --- 強化・弱化 (基礎ステータスのレベル増減や補正) ---
            case EffectType.AtkBoost:
                stats.Attack.AddModifier(val);
                break;
            case EffectType.DefBoost:
                stats.Defense.AddModifier(val);
                break;

            // --- 状態異常 (脆弱など数値変化を伴うもの) ---
            case EffectType.Vulnerable:
                stats.Defense.AddModifier(-val);
                break;
        }
    }

    // 継続処理 (フレームごとのダメージ等)
    private void ProcessContinuousEffect(ActiveEffect effect)
    {
        switch (effect.data.effectType)
        {
            case EffectType.Burn:
                // 炎上: Lvに応じた毎秒ダメージ
                stats.HP.Consume(effect.GetCurrentValue() * Time.deltaTime);
                break;
        }
    }

    // 効果終了時（ステータス変化の解除など）
    private void OnEffectRemove(ActiveEffect effect)
    {
        float val = effect.GetCurrentValue();

        switch (effect.data.effectType)
        {
            case EffectType.AtkBoost:
                stats.Attack.RemoveModifier(val);
                break;
            case EffectType.DefBoost:
                stats.Defense.RemoveModifier(val);
                break;
            case EffectType.Vulnerable:
                stats.Defense.RemoveModifier(-val);
                break;
        }
    }
}