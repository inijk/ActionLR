using UnityEngine;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("― 参照コンポーネント ―")]
    [SerializeField] private EntityStats playerStats;     //[cite: 3]
    [SerializeField] private WeaponManager weaponManager;

    [Header("― UIコンポーネント ―")]
    [SerializeField] private TextMeshProUGUI statusText;

    // 内部キャッシュ用データ
    private string hpString = "HP: --- / ---";
    private string stString = "ST: --- / ---";
    private string weaponString = "装備: なし";
    private string wpString = "WP: --- / ---";

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    private void Start()
    {
        if (playerStats == null) playerStats = GetComponentInParent<EntityStats>(); //[cite: 3]
        if (weaponManager == null) weaponManager = GetComponentInParent<WeaponManager>();

        // イベントの再登録と初期描画
        RegisterEvents();
        RefreshAllDisplay();
    }

    private void RegisterEvents()
    {
        UnregisterEvents(); // 重複登録防止

        // 1. HP / ST のイベント登録[cite: 1]
        if (playerStats != null)
        {
            if (playerStats.HP != null) playerStats.HP.OnValueChanged += OnHPChanged; //[cite: 1]
            if (playerStats.ST != null) playerStats.ST.OnValueChanged += OnSTChanged; //[cite: 1]
        }

        // 2. 武器 / WP のイベント登録
        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched += OnWeaponSwitched;
            weaponManager.OnActiveWeaponWPChanged += OnWPChanged;
        }
    }

    private void UnregisterEvents()
    {
        if (playerStats != null)
        {
            if (playerStats.HP != null) playerStats.HP.OnValueChanged -= OnHPChanged; //[cite: 1]
            if (playerStats.ST != null) playerStats.ST.OnValueChanged -= OnSTChanged; //[cite: 1]
        }

        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched -= OnWeaponSwitched;
            weaponManager.OnActiveWeaponWPChanged -= OnWPChanged;
        }
    }

    // --- イベント受信ハンドラ ---

    private void OnHPChanged(float current, float max)
    {
        hpString = $"HP: {current:F0} / {max:F0}";
        RenderUI();
        // 将来のゲージ対応例: hpSlider.value = current / max;
    }

    private void OnSTChanged(float current, float max)
    {
        stString = $"ST: {current:F1} / {max:F0}";
        RenderUI();
        // 将来のゲージ対応例: stSlider.value = current / max;
    }

    private void OnWeaponSwitched(WeaponInstance weapon)
    {
        if (weapon != null && weapon.Data != null)
        {
            weaponString = $"装備: {weapon.Data.weaponName}";
            if (weapon.WPGauge != null)
            {
                wpString = $"WP: {weapon.WPGauge.CurrentValue:F1} / {weapon.WPGauge.MaxStat.Value:F0}";
            }
        }
        else
        {
            weaponString = "装備: なし";
            wpString = "WP: --- / ---";
        }
        RenderUI();
    }

    private void OnWPChanged(float current, float max)
    {
        wpString = $"WP: {current:F1} / {max:F0}";
        RenderUI();
        // 将来のゲージ対応例: wpSlider.value = current / max;
    }

    /// <summary>
    /// キャッシュされた文字列をまとめて描画（描画呼び出しの集約）
    /// </summary>
    private void RenderUI()
    {
        if (statusText != null)
        {
            statusText.text = $"{hpString}\n{stString}\n{weaponString}\n{wpString}";
        }
    }

    /// <summary>
    /// 初期表示やリセット時の全表示強制更新
    /// </summary>
    public void RefreshAllDisplay()
    {
        if (playerStats != null)
        {
            if (playerStats.HP != null) OnHPChanged(playerStats.HP.CurrentValue, playerStats.HP.MaxStat.Value);
            if (playerStats.ST != null) OnSTChanged(playerStats.ST.CurrentValue, playerStats.ST.MaxStat.Value);
        }

        if (weaponManager != null && weaponManager.ActiveWeapon != null)
        {
            OnWeaponSwitched(weaponManager.ActiveWeapon);
        }
        else
        {
            RenderUI();
        }
    }
}