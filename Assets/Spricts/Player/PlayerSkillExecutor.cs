using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillExecutor : MonoBehaviour
{
    [Header("― 参照 ―")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private SkillCommandMenuUI commandMenuUI;

    [Header("― 入力アクション ―")]
    [SerializeField] private InputActionReference fire1Action;
    [SerializeField] private InputActionReference fire2Action;

    private void OnEnable()
    {
        if (fire1Action != null) { fire1Action.action.Enable(); }
        if (fire2Action != null) { fire2Action.action.Enable(); }
    }

    private void OnDisable()
    {
        if (fire1Action != null) { fire1Action.action.Disable(); }
        if (fire2Action != null) { fire2Action.action.Disable(); }
    }

    private void Update()
    {
        // 1. ウィンドウが開いているかどうかの詳細チェック
        bool isMenuOpen = (commandMenuUI != null && commandMenuUI.IsOpen);
        
        // 2. Fire1 の押下検知
        if (fire1Action != null && fire1Action.action.WasPressedThisFrame())
        {
            Debug.Log($"[Debug-Input] Fire1 が押されました。ウィンドウオープン状態: {isMenuOpen}");

            if (isMenuOpen)
            {
                // ウィンドウが開いているときは SkillCommandMenuUI 側でバインド処理が行われるためここでは処理しない
                return;
            }

            ExecuteFireSlot(1);
        }

        // 3. Fire2 の押下検知
        if (fire2Action != null && fire2Action.action.WasPressedThisFrame())
        {
            Debug.Log($"[Debug-Input] Fire2 が押されました。ウィンドウオープン状態: {isMenuOpen}");

            if (isMenuOpen)
            {
                return;
            }

            ExecuteFireSlot(2);
        }
    }

    private void ExecuteFireSlot(int slotIndex)
    {
        if (weaponManager == null)
        {
            Debug.LogError("[Debug-Error] WeaponManager がアサインされていません！");
            return;
        }

        WeaponInstance activeWeapon = weaponManager.ActiveWeapon;
        if (activeWeapon == null || activeWeapon.Data == null)
        {
            Debug.LogWarning("[Debug-Warning] 現在アクティブな武器が存在しません。");
            return;
        }

        SkillData targetSkill = (slotIndex == 1) ? activeWeapon.Fire1Skill : activeWeapon.Fire2Skill;

        if (targetSkill == null)
        {
            Debug.LogWarning($"[Debug-Warning] 武器「{activeWeapon.Data.weaponName}」の Fire{slotIndex}Skill が未設定です。");
            return;
        }

        Debug.Log($"[Debug-Action] 武器「{activeWeapon.Data.weaponName}」の Fire{slotIndex} スキル [{targetSkill.skillName}] を実行します。");
        ExecuteSpecificSkill(targetSkill);
    }

    private void ExecuteSpecificSkill(SkillData skill)
    {
        if (weaponManager.TryUseSkill(skill))
        {
            Transform spawnParent = attackPoint != null ? attackPoint : transform;
            Vector2 aimDirection = attackPoint != null ? (Vector2)attackPoint.up : (Vector2)transform.right;
            EntityStats playerStats = GetComponent<EntityStats>();

            skill.Execute(spawnParent, aimDirection, playerStats);
        }
        else
        {
            Debug.LogWarning($"[Debug-Warning] スキル {skill.skillName} の実行条件（WP等）を満たしていません。");
        }
    }
}