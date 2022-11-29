using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactoryUI : UIPanel
{
    [SerializeField] GameObject itemParent;
    [SerializeField] GameObject costParent;
    [SerializeField] GameObject[] costPlacements;
    [SerializeField] Image selectedItemImage;
    [SerializeField] GameObject selectAnItemText;
    [SerializeField] ItemUI selectedItem;

    [SerializeField] Button craftButton;
    [SerializeField] GameObject factoryItemPrefab;
    bool selectedItemAffordable = false;

    [SerializeField] Color inactiveColor;
    private void Start()
    {
        GenerateFactoryItems();
    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Disable()
    {
        base.Disable();
    }

    public void GenerateFactoryItems()
    {
        foreach(Transform child in itemParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach(ItemScriptable scriptable in ItemManager.Instance.factoryItems)
        {
            GameObject go = Instantiate(factoryItemPrefab);
            go.transform.SetParent(itemParent.transform, false);
            ItemUI item = go.GetComponent<ItemUI>();
            item.UpdateData(scriptable.item, this.gameObject, true, false);
        }

        SetEmptySelection();
    }

    void SetEmptySelection()
    {
        selectAnItemText.SetActive(true);
        selectedItemImage.sprite = null;
        for (int i = 0; i < costPlacements.Length; i++)
        {
            costPlacements[i].SetActive(true);
            costPlacements[i].transform.GetChild(0).gameObject.SetActive(false);
            costPlacements[i].transform.GetChild(1).gameObject.SetActive(false);
            costPlacements[i].transform.GetComponent<Image>().color = inactiveColor;
        }
    }
    
    public void SelectItem(ItemUI newItem)
    {
        if (selectedItem == newItem)
        {
            return;
        }

        selectedItem = newItem;
        selectedItemImage.sprite = selectedItem.data.sprite;
        selectAnItemText.SetActive(false);
        SetUpCostsVisuals();
    }

    void SetUpCostsVisuals()
    {
        int itemsToCheck = selectedItem.data.craftingCostsNames.Length;
        for (int i = 0; i < costPlacements.Length; i++)
        {
            if (i < itemsToCheck)
            {
                costPlacements[i].SetActive(true);
                costPlacements[i].transform.GetChild(0).gameObject.SetActive(true);
                costPlacements[i].transform.GetChild(1).gameObject.SetActive(true);
                costPlacements[i].transform.GetComponent<Image>().color = Color.white;
            }
            else
            {
                costPlacements[i].transform.GetChild(0).gameObject.SetActive(false);
                costPlacements[i].transform.GetChild(1).gameObject.SetActive(false);
                costPlacements[i].transform.GetComponent<Image>().color = inactiveColor;
            }
        }

        for (int i = 0; i < selectedItem.data.craftingCostsNames.Length; i++)
        {
            costPlacements[i].transform.GetChild(0).GetComponent<Image>().sprite = ItemManager.Instance.GetItemVisual(selectedItem.data.craftingCostsNames[i]);
            costPlacements[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectedItem.data.craftingCostsValue[i].ToString();
        }
        selectedItemAffordable = CheckIfAffordable();

        if (selectedItemAffordable)
        {
            craftButton.interactable = true;
        }
        else
        {
            craftButton.interactable = false;
        }
    }

    void UpdateAvailableItems()
    {

    }

    bool CheckIfAffordable()
    {
        bool passed = true;
        for (int i = 0; i < selectedItem.data.craftingCostsNames.Length; i++)
        {
            bool checkItem = ItemManager.Instance.DoesPlayerHaveItem(selectedItem.data.craftingCostsNames[i], selectedItem.data.craftingCostsValue[i]);
          
            if (!checkItem)
            {
                passed = false;
                costPlacements[i].GetComponent<Image>().color = HB_UIManager.Instance.unaffordableColor;
            }
            else
            {
                passed = true;
                costPlacements[i].GetComponent<Image>().color = HB_UIManager.Instance.affordableColor;
            }
        }

        return passed;
       // return (ItemManager.Instance.DoesPlayerHaveItem()
      //bool check=   ItemManager.Instance.DoesPlayerHaveItem();
    }


    public void Craft()
    {
        if (selectedItem == null || !selectedItemAffordable)
        {
            return;
        }
        for (int i = 0; i < selectedItem.data.craftingCostsNames.Length; i++)
        {
            ItemManager.Instance.UseItem(selectedItem.data.craftingCostsNames[i], selectedItem.data.craftingCostsValue[i], false);
        }

        ItemManager.Instance.AddToInventory(selectedItem.data.key, 1, false);

        SetUpCostsVisuals();
    }

    public void CloseFactory()
    {
        HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.factory, false);
    }
}
