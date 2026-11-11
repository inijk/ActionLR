using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuUI : MonoBehaviour
{
    public enum MenuTabType { Status, Equipment, Item, Options }

    [Header("― UI全体参照 ―")]
    [SerializeField] private GameObject pauseMenuPanel;      // ポーズメニュー全体の親パネル
    [SerializeField] private GameObject monoFilterPanel;     // 背景の暗転用パネル
    [SerializeField] private PlayerActionController playerActionController; // プレイヤーの行動制御コンポーネント

    [Header("― 各タブのコンテンツパネル (GameObject) ―")]
    [SerializeField] private GameObject statusContentPanel;
    [SerializeField] private GameObject equipmentContentPanel;
    [SerializeField] private GameObject itemContentPanel;
    [SerializeField] private GameObject optionsContentPanel;

    [Header("― 入力アクション ―")]
    [SerializeField] private InputActionReference menuAction; // Player Input [Menu] アクション

    private Dictionary<MenuTabType, GameObject> tabPanels = new Dictionary<MenuTabType, GameObject>();
    private MenuTabType currentTabType = MenuTabType.Status;

    public bool IsOpen => pauseMenuPanel != null && pauseMenuPanel.activeSelf;

    private void OnEnable()
    {
        if (menuAction != null && menuAction.action != null)
        {
            menuAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (menuAction != null && menuAction.action != null)
        {
            menuAction.action.Disable();
        }
    }

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (monoFilterPanel != null) monoFilterPanel.SetActive(false);

        // タブパネルの辞書登録
        if (statusContentPanel != null) tabPanels[MenuTabType.Status] = statusContentPanel;
        if (equipmentContentPanel != null) tabPanels[MenuTabType.Equipment] = equipmentContentPanel;
        if (itemContentPanel != null) tabPanels[MenuTabType.Item] = itemContentPanel;
        if (optionsContentPanel != null) tabPanels[MenuTabType.Options] = optionsContentPanel;

        // 初回はすべてのタブパネルを非表示に
        foreach (var panel in tabPanels.Values)
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    private void Update()
    {
        // [Menu] 入力を監視（タイムスケール 0 でも取得できるよう WasPressedThisFrame を使用）
        if (menuAction != null && menuAction.action.WasPressedThisFrame())
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (IsOpen)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (monoFilterPanel != null) monoFilterPanel.SetActive(true);

        Time.timeScale = 0f;

        // ★ メニューを開いた瞬間にプレイヤーの移動や攻撃を完全にロックする
        if (playerActionController != null)
        {
            playerActionController.SetActionLock(true);
        }

        // デフォルトでステータスタブを表示
        SwitchTab(MenuTabType.Status);
    }

    public void ClosePauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (monoFilterPanel != null) monoFilterPanel.SetActive(false);

        Time.timeScale = 1f;

        // ★ メニューを閉じたらプレイヤーの行動ロックを解除する
        if (playerActionController != null)
        {
            playerActionController.SetActionLock(false);
        }

        foreach (var panel in tabPanels.Values)
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    public void SwitchTabByIndex(int tabIndex)
    {
        SwitchTab((MenuTabType)tabIndex);
    }

    public void SwitchTab(MenuTabType targetType)
    {
        currentTabType = targetType;

        foreach (var pair in tabPanels)
        {
            if (pair.Value != null)
            {
                bool isActive = (pair.Key == targetType);
                pair.Value.SetActive(isActive);

                // ステータスタブに切り替わった際、アタッチされている StatusTab を取得して明示的に更新をかける
                if (isActive && targetType == MenuTabType.Status)
                {
                    StatusTab statusTab = pair.Value.GetComponent<StatusTab>();
                    if (statusTab != null)
                    {
                        statusTab.RefreshTabContent();
                    }
                }
            }
        }
    }
}