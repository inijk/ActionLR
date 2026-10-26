using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("― 行動設定 (ScriptableObject) ―")]
    [SerializeField] private EnemyMovementPattern movementPattern;

    [Header("― パラメータ ―")]
    [SerializeField] private float changeInterval = 2.0f; // 方向を変える間隔（秒）
    [SerializeField] private float moveSpeed = 3.0f;       // 移動速度

    private Rigidbody2D rb;
    private EntityStats stats;
    private Vector2 currentMoveVector = Vector2.zero;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EntityStats>(); // ステータス管理コンポーネントがあれば取得[cite: 2]
    }

    private void Update()
    {
        if (movementPattern != null)
        {
            // ScriptableObject に移動方向の計算を委託
            currentMoveVector = movementPattern.GetNextDirection(transform, stats, ref timer, changeInterval);
        }
    }

    private void FixedUpdate()
    {
        // 物理更新タイミングで移動を反映[cite: 3]
        Vector2 targetPosition = rb.position + currentMoveVector * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }
}