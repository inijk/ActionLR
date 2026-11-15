using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("― プレハブ・生成先Canvas設定 ―")]
    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private Canvas targetCanvas; // ★ ポップアップを生成する親Canvas

    [Header("― ランダム出現位置の設定（ワールド距離） ―")]
    [SerializeField] private float minSpawnRadius = 0.4f; // エンティティ中心からの最小距離
    [SerializeField] private float maxSpawnRadius = 0.9f; // エンティティ中心からの最大距離

    [Header("― 敵・味方のカラー設定 ―")]
    [SerializeField] private Color enemyDamageColor = Color.white;                 // 敵が受けたダメージの色
    [SerializeField] private Color playerDamageColor = new Color(1f, 0.3f, 0.3f);  // 味方(Player)が受けたダメージの色

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // もし Inspector で Canvas が未設定なら、自分自身の親またはシーン内から自動取得
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();
            if (targetCanvas == null)
            {
                targetCanvas = FindFirstObjectByType<Canvas>();
            }
        }
    }

    /// <summary>
    /// ダメージと倍率のポップアップを Canvas の子要素として生成する
    /// </summary>
    public void ShowDamage(float damage, Vector3 entityPosition, float breakMultiplier, bool isPlayerTarget)
    {
        if (popupPrefab == null) return;

        // 120% のようなパーセント表記(10以上)で渡された場合は 1.2 の倍率表記に自動補正
        float displayMultiplier = breakMultiplier >= 10f ? breakMultiplier / 100f : breakMultiplier;

        // エンティティの座標から少し離れたランダムなワールド位置を算出
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        if (randomDir == Vector2.zero) randomDir = Vector2.up;
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnWorldPos = entityPosition + (Vector3)(randomDir * distance);

        // 敵・味方で色を使い分ける
        Color targetColor = isPlayerTarget ? playerDamageColor : enemyDamageColor;

        // ★ Canvas の子要素としてインスタンス化（第2引数に targetCanvas.transform を指定）
        Transform parentTransform = targetCanvas != null ? targetCanvas.transform : null;
        DamagePopup popup = Instantiate(popupPrefab, parentTransform);
        
        // スケールが崩れないようにローカルスケールを1にリセット
        popup.transform.localScale = Vector3.one;

        // 初期座標とCanvas情報を渡してセットアップ
        popup.Setup(damage, displayMultiplier, targetColor, spawnWorldPos, targetCanvas);
    }
}