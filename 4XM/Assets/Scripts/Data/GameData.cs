using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData 
{
    public int day;
    public int hour;
    public int minute;
    public int second;

    public int highscore;
    public int hbscore;
    public int hempSeeds;
    public int hempFibers;

    public int quadrantsUnlocked;
    public int factoryCost;

    public int wavesCompleted;
    public float nextWaveTime;

    public string discordURL;
    public string websiteURL;
    public string itchURL;

    public int player_weapon = 1;
    public int player_maxHealth = 100;

    //values for player speed
    //bullets
    //whatever 
    //we will hook this up to remote config
    //and playfab
}
