using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityData
{
    public Abilities abilityID;
    public string abilityName;
    public string abilityDescription;
    public Sprite abilityIcon;
    public int abilityCost;
    public bool isUnlocked;
    public Abilities abilityToUnlock;
}

[System.Serializable]
public class PlayerAbilityData
{
    public Abilities abilityID;
    public int calculatedAbilityCost;
    public bool canBePurchased;
    public bool hasBeenPurchased;
}
