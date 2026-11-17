using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LockOnTargetUI : MonoBehaviour
{
    [Header("― 参照コンポーネント ―")]
    [SerializeField] private LockOnController lockOnController;

    [Header("― UI ルートパネル ―")]
    [SerializeField] private GameObject uiRootPanel; // ロックオン中のみ表示する親パネル

    [Header("― 対象名表示 ―")]
    [SerializeField] private TextMeshProUGUI targetNameText;

    [Header("― HP UI (Gauge + 数値) ―")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("― ブレイク UI (Gauge + 数値) ―")]
    [SerializeField] private Image breakFillImage;
    [SerializeField] private TextMeshProUGUI breakText;

    [Header("― ブレイク状態カラー演出 ―")]
    [SerializeField] private Color normalBreakColor = new Color(1f, 0.65f, 0f, 1f);  // 通常蓄積中（オレンジ）
    [SerializeField] private Color brokenStateColor = new Color(1f, 0.2f, 0.2f, 1f);   // ブレイク状態中（赤）

    private EntityStats currentStats;
    private BreakController currentBreak;

    private void Awake()
    {
        if (uiRootPanel == gameObject)
        {
            Debug.LogError("[LockOnTargetUI] Ui Root Panel に自分自身は設定できません！親の Canvas 等にアタッチしてください。");
        }

        if (lockOnController == null)
        {
            lockOnController = FindFirstObjectByType<LockOnController>();
        }
    }

    private void OnEnable()
    {
        if (lockOnController != null)
        {
            lockOnController.OnLockOnTargetChanged += HandleTargetChanged;
        }
    }

    private void OnDisable()
    {
        if (lockOnController != null)
        {
            lockOnController.OnLockOnTargetChanged -= HandleTargetChanged;
        }
        UnbindTargetEvents();
    }

    private void Start()
    {
        if (lockOnController != null && lockOnController.IsLockedOn)
        {
            HandleTargetChanged(
                lockOnController.LockedTarget,
                lockOnController.LockedTargetStats,
                lockOnController.LockedTargetBreak
            );
        }
        else if (uiRootPanel != null && uiRootPanel != gameObject)
        {
            uiRootPanel.SetActive(false);
        }
    }

    private void HandleTargetChanged(Transform target, EntityStats stats, BreakController breakCtrl)
    {
        UnbindTargetEvents();

        if (target == null)
        {
            if (uiRootPanel != null && uiRootPanel != gameObject) uiRootPanel.SetActive(false);
            return;
        }

        currentStats = stats;
        currentBreak = breakCtrl;

        if (uiRootPanel != null) uiRootPanel.SetActive(true);

        if (targetNameText != null)
        {
            targetNameText.text = target.name.Replace("(Clone)", "").Trim();
        }

        // HPイベント購読と初期描画
        if (currentStats != null && currentStats.HP != null)
        {
            currentStats.HP.OnValueChanged += UpdateHPUI;
            UpdateHPUI(currentStats.HP.CurrentValue, currentStats.HP.MaxStat.Value);
        }
        else
        {
            if (hpFillImage != null) hpFillImage.fillAmount = 0f;
            if (hpText != null) hpText.text = "--- / ---";
        }

        // ブレイクイベント購読と初期描画
        if (currentBreak != null && currentBreak.BreakLimit != null)
        {
            currentBreak.OnBreakChanged += UpdateBreakUI;
            currentBreak.OnBreakTimeChanged += UpdateBreakTimerGauge;
            currentBreak.OnBreakStarted += HandleBreakStateChanged;
            currentBreak.OnBreakEnded += HandleBreakStateChanged;

            UpdateBreakUI(currentBreak.CurrentBreakPercent, currentBreak.BreakLimit.Value);
            if (currentBreak.IsBroken)
            {
                UpdateBreakTimerGauge(currentBreak.RemainingBreakTime, currentBreak.MaxBreakTime);
            }
            UpdateBreakStateColor();
        }
        else
        {
            if (breakFillImage != null) breakFillImage.fillAmount = 0f;
            if (breakText != null) breakText.text = "---% / ---%";
        }
    }

    private void UnbindTargetEvents()
    {
        if (currentStats != null && currentStats.HP != null)
        {
            currentStats.HP.OnValueChanged -= UpdateHPUI;
        }

        if (currentBreak != null)
        {
            currentBreak.OnBreakChanged -= UpdateBreakUI;
            currentBreak.OnBreakTimeChanged -= UpdateBreakTimerGauge;
            currentBreak.OnBreakStarted -= HandleBreakStateChanged;
            currentBreak.OnBreakEnded -= HandleBreakStateChanged;
        }

        currentStats = null;
        currentBreak = null;
    }

    private void UpdateHPUI(float current, float max)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        }

        if (hpText != null)
        {
            hpText.text = $"{current:F0} / {max:F0}";
        }
    }

    /// <summary>
    /// ブレイク％（数値）と、非ブレイク時のゲージ更新
    /// </summary>
    private void UpdateBreakUI(float currentBreakPercent, float breakLimitPercent)
    {
        bool isBroken = currentBreak != null && currentBreak.IsBroken;

        // 非ブレイク時のみ「蓄積値 / リミット値」でゲージを更新（ブレイク中は残り時間ゲージを使う）
        if (!isBroken && breakFillImage != null)
        {
            float range = breakLimitPercent - 100f;
            float fill = range > 0f ? Mathf.Clamp01((currentBreakPercent - 100f) / range) : 0f;
            breakFillImage.fillAmount = fill;
        }

        if (breakText != null)
        {
            if (isBroken)
            {
                breakText.text = $"BREAK! {currentBreakPercent:F0}%";
            }
            else
            {
                breakText.text = $"{currentBreakPercent:F0}% / {breakLimitPercent:F0}%";
            }
        }
    }

    /// <summary>
    /// ブレイク時専用：ブレイク解除までの残り時間をゲージに反映
    /// </summary>
    private void UpdateBreakTimerGauge(float remainingTime, float maxTime)
    {
        if (currentBreak != null && currentBreak.IsBroken && breakFillImage != null)
        {
            breakFillImage.fillAmount = maxTime > 0f ? Mathf.Clamp01(remainingTime / maxTime) : 0f;
        }
    }

    private void HandleBreakStateChanged()
    {
        UpdateBreakStateColor();
        if (currentBreak != null)
        {
            UpdateBreakUI(currentBreak.CurrentBreakPercent, currentBreak.BreakLimit.Value);
        }
    }

    private void UpdateBreakStateColor()
    {
        if (breakFillImage == null || currentBreak == null) return;
        breakFillImage.color = currentBreak.IsBroken ? brokenStateColor : normalBreakColor;
    }
}