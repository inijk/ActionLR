using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileBullet : MonoBehaviour
{
    [Header("― 地形レイヤー設定 ―")]
    [SerializeField] private LayerMask obstacleLayer; // 壁や障害物の LayerMask (Inspectorで設定)

    private EntityStats attackerStats;
    private float damageMultiplier = 1.0f;

    private bool isPierce = false;
    private int remainingPierceCount = 0;

    // 同一敵への重複ヒット防止用
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    public void Setup(EntityStats stats, float multiplier, float lifetime, bool pierce, int maxPierce)
    {
        attackerStats = stats;
        damageMultiplier = multiplier;
        isPierce = pierce;
        remainingPierceCount = maxPierce;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 重複チェック
        if (hitTargets.Contains(other)) return;

        // 1. 地形・壁判定（LayerMask または Tag でチェック）
        // 接触相手のレイヤーが obstacleLayer に含まれるかチェック
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0 || other.CompareTag("Wall"))
        {
            // 地形に当たった場合は貫通数に関わらず即削除
            Destroy(gameObject);
            return;
        }

        // 2. 敵（EntityStats を持つオブジェクト）への判定
        if (other.TryGetComponent<EntityStats>(out var targetStats))
        {
            hitTargets.Add(other);

            // ダメージ処理
            float attackerAtk = attackerStats != null ? attackerStats.Attack.Value : 10f;
            float totalAtk = attackerAtk * damageMultiplier;
            float targetDef = targetStats.Defense.Value;
            float finalDamage = Mathf.Max(1f, totalAtk - targetDef);

            targetStats.HP.Consume(finalDamage);
            Debug.Log($"[弾ヒット] {other.name} に {finalDamage} ダメージ！");

            // 敵に対する貫通処理
            if (isPierce)
            {
                remainingPierceCount--;
                if (remainingPierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}