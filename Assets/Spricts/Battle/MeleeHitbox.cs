using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.2f; // 斬撃の持持続時間
    private EntityStats attackerStats;
    private float damageMultiplier = 1.0f; // ダメージ倍率を追加

    // ★ 引数を2つ（EntityStats と ダメージ倍率）受け取れるように変更
    public void Setup(EntityStats stats, float multiplier = 1.0f)
    {
        attackerStats = stats;
        damageMultiplier = multiplier;
        Destroy(gameObject, lifeTime); // 指定時間後に消滅
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EntityStats>(out var targetStats))
        {
            // 攻撃者のATKにダメージ倍率（damageMultiplier）を乗算
            float attackerAtk = attackerStats != null ? attackerStats.Attack.Value : 10f;
            float totalAtk = attackerAtk * damageMultiplier;
            
            float targetDef = targetStats.Defense.Value;
            float finalDamage = Mathf.Max(1f, totalAtk - targetDef);

            targetStats.HP.Consume(finalDamage);
            Debug.Log($"{other.name} に {finalDamage} ダメージを与えた！");
        }
    }
}