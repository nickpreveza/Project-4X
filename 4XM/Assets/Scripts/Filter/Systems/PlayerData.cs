using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    public string username;
    public string assignedUsername;

    [Header("Stats")]
    public int currentHealth;
    public Stat MaxHealth;
    public Stat Speed;
    public Stat Range;
    public Stat Damage;

    [Header("Weapon Stats")]
    public string weaponKey;
    public int[] pistolLevelData = new int[4]; //Levels for: damage, firerate, crit.damage, crit.chance
    public int[] shotgunLevelData = new int[4];//Levels for: damage, firerate, crit.damage, crit.chance

    [Header("System Variables")]
    public int spawnLocationIndex;
    public float interactableRadius;
    public int posX;
    public int posY;

    [Header("Score")]
    public int highscore;
    public int hbScore;
    public int killPoints;
    public int hempPoints;
}
