using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

public class SkillCommandButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI wpCostText;
    [SerializeField] private Button button;

    private SkillData assignedSkill;
    public SkillData AssignedSkill => assignedSkill;
    public Button ButtonComponent => button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void SetupSkill(SkillData skillData)
    {
        assignedSkill = skillData;
        
        if (skillNameText != null) skillNameText.text = skillData.skillName;
        if (wpCostText != null) wpCostText.text = $"WP: {skillData.wpCost}";
        
        if (skillIcon != null)
        {
            skillIcon.gameObject.SetActive(skillData.icon != null);
            if (skillData.icon != null) skillIcon.sprite = skillData.icon;
        }
    }

    /// <summary>
    /// マウスカーソルがボタン領域に入った瞬間、自動で選択状態（強調色）にする
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            button.Select();
        }
    }
}