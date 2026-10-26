using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Buff_", menuName = "Combat/Skills/Buff Skill")]
public class BuffSkillData : SkillData
{
    [Header("― ステータス変化固有パラメータ ―")]
    public StatusEffectData effectData; // 付与するバフ/デバフデータ
    public int effectLevel = 1;
    public float duration = 5f;
    public bool targetSelf = true; // trueなら自分に付与、falseなら狙い方向の敵に付与

    public override void Execute(Transform userTransform, Vector2 aimDirection, EntityStats userStats)
    {
        if (targetSelf)
        {
            // 自分にバフを付与
            if (userTransform.TryGetComponent<StatusEffectManager>(out var effectManager))
            {
                effectManager.ApplyEffect(effectData, effectLevel, duration);
            }
        }
    }
}