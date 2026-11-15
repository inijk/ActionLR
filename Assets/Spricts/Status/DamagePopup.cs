using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("― 参照 ―")]
    [SerializeField] private TMP_Text textMesh;

    [Header("― 表示・アニメーション設定 ―")]
    [SerializeField] private float lifeTime = 2.0f;       // 表示時間（約2秒）
    [SerializeField] private float floatUpSpeed = 0.8f;   // 上へ浮かぶ速度（ワールド空間基準）

    private float timer;
    private Color currentColor;

    // ワールド座標での現在位置と参照キャッシュ
    private Vector3 currentWorldPos;
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Camera mainCamera;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TMP_Text>();
        }
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// ダメージ数値、ブレイク倍率、表示色、初期ワールド座標、親Canvasをセットして初期化
    /// </summary>
    public void Setup(float damage, float breakMultiplier, Color color, Vector3 worldPos, Canvas canvas)
    {
        if (textMesh == null) return;

        parentCanvas = canvas;
        currentWorldPos = worldPos;

        // 小数点第1位まで倍率を表示（例: 121(x1.2)）
        int roundedDamage = Mathf.RoundToInt(damage);
        textMesh.text = $"{roundedDamage}<size=8>(x{breakMultiplier:F1}</size>)";

        currentColor = color;
        textMesh.color = currentColor;
        timer = 0f;

        // 生成直後に1回目の座標反映を行う
        UpdateCanvasPosition();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 1. ワールド空間ベースで少しずつ上に浮かび上がらせる
        currentWorldPos += Vector3.up * (floatUpSpeed * Time.deltaTime);
        UpdateCanvasPosition();

        // 2. 時間経過（2秒）に合わせてアルファ値を 1 -> 0 にフェードアウト
        if (textMesh != null)
        {
            float progress = Mathf.Clamp01(timer / lifeTime);
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = currentColor;
        }

        // 3. 寿命が尽きたら破棄
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ワールド座標（currentWorldPos）を Canvas 内の適切な座標に変換して適用する
    /// </summary>
    private void UpdateCanvasPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // UIではない通常の3D/2Dオブジェクトとして使われている場合、または WorldSpace Canvas の場合
        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.WorldSpace || rectTransform == null)
        {
            transform.position = currentWorldPos;
            return;
        }

        // Screen Space - Overlay の場合：スクリーン座標をそのまま RectTransform.position に代入
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            if (mainCamera != null)
            {
                rectTransform.position = mainCamera.WorldToScreenPoint(currentWorldPos);
            }
        }
        // Screen Space - Camera の場合：Canvas内のローカル座標に変換して代入
        else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera cam = parentCanvas.worldCamera != null ? parentCanvas.worldCamera : mainCamera;
            if (cam != null)
            {
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, currentWorldPos);
                RectTransform canvasRect = parentCanvas.transform as RectTransform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 localPos))
                {
                    rectTransform.anchoredPosition = localPos;
                }
            }
        }
    }
}