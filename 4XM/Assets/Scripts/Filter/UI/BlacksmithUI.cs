using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithUI : UIPanel
{
    [SerializeField] GameObject weaponItemParent;
    [SerializeField] GameObject upgradesParent;

    [SerializeField] ItemUI[] weaponItems;
    [SerializeField] SkillUpgradableUI[] weaponUpgrades;
    //[SerializeField] ItemUI weaponItemPrefab;

    [SerializeField] ItemUI selectedItem;

    bool selectedItemAffordable;
    WeaponScriptable targetWeapon;

    public override void Activate()
    {
        base.Activate();
        UpdateBlacksmithItems();
    }

    public override void Disable()
    {
        base.Disable();
    }

    void UpdateBlacksmithItems()
    {
        foreach(Transform child in weaponItemParent.transform)
        {
            child.gameObject.SetActive(false);
        }

        for(int i = 0; i < ItemManager.Instance.weapons.Length; i++)
        {
            weaponItemParent.transform.GetChild(i).gameObject.SetActive(true);
            weaponItemParent.transform.GetChild(i).GetComponent<ItemUI>().blacksmithVariant = true;
            weaponItemParent.transform.GetChild(i).GetComponent<ItemUI>().UpdateData(ItemManager.Instance.weapons[i].item, this.gameObject, true, false);
        }

        SelectItem(weaponItems[0]);
    }

    public void UpdateWeaponUpgrades()
    {
        if (targetWeapon == null)
        {
            return;
        }

        weaponUpgrades[0].UpdateBlackmsithData(gameObject, targetWeapon, targetWeapon.weaponData.Damage, false);
        weaponUpgrades[1].UpdateBlackmsithData(gameObject, targetWeapon, targetWeapon.weaponData.FireRate, false);
        weaponUpgrades[2].UpdateBlackmsithData(gameObject, targetWeapon, targetWeapon.weaponData.CriticalDamage, false);
        weaponUpgrades[3].UpdateBlackmsithData(gameObject, targetWeapon, targetWeapon.weaponData.CriticalChance, true);
    }

    public void SelectItem(ItemUI newItem)
    {
        if (selectedItem == newItem)
        {
            return;
        }

        selectedItem = newItem;
        targetWeapon = ItemManager.Instance.weaponDatabase[selectedItem.data.key];
        
        UpdateWeaponUpgrades();
    }

    public void CloseBlacksmith()
    {
        HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.blacksmith, false);
    }

}
