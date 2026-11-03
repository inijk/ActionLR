using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponSlot { Wp1, Wp2, Wp3 }

    [Header("― WP回復設定 ―")]
    [SerializeField] private float wpRecoveryRate = 10f;

    [Header("― 武器スロット・データ (3つ) ―")]
    [SerializeField] private WeaponData weaponData1;
    [SerializeField] private WeaponData weaponData2;
    [SerializeField] private WeaponData weaponData3;
    [SerializeField] private EntityStats playerStats;

    private Dictionary<WeaponSlot, WeaponInstance> weaponSlots = new Dictionary<WeaponSlot, WeaponInstance>();
    public WeaponSlot CurrentSlot { get; private set; } = WeaponSlot.Wp1;

    public WeaponInstance ActiveWeapon => weaponSlots.ContainsKey(CurrentSlot) ? weaponSlots[CurrentSlot] : null;

    // --- イベント定義 ---
    public event Action<WeaponInstance> OnWeaponSwitched;
    // 現在のアクティブインデックス（0, 1, 2）と全WP情報を通知するイベント
    public event Action<int, List<WeaponInstance>> OnWPStateChanged;

    [Header("― 武器切替用入力アクション ―")]
    [SerializeField] private InputActionReference switchWeaponAction;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<EntityStats>() ?? GetComponentInParent<EntityStats>();
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
        // キーボードでの手動切り替え（1, 2, 3キー）
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchWeapon(WeaponSlot.Wp1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchWeapon(WeaponSlot.Wp2);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchWeapon(WeaponSlot.Wp3);
        }

        // ★ すべての武器スロット（Wp1, Wp2, Wp3）を毎フレーム自動回復・更新する
        bool isUpdated = false;
        foreach (var kvp in weaponSlots)
        {
            WeaponInstance weapon = kvp.Value;
            if (weapon != null && weapon.WPGauge != null)
            {
                // 毎フレーム各武器のWPを更新
                weapon.UpdateWP(Time.deltaTime, wpRecoveryRate);
                isUpdated = true;
            }
        }

        // 変動があればUI側へ状態変化を通知
        if (isUpdated)
        {
            NotifyStateChanged();
        }
    }

    private void InitializeWeapons()
    {
        Stat maxWpStat = playerStats != null ? playerStats.maxWp : new Stat(100f);

        if (weaponData1 != null) weaponSlots[WeaponSlot.Wp1] = new WeaponInstance(weaponData1, maxWpStat);
        if (weaponData2 != null) weaponSlots[WeaponSlot.Wp2] = new WeaponInstance(weaponData2, maxWpStat);
        if (weaponData3 != null) weaponSlots[WeaponSlot.Wp3] = new WeaponInstance(weaponData3, maxWpStat);

        CurrentSlot = weaponSlots.ContainsKey(WeaponSlot.Wp1) ? WeaponSlot.Wp1 : WeaponSlot.Wp2;
        NotifyStateChanged();
    }

    private void OnSwitchWeaponPerformed(InputAction.CallbackContext context)
    {
        // 順番にトグル切り替え（Wp1 -> Wp2 -> Wp3 -> Wp1）
        if (CurrentSlot == WeaponSlot.Wp1) SwitchWeapon(WeaponSlot.Wp2);
        else if (CurrentSlot == WeaponSlot.Wp2) SwitchWeapon(WeaponSlot.Wp3);
        else SwitchWeapon(WeaponSlot.Wp1);
    }

    public void SwitchWeapon(WeaponSlot slot)
    {
        if (weaponSlots.ContainsKey(slot))
        {
            CurrentSlot = slot;
            OnWeaponSwitched?.Invoke(ActiveWeapon);
            NotifyStateChanged();
        }
    }

    public void NotifyStateChanged()
    {
        List<WeaponInstance> instances = new List<WeaponInstance>
        {
            weaponSlots.ContainsKey(WeaponSlot.Wp1) ? weaponSlots[WeaponSlot.Wp1] : null,
            weaponSlots.ContainsKey(WeaponSlot.Wp2) ? weaponSlots[WeaponSlot.Wp2] : null,
            weaponSlots.ContainsKey(WeaponSlot.Wp3) ? weaponSlots[WeaponSlot.Wp3] : null
        };
        int activeIndex = (int)CurrentSlot;
        OnWPStateChanged?.Invoke(activeIndex, instances);
    }

    public bool TryUseSkill(SkillData skill)
    {
        WeaponInstance active = ActiveWeapon;
        if (active == null || skill == null) return false;

        if (active.ConsumeWP(skill.wpCost))
        {
            NotifyStateChanged();
            return true;
        }
        return false;
    }
}