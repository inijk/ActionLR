using System;
using UnityEngine;

public class TimePauseManager : MonoBehaviour
{
    [Header("― 参照コンポーネント ―")]
    [SerializeField] private EntityStats playerStats;

    [Header("― 時間停止パラメータ ―")]
    [SerializeField] private float spCostPerSecond = 5f; // 時間停止中に消費する毎秒SP

    public bool IsPaused { get; private set; } = false;

    // イベント通知
    public event Action<bool> OnPauseStateChanged; // true: 停止, false: 解除

    private void Start()
    {
        if (playerStats == null) playerStats = GetComponentInParent<EntityStats>();
    }

    private void Update()
    {
        // 時間停止中、リアルタイムでSPを継続消費
        if (IsPaused)
        {
            if (playerStats != null && playerStats.ST != null)
            {
                // 時間停止中も動作する Time.unscaledDeltaTime を使用
                float consumeAmount = spCostPerSecond * Time.unscaledDeltaTime;
                
                if (!playerStats.ST.Consume(consumeAmount))
                {
                    // SPが枯渇したら強制解除
                    ResumeTime();
                }
            }
        }
    }

    /// <summary>
    /// 時間停止状態のトグル切り替え
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeTime();
        }
        else
        {
            StartPause();
        }
    }

    public void StartPause()
    {
        // SP不足チェック
        if (playerStats == null || playerStats.ST == null || playerStats.ST.CurrentValue <= 0)
        {
            Debug.LogWarning("[時間停止不可] SPが不足しています。");
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f; // ゲーム進行停止
        OnPauseStateChanged?.Invoke(true);
    }

    public void ResumeTime()
    {
        IsPaused = false;
        Time.timeScale = 1f; // 通常速度再開
        OnPauseStateChanged?.Invoke(false);
    }
}