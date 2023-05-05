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
    public int unlockedAbiltiesCount;

    //Game Editable Data

    public Dictionary<UnitType, bool> gameUnitsDictionary = new Dictionary<UnitType, bool>();

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

    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance.UpdateHUD();
    }

    public void RemoveStars(int amount)
    {
        if (amount > stars)
        {
            Debug.LogError("Not enough stars but nothing stopped it. You broke the economy");
        }

        stars -= amount;

        SI_EventManager.Instance.OnTransactionMade(index);
    }


    public void AddStars(int amount)
    {
        stars += amount;

        SI_EventManager.Instance.OnTransactionMade(index);
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

    public void GenerateUnitDictionary()
    {
        foreach(UnitStruct unitStruct in UnitManager.Instance.gameUnits)
        {
            gameUnitsDictionary.Add(unitStruct.type, unitStruct.defaultLockState);
        }
    }

    public void UpdateAvailableUnits()
    {
        gameUnitsDictionary[UnitType.Swordsman] = abilities.unitSwordsman;
        gameUnitsDictionary[UnitType.Archer] = abilities.unitArcher;
        gameUnitsDictionary[UnitType.Horseman] = abilities.unitHorserider;
        gameUnitsDictionary[UnitType.Trebuchet] = abilities.unitTrebucet;
        gameUnitsDictionary[UnitType.Shields] = abilities.unitShield;
        gameUnitsDictionary[UnitType.Trader] = abilities.unitTrader;
        gameUnitsDictionary[UnitType.Diplomat] = abilities.unitDiplomat;
       
    }

    public void RecalculateAbilityCosts()
    {

        foreach(PlayerAbilityData ability in abilityDatabase)
        {
            int baseAbilityCost = GameManager.Instance.GetBaseAbilityCost(ability.abilityID);

            switch (baseAbilityCost)
            {
                case 1:
                    baseAbilityCost += playerCities.Count;
                    break;
                case 2:
                    baseAbilityCost += Mathf.RoundToInt((float)playerCities.Count * 1.5f);
                    break;
                case 3:
                    baseAbilityCost += Mathf.RoundToInt((float)playerCities.Count * 2f);
                    break;
            }


            ability.calculatedAbilityCost = baseAbilityCost;
        }
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

        GameManager.Instance.activePlayer.RecalculateAbilityCosts();
        SI_EventManager.Instance.OnCityCaptured(index);
    }
}



public enum PlayerType
{
    LOCAL,
    ONLINE,
    AI
}
