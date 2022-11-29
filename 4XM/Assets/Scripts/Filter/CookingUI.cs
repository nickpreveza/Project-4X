using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookingUI : UIPanel
{
    [SerializeField] GameObject upgradesParent;
    [SerializeField] SkillUpgradableUI[] skillUpgrades;

    public override void Activate()
    {
        base.Activate();
        UpdateSkills();
    }

    public override void Disable()
    {
        base.Disable();
    }

    public void UpdateSkills()
    {
        skillUpgrades[0].UpdateSkillTreeData(gameObject, HB_PlayerController.Instance.data.MaxHealth, false, 0);
        skillUpgrades[1].UpdateSkillTreeData(gameObject, HB_PlayerController.Instance.data.Damage, false, 1);
        skillUpgrades[2].UpdateSkillTreeData(gameObject, HB_PlayerController.Instance.data.Speed, false, 2);
        skillUpgrades[3].UpdateSkillTreeData(gameObject, HB_PlayerController.Instance.data.Range, false, 3);
    }

    public void CloseCooking()
    {
        HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.cooking, false);
    }
}
