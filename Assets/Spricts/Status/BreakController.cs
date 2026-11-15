using System;
using UnityEngine;

public class BreakController : MonoBehaviour
{
    [Header("― ブレイクシステム基本設定 ―")]
    [SerializeField] private float baseBreakLimit = 200f;        // リミット％（ブレイクする上限値）
    [SerializeField] private float baseBreakResistance = 100f;   // ブレイク耐性（100%基準）
    [SerializeField] private float baseBreakDecayRate = 5f;      // 減少値（1秒あたりの減少スピード）
    [SerializeField] private float breakResistancePenalty = 50f; // ブレイク時に減少する耐性値

    // バフ・デバフ・特殊コマンドで増減できるよう Stat クラスで管理
    public Stat BreakLimit { get; private set; }      // リミット％
    public Stat BreakResistance { get; private set; } // ブレイク耐性
    public Stat BreakDecayRate { get; private set; }  // 減少値

    // 動的状態
    public float CurrentBreakPercent { get; private set; } = 100f; // ブレイク％（初期値100%）
    public bool IsBroken { get; private set; } = false;            // ブレイク状態フラグ

    // 外部連携用イベント（UI表示・行動制限・SE/エフェクト再生など）
    public event Action<float, float> OnBreakChanged; // (現在ブレイク%, リミット%)
    public event Action OnBreakStarted;               // ブレイク状態突入時
    public event Action OnBreakEnded;                 // ブレイク状態解除時

    private EntityStats entityStats;

    private void Awake()
    {
        entityStats = GetComponent<EntityStats>();
        InitializeBreakStats();
    }

    public void InitializeBreakStats()
    {
        BreakLimit = new Stat(baseBreakLimit);
        BreakResistance = new Stat(baseBreakResistance);
        BreakDecayRate = new Stat(baseBreakDecayRate);

        CurrentBreakPercent = 100f;
        IsBroken = false;

        OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);
    }

    private void Update()
    {
        // 死亡している場合は処理しない
        if (entityStats != null && entityStats.IsDead) return;

        UpdateBreakDecay();
    }

    /// <summary>
    /// 攻撃を受けた際にブレイク値を蓄積させる
    /// </summary>
    /// <param name="rawBreakValue">攻撃・スキルに設定された基本ブレイク値</param>
    public void AddBreakValue(float rawBreakValue)
    {
        if ((entityStats != null && entityStats.IsDead) || rawBreakValue <= 0f) return;

        // ブレイク耐性による補正（100%基準：高いほど溜まりにくく、100%未満なら溜まりやすい）
        float resistance = Mathf.Max(1f, BreakResistance.Value);
        float actualIncrease = rawBreakValue * (100f / resistance);

        CurrentBreakPercent += actualIncrease;

        // 非ブレイク状態でリミット％を超えた場合、ブレイク状態へ移行
        if (!IsBroken && CurrentBreakPercent >= BreakLimit.Value)
        {
            TriggerBreak();
        }

        OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);
    }

    /// <summary>
    /// ブレイク状態への移行処理
    /// </summary>
    private void TriggerBreak()
    {
        IsBroken = true;

        // ブレイク値を +100% し、ブレイク耐性を減少させる
        CurrentBreakPercent += 100f;
        BreakResistance.AddModifier(-breakResistancePenalty);

        Debug.Log($"[Break] {gameObject.name} がブレイク状態になりました！ (ブレイク%: {CurrentBreakPercent}%)");
        OnBreakStarted?.Invoke();
    }

    /// <summary>
    /// 時間経過によるブレイク％減少とブレイク解除処理
    /// </summary>
    private void UpdateBreakDecay()
    {
        float decay = BreakDecayRate.Value * Time.deltaTime;
        if (decay <= 0f) return;

        if (IsBroken)
        {
            // ブレイク中：100%に戻るまでのカウントダウン（減少値が残り時間の減少スピードを管理）
            CurrentBreakPercent = Mathf.Max(100f, CurrentBreakPercent - decay);
            OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);

            if (CurrentBreakPercent <= 100f)
            {
                EndBreak();
            }
        }
        else if (CurrentBreakPercent > 100f)
        {
            // 非ブレイク中：初期値100%に向かって減少
            CurrentBreakPercent = Mathf.Max(100f, CurrentBreakPercent - decay);
            OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);
        }
    }

    /// <summary>
    /// ブレイク状態の解除処理
    /// </summary>
    private void EndBreak()
    {
        IsBroken = false;
        CurrentBreakPercent = 100f;

        // 減少させていたブレイク耐性を元に戻す
        BreakResistance.RemoveModifier(-breakResistancePenalty);

        Debug.Log($"[Break] {gameObject.name} のブレイク状態が解除されました。");
        OnBreakEnded?.Invoke();
        OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);
    }

    /// <summary>
    /// ダメージ計算時に掛け合わせる現在のブレイク倍率（例: 150% -> 1.5f）
    /// </summary>
    public float GetDamageMultiplier()
    {
        return CurrentBreakPercent / 100f;
    }
}