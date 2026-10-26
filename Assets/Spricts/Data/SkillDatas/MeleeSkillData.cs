using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Melee_", menuName = "Combat/Skills/Melee Skill")]
public class MeleeSkillData : SkillData
{
    [Header("― 近接攻撃固有パラメータ ―")]
    public GameObject meleeHitboxPrefab;
    public float damageMultiplier = 1.2f;

    public override void Execute(Transform parentTransform, Vector2 aimDirection, EntityStats userStats)
    {
        if (meleeHitboxPrefab == null || parentTransform == null) return;

        // parentTransform (AttackPoint) の位置・回転に合わせて、その子要素として生成[cite: 2, 3]
        GameObject hitboxObj = Instantiate(
            meleeHitboxPrefab, 
            parentTransform.position, 
            parentTransform.rotation, 
            parentTransform // 子要素化[cite: 2, 3]
        );

        if (hitboxObj.TryGetComponent<MeleeHitbox>(out var hitbox))
        {
            hitbox.Setup(userStats, damageMultiplier);
        }
    }
}