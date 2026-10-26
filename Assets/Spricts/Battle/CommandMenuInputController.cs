using UnityEngine;
using UnityEngine.InputSystem;

public class CommandMenuInputController : MonoBehaviour
{
    [Header("― 参照 ―")]
    [SerializeField] private SkillCommandMenuUI commandMenuUI;

    [Header("― 入力アクション ―")]
    [SerializeField] private InputActionReference toggleMenuAction; // New Input System用

    private void OnEnable()
    {
        if (toggleMenuAction != null && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggleMenuPerformed;
            //Debug.Log("[CommandMenuInput] toggleMenuAction を有効化しました。");
        }
        else
        {
           // Debug.LogWarning("[CommandMenuInput] toggleMenuAction が未アサインです。Tabキーによるフォールバック入力を使用します。");
        }
    }

    private void OnDisable()
    {
        if (toggleMenuAction != null && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.performed -= OnToggleMenuPerformed;
        }
    }

    private void Update()
    {
        /*// InputActionReference が未登録の場合でも Tab キーで開閉可能にするフォールバック処理
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Debug.Log("[CommandMenuInput] Tabキー入力検知（フォールバック）");
            TriggerToggle();
        }*/
    }

    private void OnToggleMenuPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log("[CommandMenuInput] InputAction によるトグル入力検知");
        TriggerToggle();
    }

    private void TriggerToggle()
    {
        if (commandMenuUI != null)
        {
            commandMenuUI.ToggleWindowCycle();
        }
        else
        {
            //Debug.LogError("[CommandMenuInput] SkillCommandMenuUI がアサインされていません！");
        }
    }
}