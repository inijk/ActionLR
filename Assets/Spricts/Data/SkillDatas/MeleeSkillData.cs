using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Melee_", menuName = "Combat/Skills/Melee Skill")]
public class MeleeSkillData : SkillData
{
    [Header("― 近接攻撃固有パラメータ ―")]
    public GameObject meleeHitboxPrefab;

    public override void Execute(Transform parentTransform, Vector2 aimDirection, EntityStats userStats)
    {
        if (meleeHitboxPrefab == null || parentTransform == null) return;

        // parentTransform (AttackPoint) の位置・回転に合わせて、その子要素として生成
        GameObject hitboxObj = Instantiate(
            meleeHitboxPrefab, 
            parentTransform.position, 
            parentTransform.rotation, 
            parentTransform // 子要素化
        );

        if (hitboxObj.TryGetComponent<MeleeHitbox>(out var hitbox))
        {
            // ※Hitbox側をブレイク値対応にする場合は breakValue も渡せます
            hitbox.Setup(userStats, damageMultiplier, breakValue);
        }
    }
}