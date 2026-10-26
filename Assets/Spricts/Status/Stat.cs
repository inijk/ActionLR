using System;
using UnityEngine;

// 静的ステータス（MaxHP, MaxST, 回復率など）
[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    private float addModifier;
    private float multModifier = 1f;

    public float Value => (baseValue + addModifier) * multModifier;

    public Stat(float defaultValue)
    {
        baseValue = defaultValue;
    }

    public void AddModifier(float amount) => addModifier += amount;
    public void RemoveModifier(float amount) => addModifier -= amount;
}

// 動的リソース（現在HP, 現在STなど）
// 動的リソース（現在HP, 現在STなど）
[Serializable]
public class ResourceGauge
{
    public float CurrentValue { get; private set; }
    public Stat MaxStat { get; private set; }

    public event Action<float, float> OnValueChanged;

    public ResourceGauge(Stat maxStat)
    {
        MaxStat = maxStat;
        CurrentValue = MaxStat.Value;
    }

    public void Initialize()
    {
        CurrentValue = MaxStat.Value;
        OnValueChanged?.Invoke(CurrentValue, MaxStat.Value);
    }

    // ダメージ / 消費処理（大きなダメージでも0に落として実行）
    public bool Consume(float amount)
    {
        if (amount <= 0f) return false;

        // 現HPを超えるダメージが来ても 0 で固定（クランプ）して減算
        CurrentValue = Mathf.Max(0f, CurrentValue - amount);
        OnValueChanged?.Invoke(CurrentValue, MaxStat.Value);
        
        return true;
    }

    // 回復処理
    public void Recover(float amount)
    {
        CurrentValue = Mathf.Min(MaxStat.Value, CurrentValue + amount);
        OnValueChanged?.Invoke(CurrentValue, MaxStat.Value);
    }
}