using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("― 参照コンポーネント ―")]
    [SerializeField] private EntityStats playerStats;
    [SerializeField] private WeaponManager weaponManager;

    [Header("― HP / ST UI (HPは数値＋ゲージ、STはゲージのみ) ―")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image stFillImage;

    [System.Serializable]
    public struct WPGaugeUI
    {
        [Header("各スロット固有のUI要素（位置・大きさはエディタ上のRectTransformで個別に設定）")]
        public Image fillImage;          // WPゲージのFill
        public TextMeshProUGUI wpText;   // WPの数値テキスト
        public Image weaponIconImage;    // 武器のImage (アイコン等)
        public Image colorImage;         // 色を反映するImage（背景やフレームなど）
    }

    [Header("― WP 3スロット UI (固定配置されたスロット 1, 2, 3) ―")]
    [SerializeField] private WPGaugeUI[] wpGauges = new WPGaugeUI[3];
    [SerializeField] private TextMeshProUGUI activeWeaponNameText;

    [Header("― スロットごとの基本カラー (0:R, 1:G, 2:B) ―")]
    [SerializeField] private Color[] slotColors = new Color[] { Color.red, Color.green, Color.blue };

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
        if (playerStats == null) playerStats = GetComponentInParent<EntityStats>();
        if (weaponManager == null) weaponManager = GetComponentInParent<WeaponManager>();

        RegisterEvents();
        RefreshAllDisplay();
    }

    private void Update()
    {
        // 毎フレームWPの回復・変動を読み込み、UIに即時反映させる
        if (weaponManager != null)
        {
            weaponManager.NotifyStateChanged();
        }
    }

    private void RegisterEvents()
    {
        UnregisterEvents();

        if (playerStats != null)
        {
            if (playerStats.HP != null) playerStats.HP.OnValueChanged += OnHPChanged;
            if (playerStats.ST != null) playerStats.ST.OnValueChanged += OnSTChanged;
        }

        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched += OnWeaponSwitched;
            weaponManager.OnWPStateChanged += OnWPStateChanged;
        }
    }

    private void UnregisterEvents()
    {
        if (playerStats != null)
        {
            if (playerStats.HP != null) playerStats.HP.OnValueChanged -= OnHPChanged;
            if (playerStats.ST != null) playerStats.ST.OnValueChanged -= OnSTChanged;
        }

        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched -= OnWeaponSwitched;
            weaponManager.OnWPStateChanged -= OnWPStateChanged;
        }
    }

    private void OnHPChanged(float current, float max)
    {
        if (hpFillImage != null) hpFillImage.fillAmount = max > 0 ? current / max : 0f;
        if (hpText != null) hpText.text = $"{current:F0} / {max:F0}";
    }

    private void OnSTChanged(float current, float max)
    {
        // STはゲージのみ
        if (stFillImage != null) stFillImage.fillAmount = max > 0 ? current / max : 0f;
    }

    private void OnWeaponSwitched(WeaponInstance weapon)
    {
        if (activeWeaponNameText != null)
        {
            activeWeaponNameText.text = (weapon != null && weapon.Data != null) ? $"装備: {weapon.Data.weaponName}" : "装備: なし";
        }
    }

    /// <summary>
    /// 武器変更および毎フレームの更新により、Image, Color, WP の三要素を各UIスロットにローテーション割り当てして外見を変化させる
    /// </summary>
    private void OnWPStateChanged(int activeIndex, List<WeaponInstance> weapons)
    {
        for (int i = 0; i < 3; i++)
        {
            // アクティブな武器がスロット0にくるようにローテーション
            int weaponIndex = (i + activeIndex) % 3;
            WeaponInstance targetWeapon = (weapons.Count > weaponIndex) ? weapons[weaponIndex] : null;

            WPGaugeUI slotUI = wpGauges[i];

            // 1. Color の割り当て
            Color slotColor = (slotColors.Length > weaponIndex) ? slotColors[weaponIndex] : Color.white;
            if (slotUI.colorImage != null) slotUI.colorImage.color = slotColor;
            if (slotUI.fillImage != null) slotUI.fillImage.color = slotColor;

            // 2. Image (武器アイコン等) の割り当て
            if (slotUI.weaponIconImage != null)
            {
                if (targetWeapon != null && targetWeapon.Data != null && targetWeapon.Data.weaponIcon != null)
                {
                    slotUI.weaponIconImage.gameObject.SetActive(true);
                    slotUI.weaponIconImage.sprite = targetWeapon.Data.weaponIcon;
                }
                else
                {
                    slotUI.weaponIconImage.gameObject.SetActive(false);
                }
            }

            // 3. WP (ゲージ量・数値) の割り当て
            if (slotUI.fillImage != null)
            {
                if (targetWeapon != null && targetWeapon.WPGauge != null)
                {
                    float cur = targetWeapon.WPGauge.CurrentValue;
                    float max = targetWeapon.WPGauge.MaxStat.Value;
                    
                    slotUI.fillImage.fillAmount = max > 0 ? cur / max : 0f;
                    slotUI.fillImage.SetAllDirty(); // 実行中の強制再描画

                    if (slotUI.wpText != null)
                    {
                        slotUI.wpText.text = $"{cur:F1} / {max:F0}";
                    }
                }
                else
                {
                    slotUI.fillImage.fillAmount = 0f;
                    if (slotUI.wpText != null) slotUI.wpText.text = "---";
                }
            }
        }
    }

    public void RefreshAllDisplay()
    {
        if (playerStats != null)
        {
            if (playerStats.HP != null) OnHPChanged(playerStats.HP.CurrentValue, playerStats.HP.MaxStat.Value);
            if (playerStats.ST != null) OnSTChanged(playerStats.ST.CurrentValue, playerStats.ST.MaxStat.Value);
        }

        if (weaponManager != null)
        {
            OnWeaponSwitched(weaponManager.ActiveWeapon);
            weaponManager.NotifyStateChanged();
        }
    }
}