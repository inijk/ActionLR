using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityStats))]
public class PlayerMovement : MonoBehaviour
{
    [Header("通常移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Blink（瞬間回避）設定")]
    [SerializeField] private float blinkSpeed = 18f;          // Blink中の速度
    [SerializeField] private float blinkDuration = 0.15f;       // Blink本体（無敵等）の持続時間
    //[SerializeField] private float blinkInterval = 0.10f;      // Blink後のインターバル（硬直/受付時間）
    [SerializeField] private float blinkStaminaCost = 25f;     // Blink発動時の消費スタミナ

    [Header("長押しダッシュ設定")]
    [SerializeField] private float dashSpeedMultiplier = 1.4f; // 通常速度に対するダッシュ倍率
    [SerializeField] private float dashStaminaCostPerSec = 10f; // ダッシュ中の毎秒消費スタミナ

    private Rigidbody2D rb;
    private EntityStats playerStats;
    private PlayerAimController aimController;

    private Vector2 moveInput;
    private Vector2 blinkDirection;

    // フラグ・タイマー管理
    [SerializeField] private bool isDashButtonPressed;   // ダッシュボタンが現在押されているか
    [SerializeField] private bool isBlinking;            // Blink（高速移動）中か
    [SerializeField] private bool isInBlinkInterval;     // Blink後のインターバル中か
    [SerializeField] private bool isContinuousDashing;   // 長押しダッシュ状態か

    private float blinkTimer;
    private float intervalTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<EntityStats>();
        aimController = GetComponent<PlayerAimController>();
    }

    /// <summary>
    /// WASD/スティック移動入力
    /// </summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    /// <summary>
    /// Dash / Blink ボタン入力（Input System）
    /// </summary>
    public void OnDash_Blink(InputValue value)
    {
        // 現在のボタンの押しっぱなし状態を保持
        isDashButtonPressed = value.isPressed;

        // ボタンが押された瞬間 ＆ アクション可能な状態（通常時）であればBlink発動
        if (isDashButtonPressed && !isBlinking && !isInBlinkInterval && !isContinuousDashing)
        {
            TryPerformBlink();
        }

        // ボタンが離されたらダッシュ状態を即座に解除
        if (!isDashButtonPressed)
        {
            isContinuousDashing = false;
        }
    }

    // -------------------------------------------------------------
    // 【修正箇所 1】Blink（ブリンク）の移動方向取得
    // -------------------------------------------------------------
    private void TryPerformBlink()
    {
        // 1. Blink用のスタミナを消費できるかチェック
        if (playerStats.ST.Consume(blinkStaminaCost))
        {
            // 2. Blink開始処理
            isBlinking = true;
            blinkTimer = blinkDuration;

            // ★修正: 8方向変換(Get8WayDirection)を使わず、360度アナログ入力をそのまま使用
            if (moveInput != Vector2.zero)
            {
                // スティックが倒されている方向へ（長さ1に正規化）
                blinkDirection = moveInput.normalized;
            }
            else
            {
                // 入力が無ければ自機の正面（transform.up）へ
                blinkDirection = transform.up;
            }
        }
        else
        {
            Debug.Log("スタミナが足りません！");
        }
    }

    // ※ 不要になった `Get8WayDirection` メソッドは削除またはコメントアウトしてOKです。

        // -------------------------------------------------------------
    // 【確認】FixedUpdate での通常移動・ダッシュ処理
    // -------------------------------------------------------------
    private void FixedUpdate()
    {
        // 1. Blink中の処理
        if (isBlinking)
        {
            rb.linearVelocity = blinkDirection * blinkSpeed;
            
            blinkTimer -= Time.fixedDeltaTime;
            if (blinkTimer <= 0f)
            {
                isBlinking = false;
                if (isDashButtonPressed) isContinuousDashing = true;
            }
            return;
        }

        // 2. 長押しダッシュ中の処理
        if (isContinuousDashing)
        {
            if (playerStats.ST.Consume(dashStaminaCostPerSec * Time.fixedDeltaTime))
            {
                // ★ moveInput が 360度の方向をそのまま保持しているため、全方位に滑らかにダッシュします
                rb.linearVelocity = moveInput * (moveSpeed * dashSpeedMultiplier);
            }
            else
            {
                isContinuousDashing = false;
                rb.linearVelocity = moveInput * moveSpeed;
            }
            return;
        }

        // 3. 通常移動（★ 360度アナログ自由移動）
        rb.linearVelocity = moveInput * moveSpeed;
    }
    /// <summary>
    /// 入力ベクトルを8方向にスナップ（補正）するメソッド
    /// </summary>
    private Vector2 Get8WayDirection(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            return transform.up;
        }

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 45f) * 45f * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle)).normalized;
    }
}