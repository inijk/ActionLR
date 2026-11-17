using UnityEngine;

public class LockOnIndicatorUI : MonoBehaviour
{
    [Header("見た目・アニメーション設定")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.2f, 0.5f); // ホバー時：半透明の黄色
    [SerializeField] private Color lockedColor = new Color(1f, 0.15f, 0.15f, 1f); // ロックオン時：濃い赤色

    [Header("サイズ・回転演出")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float lockedScale = 1.0f;
    [SerializeField] private float rotateSpeed = 90f; // ロックオン時の回転速度（度/秒）

    private Transform currentTarget;
    private bool isLockedState;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 追尾する対象と状態（ホバー中か、ロックオン確定中か）を設定
    /// </summary>
    public void SetTarget(Transform target, bool isLocked)
    {
        if (target == null)
        {
            Hide();
            return;
        }

        currentTarget = target;
        isLockedState = isLocked;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // 状態に応じて色と大きさを切り替え
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isLocked ? lockedColor : hoverColor;
        }

        float targetScale = isLocked ? lockedScale : hoverScale;
        transform.localScale = Vector3.one * targetScale;
    }

    public void Hide()
    {
        currentTarget = null;
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (currentTarget == null)
        {
            Hide();
            return;
        }

        // 対象のワールド座標にピタッと追尾（LateUpdateで移動後の位置に合わせる）
        Vector3 targetPos = currentTarget.position;
        targetPos.z = transform.position.z;
        transform.position = targetPos;

        // ロックオン確定中はゆっくり回転させて「ロック中」であることを強調
        if (isLockedState)
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
}