using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item 
{
    public string key;

    public int startingAmount = 1;
    public int effectAmount;

    public Sprite sprite;
    public Sprite placedSprite;

    public string displayName;
    [TextArea(15, 20)]
    public string description;

    public ItemType type;
    public ItemRarity rarity;
    public ItemAbility ability;

    public string[] craftingCostsNames;
    public int[] craftingCostsValue;

    public bool hideFromInventory;
    public bool keepInInventoryWhenEmpty;
    public bool isUnlocked;
    /*
    Flower,
    Medicine,
    Clothing,
    BlueMushroom,
    BrownMushroom,
    Worms,
    GrowthDust,
    Batteries,
    Hempcrete,
    Rock,
    Wood,
    HempHarvest,
    MaleBoar */
}

public enum ItemType
{
   Currency, //Maybe just have currency
   Consumable, //Blue Mushroom, Brown mushroom
   Placeable, //Flower, Honey 
   Material, //Steel, Sheep(?)
   Clothing,
   Weapon
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    VeryRare,
    Legendary,
    Excotic
}
public enum ItemAbility
{
    NONE,
    HEAL,
    WEAPON// 
    /*
    DEFENSEBUFF, //Blue Mushroom
    ATTACKBUFF, 
    SLOWENEMIES,//Brown mushroom 
    HARVESTBOOSTGROWTH,
    HARVESTBOOSTPOINTS, //Grow Dust */
}