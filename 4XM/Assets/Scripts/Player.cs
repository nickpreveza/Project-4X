using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using System.Linq;

[System.Serializable]
public class Player 
{
    //Setup
    public bool isFirstPlayer;
    public bool activatedOnSetup;
    public bool isAlive = true;
    public PlayerType type;
    public Civilizations civilization;

    public AbilityTree abilities;
    public int index;
    //Score Related
    public int totalScore;
    public int developmentScore;
    public int researchScore;
    public int militaryScore;
    public int turnCount;
    public int stars;
    public int expectedStars;
    public int unlockedAbiltiesCount;
    public int currentRank;
    //Game Editable Data
    public WorldHex capitalCity;

    public Dictionary<UnitType, bool> gameUnitsDictionary = new Dictionary<UnitType, bool>();

    public List<WorldUnit> playerUnits = new List<WorldUnit>();
    public List<WorldHex> playerCities = new List<WorldHex>();
    public List<WorldHex> playerBorders = new List<WorldHex>();
    public List<WorldUnit> unitsWithActions = new List<WorldUnit>();
    

    List<TurnAction> activeTurnActions = new List<TurnAction>();

    List<Turn> turns = new List<Turn>();

    public WorldUnit lastMovedUnit;

    public List<PlayerAbilityData> abilityDatabase = new List<PlayerAbilityData>();
    public Dictionary<Abilities, PlayerAbilityData> abilityDictionary = new Dictionary<Abilities, PlayerAbilityData>();

    public List<WorldHex> clearedHexes = new List<WorldHex>();
    NetworkedPlayer networkedPlayer;

    public ulong clientID;

    public bool playerCanBuyAbility;
    public bool playerCanMoveUnit;
    public bool playerCanPlaceResource;

    public bool playerHasActions;

    public void SetupForNetworkPlay(NetworkedPlayer newNetworkPlayer)
    {
        networkedPlayer = newNetworkPlayer;
        clientID = newNetworkPlayer.OwnerClientId;
        type = PlayerType.ONLINE;
    }
    public void StartTurn()
    {
        List<TurnAction> activeTurnActions = new List<TurnAction>();
        turnCount++;

        unitsWithActions = new List<WorldUnit>(playerUnits);

        CheckForAvailableUnitMoves();
        CheckForAvailableResearch();
        CheckForAvailableHexActions(false);

        if (type == PlayerType.AI)
        {
            UnitManager.Instance.PlayRandomTurnForAIUnits(this);
            //evaluate resoure purchases 
            //evalute ability purchases 
            //end  turn
        }
    }

    public void AddScore(int scoreType, int amount)
    {
        if (scoreType == 1)
        {
            developmentScore += amount;
        }
        else if (scoreType == 2)
        {
            researchScore += amount;
        }
        else if (scoreType == 3)
        {
            militaryScore += amount;
        }

        totalScore = researchScore + developmentScore + militaryScore; 
    }

    public void ActionCheck(bool updateHUD)
    {
        if (!playerCanBuyAbility && !playerCanMoveUnit && !playerCanPlaceResource)
        {
            playerHasActions = false;
        }
        else
        {
            playerHasActions = true;
        }

        if (updateHUD)
        {
            UIManager.Instance.UpdateGUIButtons();
        }
    }

    public void CheckForAvailableResearch()
    {
        //possible optimziation to sort this in a list of unpurchased abilities
        foreach(PlayerAbilityData ability in abilityDatabase)
        {
            if (stars >= ability.calculatedAbilityCost)
            {
                playerCanBuyAbility = true;
                ActionCheck(false);
                return;
            }
        }

        playerCanBuyAbility = false;
        ActionCheck(false);
    }

    public void CheckForAvailableUnitMoves()
    {
        if (unitsWithActions.Count == 0)
        {
            playerCanMoveUnit = false;
            ActionCheck(false);
        }
        else
        {
            playerCanMoveUnit = true;
            ActionCheck(false);
        }
    }

    public void UpdateAvailbleResourceActionState(WorldHex cityHex)
    {
        if (!playerCities.Contains(cityHex))
        {
            return;
        }

        if (cityHex.cityData.cityHexesThatCanBeWorked.Count > 0)
        {
            playerCanPlaceResource = true;
            ActionCheck(false);
            return;
        }
    }
    public void CheckForAvailableHexActions(bool forceManualCheck)
    {
        foreach (WorldHex city in playerCities)
        {
            if (forceManualCheck)
            {

                foreach (WorldHex cityHex in city.cityData.cityHexes)
                {
                    if (city.EvaluateIfHexHasPossibleActions(cityHex))
                    {
                        playerCanPlaceResource = true;
                        ActionCheck(false);
                        return;
                    }
                }

            }
            else
            {
                if (city.cityData.cityHexesThatCanBeWorked.Count > 0)
                {
                    playerCanPlaceResource = true;
                    ActionCheck(false);
                    return;
                }
            }
        }

        ActionCheck(false);

    }

