using UnityEngine;
using UnityEngine.InputSystem; // 追加

public class DebugEnemyHPViewer : MonoBehaviour
{
    [Header("― デバッグ表示設定 ―")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector2 guiOffset = new Vector2(15f, -15f);

    private Camera mainCamera;
    private EntityStats hoverTargetStats;
    private string hoverTargetName;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        DetectEnemyUnderMouse();
    }

    /// <summary>
    /// マウスカーソル直下にある敵を Raycast で検知する
    /// </summary>
    private void DetectEnemyUnderMouse()
    {
        if (Mouse.current == null) return; // マウスが存在しない場合の安全対策

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // 【修正】 Old Input の Input.mousePosition から New Input System の取得方法へ変更
        Vector2 mousePosInput = Mouse.current.position.ReadValue();

        // マウスの画面座標をワールド座標に変換
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePosInput);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // レイキャストを実行
        RaycastHit2D hit;
        if (enemyLayer != 0)
        {
            hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, enemyLayer);
        }
        else
        {
            hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        }

        // コライダーに当たっており、EntityStats を持っているか確認
        if (hit.collider != null && hit.collider.TryGetComponent<EntityStats>(out var stats))
        {
            hoverTargetStats = stats;
            hoverTargetName = hit.collider.gameObject.name;
        }
        else
        {
            hoverTargetStats = null;
        }
    }

    /// <summary>
    /// Unityのデバッグ用OnGUI描画処理
    /// </summary>
    private void OnGUI()
    {
        if (hoverTargetStats == null || Mouse.current == null) return;

        // 現在のHPと最大HPを取得
        float currentHP = hoverTargetStats.HP.CurrentValue;
        float maxHP = hoverTargetStats.MaxHP.Value;

        // 表示文字列の作成
        string displayText = $"{hoverTargetName}\nHP: {currentHP:F0} / {maxHP:F0}";

        // 【修正】 Input.mousePosition から Mouse.current.position.ReadValue() に変更
        Vector2 mousePos = Mouse.current.position.ReadValue();
        float guiX = mousePos.x + guiOffset.x;
        float guiY = Screen.height - mousePos.y + guiOffset.y;

        // スタイル設定
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleLeft;

        // ラベル描画
        Vector2 size = style.CalcSize(new GUIContent(displayText));
        GUI.Box(new Rect(guiX, guiY, size.x + 12f, size.y + 8f), displayText, style);
    }
}