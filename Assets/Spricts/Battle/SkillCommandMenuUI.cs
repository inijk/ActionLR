using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SkillCommandMenuUI : MonoBehaviour
{
    public enum WindowState { Closed, WeaponSkills, ItemSection }

    [Header("― UI参照 ―")]
    [SerializeField] private GameObject menuWindow;          
    [SerializeField] private GameObject monoFilterPanel;     
    [SerializeField] private TextMeshProUGUI weaponTitleText;
    [SerializeField] private Transform commandListContainer; 
    [SerializeField] private GameObject commandButtonPrefab; 
    
    [Header("― カーソル枠設定 ―")]
    [SerializeField] private RectTransform cursorFrame; // ボタンより一回り大きい長方形の枠UI
    [SerializeField] private Vector2 cursorPadding = new Vector2(16f, 12f); // ボタンサイズに対する余白（幅と高さのゆとり）

    [Header("― システム・入力参照 ―")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private InputActionReference fire1Action; 
    [SerializeField] private InputActionReference fire2Action; 

    private WindowState currentState = WindowState.Closed;
    private List<SkillCommandButton> spawnedButtons = new List<SkillCommandButton>();
    
    private SkillCommandButton selectedButton; // 現在カーソル枠がついているボタン
    private WeaponInstance targetWeaponForBind;

    public bool IsOpen => currentState != WindowState.Closed;

    private void OnEnable()
    {
        if (weaponManager != null) weaponManager.OnWeaponSwitched += OnWeaponSwitched;

        if (fire1Action != null && fire1Action.action != null)
        {
            fire1Action.action.Enable();
            fire1Action.action.performed += OnFire1ForBinding;
        }
        if (fire2Action != null && fire2Action.action != null)
        {
            fire2Action.action.Enable();
            fire2Action.action.performed += OnFire2ForBinding;
        }
    }

    private void OnDisable()
    {
        if (weaponManager != null) weaponManager.OnWeaponSwitched -= OnWeaponSwitched;

        if (fire1Action != null && fire1Action.action != null) fire1Action.action.performed -= OnFire1ForBinding;
        if (fire2Action != null && fire2Action.action != null) fire2Action.action.performed -= OnFire2ForBinding;
    }

    private void Start()
    {
        if (menuWindow != null) menuWindow.SetActive(false);
        if (monoFilterPanel != null) monoFilterPanel.SetActive(false);
        if (cursorFrame != null) cursorFrame.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen || currentState != WindowState.WeaponSkills) return;

        // 1. EventSystemから現在選択中（またはマウスオーバー中）のボタンを特定
        UpdateSelectedButton();

        // 2. カーソル枠の位置合わせと、ボタンの大きさに依存した幅・高さの自動追従
        UpdateCursorFrameTransform();
    }

    /// <summary>
    /// EventSystemのフォーカス状態から、現在選択されているボタンを特定する
    /// </summary>
    private void UpdateSelectedButton()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected != null)
        {
            SkillCommandButton btn = currentSelected.GetComponent<SkillCommandButton>();
            if (btn == null)
            {
                btn = currentSelected.GetComponentInParent<SkillCommandButton>();
            }

            if (btn != null && spawnedButtons.Contains(btn))
            {
                selectedButton = btn;
                return;
            }
        }

        // 何も選択されていない場合のフォールバック（一番上のボタンを選択状態にする）
        if (spawnedButtons.Count > 0 && selectedButton == null)
        {
            selectedButton = spawnedButtons[0];
            selectedButton.ButtonComponent?.Select();
        }
    }

    /// <summary>
    /// カーソル枠の座標を選択中ボタンに合わせ、幅と高さをボタンのサイズ＋余白に依存させる
    /// </summary>
    private void UpdateCursorFrameTransform()
    {
        if (cursorFrame == null || selectedButton == null) return;

        if (!cursorFrame.gameObject.activeSelf) cursorFrame.gameObject.SetActive(true);

        RectTransform btnRect = selectedButton.GetComponent<RectTransform>();
        if (btnRect != null)
        {
            // 1. 座標をボタンに一致させる
            cursorFrame.position = btnRect.position;

            // 2. ボタンの大きさに依存させ、一回り大きい長方形サイズにする（幅・高さを追従）
            Vector2 btnSize = btnRect.rect.size;
            cursorFrame.sizeDelta = new Vector2(btnSize.x + cursorPadding.x, btnSize.y + cursorPadding.y);
        }
    }

    public void ToggleWindowCycle()
    {
        switch (currentState)
        {
            case WindowState.Closed:
                currentState = WindowState.WeaponSkills;
                OpenWindow(WindowState.WeaponSkills);
                break;

            case WindowState.WeaponSkills:
                currentState = WindowState.ItemSection;
                OpenWindow(WindowState.ItemSection);
                break;

            case WindowState.ItemSection:
                CloseWindow();
                break;
        }
    }

    private void OpenWindow(WindowState state)
    {
        if (menuWindow != null) menuWindow.SetActive(true);
        if (monoFilterPanel != null) monoFilterPanel.SetActive(true);

        Time.timeScale = 0f; 

        if (state == WindowState.WeaponSkills)
        {
            RefreshWeaponSkillList();
        }
        else if (state == WindowState.ItemSection)
        {
            ClearCommandList();
            if (weaponTitleText != null) weaponTitleText.text = "― アイテム欄 ―";
        }
    }

    public void CloseWindow()
    {
        currentState = WindowState.Closed;
        if (menuWindow != null) menuWindow.SetActive(false);
        if (monoFilterPanel != null) monoFilterPanel.SetActive(false);
        if (cursorFrame != null) cursorFrame.gameObject.SetActive(false);

        Time.timeScale = 1f; 
        ClearCommandList();
    }

    private void OnWeaponSwitched(WeaponInstance newWeapon)
    {
        if (currentState == WindowState.WeaponSkills)
        {
            RefreshWeaponSkillList();
        }
    }

    public void RefreshWeaponSkillList()
    {
        ClearCommandList();

        if (weaponManager == null || commandButtonPrefab == null || commandListContainer == null) return;

        WeaponInstance activeWeapon = weaponManager.ActiveWeapon;
        if (activeWeapon == null || activeWeapon.Data == null)
        {
            if (weaponTitleText != null) weaponTitleText.text = "装備なし";
            return;
        }

        targetWeaponForBind = activeWeapon;

        if (weaponTitleText != null)
        {
            string f1 = activeWeapon.Data.fire1Skill != null ? activeWeapon.Data.fire1Skill.skillName : "未設定";
            string f2 = activeWeapon.Data.fire2Skill != null ? activeWeapon.Data.fire2Skill.skillName : "未設定";
            weaponTitleText.text = $"<size=20><i><u>{activeWeapon.Data.weaponName}</u></i></size>\nL: {f1} \n R: {f2}";
        }

        SkillCommandButton firstButton = null;

        foreach (var skill in activeWeapon.Data.settableSkills)
        {
            if (skill == null) continue;

            GameObject btnObj = Instantiate(commandButtonPrefab, commandListContainer);
            if (btnObj.TryGetComponent<SkillCommandButton>(out var commandBtn))
            {
                commandBtn.SetupSkill(skill);
                spawnedButtons.Add(commandBtn);

                if (firstButton == null) firstButton = commandBtn;
            }
        }

        // ウィンドウを開いた時に一番上のボタンを自動選択（強調色＆カーソル枠を表示）する
        if (firstButton != null)
        {
            selectedButton = firstButton;
            firstButton.ButtonComponent?.Select();
        }
    }

    private void OnFire1ForBinding(InputAction.CallbackContext context)
    {
        if (IsOpen && currentState == WindowState.WeaponSkills && selectedButton != null)
        {
            BindSkillToSlot(1, selectedButton.AssignedSkill);
        }
    }

    private void OnFire2ForBinding(InputAction.CallbackContext context)
    {
        if (IsOpen && currentState == WindowState.WeaponSkills && selectedButton != null)
        {
            BindSkillToSlot(2, selectedButton.AssignedSkill);
        }
    }

    private void BindSkillToSlot(int slotType, SkillData skillToBind)
    {
        if (targetWeaponForBind == null || targetWeaponForBind.Data == null || skillToBind == null) return;

        if (!targetWeaponForBind.Data.settableSkills.Contains(skillToBind)) return;

        if (slotType == 1)
        {
            targetWeaponForBind.Data.fire1Skill = skillToBind;
            Debug.Log($"[SkillBind] Fire1 に [{skillToBind.skillName}] を登録しました");
        }
        else if (slotType == 2)
        {
            targetWeaponForBind.Data.fire2Skill = skillToBind;
            Debug.Log($"[SkillBind] Fire2 に [{skillToBind.skillName}] を登録しました");
        }

        CloseWindow();
    }

    private void ClearCommandList()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null && btn.gameObject != null) Destroy(btn.gameObject);
        }
        spawnedButtons.Clear();
        selectedButton = null;
        if (cursorFrame != null) cursorFrame.gameObject.SetActive(false);
    }
}