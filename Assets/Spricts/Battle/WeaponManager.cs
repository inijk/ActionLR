using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponSlot { Weapon1, Weapon2 }

    [Header("― WP回復設定 ―")]
    [SerializeField] private float wpRecoveryRate = 10f; // 1秒あたりのWP自然回復量

    [Header("― 武器スロット・データ ―")]
    [SerializeField] private WeaponData weaponData1;
    [SerializeField] private WeaponData weaponData2;
    [SerializeField] private EntityStats playerStats; // プレイヤーのステータス

    private Dictionary<WeaponSlot, WeaponInstance> weaponSlots = new Dictionary<WeaponSlot, WeaponInstance>();
    public WeaponSlot CurrentSlot { get; private set; } = WeaponSlot.Weapon1;

    public WeaponInstance ActiveWeapon => weaponSlots.ContainsKey(CurrentSlot) ? weaponSlots[CurrentSlot] : null;

    // --- イベント定義 ---
    public event Action<WeaponInstance> OnWeaponSwitched;
    
    // ★ PlayerStatusUI が求めている WP 変更通知イベントを追加
    public event Action<float, float> OnActiveWeaponWPChanged;

    [Header("― 武器切替用入力アクション ―")]
    [SerializeField] private InputActionReference switchWeaponAction;

    private void Start()
    {
        // もし playerStats がインスペクターでアサインされていなければ自動取得を試みる
        if (playerStats == null)
        {
            playerStats = GetComponent<EntityStats>();
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<EntityStats>();
            }
        }

        InitializeWeapons();
    }

    private void OnEnable()
    {
        if (switchWeaponAction != null)
        {
            switchWeaponAction.action.Enable();
            switchWeaponAction.action.performed += OnSwitchWeaponPerformed;
        }
    }

    private void OnDisable()
    {
        if (switchWeaponAction != null)
        {
            switchWeaponAction.action.performed -= OnSwitchWeaponPerformed;
        }
    }

    private void Update()
    {
        // キーボードの数字キーでの手動切り替えテスト（1キー: 武器1, 2キー: 武器2）
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchWeapon(WeaponSlot.Weapon1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchWeapon(WeaponSlot.Weapon2);
        }

        // アクティブな武器のWPゲージの変動を監視してUIに通知する例
        // （ResourceGauge側に値の変更検知や現在値プロパティがある場合のサンプル）
        // ★ 装備中の武器のWPを毎フレーム自動回復させる
        WeaponInstance active = ActiveWeapon;
        if (active != null && active.WPGauge != null)
        {
            float beforeValue = active.WPGauge.CurrentValue;
            
            // 武器ごとの回復処理を呼ぶ
            active.UpdateWP(Time.deltaTime, wpRecoveryRate);

            // 値が変動していたらUIに通知する
            if (!Mathf.Approximately(beforeValue, active.WPGauge.CurrentValue))
            {
                NotifyWPChanged(active.WPGauge.CurrentValue, active.WPGauge.MaxStat.Value);
            }
        }
    }

    private void InitializeWeapons()
    {
        // EntityStats 側の最大WPプロパティ名に合わせる
        // ※もし EntityStats 側に maxWp がなく maxWP や wpMax の場合はここを書き換えてください
        Stat maxWpStat = playerStats != null ? playerStats.maxWp : new Stat(100f);

        if (weaponData1 != null)
            weaponSlots[WeaponSlot.Weapon1] = new WeaponInstance(weaponData1, maxWpStat);
        if (weaponData2 != null)
            weaponSlots[WeaponSlot.Weapon2] = new WeaponInstance(weaponData2, maxWpStat);

        Debug.Log($"[WeaponManager] 武器の初期化完了。現在の装備: {ActiveWeapon?.Data.weaponName ?? "なし"}");
    }

    private void OnSwitchWeaponPerformed(InputAction.CallbackContext context)
    {
        WeaponSlot nextSlot = (CurrentSlot == WeaponSlot.Weapon1) ? WeaponSlot.Weapon2 : WeaponSlot.Weapon1;
        SwitchWeapon(nextSlot);
    }

    public void SwitchWeapon(WeaponSlot slot)
    {
        if (weaponSlots.ContainsKey(slot))
        {
            CurrentSlot = slot;
            Debug.Log($"[WeaponManager] 武器を切り替えました: {ActiveWeapon?.Data.weaponName}");
            OnWeaponSwitched?.Invoke(ActiveWeapon);
        }
        else
        {
            Debug.LogWarning($"[WeaponManager] 指定されたスロット {slot} に武器が存在しません。");
        }
    }

    public void NotifyWPChanged(float currentWP, float maxWP)
    {
        OnActiveWeaponWPChanged?.Invoke(currentWP, maxWP);
    }

    public bool TryUseSkill(SkillData skill)
    {
        WeaponInstance active = ActiveWeapon;
        if (active == null || skill == null) return false;

        // スキル側に設定されているコスト（例: skill.wpCost など）を取得して判定
        float cost = skill.wpCost; // SkillDataにwpCostプロパティがある想定

        if (active.ConsumeWP(cost))
        {
            // 消費成功した場合にスキル実行を許可
            if (active.WPGauge != null)
            {
                NotifyWPChanged(active.WPGauge.CurrentValue, active.WPGauge.MaxStat.Value);
            }
            return true;
        }

        return false; // WP不足で発動失敗
    }

    private void CheckAndInvokeWPChange()
    {
        WeaponInstance active = ActiveWeapon;
        if (active != null && active.WPGauge != null)
        {
            // ResourceGauge の仕様に合わせて現在値と最大値をイベントで通知
            OnActiveWeaponWPChanged?.Invoke(active.WPGauge.CurrentValue, active.WPGauge.MaxStat.Value);
        }
    }
}