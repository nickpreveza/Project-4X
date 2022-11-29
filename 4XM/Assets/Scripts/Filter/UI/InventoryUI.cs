using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryUI : UIPanel
{
    [SerializeField] GameObject inventoryItemsParent;
    [SerializeField] GameObject inventoryRecipiesParent;
    [SerializeField] GameObject inventoryWeaponParent;
    [Header("Prefabs to spawn")]
    [SerializeField] GameObject inventoryItemPrefab;
    [SerializeField] GameObject weaponItemPrefab;
    [SerializeField] GameObject recipeItemPrefab;
    [SerializeField] GameObject statItemPrefab;
    [SerializeField] TabGroup tabGroup;
    ItemUI selectedItem;
    List<ItemUI> weaponsInTab = new List<ItemUI>();

    void CreateUIItem(Item newItem)
    {
        GameObject go = Instantiate(inventoryItemPrefab);
        go.transform.SetParent(inventoryItemsParent.transform, false);
        go.GetComponent<ItemUI>().UpdateData(newItem, this.gameObject, false, true);
    }

    public void UpdateValues()
    {
        foreach(Transform child in inventoryItemsParent.transform)
        {
            if (child != null)
            child.GetComponent<ItemUI>().UpdateAmount();
        }
    }

    public void UpdateInventory()
    {
        for (int i = 0; i < ItemManager.Instance.playerInventory.Count; i++)
        {
            Item newItem = ItemManager.Instance.itemDatabase[ItemManager.Instance.playerInventory.ElementAt(i).Key].item;

            if (newItem.hideFromInventory)
            {
                continue;
            }

            if (i - ItemManager.Instance.removeFromInventoryCount >= inventoryItemsParent.transform.childCount)
            {
                CreateUIItem(newItem);
                continue;
            }
            else
            {
                GameObject itemUI = inventoryItemsParent.transform.GetChild(i - ItemManager.Instance.removeFromInventoryCount).gameObject;
                inventoryItemsParent.transform.GetChild(i - ItemManager.Instance.removeFromInventoryCount).GetComponent<ItemUI>().UpdateData(newItem, this.gameObject, false, true);
            }
        }

        //Remove risidual itemUI objects
        if (inventoryItemsParent.transform.childCount > ItemManager.Instance.playerInventory.Count - (ItemManager.Instance.removeFromInventoryCount))
        {
            int leftToRemove = Mathf.Abs(inventoryItemsParent.transform.childCount - (ItemManager.Instance.playerInventory.Count - ItemManager.Instance.removeFromInventoryCount));
            while (leftToRemove > 0)
            {
                Destroy(inventoryItemsParent.transform.GetChild(inventoryItemsParent.transform.childCount-1).gameObject);
                leftToRemove--;
            }
        }
    }

    public void UpdateRecipies()
    {
        foreach (Transform child in inventoryRecipiesParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemScriptable scriptable in ItemManager.Instance.factoryItems)
        {
            if (scriptable.item.isUnlocked)
            {
                GameObject go = Instantiate(recipeItemPrefab);
                go.transform.SetParent(inventoryRecipiesParent.transform, false);
                RecipeUI recipeUI = go.GetComponent<RecipeUI>();
                recipeUI.UpdateData(scriptable.item);
            }
            else
            {
                continue;
            }
            
        }

    }

    public void UpdateWeapons()
    {
        weaponsInTab.Clear();
        foreach (Transform child in inventoryWeaponParent.transform)
        {
            Destroy(child.gameObject);
        } 

        foreach (ItemScriptable scriptable in ItemManager.Instance.weapons)
        {
            GameObject go = Instantiate(weaponItemPrefab);
            go.transform.SetParent(inventoryWeaponParent.transform, false);
            go.GetComponent<ItemUI>().UpdateData(scriptable.item, this.gameObject, false, false);
            weaponsInTab.Add(go.GetComponent<ItemUI>());

            /*
            if (scriptable.item.isUnlocked)
            {
                GameObject go = Instantiate(weaponItemPrefab);
                go.transform.SetParent(inventoryWeaponParent.transform, false);
                go.GetComponent<ItemUI>().UpdateData(scriptable.item, this.gameObject, false, false);
                weaponsInTab.Add(go.GetComponent<ItemUI>());
            }
            else
            {
                continue;
            }*/
        }


        foreach(ItemUI item in weaponsInTab)
        {
            item.OnDeselect();
            if (item.data.key == HB_PlayerController.Instance.data.weaponKey)
            {
                item.SetSelectedInWeaponsTab();
            }
        }
    }

    public void SelectedWeapon(ItemUI newItem)
    {
        if (selectedItem != null)
        {
            selectedItem.OnDeselect();
        }

        selectedItem = newItem;
        selectedItem.OnSelect();
    }

    public override void Activate()
    {
        base.Activate();
        tabGroup.SetDefault();
    }

    public override void Disable()
    {
        base.Disable();
    }
}
