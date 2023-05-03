using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

[System.Serializable]
public class Player
{
    //Setup
    public PlayerType type;
    public AbilityTree abilities;
    public int index;
    public string name;
    public Color playerColor;

    //Score Related
    public int score;
    public int turnCount;
    public int stars;

   
    //Game Editable Data
    public List<WorldUnit> playerUnitsThatCanBeSpawned = new List<WorldUnit>();

    public List<WorldUnit> playerUnits = new List<WorldUnit>();
    public List<WorldHex> playerCities = new List<WorldHex>();
    public List<WorldHex> playerBorders = new List<WorldHex>();

    List<TurnAction> activeTurnActions = new List<TurnAction>();

    List<Turn> turns = new List<Turn>();

    public WorldUnit lastMovedUnit;

    public List<PlayerAbilityData> abilityDatabase = new List<PlayerAbilityData>();
    public Dictionary<Abilities, PlayerAbilityData> abilityDictionary = new Dictionary<Abilities, PlayerAbilityData>();
    public void StartTurn()
    {
        List<TurnAction> activeTurnActions = new List<TurnAction>();
        turnCount++;

        //TODO: Calculate Stars
        //TODO: Recalculate Score


        if (type == PlayerType.AI)
        {
            UnitManager.Instance.PlayRandomTurnForAIUnits(this);
        }
    }

    public void BuyAbility(Abilities ability)
    {
        abilityDictionary[ability].canBePurchased = true;
       abilityDictionary[ability].hasBeenPurchased = true;
    }

    public void UnlockAbility(Abilities ability)
    {
        abilityDictionary[ability].canBePurchased = true;
    }

    public void EndTurn()
    {
        Turn thisTurn = new Turn();
        thisTurn.actions = activeTurnActions;
        turns.Add(thisTurn);

        activeTurnActions.Clear();

       // GameManager.Instance.EndTurn(this);
    }

    public void AddUnit(WorldUnit newUnit)
    {
        playerUnits.Add(newUnit);
    }

    public void AddCity(WorldHex cityHex)
    {
        //TODO: Some security checks to make sure this is the correct tile;
        playerCities.Add(cityHex);
        cityHex.OccupyCityByPlayer(this);
    }
}



public enum PlayerType
{
    LOCAL,
    ONLINE,
    AI
}
