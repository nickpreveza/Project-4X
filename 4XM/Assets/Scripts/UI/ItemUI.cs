using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] bool debugInteract;
    public Item data;
    GameObject handler;
    Button button;
    [SerializeField] Image targetGraphic;
    [SerializeField] GameObject emptyGraphic;
    [SerializeField] TextMeshProUGUI targetAmountText;
    bool isInteractable;
    bool craftOnInteract;
    [HideInInspector] public bool blacksmithVariant;
    bool isHovering;

    private void Start()
    {

        if (debugInteract)
        {
            button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Interact());
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data.isUnlocked)
        {
            isHovering = true;
            StartCoroutine(WaitForTooltip());
        }
    }

    IEnumerator WaitForTooltip()
    {
        yield return new WaitForSeconds(0.5f);
        SI_UIManager.Instance.ShowTooltip(data.description, data.displayName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        SI_UIManager.Instance.HideTooltip();
    }

    public void UpdateData(GameObject _handler, bool showAmount)
    {
        button = GetComponent<Button>();
        handler = _handler;

        if (showAmount)
        {
           // targetAmountText.text = newItem.startingAmount.ToString();
        }
        else
        {
            targetAmountText.text = "";
        }

        button.onClick.RemoveAllListeners();
        isInteractable = false;
        button.interactable = false;
        emptyGraphic.SetActive(true);
    }
    public void UpdateData(Item newItem, GameObject _handler, bool _craftable, bool showAmount)
    {
        craftOnInteract = _craftable;
        button = GetComponent<Button>();
        handler = _handler;
        data = newItem;
        targetGraphic.sprite = newItem.sprite;
        targetGraphic.gameObject.SetActive(true);
        if (showAmount)
        {
            targetAmountText.text = newItem.startingAmount.ToString();
        }
        else
        {
            targetAmountText.text = "";
        }
       
        button.onClick.RemoveAllListeners();

        if (newItem == null)
        {
            isInteractable = false;
            button.interactable = false;
            return;
        }

        if (newItem.isUnlocked)
        {
            emptyGraphic.SetActive(false);
        }
        else
        {
            emptyGraphic.SetActive(true);
        }
       
        if (craftOnInteract && newItem.isUnlocked)
        {
            button.onClick.AddListener(() => SetSelected());
        }

        if (!craftOnInteract && newItem.isUnlocked)
        {
            switch (data.type)
            {
                case ItemType.Currency:
                case ItemType.Material:
                    isInteractable = false;
                    button.interactable = false;
                    break;
                case ItemType.Clothing:
                case ItemType.Placeable:
                case ItemType.Consumable:
                case ItemType.Weapon:
                    isInteractable = true;
                    button.interactable = true;
                    break;
            }
        }

        if (isInteractable && !craftOnInteract)
        {
            button.onClick.AddListener(() => Interact());
        } 
    }

    public void UpdateAmount()
    {
        if (!craftOnInteract && ItemManager.Instance.playerInventory.ContainsKey(data.key))
        {
            targetAmountText.text = ItemManager.Instance.playerInventory[data.key].ToString();
        }
        
    }

    public void Interact() //When is in inventory
    {
        switch (data.type)
        {
            case ItemType.Currency:
            case ItemType.Material:
                Debug.LogWarning("These items should not be interactable");
                isInteractable = false;
                button.interactable = false;
                break;
            case ItemType.Weapon:
                ItemManager.Instance.EquipItem(data.key);
                SetSelectedInWeaponsTab();
                break;
            case ItemType.Placeable:
                //ItemMaanager.Instance.TryToPlace();
            case ItemType.Consumable:
            case ItemType.Clothing:
                ItemManager.Instance.UseItem(data.key, 1, true);
                break;
        }
    }

    public void OnSelect()
    {
        GetComponent<Image>().color = SI_UIManager.Instance.itemUIselected;
    }

    public void OnDeselect()
    {
        GetComponent<Image>().color = SI_UIManager.Instance.itemUIDeselected;
    }

    public void SetSelected()//When is in factory
    {
        if (!blacksmithVariant)
        {
            //handler.GetComponent<FactoryUI>().SelectItem(this);
        }
        else
        {
           // handler.GetComponent<BlacksmithUI>().SelectItem(this);
        }
    }

    public void SetSelectedInWeaponsTab()
    {
       // handler.GetComponent<InventoryUI>().SelectedWeapon(this);
    }
}