    public void CalculateDevelopmentScore(bool updateHUD)
    {
        int newDevelopmentScore = 0;
        foreach(WorldHex hex in playerCities)
        {
            if (hex.cityData.level <= 10)
            {
                newDevelopmentScore += hex.cityData.level * 10;
            }
            else if (hex.cityData.level > 10)
            {
                int baseScore = hex.cityData.level - 10;
                newDevelopmentScore += 100 + (baseScore * 50);
            }
           
        }

        developmentScore = newDevelopmentScore;

        if (updateHUD)
        {
            UIManager.Instance.UpdateHUD();
        }
    }

    public void RemoveStars(int amount) //make this a tryremovestarts instead
    {
        if (amount > stars)
        {
            Debug.LogError("Not enough stars but nothing stopped it. You broke the economy");
        }

        stars -= amount;

        SI_EventManager.Instance.OnTransactionMade(index);
    }

    public void CalculateExpectedStars()
    {
        int starsToReceive = 0;

        foreach(WorldHex hex in playerCities)
        {
            if (!hex.cityData.isUnderSiege)
            {
                starsToReceive += hex.cityData.output;
            }
          
        }

        expectedStars = starsToReceive;
    }

    public void AddStars(int amount)
    {
        stars += amount;

        SI_EventManager.Instance.OnTransactionMade(index);
    }

    public void BuyAbility(Abilities ability, bool removeStars)
    {
        if (removeStars)
        {
            RemoveStars(abilityDictionary[ability].calculatedAbilityCost);
        }

        abilityDictionary[ability].canBePurchased = true;
        abilityDictionary[ability].hasBeenPurchased = true;
    }

    public void UnlockAbility(Abilities ability)
    {
        abilityDictionary[ability].canBePurchased = true;
    }

    public void GenerateUnitDictionary()
    {
        foreach(UnitData unitData in UnitManager.Instance.gameUnits)
        {
            gameUnitsDictionary.Add(unitData.type, unitData.defaultLockState);
        }
    }

    void UpdateLockStateForUnitType(UnitType type, bool lockState)
    {
        if (gameUnitsDictionary.ContainsKey(type))
        {
            gameUnitsDictionary[type] = lockState;
        }
    }
    public void UpdateAvailableUnitsFromAbilities()
    {
        UpdateLockStateForUnitType(UnitType.Melee, abilities.unitSwordsman);
        UpdateLockStateForUnitType(UnitType.Cavalry, abilities.unitHorserider);
        UpdateLockStateForUnitType(UnitType.Ranged, abilities.unitArcher);

        UpdateLockStateForUnitType(UnitType.Siege, abilities.unitTrebucet);
        UpdateLockStateForUnitType(UnitType.Defensive, abilities.unitShield);

        UpdateLockStateForUnitType(UnitType.Lance, abilities.unitLance);
        UpdateLockStateForUnitType(UnitType.Knight, abilities.unitKnight);
        UpdateLockStateForUnitType(UnitType.Leader, abilities.unitTrader);

        UpdateLockStateForUnitType(UnitType.Trader, abilities.unitTrader);
        UpdateLockStateForUnitType(UnitType.Diplomat, abilities.unitDiplomat);
    }

    public void RecalculateAbilityCosts()
    {

        foreach(PlayerAbilityData ability in abilityDatabase)
        {
            int baseAbilityCost = GameManager.Instance.GetBaseAbilityCost(ability.abilityID);

            if (baseAbilityCost <= 2)
            {
                baseAbilityCost += playerCities.Count;
            }
            else if (baseAbilityCost > 2 && baseAbilityCost <= 5)
            {
                baseAbilityCost += Mathf.RoundToInt((float)playerCities.Count * 1.5f);
            }
            else if (baseAbilityCost > 5)
            {
                baseAbilityCost += Mathf.RoundToInt((float)playerCities.Count * 2f);
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
        cityHex.cityData.isUnderSiege = false;
        //TODO: Some security checks to make sure this is the correct tile;
        playerCities.Add(cityHex);
        cityHex.OccupyCityByPlayer(this);

        RecalculateAbilityCosts();
        CalculateExpectedStars();
        SI_EventManager.Instance.OnCityCaptured(index);
    }

    public void RemoveCity(WorldHex cityHex, bool showPopup)
    {
        //probably master resources will not update
        if (playerCities.Contains(cityHex))
        {
            playerCities.Remove(cityHex);
        }

        if (playerCities.Count > 0)
        {
            RecalculateAbilityCosts();
        }
        else if (playerCities.Count == 0)
        {
            GameManager.Instance.RemovePlayerFromGame(this, showPopup);
        }

       

    }

}


public enum PlayerType
{
    LOCAL = 1,
    ONLINE = 2,
    AI  = 3
}
