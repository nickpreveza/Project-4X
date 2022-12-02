using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public WeaponScriptable[] weapons;
    public ItemScriptable[] blacksmithItems; //items that can be found in the blacksmith
    public ItemScriptable[] factoryItems; //craftable items that can be found in the factory
    public ItemScriptable[] startingItems; //items that will be loaded in the player inventory at the start of the game
    public ItemScriptable[] items; //all the items of the game 
   
    public int removeFromInventoryCount; //items that are hidden from inventory visualization should be accounted for
    public GameObject[] dropsLevel1;
    public GameObject[] hempDrops;
    public GameObject[] spawnableItems;
    public Dictionary<string, GameObject> spawnableItemDatabase = new Dictionary<string, GameObject>();

    public Dictionary<string, ItemScriptable> itemDatabase = new Dictionary<string, ItemScriptable>(); //complete database of all the items of the game
    public Dictionary<string, int> playerInventory = new Dictionary<string, int>(); //playerInventory
    public Dictionary<string, WeaponScriptable> weaponDatabase = new Dictionary<string, WeaponScriptable>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    public void InitializeItems()
    {
        GenerateItems();
    }
    void GenerateItems()
    {
        foreach (ItemScriptable scriptable in items)
        {
            itemDatabase.Add(scriptable.item.key, scriptable);
        }

        foreach(GameObject obj in spawnableItems)
        {
            spawnableItemDatabase.Add(obj.GetComponent<ItemInteractable>().key, obj);
        }

        foreach(WeaponScriptable scriptable in weapons)
        {
            weaponDatabase.Add(scriptable.item.key, scriptable);
        }

        foreach (ItemScriptable scriptable in startingItems)
        {
            AddToInventory(scriptable.item.key, scriptable.item.startingAmount, true);
        }

        LockWeapons();
        SI_GameManager.Instance.OnDataReady();

    }


    public void LockWeapons()
    {
        foreach(ItemScriptable item in weapons)
        {
            item.item.isUnlocked = false;
        }

        //weapons[0].item.isUnlocked = true;
        //temporary
    }

    public void UnlockWeapon(string itemKey)
    {
        weaponDatabase[itemKey].item.isUnlocked = true;
    }

    public void EquipItem(string itemName)
    {
        UnlockWeapon(itemName);
       // HB_PlayerController.Instance.SetWeapon(itemName);
        //ApplyItemEffects(itemDatabase[itemName].item.ability, itemDatabase[itemName].item.effectAmount);

        //SI_UIManager.Instance.UpdateBackpack(false);
        SI_UIManager.Instance.UpdatePlayerCurrencies();
    }

    //function to mass remove items from cafting here
    public void UseItem(string itemName, int amount, bool applyEffect)
    {
        if (!playerInventory.ContainsKey(itemName))
        {
            Debug.LogError("Inventory Manager does not contain item: " + itemName);
            return;
        }

        if (playerInventory[itemName] < amount)
        {
            Debug.LogError("Requested more items than existing");
            return;
        }

        playerInventory[itemName] -= amount;

        if (applyEffect)
        {
            ApplyItemEffects(itemDatabase[itemName].item.ability, itemDatabase[itemName].item.effectAmount);
        }
       

        if (playerInventory[itemName] <= 0)
        {
            if (!itemDatabase[itemName].item.keepInInventoryWhenEmpty)
            {
                playerInventory.Remove(itemName);
                //SI_UIManager.Instance.UpdatePlayerCurrencies();SI_UIManager.Instance.UpdateBackpack(true);
                return;
            }
        }

        //SI_UIManager.Instance.UpdateBackpack(false);
        SI_UIManager.Instance.UpdatePlayerCurrencies();
    }
   
    //this is fucked up, will fix, just want the functionallity now
    void ApplyItemEffects(ItemAbility ability, int effectAmount)
    {
        switch (ability)
        {
            case ItemAbility.NONE:
                break;
            case ItemAbility.HEAL:
               // HB_PlayerController.Instance.Heal(effectAmount);
                break;
            case ItemAbility.WEAPON:
                Debug.LogWarning("Weapons should not be applied through this funciton. Use EquipItem instead");
                break;
        }
    }

    public void AddToInventory(string itemName, int amount, bool skipInventoryUpdate)
    {
        Item newItem = itemDatabase[itemName].item;

        switch (newItem.type)
        {
            case ItemType.Currency:
            case ItemType.Material:
            case ItemType.Placeable:
            case ItemType.Consumable:
            case ItemType.Clothing:
                if (!playerInventory.ContainsKey(itemName))
                {
                    playerInventory.Add(itemName, amount);
                    if (newItem.hideFromInventory)
                    {
                        removeFromInventoryCount++;
                    }

                    if (!skipInventoryUpdate)
                    {
                       // SI_UIManager.Instance.UpdateBackpack(true);
                    }
                }
                else
                {
                    playerInventory[itemName] += amount;
                    //SI_UIManager.Instance.UpdateBackpack(false);
                }
                break;
            case ItemType.Weapon:
                UnlockWeapon(itemName);
                //SI_UIManager.Instance.UpdateBackpack(false);
                break;
        }
        
        SI_UIManager.Instance.UpdatePlayerCurrencies();
    }

    public bool DoesPlayerHaveItem(string itemName, int amount)
    {
        if (playerInventory.ContainsKey(itemName))
        {
            if (playerInventory[itemName] >= amount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false; 
        }
    }

    public Sprite GetItemVisual(string itemName)
    {
        if (itemDatabase.ContainsKey(itemName))
        {
            return itemDatabase[itemName].item.sprite;
        }
        else
        {
            Debug.LogError("Item does not exist in ItemDatabase");
            return null;
        }
    }

    public GameObject GetWorlditem(string itemName)
    {
        if (spawnableItemDatabase.ContainsKey(itemName))
        {
            return spawnableItemDatabase[itemName];
        }
        else
        {
            return null;
        }
    }

    public int GetItemAmount(string itemName)
    {
        if (playerInventory.ContainsKey(itemName))
        {
            return playerInventory[itemName];
        }
        else
        {
            return -1;
        }
    }

    public GameObject GetRandomDrop(int quality)
    {
        return dropsLevel1[(int)Random.Range(0, dropsLevel1.Length)];
    }

    public GameObject GetHempDrop()
    {
        return hempDrops[(int)Random.Range(0, hempDrops.Length)];
    }

}
