using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileBullet : MonoBehaviour
{
    [Header("― 地形レイヤー設定 ―")]
    [SerializeField] private LayerMask obstacleLayer; // 壁や障害物の LayerMask (Inspectorで設定)

    private EntityStats attackerStats;
    private float damageMultiplier = 1.0f;
    private float breakValue = 0f; // 与えるブレイク値

    private bool isPierce = false;
    private int remainingPierceCount = 0;

    // 同一敵への重複ヒット防止用
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    // ★ 引数に breakVal（与えるブレイク値）を追加
    public void Setup(EntityStats stats, float multiplier, float lifetime, bool pierce, int maxPierce, float breakVal = 0f)
    {
        attackerStats = stats;
        damageMultiplier = multiplier;
        isPierce = pierce;
        remainingPierceCount = maxPierce;
        breakValue = breakVal;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 重複チェック
        if (hitTargets.Contains(other)) return;

        // 1. 地形・壁判定（LayerMask または Tag でチェック）
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0 || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // 2. 敵（EntityStats を持つオブジェクト）への判定
        if (other.TryGetComponent<EntityStats>(out var targetStats))
        {
            hitTargets.Add(other);

            // 基礎ダメージ計算
            float attackerAtk = attackerStats != null ? attackerStats.Attack.Value : 10f;
            float totalAtk = attackerAtk * damageMultiplier;
            float targetDef = targetStats.Defense.Value;
            float baseDamage = Mathf.Max(1f, totalAtk - targetDef);

            // ブレイク倍率の取得
            float breakDamageMult = 1.0f;
            if (other.TryGetComponent<BreakController>(out var breakController))
            {
                breakDamageMult = breakController.GetDamageMultiplier();
            }

            // 最終ダメージ（基礎ダメージ × 現在のブレイク％）
            float finalDamage = baseDamage * breakDamageMult;
            targetStats.HP.Consume(finalDamage);
            Debug.Log($"[弾ヒット] {other.name} に {finalDamage:F1} ダメージ！");

            // ブレイク値の蓄積
            if (breakController != null && breakValue > 0f)
            {
                breakController.AddBreakValue(breakValue);
            }

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