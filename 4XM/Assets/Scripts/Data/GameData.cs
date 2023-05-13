using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData 
{
    //ability related, maybe move
    public int roadCost = 2;
    public int destroyCost = 0;

    //quest rewards
    //level 2
    public int visibilityReward = 2;
    public UnitType unitReward = UnitType.Melee;//shouldnotbe
                                                //level 3
    public int currencyReward = 5;
    public int productionReward = 1;
    //level 4
    public int populationReward = 3;
    public int rangeReward = 2;
    public int startCityOutput = 2;
    public int traderActionReward = 10;
    public int startCurrencyAmount = 10;

    public string itchURL;
    public string websiteURL;
    public string discordURL;
    public string linktrURL;
}
