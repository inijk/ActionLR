using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    public enum AimControlMode
    {
        Gamepad,
        Mouse
    }

    [Header("操作モード設定")]
    [SerializeField] private AimControlMode currentControlMode = AimControlMode.Mouse;

    [Header("コンポーネント参照")]
    [SerializeField] private LockOnController lockOnController; // ロックオン管理の参照
    [SerializeField] private Transform aimCursorTransform;        // 照準用空オブジェクト
    [SerializeField] private Transform cameraTarget;              // Cinemachineが追従する空のTarget
    [SerializeField] private Transform aimPointer;                // プレイヤーの周りを回る指示スプライト

    [Header("エイム＆カメラ設定")]
    [SerializeField] private float orbitRadius = 1.5f;             // 指示スプライトの周回半径
    [SerializeField] private float cameraOffsetDistance = 3.0f;    // カメラをずらす最大距離
    [SerializeField] private float cameraLerpSpeed = 8.0f;         // カメラターゲットの移動補正速度
    [SerializeField] private float cursorDeadzoneDistance = 1.5f;  // カメラが動き始める不感帯距離
    [SerializeField] private float cursorMaxFocusDistance = 6.0f;  // カメラシフトが最大に達する距離

    [Header("ゲームパッド用カーソル設定")]
    [SerializeField] private float gamepadCursorRadius = 5.0f;     // スティック最大倒し時のカーソル移動半径 r
    [SerializeField] private float gamepadStickDeadzone = 0.1f;    // スティックの遊び（不感帯）
    [SerializeField] private float gamepadCursorLerpSpeed = 20f;   // カーソルが戻る・動く滑らかさ

    private Camera mainCamera;
    private Vector2 rawStickInput;
    private Vector2 currentAimDirection = Vector2.up;
    private float currentCameraOffsetFactor = 0f;

    public AimControlMode CurrentControlMode => currentControlMode;
    public Vector2 CurrentAimDirection => currentAimDirection;
    public Transform AimCursorTransform => aimCursorTransform;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (lockOnController == null)
        {
            lockOnController = GetComponent<LockOnController>();
        }
    }

    private void Update()
    {
        UpdateCursorPosition();
        UpdateAimDirectionAndCameraFactor();
        UpdatePointerTransform();
        UpdateCameraTargetPosition();
    }

    public void OnLook(InputValue value)
    {
        rawStickInput = value.Get<Vector2>();
    }

    public void SwitchToGamepadMode() => currentControlMode = AimControlMode.Gamepad;
    public void SwitchToMouseMode() => currentControlMode = AimControlMode.Mouse;

    /// <summary>
    /// 1. 照準用空オブジェクトの位置を更新（ロックオン有無で挙動を切り替え）
    /// </summary>
    private void UpdateCursorPosition()
    {
        if (aimCursorTransform == null) return;

        // 【状態A：ロックオン中】ロックオンした敵の座標を常に追いかける
        if (lockOnController != null && lockOnController.IsLockedOn)
        {
            Vector3 targetPos = lockOnController.LockedTarget.position;
            targetPos.z = transform.position.z;
            aimCursorTransform.position = targetPos;
            return;
        }

        // 【状態B：非ロックオン中（ゲームパッド）】プレイヤー中心から半径 r 内で移動
        if (currentControlMode == AimControlMode.Gamepad)
        {
            Vector2 stickOffset = Vector2.zero;

            if (rawStickInput.magnitude > gamepadStickDeadzone)
            {
                Vector2 clampedStick = Vector2.ClampMagnitude(rawStickInput, 1.0f);
                stickOffset = clampedStick * gamepadCursorRadius;
            }

            Vector3 targetWorldPos = transform.position + (Vector3)stickOffset;
            targetWorldPos.z = transform.position.z;

            aimCursorTransform.position = Vector3.Lerp(
                aimCursorTransform.position,
                targetWorldPos,
                Time.deltaTime * gamepadCursorLerpSpeed
            );
            return;
        }

        // 【状態C：非ロックオン中（マウス）】マウスポインタにピッタリ追従
        if (currentControlMode == AimControlMode.Mouse && Mouse.current != null && mainCamera != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            float zDepth = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, zDepth));
            worldPos.z = transform.position.z;

            aimCursorTransform.position = worldPos;
        }
    }

    /// <summary>
    /// 2. プレイヤーから「照準用空オブジェクト」への方向とカメラシフト割合を計算
    /// </summary>
    private void UpdateAimDirectionAndCameraFactor()
    {
        if (aimCursorTransform == null) return;

        Vector2 toCursor = (Vector2)(aimCursorTransform.position - transform.position);
        float distance = toCursor.magnitude;

        if (distance > 0.05f)
        {
            currentAimDirection = toCursor.normalized;
        }

        if (distance <= cursorDeadzoneDistance)
        {
            currentCameraOffsetFactor = 0f;
        }
        else
        {
            float range = cursorMaxFocusDistance - cursorDeadzoneDistance;
            if (range > 0.0001f)
            {
                currentCameraOffsetFactor = Mathf.Clamp01((distance - cursorDeadzoneDistance) / range);
            }
        }
    }

    /// <summary>
    /// 3. 衛星インジケーターの位置と向きを更新
    /// </summary>
    private void UpdatePointerTransform()
    {
        if (aimPointer == null) return;

        aimPointer.position = transform.position + (Vector3)(currentAimDirection * orbitRadius);

        float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg - 90f;
        aimPointer.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 4. Cinemachine追従用の CameraTarget を空オブジェクト方向へずらす
    /// </summary>
    private void UpdateCameraTargetPosition()
    {
        if (cameraTarget == null) return;

        Vector3 targetLocalPos = (Vector3)(currentAimDirection * (cameraOffsetDistance * currentCameraOffsetFactor));

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            targetLocalPos,
            Time.deltaTime * cameraLerpSpeed
        );
    }
}