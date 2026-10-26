using UnityEngine;

public enum EffectType
{
    // --- 状態異常 (Status Effects) ---
    Burn,      // 炎上 (スリップダメージ)
    Wet,       // 浸水
    Freeze,    // 氷結
    Paralysis, // 麻痺
    Vulnerable, // 脆弱

    // --- 強化・弱化 (Buffs / Debuffs) ---
    AtkBoost,  // 攻撃力変化
    DefBoost,  // 防御力変化
    VitBoost   // VIT変化
}

public enum EffectCategory
{
    StatusEffect, // 状態異常
    Buff,         // 強化
    Debuff        // 弱化
}

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Stats/Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    public string effectName;
    public EffectType effectType;
    public EffectCategory category;
    public Sprite icon; // UI表示用アイコン

    [Header("― レベル設定 ―")]
    public int maxLevel = 5; // Lv.I ~ V (1 ~ 5)

    [Header("― レベルごとの効果量 (配列要素0=Lv1, 4=Lv5) ―")]
    // 例: バフならステータス上昇量、炎上なら毎秒ダメージ量
    public float[] valuePerLevel = new float[5] { 5f, 10f, 15f, 20f, 25f };
}