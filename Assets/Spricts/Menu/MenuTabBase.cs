using UnityEngine;

public abstract class MenuTabBase : MonoBehaviour
{
    [Header("タブ共通設定")]
    [SerializeField] private GameObject contentPanel; // このタブのルートパネル

    public virtual void Initialize()
    {
        // 初期化処理（必要に応じて）
    }

    /// <summary>
    /// タブが開かれたときに呼ばれる
    /// </summary>
    public virtual void OnTabOpen()
    {
        if (contentPanel != null) contentPanel.SetActive(true);
        RefreshTabContent();
        Debug.Log($"[MenuTabBase] タブが開かれました: {gameObject.name}");
    }

    /// <summary>
    /// タブが閉じられた（非表示になった）ときに呼ばれる
    /// </summary>
    public virtual void OnTabClose()
    {
        if (contentPanel != null) contentPanel.SetActive(false);
    }

    /// <summary>
    /// 表示内容やデータの更新
    /// </summary>
    public abstract void RefreshTabContent();
}