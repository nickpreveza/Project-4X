using SignedInitiative;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Brain : MonoBehaviour
{
    public AIState state = AIState.Start;

    [Header("Hex Evaluation Scores")]
    public int cityScore;
    public int monumentScore;
    public int adjCityScore;
    public int adjToEnemyScore;
    public int adjHiddenScore;
    public int adjMonument;
    public int adjUnreachable; //use this to avoid being stuck 

    Player player;

    public List<WorldUnit> unitsWithPaths = new List<WorldUnit>(); //retain these to reach target
    public List<WorldHex> assignedHexes = new List<WorldHex>(); //store unit targets in order to not assign them again
    public List<WorldHex> knownPlayerHexes = new List<WorldHex>(); //get hexes to check 
    Dictionary<Abilities, int> abilitiesTargetToUnlock = new Dictionary<Abilities, int>();
    
    public bool upgradingCities;
    public bool lookingForResearch;
    public bool checkForActions;
    public bool checkForPaths;
    public bool assignPaths;
    public void StartEvaluation(Player _player)
    {
        player = _player;

        //1. get the player units
        //2. get the player units with assigned paths
        //3. get the cleared hexes 
        //4. clear the research goal 
        //5. find the assigned hexes
        //6. remove them from the cleared hexes 
        //7. Start upgrading cities - Upgrade resources, spawn units 
        //8. Find the research to buy - Find the tech tree paths, and also infuse things you might want 
        //9: TODO: Force Guild and Port lookups
        //10. Upgrade cities again, with the newfound research you've gained 
        //11. Foreach unit, check if you can do an action first: City capture, monument capture 
        //12. Check for active paths, move on the paths
        //13. Assign new paths by evaluating the known tiles 
        //14. if any units remain without paths, assign a tile of its adjacent ones 
        //15 combat check for all the units 
        unitsWithPaths = new List<WorldUnit>(player.unitsWithPaths);
        knownPlayerHexes = new List<WorldHex>(player.clearedHexes);
        assignedHexes = new List<WorldHex>(player.assignedHexes);

        abilitiesTargetToUnlock.Clear();

        FindAssignedHexes();

       
        StartCoroutine(NormalTurn());
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

        assignPaths = true;
        StartCoroutine(AssignPathsToUnits());
        while (assignPaths)
        {
            yield return new WaitForSeconds(0.1f);
        }
       
        //last check for combat 
        List<WorldUnit> remainingUnits = new List<WorldUnit>(player.playerUnits);
        foreach (WorldUnit unit in remainingUnits)
        {
            unit.ValidateRemainigActions();

            if (unit.currentAttackCharges > 0)
            {
                if (CanUnitEngageInCombat(unit))
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
            }
        }

        yield return new WaitForSeconds(1f);
        GameManager.Instance.LocalEndTurn();
    }

    IEnumerator AssignPathsToUnits()
    {
        assignPaths = true;

        List<WorldHex> hexesToReach = new List<WorldHex>();
        List<WorldHex> hexesToGoClose = new List<WorldHex>();

        foreach (WorldHex hex in knownPlayerHexes)
        {
            hex.aiScore = EvaluateHex(hex);

            if (assignedHexes.Contains(hex))
            {
                continue;
            }
          
            if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex != player.index)
            {
                hexesToGoClose.Add(hex);
            }
           
            if (hex.aiScore > 0)
            {
                hexesToReach.Add(hex);
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
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                break;
            }
        }
        List<WorldUnit> remainingUnits = new List<WorldUnit>(player.unitsWithActions);
        if (remainingUnits.Count > 0)
        {
            List<WorldHex> hexesToSort = new List<WorldHex>(hexesToReach);


            hexesToReach = hexesToSort.OrderByDescending(x => x.aiScore).ToList();


            //choose how many units to send to where
            foreach (WorldHex hex in hexesToReach)
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
                else
                {
                    break;
                }
            }

        }

        assignPaths = false;
    }


    void FindAssignedHexes()
    {
        assignedHexes.Clear();

        foreach(WorldUnit unit in player.unitsWithPaths)
        {
            if (unit.assignedPathTarget != null)
            {
                assignedHexes.Add(unit.assignedPathTarget);
            }
            else
            {
                unitsWithPaths.Remove(unit);
            }
        }

        foreach (WorldHex assignedHex in assignedHexes)
        {
            if (knownPlayerHexes.Contains(assignedHex))
            {
                knownPlayerHexes.Remove(assignedHex);
            }
        }
    }

    
    void FindResearch()
    {
        lookingForResearch = true;
        List<WorldHex> playerCities = new List<WorldHex>(player.playerCities);
        foreach (WorldHex city in playerCities)
        {
            List<WorldHex> cityHexes = new List<WorldHex>(city.cityData.cityHexes);
            foreach (WorldHex cityHex in cityHexes)
            {
                if (cityHex.hexData.hasResource)
                {
                    if (!GameManager.Instance.CanPlayerHarvestResource(player.index, cityHex.hexData.resourceType))
                    {
                        Abilities abilityTarget = GameManager.Instance.GetAbilityOrPreviousTarget(player.index, cityHex.hexData.resourceType);   
                        if (abilitiesTargetToUnlock.ContainsKey(abilityTarget))
                        {
                            abilitiesTargetToUnlock[abilityTarget]++;
                        }
                        else
                        {
                            abilitiesTargetToUnlock.Add(abilityTarget, 1);
                        }
                    }
                }
                else if (!cityHex.hexData.hasResource && !cityHex.hexData.hasBuilding)
                {
                    int forestsWorked = 0;
                    int minesWorked = 0;
                    int farmsWorked = 0;
                    foreach(WorldHex adj in cityHex.adjacentHexes)
                    {
                        if (adj.hexData.hasBuilding && adj.hexData.playerOwnerIndex == GameManager.Instance.activePlayer.index)
                        {
                            if (adj.hexData.buildingType == BuildingType.ForestWorked)
                            {
                                if (!cityHex.parentCity.cityData.masterBuildings.Contains(BuildingType.ForestMaster))
                                {
                                    forestsWorked++;
                                }
                            }
                            else if (adj.hexData.buildingType == BuildingType.FarmWorked)
                            {
                                if (!cityHex.parentCity.cityData.masterBuildings.Contains(BuildingType.FarmMaster))
                                {
                                    farmsWorked++;
                                }
                            }
                            else if (adj.hexData.buildingType == BuildingType.MineWorked)
                            {
                                if (!cityHex.parentCity.cityData.masterBuildings.Contains(BuildingType.MineMaster))
                                {
                                    minesWorked++;
                                }
                            }
                        }
                    }

                    if (player.turnCount > 15)
                    {
                        if (!cityHex.parentCity.cityData.masterBuildings.Contains(BuildingType.Guild))
                        {
                            Abilities abilityTarget = GameManager.Instance.GetAbilityOrPreviousTarget(player.index, BuildingType.Guild);
                            if (!GameManager.Instance.IsAbilityPurchased(player.index, abilityTarget))
                            {
                                if (abilitiesTargetToUnlock.ContainsKey(abilityTarget))
                                {
                                    abilitiesTargetToUnlock[abilityTarget]++;
                                }
                                else
                                {
                                    abilitiesTargetToUnlock.Add(abilityTarget, 1);
                                }
                            }
                        }
                    }
                   

                    if (forestsWorked > 0)
                    {
                        Abilities abilityTarget = GameManager.Instance.GetAbilityOrPreviousTarget(player.index, BuildingType.ForestMaster);
                        if (!GameManager.Instance.IsAbilityPurchased(player.index, abilityTarget))
                        {
                            if (abilitiesTargetToUnlock.ContainsKey(abilityTarget))
                            {
                                abilitiesTargetToUnlock[abilityTarget] += forestsWorked;
                            }
                            else
                            {
                                abilitiesTargetToUnlock.Add(abilityTarget, forestsWorked);
                            }
                        }
                    }

                    if (minesWorked > 0)
                    {
                        Abilities abilityTarget = GameManager.Instance.GetAbilityOrPreviousTarget(player.index, BuildingType.MineMaster);
                        if (!GameManager.Instance.IsAbilityPurchased(player.index, abilityTarget))
                        {
                            if (abilitiesTargetToUnlock.ContainsKey(abilityTarget))
                            {
                                abilitiesTargetToUnlock[abilityTarget] += minesWorked;
                            }
                            else
                            {
                                abilitiesTargetToUnlock.Add(abilityTarget, minesWorked);
                            }
                        }
                    }

                    if (farmsWorked > 0)
                    {
                        Abilities abilityTarget = GameManager.Instance.GetAbilityOrPreviousTarget(player.index, BuildingType.FarmMaster);
                        if (!GameManager.Instance.IsAbilityPurchased(player.index, abilityTarget))
                        {
                            if (abilitiesTargetToUnlock.ContainsKey(abilityTarget))
                            {
                                abilitiesTargetToUnlock[abilityTarget] += farmsWorked;
                            }
                            else
                            {
                                abilitiesTargetToUnlock.Add(abilityTarget, farmsWorked);
                            }
                        }
                    }

                   
                }
            }
        }

        //force the port purchase
        if (player.turnCount > 5)
        {
            if (!GameManager.Instance.IsAbilityPurchased(player.index, Abilities.Port))
            {
                if (abilitiesTargetToUnlock.ContainsKey(Abilities.Port))
                {
                    abilitiesTargetToUnlock[Abilities.Port] += 5;
                }
                else
                {
                    abilitiesTargetToUnlock.Add(Abilities.Port, 5);
                }
            }
        }

        //force to expand into ocean
        if (player.turnCount > 10 && GameManager.Instance.IsAbilityPurchased(player.index, Abilities.Port))
        {
            if (!GameManager.Instance.IsAbilityPurchased(player.index, Abilities.Ocean))
            {
                if (abilitiesTargetToUnlock.ContainsKey(Abilities.Ocean))
                {
                    abilitiesTargetToUnlock[Abilities.Ocean] += 5;
                }
                else
                {
                    abilitiesTargetToUnlock.Add(Abilities.Ocean, 5);
                }
            }
        }

        //find resource to buy - TODO: applicable to all types of research 
        Abilities abilityToBuy = Abilities.NONE;
        int mostWanted = 0;
        foreach (Abilities foundType in abilitiesTargetToUnlock.Keys)
        {
            if (abilitiesTargetToUnlock[foundType] > mostWanted)
            {
                abilityToBuy = foundType;
                mostWanted = abilitiesTargetToUnlock[foundType];
            }
        }

        Debug.Log("Ability AI decided to buy is: " + abilityToBuy);

        if (abilityToBuy != Abilities.NONE)
        {
            if (GameManager.Instance.CanPlayerAfford(player.index, player.abilityDictionary[abilityToBuy].calculatedAbilityCost))
            {
                GameManager.Instance.UnlockAbility(player.index, abilityToBuy, player.showAction(), true);
            }
        }

        lookingForResearch = false;

    }

    IEnumerator UpgradeCities()
    {
        upgradingCities = true;
        List<WorldHex> playerCities = new List<WorldHex>(player.playerCities);
        foreach (WorldHex city in playerCities)
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

            List<WorldHex> cityHexes = new List<WorldHex>(city.cityData.cityHexes);
            foreach (WorldHex cityHex in cityHexes)
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

  
    IEnumerator CheckForActionsAndCombatFirst()
    {
        checkForActions = true;
        List<WorldUnit> remainingUnits = new List<WorldUnit>(player.unitsWithActions);
        foreach (WorldUnit unit in remainingUnits)
        {
            if (CanUnitDoButtonAction(unit))
            {
                player.unitsWithActions.Remove(unit);
                yield return new WaitForSeconds(1f);
                continue;
            }
            else if (CanUnitEngageInCombat(unit))
            {
                player.unitsWithActions.Remove(unit);
                yield return new WaitForSeconds(1f);
                continue;
            }
        }

        checkForActions = false;
    }


    IEnumerator CheckForActivePaths()
    {
        checkForPaths = true;

        List<WorldUnit> unitsWithPathsTemp = new List<WorldUnit>(unitsWithPaths);

        foreach (WorldUnit unit in unitsWithPathsTemp)
        {
            unit.ValidateRemainigActions();

            if (unit.currentMovePoints <= 0 || !unit.isInteractable)
            {
                continue;
            }
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
                if (!assignedHexes.Contains(path[path.Count - 1]))
                {
                    assignedHexes.Add(path[path.Count - 1]);
                }

                if (player.unitsWithActions.Contains(unit))
                {
                    player.unitsWithActions.Remove(unit);
                }

                int pathSteps = path.Count - 1;

                if (turnsToTarget > 1)
                {
                    if (!player.unitsWithPaths.Contains(unit))
                    {
                        player.unitsWithPaths.Add(unit);
                    }

                    pathSteps = unit.currentWalkRange - 1;
                }


                unit.assignedPathTarget = path[path.Count - 1];
                unit.SetMoveTarget(path[pathSteps], true, true, false);
                yield return new WaitForSeconds(1f);
            } 
        }

        checkForPaths = false;
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
                    leastHealth = hex.associatedUnit.localHealth;
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
                if (player.unitsWithActions.Contains(unit))
                {
                    player.unitsWithActions.Remove(unit);
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

                if (player.unitsWithActions.Contains(unit))
                {
                    player.unitsWithActions.Remove(unit);
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
        List<WorldUnit> remainingUnits = new List<WorldUnit>(player.unitsWithActions);

        foreach(WorldUnit unit in remainingUnits)
        {
            if (player.unitsWithPaths.Contains(unit))
            {
                continue;
            }
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

            if (player.unitsWithActions.Contains(unit))
            {
                player.unitsWithActions.Remove(unit);
            }

            int pathSteps = path.Count - 1;
            if (turnsToTarget > 1)
            {
                if (!player.unitsWithPaths.Contains(unit))
                {
                    player.unitsWithPaths.Add(unit);
                }
                
                if (!player.assignedHexes.Contains(hex))
                {
                    player.assignedHexes.Add(hex);
                }

                pathSteps = unit.currentWalkRange - 1;
            }


            unit.assignedPathTarget = hex;
            unit.SetMoveTarget(path[pathSteps], true, true, false);
            return true;
        }
        else
        {
            return false;
        }
    }
 

    public int EvaluateHex(WorldHex hex)
    {
        int hexBaseValue = 0;

        foreach (WorldHex adj in hex.adjacentHexes)
        {
            hexBaseValue += ScoreAdjHex(adj);
        }

        if (hex.hexData.hasCity && !hex.hexData.cityHasBeenClaimed)
        {
            hexBaseValue += cityScore;
        }

        if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
        {
            hexBaseValue += monumentScore;
        }

        return hexBaseValue;
    }

    public int ScoreAdjHex(WorldHex hex)
    {
        int hexBaseValue = 0;

        if (!hex.CanBeReached(player.index))
        {
            hexBaseValue += -1;
        }
        if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex != player.index)
        {
            hexBaseValue += adjToEnemyScore;
        }
        if (hex.hexData.hasCity && !hex.hexData.cityHasBeenClaimed)
        {
            hexBaseValue += adjCityScore;
        }

        if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
        {
            hexBaseValue += adjMonument;
        }
        if (!player.clearedHexes.Contains(hex))
        {
            hexBaseValue += adjHiddenScore;
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

