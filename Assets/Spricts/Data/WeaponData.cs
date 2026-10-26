using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_", menuName = "Combat/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("― 武器基本情報 ―")]
    public string weaponName;
    [TextArea] public string description;
    public Sprite weaponIcon;

    [Header("― ショートカット割り当てスキル ―")]
    public SkillData fire1Skill;
    public SkillData fire2Skill;

    [Header("― 所持・アンロック済み全スキル一覧 ―")]
    public List<SkillData> settableSkills = new List<SkillData>();

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(weaponName))
        {
            weaponName = name;
        }
    }
}