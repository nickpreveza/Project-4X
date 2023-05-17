using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using System;
public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    public UnitData[] gameUnits; 

    public bool hexSelectMode;

    public WorldUnit selectedUnit;
    public Material unitActive;
    public Material unitUsed;
    public Material highlightHex;

    public WorldHex startHex;
    bool waitingForCameraPan;

    List<WorldHex> selectedWalkList = new List<WorldHex>();
    List<WorldHex> selectedAttackList = new List<WorldHex>();

    [SerializeField] GameObject emptyUnitPrefab;

    public GameObject unitSpawnParticle;
    public GameObject unitAttackParticle;
    public GameObject unitDeathParticle;
    public GameObject unitHealParticle;
    public GameObject unitHitParticle;
    public GameObject unitWalkParticle;
    public GameObject unitSelectParticle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        SI_EventManager.Instance.onAutopanCompleted += OnAutopanCompletedCallback;
        SI_EventManager.Instance.onAbilityUnlocked += OnAbilityUnlockUpdate;
    }

    public void OnUnitMovedCallback(WorldHex oldHex, WorldHex newHex)
    {

    }

    public void OnAbilityUnlockUpdate(int playerIndex)
    {
        if (GameManager.Instance.IsIndexOfActivePlayer(playerIndex))
        {
            if (hexSelectMode)
            SelectUnit(selectedUnit);
        }
    }

    void OnAutopanCompletedCallback(int hexIdentifier)
    {
        waitingForCameraPan = false;
    }


    public UnitData GetUnitDataByType(UnitType type, Civilizations civilization)
    {
        Civilization currentCiv = GameManager.Instance.GetCivilizationByType(civilization);

        if (currentCiv != null)
        {
            if (currentCiv.unitDictionary.ContainsKey(type))
            {
                return currentCiv.GetUnitOverride(type);
            }
        }

        switch (type)
        {
            case UnitType.Melee:
                return gameUnits[0];
            case UnitType.Ranged:
                return gameUnits[1];
            case UnitType.Cavalry:
                return gameUnits[2];
            case UnitType.Boat:
                return gameUnits[3];
            case UnitType.Ship:
                return gameUnits[4];
            case UnitType.Defensive: //This is now a trader for VC
                return gameUnits[5];
            case UnitType.Siege:
                return gameUnits[6];
            case UnitType.Lance:
                return gameUnits[7];
            case UnitType.Knight:
                return gameUnits[8];
            case UnitType.Trader:
                return gameUnits[5];

        }

        Debug.LogError("Unit type given was not handled. Returned default unit");
        return gameUnits[0];

    }

    public void InitializeStartUnits()
    {
        foreach(Player player in GameManager.Instance.sessionPlayers)
        {
            player.GenerateUnitDictionary();

            player.UpdateAvailableUnitsFromAbilities();

            if (player.playerUnits.Count > 0)
            {
                //TODO: place the units of each players
            }
            else
            {
                //spanw a unit at the first city of each player
                SpawnUnitAt(player, UnitType.Melee, player.playerCities[0], false, false, true);
            }
        }
       
        SI_EventManager.Instance.OnUnitsPlaced();
    }

    public void SpawnUnitAt(Player player, UnitType newUnit, WorldHex targetHex, bool exhaustMoves, bool applyCost, bool addTocityPopulation)
    {
        UnitData unitData = GetUnitDataByType(newUnit, player.civilization);

        if (applyCost)
        {
            GameManager.Instance.RemoveStars(player.index, unitData.cost);
        }
        player.AddScore(3, unitData.scoreForPlayer);

        GameObject obj = Instantiate(emptyUnitPrefab, targetHex.unitParent.position, Quaternion.identity, targetHex.unitParent);

        obj.transform.localPosition = Vector3.zero;
        targetHex.SpawnParticle(unitHealParticle);
        WorldUnit unit = obj.GetComponent<WorldUnit>();
        player.AddUnit(unit);
        unit.SpawnSetup(targetHex, player.index, unitData, exhaustMoves, addTocityPopulation);
    }

    public void SelectUnit(WorldUnit newUnit)
    {
        
        if (selectedUnit != null)
        {
            selectedUnit.Deselect();
        }

        ClearHexSelectionMode();

        selectedUnit = newUnit;

        if (GameManager.Instance.activePlayer.index == selectedUnit.playerOwnerIndex)
        {
            if (selectedUnit.currentMovePoints > 0)
            {
                if (selectedUnit.hasAttacked && !selectedUnit.unitReference.canAttackAfterMove)
                {
                    selectedWalkList.Clear();
                    selectedUnit.noWalkHexInRange = true;
                }
                else
                {
                    selectedWalkList = GetWalkableHexes(newUnit, newUnit.currentWalkRange);
                    if (selectedWalkList.Count == 0)
                    {
                        selectedUnit.noWalkHexInRange = true;
                    }
                    else
                    {
                        foreach (WorldHex hex in selectedWalkList)
                        {
                            hex.ShowHighlight(false);
                        }
                    }
                }
               
            }

            if (selectedUnit.currentAttackCharges > 0)
            {
                selectedAttackList = GetAttackableHexes(newUnit);
                if (selectedAttackList.Count > 0)
                {
                    foreach (WorldHex hex in selectedAttackList)
                    {
                        hex.ShowHighlight(true);
                    }

                    selectedUnit.noAttackHexInRange = false;
                }
                else
                {
                    selectedUnit.noAttackHexInRange = true;
                }
            }

            if (selectedUnit.isBoat || selectedUnit.isShip)
            {
                selectedUnit.ValidateRemainigActions(selectedUnit.boatReference);
            }
            else
            {
                selectedUnit.ValidateRemainigActions(selectedUnit.unitReference);
            }
          

            if (selectedUnit.isInteractable)
            {
                if (!selectedUnit.noAttackHexInRange || !selectedUnit.noWalkHexInRange)
                hexSelectMode = true;
            }

            UIManager.Instance.ShowHexView(newUnit.parentHex, newUnit);
        }
    }

    public void ClearFoundHexes()
    {
        if (selectedAttackList != null && selectedAttackList.Count > 0)
        {
            foreach (WorldHex hex in selectedAttackList)
            {
                hex.HideHighlight();
            }
        }

        if (selectedWalkList != null && selectedWalkList.Count > 0)
        {
            foreach (WorldHex hex in selectedWalkList)
            {
                hex.HideHighlight();
            }
        }
    }

    public bool IsHexValidMove(WorldHex hex)
    {
        if (selectedWalkList.Contains(hex) && selectedUnit.currentMovePoints > 0) { return true; }
        else { return false; }
    }

    public bool IsHexValidAttack(WorldHex hex)
    {
        if (selectedAttackList.Contains(hex) && selectedUnit.currentAttackCharges > 0) { return true; }
        else { return false; }
    }

    public bool isUnitInAttackRange(WorldUnit attackingUnit, WorldUnit receivingUnit)
    {
        int attackingUnitRange = 0;

        if (attackingUnit.isBoat || attackingUnit.isShip)
        {
            attackingUnitRange = attackingUnit.boatReference.attackRange;
        }
        else
        {

            attackingUnitRange = attackingUnit.unitReference.attackRange;
        }

        //

        List<WorldHex> hexesInRange = MapManager.Instance.GetHexesListWithinRadius(attackingUnit.parentHex.hexData, attackingUnitRange);
        List<WorldHex> hexesToRemove = new List<WorldHex>();

        foreach (WorldHex hex in hexesInRange)
        {
            if (hex == receivingUnit.parentHex)
            {
                return true;
            }
        }

        return false;

    }

    public List<WorldHex> FindMultiturnPath(WorldUnit unit, WorldHex end, out int turnsToTarget)
    {
        turnsToTarget = 0;
        WorldHex start = unit.parentHex;
        Player player = GameManager.Instance.GetPlayerByIndex(unit.playerOwnerIndex);
        List<WorldHex> openSet = new List<WorldHex>();
        List<WorldHex> closedSet = new List<WorldHex>();

        openSet.Add(start);

        bool startedFromSea = false;
        if (start.hexData.type == TileType.DEEPSEA || start.hexData.type == TileType.SEA)
        {
            startedFromSea = true;
        }

        start.hexData.gCost = 0;
        start.hexData.hCost = 0;

        while (openSet.Count > 0)
        {
            WorldHex current = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].hexData.fCost < current.hexData.fCost ||
                    openSet[i].hexData.fCost == current.hexData.fCost && openSet[i].hexData.hCost < openSet[i].hexData.hCost)
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);

            closedSet.Add(current);

            if (current == end)
            {
                List<WorldHex> path = new List<WorldHex>();
                WorldHex retractCurrent = end;

                while (retractCurrent != start)
                {
                    path.Add(retractCurrent);
                    retractCurrent = retractCurrent.pathParent;
                }

                path.Reverse();
                turnsToTarget = Mathf.RoundToInt((float)path.Count / (float)unit.unitReference.walkRange);
                Debug.Log("Unit: " + unit.unitReference.name + "Tile: " + end.name + " Turns to target tile: " + turnsToTarget);
                return path;
            }

            bool shouldSkipAdj = false;

            if (!startedFromSea)
            {
                if (current.hexData.type == TileType.DEEPSEA || current.hexData.type == TileType.SEA)
                {
                    shouldSkipAdj = true;
                }
            }
            else
            {
                if (current.hexData.type != TileType.DEEPSEA && current.hexData.type != TileType.SEA)
                {
                    shouldSkipAdj = true;
                }
            }

            if (!shouldSkipAdj)
            {
                foreach (WorldHex adj in current.adjacentHexes)
                {

                    if (closedSet.Contains(adj) || !adj.CanBeWalked(unit.playerOwnerIndex, unit.unitReference.flyAbility, adj == end)) //can't afford to enter
                    {
                        continue;
                    }

                    //distance from this tile to the next 
                    int distanceCost = MapManager.Instance.GetDistance(current, adj);
                    int movementCostToAdj = current.hexData.gCost + distanceCost;
                    if (movementCostToAdj < adj.hexData.gCost || !openSet.Contains(adj))
                    {
                        adj.hexData.gCost = movementCostToAdj;
                        int distancCostforH = MapManager.Instance.GetDistance(adj, end);
                        adj.hexData.hCost = distancCostforH;
                        adj.pathParent = current;

                        if (!openSet.Contains(adj))
                        {
                            openSet.Add(adj);
                        }
                    }

                }
            }
        }

        return null;
    }
    public List<WorldHex> FindPath(WorldUnit unit, WorldHex start, WorldHex end, bool roadCheck = false)
    {
        Player player = GameManager.Instance.GetPlayerByIndex(unit.playerOwnerIndex);
        List<WorldHex> openSet = new List<WorldHex>();
        List<WorldHex> closedSet = new List<WorldHex>();

        openSet.Add(start);
        bool startedFromSea = false;

        int maxDistance = unit.currentWalkRange;
        if (roadCheck)
        {
            maxDistance += unit.unitReference.roadModifier;
        }

        //if parent hex has road also check for taht 
        if (start.hexData.type == TileType.DEEPSEA || start.hexData.type == TileType.SEA)
        {
            startedFromSea = true;
            if (roadCheck)
            {
                return null;
            }
        }

        start.hexData.gCost = 0;
        start.hexData.hCost = 0;

        while (openSet.Count > 0)
        {
            WorldHex current = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].hexData.fCost < current.hexData.fCost ||
                    openSet[i].hexData.fCost == current.hexData.fCost && openSet[i].hexData.hCost < openSet[i].hexData.hCost)
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);

            closedSet.Add(current);

            if (current == end)
            {
                List<WorldHex> path = new List<WorldHex>();
                WorldHex retractCurrent = end;

                while (retractCurrent != start)
                {
                    path.Add(retractCurrent);
                    retractCurrent = retractCurrent.pathParent;
                }

                path.Reverse();
                return path;
            }

            bool shouldSkipAdj = false;

            if (!startedFromSea)
            {
                if (current.hexData.type == TileType.DEEPSEA || current.hexData.type == TileType.SEA)
                {
                    shouldSkipAdj = true;
                }
            }
            else
            {
                if (current.hexData.type != TileType.DEEPSEA && current.hexData.type != TileType.SEA)
                {
                    shouldSkipAdj = true;
                }
            }

            if (roadCheck)
            {
                if (!current.hexData.hasRoad)
                {
                    shouldSkipAdj = true;
                }
            }


            if (!shouldSkipAdj)
            {
                foreach (WorldHex adj in current.adjacentHexes)
                {
                    
                    if (closedSet.Contains(adj) || !adj.CanBeWalked(unit.playerOwnerIndex, unit.unitReference.flyAbility, adj == end)) //can't afford to enter
                    {
                        continue;
                    }

                    //distance from the starting tile
                    int distanceFromSource = MapManager.Instance.GetDistance(start, adj);
                    if (distanceFromSource > maxDistance)
                    {
                        continue;
                    }

                    //distance from this tile to the next 
                    int distanceCost = MapManager.Instance.GetDistance(current, adj);
                    int movementCostToAdj = current.hexData.gCost + distanceCost;
                    if (movementCostToAdj <= maxDistance)
                    {
                        if (movementCostToAdj < adj.hexData.gCost || !openSet.Contains(adj))
                        {
                            adj.hexData.gCost = movementCostToAdj;
                            int distancCostforH = MapManager.Instance.GetDistance(adj, end);
                            adj.hexData.hCost = distancCostforH;
                            adj.pathParent = current;

                            if (!openSet.Contains(adj))
                            {
                                openSet.Add(adj);
                            }
                        }
                    }
                   
                }
            }
        }

        return null;
    }

    public List<WorldHex> GetWalkableHexes(WorldUnit targetUnit, int customRange = -1)
    {
        int range = targetUnit.currentWalkRange;
        startHex = targetUnit.parentHex;
        int roadModifier = targetUnit.unitReference.roadModifier; // this should probably be in the unit reference, but alas, don't think we're gonna change it. 

        if (customRange != -1)
        {
            range = customRange;
        }

        List<WorldHex> hexesInGeneralRange = MapManager.Instance.GetHexesListWithinRadius(startHex.hexData, range);

        if (hexesInGeneralRange.Contains(startHex))
        {
            hexesInGeneralRange.Remove(startHex);
        }

        List<WorldHex> hexesToRemove = new List<WorldHex>();

        bool flyRelatedAbility = false;

        if (targetUnit.unitReference != null)
        {
            flyRelatedAbility = targetUnit.unitReference.flyAbility;
        }

        foreach (WorldHex hex in hexesInGeneralRange)
        {
            if (!hex.CanBeWalked(targetUnit.playerOwnerIndex, flyRelatedAbility))
            {
                hexesToRemove.Add(hex);
                continue;
            }
        }

        foreach(WorldHex hex in hexesToRemove)
        {
            if (hexesInGeneralRange.Contains(hex))
            {
                hexesInGeneralRange.Remove(hex);
            }
        }

        List<WorldHex> checkedHexes = new List<WorldHex>();

        foreach(WorldHex hex in hexesInGeneralRange)
        {
            List<WorldHex> pathToHex = FindPath(targetUnit, targetUnit.parentHex, hex);
            if (pathToHex != null)
            {
                checkedHexes.Add(hex);
            }
        }

        if (targetUnit.parentHex.hexData.hasRoad)
        {
            List<WorldHex> roadConnectedHexes = GetRoadPaths(targetUnit, checkedHexes);

            if (roadConnectedHexes != null)
            {
                foreach (WorldHex hex in roadConnectedHexes)
                {
                    checkedHexes.Add(hex);
                }
            }
        }
    
        return checkedHexes;
    }

    public List<WorldHex> GetRoadPaths(WorldUnit targetUnit, List<WorldHex> checkHexes)
    {
        //call this after the normal GetWalkHexes to cross Reference;
        int range = targetUnit.currentWalkRange + targetUnit.unitReference.roadModifier;
        startHex = targetUnit.parentHex;

        if (!startHex.hexData.hasRoad)
        {
            return null;
        }

        //get the range we'd have if everything was connected with a raod
        //possible optimization here to have a function to return us only the hexes at the specific range. So if road adds +1, we check for unit.range + roadrange only
        List<WorldHex> hexesInGeneralRange = MapManager.Instance.GetHexesListWithinRadius(startHex.hexData, range);
        //remove the start hex
        if (hexesInGeneralRange.Contains(startHex))
        {
            hexesInGeneralRange.Remove(startHex);
        }

        //remove the hexes we can already reach and have validated in previous step
        List<WorldHex> crossReferenceHexes = new List<WorldHex>();

        foreach(WorldHex hex in hexesInGeneralRange)
        {
            if (!checkHexes.Contains(hex))
            {
                crossReferenceHexes.Add(hex);
            }
        }

        //Try to find paths for each new hex;
        List<WorldHex> hexesWithPath = new List<WorldHex>();
        foreach (WorldHex hex in crossReferenceHexes)
        {
            List<WorldHex> pathToHex = FindPath(targetUnit, targetUnit.parentHex, hex, true);
            if (pathToHex != null)
            {
                hexesWithPath.Add(hex);
            }
        }

        return hexesWithPath;

    }


    public List<WorldHex> GetAttackableHexes(WorldUnit targetUnit)
    {
        startHex = targetUnit.parentHex;
        int range = targetUnit.currentAttackRange;

        List<WorldHex> hexesInGeneralRange = MapManager.Instance.GetHexesListWithinRadius(startHex.hexData, range);
        List<WorldHex> hexesToRemove = new List<WorldHex>();
        foreach(WorldHex hex in hexesInGeneralRange)
        {
            if (!hex.hexData.occupied)
            {
                hexesToRemove.Add(hex);
                continue;
            }
            if (hex.Hidden())
            {
                hexesToRemove.Add(hex);
                continue;
            }

            if (hex.hexData.occupied && hex.associatedUnit.BelongsToActivePlayer)
            {
                hexesToRemove.Add(hex);
                continue;
            }
        }

        foreach (WorldHex hex in hexesToRemove)
        {
            if (hexesInGeneralRange.Contains(hex))
            {
                hexesInGeneralRange.Remove(hex);
            }
        }

        return hexesInGeneralRange;
    }
   
    public void PlayRandomTurnForAIUnits(Player player)
    {
        StartCoroutine(TestRandomAIMove(player));
    }

    IEnumerator TestRandomAIMove(Player player)
    {
        foreach (WorldUnit unit in player.playerUnits)
        {
            waitingForCameraPan = true;
            SI_CameraController.Instance.PanToHex(unit.parentHex);
            yield return new WaitForSeconds(1);
            while (waitingForCameraPan)
            {
                yield return new WaitForSeconds(.1f);
            }
            waitingForCameraPan = true;
            //unit.AutomoveRandomly();
            yield return new WaitForSeconds(0.5f);
            while (waitingForCameraPan)
            {
                yield return new WaitForSeconds(.1f);
            }
        }

        GameManager.Instance.LocalEndTurn();
    }

    

    public void ClearHexSelectionMode()
    {
        ClearFoundHexes();
        hexSelectMode = false;
        selectedUnit = null;
    }

    public void MoveToTargetHex(WorldHex hex)
    {
        ClearFoundHexes();
        hexSelectMode = false;
        selectedUnit.SetMoveTarget(hex, true, false, false);
    }

    public void AttackTargetUnitInHex(WorldHex hex)
    {
        ClearFoundHexes();
        hexSelectMode = false;
        selectedUnit.Attack(hex);
    }

   
}


[System.Serializable]
public class UnitData
{
    public string name;
    public Sprite icon;
    public bool defaultLockState;
    public UnitType type;
    public Civilizations civType;
    public int cost;
    public int scoreForPlayer;

    [Header("Unit Stats")]
    public int level = 1;
    public int health;
    public int heal;
    public int attack;
    public int counterAttack;
    public int defense;
    public int walkRange;
    public int attackRange;
    public int roadModifier = 1;

    public int attackCharges;
    public int moveCharges;

    [Header("Unit Abilities")]
    public bool flyAbility;
    public bool attackContinuisly;
    public bool canHeal;
    public bool healAtTurnEnd;
    public bool canAttackAfterMove;
    public bool canMoveAfterAttack;

    public GameObject[] visualPrefab; //associatedToUnitLevels if we have the time

    public GameObject GetPrefab()
    {
        if (visualPrefab.Length > level-1)
        {
            return visualPrefab[level-1];
        }
        else
        {
            return visualPrefab[0];
        }
    }

}

public enum UnitType
{
    Melee,
    Ranged,
    Cavalry,
    Siege,
    Defensive,
    Trader,
    Diplomat,
    Boat,
    Ship,
    Lance,
    Leader,
    Knight
}
