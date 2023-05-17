using SignedInitiative;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Brain : MonoBehaviour
{
    public AIState state = AIState.Start;

    [Header("Tile Type Score")]
    public int unexploredTileValue = 1;
    public int occupiedTileValue = 1;


    [Header("Adjacent Score")]
    public int playerOwnerIndex = -1;


    Player player;

    public List<WorldUnit> unitsWithPaths = new List<WorldUnit>(); //retain these to reach target
    public List<WorldHex> assignedHexes = new List<WorldHex>(); //store unit targets in order to not assign them again

    public List<WorldUnit> playerUnitsToMove = new List<WorldUnit>(); // get player units
  
    public List<WorldHex> unexploredHexesOfInterest = new List<WorldHex>(); //get hexes to check 
    Dictionary<ResourceType, int> bestResearchToUnlockDictionary = new Dictionary<ResourceType, int>();
    public bool upgradingCities;
    public void StartEvaluation(Player _player)
    {
        player = _player;

        //all the units start as unmoved
        playerUnitsToMove = new List<WorldUnit>(player.playerUnits);
        //hexes to check become that as they are uncovered
        unexploredHexesOfInterest = new List<WorldHex>(player.clearedHexes);
        bestResearchToUnlockDictionary.Clear();
        StartCoroutine(NormalTurn());
    }

    public bool lookingForResearch;
    void FindResearch()
    {
        lookingForResearch = true;
        foreach (WorldHex city in player.playerCities)
        {
            foreach (WorldHex cityHex in city.cityData.cityHexes)
            {
                if (cityHex.hexData.hasResource)
                {
                    if (!GameManager.Instance.CanPlayerHarvestResource(player.index, cityHex.hexData.resourceType))
                    {
                        if (bestResearchToUnlockDictionary.ContainsKey(cityHex.hexData.resourceType))
                        {
                            bestResearchToUnlockDictionary[cityHex.hexData.resourceType]++;
                        }
                        else
                        {
                            bestResearchToUnlockDictionary.Add(cityHex.hexData.resourceType, 1);
                        }
                    }
                }
                if (cityHex.hexData.hasBuilding)
                {
                    //todo: find reserach for master
                }
            }
        }

        //find resource to buy - TODO: applicable to all types of research 
        ResourceType resourceType = ResourceType.EMPTY;
        int mostWanted = 0;
        foreach (ResourceType foundType in bestResearchToUnlockDictionary.Keys)
        {
            if (bestResearchToUnlockDictionary[foundType] > mostWanted)
            {
                resourceType = foundType;
                mostWanted = bestResearchToUnlockDictionary[foundType];
            }
        }

        Debug.Log("Resource to buy ability for: " + resourceType);

        //that's probably not being stopped from unnlcoking abilities that are not available to the player.
        Abilities ability = GameManager.Instance.GetAbilityAssociation(resourceType);
        if (ability != Abilities.NONE)
        {
            if (GameManager.Instance.CanPlayerAfford(player.index, player.abilityDictionary[ability].calculatedAbilityCost))
            {
                GameManager.Instance.UnlockAbility(player.index, ability, player.showAction(), true);
            }
        }

        lookingForResearch = false;

    }

    IEnumerator UpgradeCities()
    {
        upgradingCities = true;
        foreach (WorldHex city in player.playerCities)
        {
            int unitCost;
            UnitType unitToSpawn = SelectUnitTypeToSpawn(out unitCost);

            if (!city.hexData.occupied && !city.cityData.HasReachedMaxPopulation)
            {
                if (GameManager.Instance.CanActivePlayerAfford(unitCost))
                {
                    GameManager.Instance.SpawnUnitAction(unitCost, unitToSpawn, city);
                    yield return new WaitForSeconds(1f);
                }
            }

            foreach (WorldHex cityHex in city.cityData.cityHexes)
            {
                if (cityHex.hexData.hasResource)
                {
                    if (cityHex.hexData.occupied && cityHex.associatedUnit.playerOwnerIndex != player.index)
                    {
                        continue;
                    }

                    Resource resource = MapManager.Instance.GetResourceByType(cityHex.hexData.resourceType);

                    if (GameManager.Instance.CanPlayerHarvestResource(player.index, cityHex.hexData.resourceType))
                    {
                        if (GameManager.Instance.CanPlayerAfford(player.index, resource.harvestCost))
                        {
                            GameManager.Instance.RemoveStars(player.index, resource.harvestCost);
                            cityHex.HarvestResource();
                            yield return new WaitForSeconds(3f);
                        }

                    }
                }
            }
        }

        upgradingCities = false;

    }

    public bool checkForActions = false;
    IEnumerator CheckForActionsAndCombatFirst()
    {
        checkForActions = true;
        List<WorldUnit> remainingUnits = new List<WorldUnit>(playerUnitsToMove);
        foreach (WorldUnit unit in remainingUnits)
        {
            if (CanUnitDoButtonAction(unit))
            {
                playerUnitsToMove.Remove(unit);
                if (unitsWithPaths.Contains(unit))
                {
                    unitsWithPaths.Remove(unit);
                }
                yield return new WaitForSeconds(1f);
                continue;
            }
            if (CanUnitEngageInCombat(unit))
            {
                playerUnitsToMove.Remove(unit);
                if (unitsWithPaths.Contains(unit))
                {
                    unitsWithPaths.Remove(unit);
                }
                yield return new WaitForSeconds(1f);
                continue;
            }
        }

        checkForActions = false;
    }

    public bool checkForPaths = false;
    IEnumerator CheckForActivePaths()
    {
        checkForPaths = true;

        List<WorldUnit> unitsWithPathsTemp = new List<WorldUnit>(unitsWithPaths);
        foreach (WorldUnit unit in unitsWithPathsTemp)
        {
            if (unit.assignedPathTarget == null)
            {
                unitsWithPaths.Remove(unit);
                continue;
            }

            int turnsToTarget;
            //check if path is still valid
            List<WorldHex> path = UnitManager.Instance.FindMultiturnPath(unit, unit.assignedPathTarget, out turnsToTarget);

            if (path != null && path.Count > 0)
            {
                if (turnsToTarget <= 1)
                {
                    unitsWithPaths.Remove(unit);
                }

                if (unit.currentWalkRange > path.Count)
                {
                    unit.SetMoveTarget(path[path.Count - 1], true, true, false);
                    playerUnitsToMove.Remove(unit);
                }
                else
                {
                    unit.SetMoveTarget(path[unit.currentWalkRange - 1], true, true, false); ;
                    unit.assignedPathTarget = null;
                    playerUnitsToMove.Remove(unit);
                }
            }
            else
            {
                unitsWithPaths.Remove(unit);
                unit.assignedPathTarget = null;
            }
            yield return new WaitForSeconds(1f);
        }

        checkForPaths = false;
    }

    IEnumerator NormalTurn()
    {
        yield return new WaitForSeconds(1f);
        upgradingCities = true;
        StartCoroutine(UpgradeCities());
        while (upgradingCities)
        {
            yield return new WaitForSeconds(0.1f);
        }

        lookingForResearch = true;
        FindResearch();
        while (lookingForResearch)
        {
            yield return new WaitForSeconds(0.1f);
        }

        upgradingCities = true;
        StartCoroutine(UpgradeCities());
        while (upgradingCities)
        {
            yield return new WaitForSeconds(0.1f);
        }

        checkForActions = true;
        StartCoroutine(CheckForActionsAndCombatFirst());
        while (checkForActions)
        {
            yield return new WaitForSeconds(0.1f);
        }
        //For units with actions, do them actions

        checkForPaths = true;
        StartCoroutine(CheckForActivePaths());
        while (checkForPaths)
        {
            yield return new WaitForSeconds(0.1f);
        }
       

        List<WorldHex> hexesToReach = new List<WorldHex>();
        List<WorldHex> hexesToGoClose = new List<WorldHex>();
        List<WorldHex> hexesToAssign = new List<WorldHex>();
        foreach (WorldHex hex in unexploredHexesOfInterest)
        {

            int hexScore;
            EvaluateHex(hex, out hexScore);
            hex.aiScore = hexScore;

            if (assignedHexes.Contains(hex))
            {
                continue;
            }
            if (hex.hexData.hasCity && hex.hexData.playerOwnerIndex != player.index)
            {
                hexesToReach.Add(hex);
            }
            else if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex != player.index)
            {
                hexesToGoClose.Add(hex);
                player.hexesToCheck.Remove(hex);
            }
            else if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
            {
                hexesToReach.Add(hex);
            }
            else if (hex.aiScore > 0)
            {
                hexesToReach.Add(hex);
            }
            else
            {
                player.hexesToCheck.Remove(hex);
            }

        }

        foreach(WorldHex hex in hexesToReach)
        {
            WorldUnit unitToAssignToHex = FindClosestUnit(hex);
            if (unitToAssignToHex != null)
            {
                if (TryAssignUnit(unitToAssignToHex, hex))
                {
                    //unit will be removed from playerUnitsToMove
                    //hex will be added to hexesWithpaths
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        foreach (WorldHex hex in hexesToGoClose)
        {
            WorldUnit unitToAssignToHex = FindClosestUnit(hex);
            if (unitToAssignToHex != null)
            {
                WorldHex adjHex = FindAdjacentOfEnemyUnit(unitToAssignToHex, hex);

                if (TryAssignUnit(unitToAssignToHex, adjHex))
                {
                    if (player.hexesToCheck.Contains(hex))
                    {
                        player.hexesToCheck.Remove(hex);
                    }

                    yield return new WaitForSeconds(1f);
                }
            }
        }

        List<WorldUnit> remainingUnits = new List<WorldUnit>(playerUnitsToMove);
        foreach (WorldUnit unit in remainingUnits)
        {
            List<WorldHex> possibleMovesForUnit = UnitManager.Instance.GetWalkableHexes(unit);

            int highestRating = -1;
            WorldHex selectedHex = null;

            foreach (WorldHex hex in possibleMovesForUnit)
            {
                if (hex.hexData.occupied)
                {
                    continue;
                }

                int hexRating;
                EvaluateHex(hex, out hexRating);

                if (hexRating > highestRating)
                {
                    selectedHex = hex;
                }
            }

            if (selectedHex != null)
            {
                Debug.Log("Selected Hex was: " + selectedHex + ", rating of: " + highestRating);
                if (TryAssignUnit(unit, selectedHex))
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            if (CanUnitEngageInCombat(unit))
            {
                yield return new WaitForSeconds(1f);
            }
        }

        foreach (WorldUnit unit in player.playerUnits)
        {
            unit.ValidateRemainigActions(unit.unitReference);

            if (unit.currentAttackCharges > 0)
            {
                if (CanUnitEngageInCombat(unit))
                {
                    unitsWithPaths.Remove(unit);
                    playerUnitsToMove.Remove(unit);
                    yield return new WaitForSeconds(2f);
                    continue;
                }
            }
        }
    }

    public bool CanUnitEngageInCombat(WorldUnit unit)
    {
        if (unit.currentAttackCharges <= 0)
        {
            return false;
        }

        List<WorldHex> attackableHexes = UnitManager.Instance.GetAttackableHexes(unit);
        WorldUnit selectedUnitToAttack = null;
        int leastHealth = 0;
        if (attackableHexes != null && attackableHexes.Count > 0)
        {
            foreach (WorldHex hex in attackableHexes)
            {
                if (hex.associatedUnit.localHealth > leastHealth)
                {
                    selectedUnitToAttack = hex.associatedUnit;
                }
            }
        }

        if (selectedUnitToAttack != null)
        {
            unit.Attack(selectedUnitToAttack.parentHex);
            return true;
        }

        return false;
      
    }
    public bool CanUnitDoButtonAction(WorldUnit unit)
    {
        if (unit.parentHex.hexData.hasCity && unit.parentHex.hexData.playerOwnerIndex != unit.playerOwnerIndex)
        {
            if (unit.buttonActionPossible)
            {
                unit.CityCaptureAction();
                if (playerUnitsToMove.Contains(unit))
                {
                    playerUnitsToMove.Remove(unit);
                }

                if (player.hexesToCheck.Contains(unit.parentHex))
                {
                    player.hexesToCheck.Remove(unit.parentHex);
                }

                if (assignedHexes.Contains(unit.parentHex))
                {
                    assignedHexes.Remove(unit.assignedPathTarget);
                }

                return true;
            }
        }
        else if (unit.parentHex.hexData.hasResource && unit.parentHex.hexData.resourceType == ResourceType.MONUMENT)
        {
            if (unit.buttonActionPossible)
            {
                unit.parentHex.HarvestResource();

                if (playerUnitsToMove.Contains(unit))
                {
                    playerUnitsToMove.Remove(unit);
                }

                if (player.hexesToCheck.Contains(unit.parentHex))
                {
                    player.hexesToCheck.Remove(unit.parentHex);
                }

                if (assignedHexes.Contains(unit.parentHex))
                {
                    assignedHexes.Remove(unit.assignedPathTarget);
                }


                return true;
            }
        }

        return false;
    }


    public UnitType SelectUnitTypeToSpawn(out int unitCost)
    {
        if (player.turnCount <= 5)
        {
            unitCost = UnitManager.Instance.GetUnitDataByType(UnitType.Melee, player.civilization).cost;
            return UnitType.Melee;
        }
        List<UnitType> unlockedUnits = new List<UnitType>();
        foreach (UnitType unitType in player.gameUnitsDictionary.Keys)
        {
            if (player.gameUnitsDictionary[unitType])
            {
                unlockedUnits.Add(unitType);
            }
        }

        UnitType randomType = unlockedUnits[Random.Range(0, unlockedUnits.Count)];
        UnitData unitData = UnitManager.Instance.GetUnitDataByType(randomType, player.civilization);
        unitCost = unitData.cost;
        return randomType;

        //later on add better ways to do more purpousful spawns
        //UnitData selectedUnitToSpawn = UnitManager.Instance.GetUnitDataByType(randomType, player.civilization);
    }

    
   
    IEnumerator TurnMasterEnum()
    {
        List<WorldUnit> unitsWithPathsTemp = new List<WorldUnit>(unitsWithPaths);

        foreach (WorldUnit unit in unitsWithPathsTemp)
        {
            if (CanUnitDoButtonAction(unit))
            {
                unitsWithPaths.Remove(unit);
                playerUnitsToMove.Remove(unit);
                yield return new WaitForSeconds(1f);
                continue;
            }
            if (CanUnitEngageInCombat(unit))
            {
                unitsWithPaths.Remove(unit);
                playerUnitsToMove.Remove(unit);
                yield return new WaitForSeconds(1f);
                continue;
            }
            if (unit.assignedPathTarget == null)
            {
                unitsWithPaths.Remove(unit);
                continue;
            }

           // MoveUnitWithPath(unit);
            yield return new WaitForSeconds(1f);
        }

        List<WorldHex> hexesToReach = new List<WorldHex>();
        List<WorldHex> hexesToGoClose = new List<WorldHex>();
        //from the new tiles we've found, sort them 
        foreach (WorldHex hex in unexploredHexesOfInterest)
        {
            if (assignedHexes.Contains(hex))
            {
                continue;
            }
            if (hex.hexData.hasCity && hex.hexData.playerOwnerIndex != player.index)
            {
                hexesToReach.Add(hex);
            }
            else if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex != player.index)
            {
                hexesToGoClose.Add(hex);
                player.hexesToCheck.Remove(hex);
            }
            else if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
            {
                hexesToReach.Add(hex);
            }
            else
            {
                player.hexesToCheck.Remove(hex);
            }
        }

        foreach(WorldHex hex in hexesToReach)
        {
            WorldUnit unitToAssignToHex = FindClosestUnit(hex);
            if (unitToAssignToHex != null)
            {
                if (TryAssignUnit(unitToAssignToHex, hex))
                {
                    //unit will be removed from playerUnitsToMove
                    //hex will be added to hexesWithpaths
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        foreach(WorldHex hex in hexesToGoClose)
        {
            WorldUnit unitToAssignToHex = FindClosestUnit(hex);
            if (unitToAssignToHex != null)
            {
                WorldHex adjHex = FindAdjacentOfEnemyUnit(unitToAssignToHex, hex);

                if (TryAssignUnit(unitToAssignToHex, adjHex))
                {
                    if (player.hexesToCheck.Contains(hex))
                    {
                        player.hexesToCheck.Remove(hex);
                    }

                    yield return new WaitForSeconds(1f);
                }
            }
        }

        foreach (WorldHex city in player.playerCities)
        {
            upgradingCities = true;
            //StartCoroutine(CityCheck(city));

            while (upgradingCities)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        List<WorldUnit> remainingUnits = new List<WorldUnit>(playerUnitsToMove);
        foreach (WorldUnit unit in remainingUnits)
        {
            if (CanUnitDoButtonAction(unit))
            {
                playerUnitsToMove.Remove(unit);
                yield return new WaitForSeconds(1f);
                continue;
            }
            if (CanUnitEngageInCombat(unit))
            {
                playerUnitsToMove.Remove(unit);
                yield return new WaitForSeconds(1f);
                continue;
            }
        }

        remainingUnits = new List<WorldUnit>(playerUnitsToMove);

        foreach (WorldUnit unit in remainingUnits)
        {
            List<WorldHex> possibleMovesForUnit = UnitManager.Instance.GetWalkableHexes(unit);

            int highestRating = -1;
            WorldHex selectedHex = null;

            foreach (WorldHex hex in possibleMovesForUnit)
            {
                if (hex.hexData.occupied)
                {
                    continue;
                }

                int hexRating;
                EvaluateHex(hex, out hexRating);

                if (hexRating > highestRating)
                {
                    selectedHex = hex;
                }
            }

            if (selectedHex != null)
            {
                Debug.Log("Selected Hex was: " + selectedHex + ", rating of: " + highestRating);
                if (TryAssignUnit(unit, selectedHex))
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            if (CanUnitEngageInCombat(unit))
            {
                yield return new WaitForSeconds(1f);
            }
        }

        foreach (WorldUnit unit in player.playerUnits)
        {
            unit.ValidateRemainigActions(unit.unitReference);

            if (unit.currentAttackCharges > 0)
            {
                if (CanUnitEngageInCombat(unit))
                {
                    unitsWithPaths.Remove(unit);
                    playerUnitsToMove.Remove(unit);
                    yield return new WaitForSeconds(2f);
                    continue;
                }
            }
        }


    }



    public WorldHex FindAdjacentOfEnemyUnit(WorldUnit unit, WorldHex hex)
    {
        WorldHex selectedhex = null;
        int distance = 1000;

        foreach(WorldHex adj in hex.adjacentHexes)
        {
            int newDistance = MapManager.Instance.GetDistance(unit.parentHex, hex);
            if (newDistance < distance && adj.CanBeWalked(unit.playerOwnerIndex, false, true))
            {
                distance = newDistance;
                selectedhex = adj;
            }
        }

        return selectedhex;
    }

    public WorldUnit FindClosestUnit(WorldHex hex)
    {
        WorldUnit closestUnit = null;
        int distance = 1000;
        foreach(WorldUnit unit in playerUnitsToMove)
        {
            int newDistance = MapManager.Instance.GetDistance(unit.parentHex, hex);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestUnit = unit;
            }
        }

        return closestUnit;
    }


    public bool TryAssignUnit(WorldUnit unit, WorldHex hex)
    {
        int turnsToTarget;
        unit.assignedPathTarget = hex;
        List<WorldHex> path = UnitManager.Instance.FindMultiturnPath(unit, hex, out turnsToTarget);

        if (path != null && path.Count > 0)
        {
            if (!assignedHexes.Contains(hex)) 
            {
                assignedHexes.Add(hex);
            }

            playerUnitsToMove.Remove(unit);

            if (turnsToTarget > 1)
            {
                if (!unitsWithPaths.Contains(unit))
                {
                    unitsWithPaths.Add(unit);
                }
             
                unit.assignedPathTarget = hex;
            }

            unit.SetMoveTarget(path[unit.currentWalkRange - 1], true, true, false);
            return true;
        }
        else
        {
            return false;
        }
    }
 

    public void EvaluateHex(WorldHex hex, out int hexRating)
    {
        int hexBaseValue = 0;
        int adjScore = 0;
        foreach (WorldHex adj in hex.adjacentHexes)
        {
            adjScore += ScoreAdjHex(hex);
        }

        hexBaseValue += adjScore;

        if (hex.hexData.hasCity && !hex.hexData.cityHasBeenClaimed)
        {
            hexBaseValue += 10;
        }

        if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
        {
            hexBaseValue += 10;
        }

        if (assignedHexes.Contains(hex))
        {
            hexRating = 0;
        }
        else
        {
            hexRating = hexBaseValue;
        }
       
    }

    public int ScoreAdjHex(WorldHex hex)
    {
        int hexBaseValue = 0;
        bool canBeWalked = hex.CanBeWalked(player.index, false, false);
        if (canBeWalked && !hex.Hidden())
        {
            hexBaseValue += 0;
        }
        if (!hex.CanBeReached(player.index))
        {
            hexBaseValue += 0;
        }
        if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex != player.index)
        {
            hexBaseValue += 10;
        }
        if (hex.hexData.hasCity && !hex.hexData.cityHasBeenClaimed)
        {
            hexBaseValue += 10;
        }

        if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
        {
            hexBaseValue += 10;
        }
        if (hex.Hidden())
        {
            hexBaseValue += 2;
        }

        return hexBaseValue;
    }

   

}

public class AIUnitMove
{
    public WorldHex hex;
    public WorldUnit unit;
    public int rating = 0;
}
public enum AIActionType
{
    Upgrade,
    Research,
    Move,
    Attack,
}
public enum AIState
{
    Defend,
    Attack,
    Explore,
    Research,
    Start
}

public class BuildingAction
{
    public BuildingType building;
    public bool canBeBuild;
}
public class ResourceAction
{
    public ResourceType resource;
    public bool canBeHarvested;
}

