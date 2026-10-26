using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    // 操作モードの定義
    public enum AimControlMode
    {
        Gamepad,
        Mouse
    }

    [Header("操作モード設定")]
    [SerializeField] private AimControlMode currentControlMode = AimControlMode.Mouse;

    [Header("コンポーネント参照")]
    [SerializeField] private Transform cameraTarget;      // Cinemachineが追従する空のTarget
    [SerializeField] private Transform aimPointer;       // プレイヤーの周りを回る指示スプライト

    [Header("エイム＆カメラ設定")]
    [SerializeField] private float orbitRadius = 1.5f;            // 三角形の周回半径
    [SerializeField] private float cameraOffsetDistance = 3.0f;        // カメラをずらす距離
    [SerializeField] private float cameraLerpSpeed = 8.0f;        // カメラターゲットの移動補正速度
    [SerializeField] private float mouseDeadzoneRatio = 0.15f;       // マウス用: 画面中央からの不感帯領域（0.0〜0.5）
    [SerializeField] private float mouseMaxRangeRatio = 0.40f;       // マウス用: 最大カメラ移動に達する領域（0.0〜0.5）

    private PlayerInput playerInput;
    private Camera mainCamera;
    
    private Vector2 rawAimInput;                       // スティック入力値
    private Vector2 currentAimDirection = Vector2.up; // 現在の狙い方向
    private float currentCameraOffsetFactor = 0f;     // カメラ移動の適用割合 (0.0 ～ 1.0)

    public AimControlMode CurrentControlMode => currentControlMode;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateAimDirectionAndCameraFactor();
        UpdatePointerTransform();
        UpdateCameraTargetPosition();
    }

    /// <summary>
    /// Input System: Look アクションの入力受取
    /// </summary>
    public void OnLook(InputValue value)
    {
        rawAimInput = value.Get<Vector2>();
    }

    /// <summary>
    /// UIボタン等から操作モードを変更するための公開メソッド
    /// </summary>
    public void SetControlMode(int modeIndex)
    {
        // 0: Gamepad, 1: Mouse (DropdownやButtonのOnClickから指定可能)
        currentControlMode = (AimControlMode)modeIndex;
    }

    public void SwitchToGamepadMode() => currentControlMode = AimControlMode.Gamepad;
    public void SwitchToMouseMode() => currentControlMode = AimControlMode.Mouse;

    /// <summary>
    /// 選択された操作モードに基づいてエイム＆カメラを計算
    /// </summary>
    private void UpdateAimDirectionAndCameraFactor()
    {
        if (mainCamera == null) return;

        // =================================================================
        // A. 【ゲームパッドモード】
        // =================================================================
        if (currentControlMode == AimControlMode.Gamepad)
        {
            if (rawAimInput.sqrMagnitude > 0.05f)
            {
                // スティックを倒している間：向きを更新し、傾きに応じてカメラをオフセット
                currentAimDirection = rawAimInput.normalized;
                currentCameraOffsetFactor = Mathf.Clamp01(rawAimInput.magnitude);
            }
            else
            {
                // スティックを離した時：向き(currentAimDirection)は維持し、カメラのみ中心に戻す
                currentCameraOffsetFactor = 0f;
            }
            return; // マウス処理へ落とさず確実に終了
        }

        // =================================================================
        // B. 【マウスモード】
        // =================================================================
        if (currentControlMode == AimControlMode.Mouse && Mouse.current != null)
        {
            // 1. マウス位置をビューポート座標（0.0 ~ 1.0）へ変換
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseViewportPos = mainCamera.ScreenToViewportPoint(mouseScreenPos);

            // 2. プレイヤー（画面中心）のビューポート座標を (0.5, 0.5) に固定
            Vector2 centerViewportPos = new Vector2(0.5f, 0.5f);

            // 3. 画面中心からの差分と距離（distance）を計算
            Vector2 diff = (Vector2)mouseViewportPos - centerViewportPos;
            float distance = diff.magnitude; // 画面中央からの距離（画面端で約0.5）

            // マウスの向き計算
            if (distance > 0.001f)
            {
                currentAimDirection = diff.normalized;
            }

            // 画面中央からの距離（しきい値）に応じたカメラオフセット計算
            if (distance <= mouseDeadzoneRatio)
            {
                currentCameraOffsetFactor = 0f;
            }
            else
            {
                float range = mouseMaxRangeRatio - mouseDeadzoneRatio;
                if (range > 0.0001f)
                {
                    currentCameraOffsetFactor = Mathf.Clamp01((distance - mouseDeadzoneRatio) / range);
                }
            }
}
    }

    private void UpdatePointerTransform()
    {
        if (aimPointer == null) return;

        Vector3 targetPosition = transform.position + (Vector3)(currentAimDirection * orbitRadius);
        aimPointer.position = targetPosition;

        float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg - 90f;
        aimPointer.rotation = Quaternion.Euler(0, 0, angle);
    }

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