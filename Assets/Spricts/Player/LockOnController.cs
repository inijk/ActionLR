using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnController : MonoBehaviour
{
    [Header("ロックオン設定")]
    [SerializeField] private InputActionReference lockOnAction; // ロックオンキーのアクション参照
    [SerializeField] private LayerMask enemyLayer;              // 敵のレイヤー
    [SerializeField] private float maxLockOnDistance = 15f;     // レイの最大飛距離

    private Transform lockedTarget;
    private EntityStats lockedTargetStats;

    public Transform LockedTarget => lockedTarget;
    public EntityStats LockedTargetStats => lockedTargetStats;
    public bool IsLockedOn => lockedTarget != null;

    private void OnEnable()
    {
        if (lockOnAction != null && lockOnAction.action != null)
        {
            lockOnAction.action.Enable();
            lockOnAction.action.performed += OnLockOnPerformed;
        }
    }

    private void OnDisable()
    {
        if (lockOnAction != null && lockOnAction.action != null)
        {
            lockOnAction.action.performed -= OnLockOnPerformed;
            lockOnAction.action.Disable();
        }
    }

    private void Update()
    {
        ValidateLockedTarget();
    }

    /// <summary>
    /// ロックオンキー押下時の処理
    /// </summary>
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        // PlayerAimController から現在のカーソル位置を取得してレイ判定を行う
        if (!TryGetComponent<PlayerAimController>(out var aimController) || aimController.AimCursorTransform == null)
        {
            return;
        }

        Transform detectedEnemy = DetectEnemyAlongRay(aimController.AimCursorTransform.position);

        // 狙ったところに敵がいる場合はその敵をロックオン（別ターゲットへの切り替えも対応）
        if (detectedEnemy != null)
        {
            // すでに同じ敵をロックオン中の場合は解除、別の敵なら切り替え
            if (lockedTarget == detectedEnemy)
            {
                ClearLockOnTarget();
            }
            else
            {
                SetLockOnTarget(detectedEnemy);
            }
        }
        // 狙ったところに敵がいない場合はロックオンオブジェクトを空（解除）にする
        else
        {
            ClearLockOnTarget();
        }
    }

    /// <summary>
    /// プレイヤー座標から指定位置（カーソル）へレイを飛ばし、触れた敵を返す
    /// </summary>
    private Transform DetectEnemyAlongRay(Vector3 cursorWorldPos)
    {
        Vector2 origin = transform.position;
        Vector2 targetPos = cursorWorldPos;
        Vector2 toCursor = targetPos - origin;

        float distanceToCursor = toCursor.magnitude;
        if (distanceToCursor < 0.01f) return null;

        float rayDistance = Mathf.Min(distanceToCursor, maxLockOnDistance);
        Vector2 direction = toCursor.normalized;

        Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.0f);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, enemyLayer);
        return hit.collider != null ? hit.collider.transform : null;
    }

    /// <summary>
    /// ロックオン対象の登録と名前・HPの出力
    /// </summary>
    public void SetLockOnTarget(Transform target)
    {
        lockedTarget = target;
        lockedTargetStats = target.GetComponent<EntityStats>();

        if (lockedTargetStats != null)
        {
            float currentHp = lockedTargetStats.HP.CurrentValue;
            float maxHp = lockedTargetStats.MaxHP.Value;
            Debug.Log($"[LockOn] ロックオン: {lockedTarget.name} | HP: {currentHp:F0} / {maxHp:F0}");
        }
        else
        {
            Debug.Log($"[LockOn] ロックオン: {lockedTarget.name} (EntityStatsなし)");
        }
    }

    /// <summary>
    /// ロックオン対象を空にする
    /// </summary>
    public void ClearLockOnTarget()
    {
        if (lockedTarget != null)
        {
            Debug.Log($"[LockOn] 解除: {lockedTarget.name}");
        }
        lockedTarget = null;
        lockedTargetStats = null;
    }

    /// <summary>
    /// ロックオン中の敵が倒された・消滅した場合の自動クリア
    /// </summary>
    private void ValidateLockedTarget()
    {
        if (lockedTarget == null) return;

        if (!lockedTarget.gameObject.activeInHierarchy ||
            (lockedTargetStats != null && lockedTargetStats.HP.CurrentValue <= 0f))
        {
            ClearLockOnTarget();
        }
    }
}