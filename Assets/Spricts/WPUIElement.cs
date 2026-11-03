using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WPUIElement : MonoBehaviour
{
    [Header("― コンポーネント ―")]
    [SerializeField] private RectTransform rectTransform; // 太さ（高さ）変更用
    [SerializeField] private Image wpFillImage;           // ゲージのImage (Filled)
    [SerializeField] private TextMeshProUGUI wpText;      // ゲージ内の数値 (例: 50/100)

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// ゲージと数値の更新
    /// </summary>
    public void UpdateDisplay(float current, float max)
    {
        if (wpFillImage != null)
        {
            wpFillImage.fillAmount = max > 0 ? current / max : 0f;
        }

        if (wpText != null)
        {
            wpText.text = $"{current:F0} / {max:F0}";
        }
    }

    /// <summary>
    /// ゲージの太さ（高さ）を動的に変更する
    /// </summary>
    public void SetHeight(float height)
    {
        if (rectTransform != null)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.y = height;
            rectTransform.sizeDelta = size;
        }
    }

    /// <summary>
    /// メイン（アクティブ）とサブ（非アクティブ）で見た目を切り替える
    /// </summary>
    public void SetHighlight(bool isActive)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        
        // メインはくっきり、サブは少し透過させるなどの演出
        group.alpha = isActive ? 1.0f : 0.5f;
    }
}