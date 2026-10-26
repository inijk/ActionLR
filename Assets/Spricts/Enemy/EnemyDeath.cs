
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    private EntityStats stats;
    private Animator animator; // 将来のアニメーション用
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// HPが0以下になった際の死亡ハンドラー
    /// </summary>
    private void HandleDeath()
    {
        // 1. 当たり判定やAIの物理挙動を無効化（死体が攻撃を受けたり引っかかったりするのを防ぐ）
        if (enemyCollider != null) enemyCollider.enabled = false;
        if (rb != null) rb.simulated = false;

        // 2. 部屋の敵全滅チェック用にマネージャーへ通知[cite: 5]
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnEnemyDefeated(); // 敵部屋クリア判定の呼び出し[cite: 5]
        }

        // 3. 将来的な死亡アニメーション分岐
        if (animator != null && HasParameter(animator, "IsDead"))
        {
            // 将来: アニメーション再生（例: animator.SetTrigger("Die");）
            // アニメーションイベント（Animation Event）経由で DestroyEnemy() を呼ぶか、指定秒数後に削除
            animator.SetBool("IsDead", true);
            Invoke(nameof(DestroyEnemy), 1.0f); // アニメーション時間に合わせた遅延削除
        }
        else
        {
            // 現段階（アニメーション未セット）: 直ちにクローンを破棄
            DestroyEnemy();
        }
    }

    /// <summary>
    /// 敵オブジェクト（クローン）の完全削除
    /// </summary>
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    // Animatorに指定パラメーターが存在するか確認するヘルパー
    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}