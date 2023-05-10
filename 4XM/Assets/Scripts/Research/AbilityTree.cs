using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityTree 
{
    //unit traversal

    public bool travelMountain;
    public bool travelSea;
    public bool travelOcean;

    //combat Units
    public bool unitSwordsman;
    public bool unitArcher;
    public bool unitHorserider;
    public bool unitTrebucet;
    public bool unitShield;
    public bool unitKnight;

    //civlian unlockables
    public bool unitTrader;
    public bool unitDiplomat;

    //resource harvesting
    public bool fishHarvest;
    public bool fruitHarvest;
    public bool farmHarvest;
    public bool mineHarvest;
    public bool animalHarvest;
    public bool forestHarvest;

    public bool roads;
    public bool destroyAbility;
    public bool forestCut;

    public bool createFish;
    public bool createFruit;
    public bool createForest;
    public bool createFarm;
    public bool createAnimals;
    public bool createMine;

    public bool shipUpgrade;

    /*
    public bool createForest;
    public bool createFarm;
    public bool createFish;
    public bool createAnimals;
    public bool createMine; */


    public bool forestMasterBuilding;
    public bool mineMasterBuilding;
    public bool farmMasterBuilding;
    public bool fishMasterBuilding;
    public bool fishBuilding;
    public bool portBuilding;
    public bool guildBuilding;

}

//All the game's abilities that can be unlocked
public enum Abilities //more like ability grousps
{
    Climbing, //allows mountain traversal
    Mining, //allows mountain harvest
    Shields, //allows shield unit
    Smithery, //alows mountain master

    FruitHarvest, //allows fruit harvest
    Roads, //allows roads
    Guild, //allows guild
    Creator, //allows destroy

    Forestry, //allows forest harvest
    Archery, //allows archer
    Engineering, //allows trebuchet
    ForestMaster, //allows forest master, forest creation

    Fishing, //allows fish harvest
    Port, //allows port building and sea traversal
    Ocean, //allows open sea travel
    Ship, //allows ship

    Husbandry, //allows animal harvest
    Horserider, //allows cavalry
    Farming, //allows farm harvest
    Windmill, //allows windmill 

    NONE
}
