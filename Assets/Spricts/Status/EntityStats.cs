using System;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    [System.Serializable]
    public struct BaseStatLevels
    {
        [Range(0, 9)] public int ATK;
        [Range(0, 9)] public int INT;
        [Range(0, 9)] public int DEF;
        [Range(0, 9)] public int MND;
        [Range(0, 9)] public int VIT;
        [Range(0, 9)] public int DEX;
    }

    [Header("― 基本ステータスレベル (0 ~ 9) ―")]
    [SerializeField] private BaseStatLevels baseLevels;
    public BaseStatLevels BaseLevels => baseLevels;

    // 戦闘計算用ステータス (Stat.cs を再利用)
    public Stat Attack { get; private set; }
    public Stat Intelligence { get; private set; }
    public Stat Defense { get; private set; }
    public Stat Mind { get; private set; }
    public Stat MaxHP { get; private set; }
    public Stat MaxST { get; private set; }
    public Stat maxWp;
    public Stat STRecoveryRate { get; private set; }

    // 死亡時に通知されるイベント
    public event Action OnDeath;
    private bool isDead = false;
    public bool IsDead => isDead;

    // 画面表示・動的リソース (ResourceGauge を再利用)
    public ResourceGauge HP { get; private set; }
    public ResourceGauge ST { get; private set; }
    public ResourceGauge FP { get; private set; } // 疲労度 (0 ～ 100想定)

    private void Awake()
    {
        InitializeStats();
    }

    public void InitializeStats()
    {
        float calculatedMaxHP = 50f + (baseLevels.VIT * 20f);
        float calculatedMaxST = 30f + (baseLevels.DEX * 10f);
        float calculatedSTRec = 5f + (baseLevels.DEX * 1.5f);
        float calculatedMaxWP = 100f;

        MaxHP = new Stat(calculatedMaxHP);
        MaxST = new Stat(calculatedMaxST);
        STRecoveryRate = new Stat(calculatedSTRec);
        maxWp = new Stat(calculatedMaxWP);

        Attack = new Stat(baseLevels.ATK * 10f);
        Intelligence = new Stat(baseLevels.INT * 10f);
        Defense = new Stat(baseLevels.DEF * 5f);
        Mind = new Stat(baseLevels.MND * 5f);

        HP = new ResourceGauge(MaxHP);
        ST = new ResourceGauge(MaxST);
        FP = new ResourceGauge(new Stat(100f));

        HP.Initialize();
        ST.Initialize();
        FP.Initialize();
    }

    private void Update()
    {
        if (ST.CurrentValue < MaxST.Value)
        {
            ST.Recover(STRecoveryRate.Value * Time.deltaTime);
        }
        CheckDeath();
    }

    public void CheckDeath()
    {
        if (isDead) return;

        if (HP != null && HP.CurrentValue <= 0f)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }
}