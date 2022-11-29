using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] GameObject[] craftingItems;
  
    public void UpdateData(Item newItem)
    {
        if (ItemManager.Instance == null)
        {
            return;
        }
        foreach(GameObject obj in craftingItems)
        {
            obj.SetActive(false);
        }

        int itemsToCheck = newItem.craftingCostsNames.Length;
        craftingItems[0].GetComponent<Image>().sprite = ItemManager.Instance.GetItemVisual(newItem.key);
        craftingItems[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        craftingItems[0].SetActive(true);

        for (int i = 0; i < newItem.craftingCostsNames.Length; i++)
        {
            craftingItems[i+1].GetComponent<Image>().sprite = ItemManager.Instance.GetItemVisual(newItem.craftingCostsNames[i]);
            craftingItems[i+1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newItem.craftingCostsValue[i].ToString();
            craftingItems[i+1].SetActive(true);
        }
    }
}
