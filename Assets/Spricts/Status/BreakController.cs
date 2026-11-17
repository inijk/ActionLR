using System;
using UnityEngine;

public class BreakController : MonoBehaviour
{
    [Header("― ブレイクシステム基本設定 ―")]
    [SerializeField] private float baseBreakLimit = 200f;          // リミット％（ブレイクする閾値）
    [SerializeField] private float baseBreakResistance = 1.0f;     // ブレイク耐性（乗算係数: 1.0=等倍, 0.5=半減, 1.25=1.25倍）
    [SerializeField] private float baseBreakDecayRate = 5f;        // 減少値（非ブレイク時の1秒あたりの減少スピード）
    [SerializeField] private float baseBreakDuration = 8f;         // ブレイク状態が継続する時間（秒）
    [SerializeField] private float maxBreakPercent = 999f;         // ブレイク％の最大上限値
    [SerializeField] private float breakBonusPercent = 100f;       // ブレイク突入時のボーナス加算％
    [SerializeField] private float breakStateResistanceBonus = 0.5f; // ブレイク中の耐性（蓄積倍率）上昇ボーナス

    // 外部スキル・バフ/デバフから動的に変更できるよう Stat クラスで公開
    public Stat BreakLimit { get; private set; }       // リミット％
    public Stat BreakResistance { get; private set; }  // ブレイク耐性（倍率）
    public Stat BreakDecayRate { get; private set; }   // 減少値（外部スキルで変更可能）
    public Stat BreakDuration { get; private set; }    // ブレイク継続時間（秒）

    // 動的状態
    public float CurrentBreakPercent { get; private set; } = 100f; // 現在のブレイク％（初期値100%）
    public bool IsBroken { get; private set; } = false;            // ブレイク状態フラグ
    public float RemainingBreakTime { get; private set; } = 0f;    // ブレイク解除までの残り時間（秒）
    public float MaxBreakTime { get; private set; } = 0f;          // 今回のブレイクの最大継続時間（ゲージ計算用）

    // 外部連携用イベント
    public event Action<float, float> OnBreakChanged;     // (現在ブレイク%, リミット%)
    public event Action<float, float> OnBreakTimeChanged; // (残りブレイク時間, 最大ブレイク時間)
    public event Action OnBreakStarted;                   // ブレイク状態突入時
    public event Action OnBreakEnded;                     // ブレイク状態解除時

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
        BreakDuration = new Stat(baseBreakDuration);

        CurrentBreakPercent = 100f;
        IsBroken = false;
        RemainingBreakTime = 0f;
        MaxBreakTime = baseBreakDuration;

        OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);
    }

    private void Update()
    {
        if (entityStats != null && entityStats.IsDead) return;

        if (IsBroken)
        {
            UpdateBrokenTimer();
        }
        else
        {
            UpdateNormalDecay();
        }
    }

    /// <summary>
    /// 攻撃を受けた際にブレイク値を蓄積させる（非ブレイク時・ブレイク時共通）
    /// </summary>
    /// <param name="rawBreakValue">攻撃・スキルに設定された基本ブレイク値</param>
    public void AddBreakValue(float rawBreakValue)
    {
        if ((entityStats != null && entityStats.IsDead) || rawBreakValue <= 0f) return;

        // 与えられたブレイク値 × ブレイク耐性（0.5 や 1.25 などの乗算係数）
        float resistanceMult = Mathf.Max(0f, BreakResistance.Value);
        float actualIncrease = rawBreakValue * resistanceMult;

        // 上限（maxBreakPercent: 999%）まで加算
        CurrentBreakPercent = Mathf.Min(maxBreakPercent, CurrentBreakPercent + actualIncrease);

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

        // ブレイク値を +100% し、上限（999%）でクランプ
        CurrentBreakPercent = Mathf.Min(maxBreakPercent, CurrentBreakPercent + breakBonusPercent);

        // ブレイク中は蓄積しやすくなるよう耐性倍率を上昇
        BreakResistance.AddModifier(breakStateResistanceBonus);

        // ブレイク解除までのタイマーをセット
        MaxBreakTime = Mathf.Max(0.1f, BreakDuration.Value);
        RemainingBreakTime = MaxBreakTime;

        //Debug.Log($"[Break] {gameObject.name} がブレイク状態になりました！ (ブレイク%: {CurrentBreakPercent:F0}%, 継続時間: {MaxBreakTime:F1}秒)");
        OnBreakStarted?.Invoke();
        OnBreakTimeChanged?.Invoke(RemainingBreakTime, MaxBreakTime);
    }

    /// <summary>
    /// 非ブレイク時：時間経過で初期値（100%）まで減少する処理
    /// </summary>
    private void UpdateNormalDecay()
    {
        if (CurrentBreakPercent <= 100f) return;

        float decay = Mathf.Max(0f, BreakDecayRate.Value) * Time.deltaTime;
        if (decay <= 0f) return;

        CurrentBreakPercent = Mathf.Max(100f, CurrentBreakPercent - decay);
        OnBreakChanged?.Invoke(CurrentBreakPercent, BreakLimit.Value);
    }

    /// <summary>
    /// ブレイク時：解除までの残り時間をカウントダウンする処理
    /// </summary>
    private void UpdateBrokenTimer()
    {
        RemainingBreakTime = Mathf.Max(0f, RemainingBreakTime - Time.deltaTime);
        OnBreakTimeChanged?.Invoke(RemainingBreakTime, MaxBreakTime);

        if (RemainingBreakTime <= 0f)
        {
            EndBreak();
        }
    }

    /// <summary>
    /// ブレイク状態の解除処理
    /// </summary>
    private void EndBreak()
    {
        IsBroken = false;
        CurrentBreakPercent = 100f;
        RemainingBreakTime = 0f;

        // ブレイク中に加算していた耐性ボーナスを元に戻す
        BreakResistance.RemoveModifier(breakStateResistanceBonus);

        //Debug.Log($"[Break] {gameObject.name} のブレイク状態が解除されました。");
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

    /// <summary>
    /// 外部スキル等から減少スピード（BreakDecayRate）を一時変更するためのヘルパー
    /// </summary>
    public void AddDecayRateModifier(float amount)
    {
        BreakDecayRate.AddModifier(amount);
    }

    public void RemoveDecayRateModifier(float amount)
    {
        BreakDecayRate.RemoveModifier(amount);
    }
}