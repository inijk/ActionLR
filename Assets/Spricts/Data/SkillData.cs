using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    [Header("― スキル共通情報 ―")]
    public string skillName;
    [TextArea(2, 5)] public string description;
    public Sprite icon;
    public float wpCost = 3f;

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(name))
        {
            skillName = name;
        }
    }

    /// <summary>
    /// スキル発動処理
    /// </summary>
    /// <param name="parentTransform">生成元となる AttackPoint 等の Transform</param>
    /// <param name="aimDirection">狙い方向</param>
    /// <param name="userStats">発動者のステータス</param>
    public abstract void Execute(Transform parentTransform, Vector2 aimDirection, EntityStats userStats);
}