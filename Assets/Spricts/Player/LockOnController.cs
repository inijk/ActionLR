using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnController : MonoBehaviour
{
    [Header("ロックオン設定")]
    [SerializeField] private InputActionReference lockOnAction; // ロックオンキーのアクション参照
    [SerializeField] private LayerMask enemyLayer;              // 敵のレイヤー
    [SerializeField] private float maxLockOnDistance = 15f;     // レイの最大飛距離

    [Header("視覚フィードバック参照")]
    [SerializeField] private LockOnIndicatorUI lockOnIndicator; // マーカー追尾UI（またはスプライト）

    private PlayerAimController aimController;
    private Transform lockedTarget;
    private EntityStats lockedTargetStats;
    private Transform hoveredTarget; // ロックオン前にレイが触れている対象

    public Transform LockedTarget => lockedTarget;
    public EntityStats LockedTargetStats => lockedTargetStats;
    public Transform HoveredTarget => hoveredTarget;
    public bool IsLockedOn => lockedTarget != null;

    private BreakController lockedTargetBreak;
    public BreakController LockedTargetBreak => lockedTargetBreak;

    // UI連携用イベント（ロックオン対象が変更・解除された瞬間に通知）
    public event System.Action<Transform, EntityStats, BreakController> OnLockOnTargetChanged;

    private void Awake()
    {
        aimController = GetComponent<PlayerAimController>();
    }

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
        UpdateHoveredTarget();
        UpdateIndicatorVisuals();
    }

    /// <summary>
    /// ロックオンしていない間、常にレイを飛ばして触れている対象（候補）を更新
    /// </summary>
    private void UpdateHoveredTarget()
    {
        if (IsLockedOn)
        {
            hoveredTarget = null;
            return;
        }

        if (aimController != null && aimController.AimCursorTransform != null)
        {
            hoveredTarget = DetectEnemyAlongRay(aimController.AimCursorTransform.position);
        }
        else
        {
            hoveredTarget = null;
        }
    }

    /// <summary>
    /// マーカーUIへ「追尾対象」と「ロックオン確定状態か」を伝える
    /// </summary>
    private void UpdateIndicatorVisuals()
    {
        if (lockOnIndicator == null) return;

        if (IsLockedOn)
        {
            // ロックオン確定対象を追尾（強調表示）
            lockOnIndicator.SetTarget(lockedTarget, isLocked: true);
        }
        else if (hoveredTarget != null)
        {
            // ロックオン前だがレイが当たっている対象を追尾（候補表示）
            lockOnIndicator.SetTarget(hoveredTarget, isLocked: false);
        }
        else
        {
            // どちらもない場合は非表示
            lockOnIndicator.Hide();
        }
    }

    /// <summary>
    /// ロックオンキー押下時の処理
    /// </summary>
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        if (aimController == null || aimController.AimCursorTransform == null) return;

        // すでにロックオン中の場合：キーを押したら解除
        if (IsLockedOn)
        {
            ClearLockOnTarget();
            return;
        }

        // 未ロックオン時：レイが触れている対象がいればロックオン登録
        Transform detectedEnemy = DetectEnemyAlongRay(aimController.AimCursorTransform.position);
        if (detectedEnemy != null)
        {
            SetLockOnTarget(detectedEnemy);
        }
        else
        {
            ClearLockOnTarget();
        }
    }

    /// <summary>
    /// プレイヤー座標からカーソル方向へ最大距離（maxLockOnDistance）までレイを伸ばし、触れた敵を返す
    /// </summary>
    private Transform DetectEnemyAlongRay(Vector3 cursorWorldPos)
    {
        Vector2 origin = transform.position;
        Vector2 toCursor = (Vector2)cursorWorldPos - origin;

        // カーソルが自機と完全に重なっている（方向が出せない）場合は判定しない
        if (toCursor.sqrMagnitude < 0.001f) return null;

        // カーソルまでの距離で止めず、その方向へ常に maxLockOnDistance まで伸ばす
        Vector2 direction = toCursor.normalized;

        // デバッグ確認用（Sceneビューに赤い線で射程を表示）
        Debug.DrawRay(origin, direction * maxLockOnDistance, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxLockOnDistance, enemyLayer);
        return hit.collider != null ? hit.collider.transform : null;
    }

    public void SetLockOnTarget(Transform target)
    {
        lockedTarget = target;
        lockedTargetStats = target.GetComponent<EntityStats>();
        lockedTargetBreak = target.GetComponent<BreakController>();

        if (lockedTargetStats != null)
        {
            float currentHp = lockedTargetStats.HP.CurrentValue;
            float maxHp = lockedTargetStats.MaxHP.Value;
            //Debug.Log($"[LockOn] ロックオン: {lockedTarget.name} | HP: {currentHp:F0} / {maxHp:F0}");
        }
        else
        {
            //Debug.Log($"[LockOn] ロックオン: {lockedTarget.name}");
        }

        OnLockOnTargetChanged?.Invoke(lockedTarget, lockedTargetStats, lockedTargetBreak);
    }

    public void ClearLockOnTarget()
    {
        if (lockedTarget != null)
        {
            //Debug.Log($"[LockOn] 解除: {lockedTarget.name}");
        }
        lockedTarget = null;
        lockedTargetStats = null;
        lockedTargetBreak = null;

        OnLockOnTargetChanged?.Invoke(null, null, null);
    }

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