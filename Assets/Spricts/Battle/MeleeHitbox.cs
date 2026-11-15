using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.2f; // 斬撃の持続時間
    private EntityStats attackerStats;
    private float damageMultiplier = 1.0f; // ダメージ倍率
    private float breakValue = 0f;         // 与えるブレイク値

    // 同一対象への重複ヒット防止用
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    // ★ 引数に breakVal（与えるブレイク値）を追加
    public void Setup(EntityStats stats, float multiplier = 1.0f, float breakVal = 0f)
    {
        attackerStats = stats;
        damageMultiplier = multiplier;
        breakValue = breakVal;
        Destroy(gameObject, lifeTime); // 指定時間後に消滅
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitTargets.Contains(other)) return;

        if (other.TryGetComponent<EntityStats>(out var targetStats))
        {
            hitTargets.Add(other);

            // 攻撃者のATKにダメージ倍率（damageMultiplier）を乗算
            float attackerAtk = attackerStats != null ? attackerStats.Attack.Value : 10f;
            float totalAtk = attackerAtk * damageMultiplier;

            float targetDef = targetStats.Defense.Value;
            float baseDamage = Mathf.Max(1f, totalAtk - targetDef);

            // ブレイク倍率の取得（BreakController がアタッチされている場合）
            float breakDamageMult = 1.0f;
            if (other.TryGetComponent<BreakController>(out var breakController))
            {
                breakDamageMult = breakController.GetDamageMultiplier();
            }

            // 最終ダメージ（基礎ダメージ × 現在のブレイク％）
            float finalDamage = baseDamage * breakDamageMult;
            targetStats.HP.Consume(finalDamage);
            Debug.Log($"{other.name} に {finalDamage:F1} ダメージを与えた！");

            // ブレイク値の蓄積
            if (breakController != null && breakValue > 0f)
            {
                breakController.AddBreakValue(breakValue);
            }
        }
    }
}