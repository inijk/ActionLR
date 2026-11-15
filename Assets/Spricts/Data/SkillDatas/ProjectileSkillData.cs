using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Projectile_", menuName = "Combat/Skills/Projectile Skill")]
public class ProjectileSkillData : SkillData
{
    [Header("― 弾丸スキル固有パラメータ ―")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float rangeLifetime = 3f;

    [Header("― 貫通設定 ―")]
    public bool isPierce = false;  // 敵を貫通するかどうか
    public int maxPierceCount = 3; // 敵の最大貫通回数

    public override void Execute(Transform parentTransform, Vector2 aimDirection, EntityStats userStats)
    {
        if (bulletPrefab == null || parentTransform == null) return;

        // parentTransform (AttackPoint) の位置・回転をそのまま適用して生成
        GameObject bulletObj = Instantiate(
            bulletPrefab, 
            parentTransform.position, 
            parentTransform.rotation
        );

        // parentTransform (AttackPoint) の「正面（up）」方向に弾速を与える
        Vector2 flyDirection = parentTransform.up; 

        if (bulletObj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = flyDirection * bulletSpeed;
        }

        // 弾にパラメータを渡す（※Hitbox側をブレイク値対応にする場合は breakValue も渡せます）
        if (bulletObj.TryGetComponent<ProjectileBullet>(out var bullet))
        {
            bullet.Setup(userStats, damageMultiplier, rangeLifetime, isPierce, maxPierceCount, breakValue);
        }
    }
}