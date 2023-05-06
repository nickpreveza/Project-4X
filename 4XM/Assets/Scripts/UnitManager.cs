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

    List<WorldHex> hexesInWalkRange = new List<WorldHex>();
    List<WorldHex> hexesInAttackRange = new List<WorldHex>();

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
            case UnitType.Siege:
                return gameUnits[3];
            case UnitType.Defensive:
                return gameUnits[4];
            case UnitType.Trader:
                return gameUnits[5];
            case UnitType.Diplomat:
                return gameUnits[6];
        }

        Debug.LogError("Unit type given was invalid. Returned default unit");
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
                SpawnUnitAt(player, UnitType.Melee, player.playerCities[0], false, false);
            }
        }
       
        SI_EventManager.Instance.OnUnitsPlaced();
    }


    public void SpawnUnitAt(Player player, UnitType newUnit, WorldHex targetHex, bool exhaustMoves, bool applyCost)
    {
        UnitData unitData = GetUnitDataByType(newUnit, player.civilization);

        if (applyCost)
        {
            GameManager.Instance.RemoveStars(player.index, unitData.cost);
        }

        GameObject obj = Instantiate(unitData.GetPrefab(), targetHex.unitParent.position, Quaternion.identity, targetHex.unitParent);
        obj.transform.localPosition = Vector3.zero;
        WorldUnit unit = obj.GetComponent<WorldUnit>();
        unit.SpawnSetup(targetHex, player.index, unitData, exhaustMoves);

        player.AddUnit(unit);
    }

   
    public void SelectUnit(WorldUnit newUnit)
    {

        if (selectedUnit != null)
        {
            selectedUnit.Deselect();
        }

        ClearHexSelectionMode();

        selectedUnit = newUnit;

        if (GameManager.Instance.activePlayer.index == newUnit.playerOwnerIndex)
        {
           
            if (selectedUnit.currentMovePoints > 0)
            {
                FindWalkableHexes(newUnit.parentHex, newUnit.unitReference.walkRange);
            }

            if (selectedUnit.currentAttackCharges > 0)
            {
                FindAttackableHexes(newUnit.parentHex, newUnit.unitReference.attackRange);
            }

            selectedUnit.ValidateRemainigActions();

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
        if (hexesInAttackRange != null && hexesInAttackRange.Count > 0)
        {
            foreach (WorldHex hex in hexesInAttackRange)
            {
                hex.HideHighlight();
            }
        }

        if (hexesInWalkRange != null && hexesInWalkRange.Count > 0)
        {
            foreach (WorldHex hex in hexesInWalkRange)
            {
                hex.HideHighlight();
            }
        }
    }

    public bool IsHexValidMove(WorldHex hex)
    {
        if (hexesInWalkRange.Contains(hex)) { return true; }
        else { return false; }
    }

    public bool IsHexValidAttack(WorldHex hex)
    {
        if (hexesInAttackRange.Contains(hex)) { return true; }
        else { return false; }
    }

    public void FindWalkableHexes(WorldHex hexCenter, int range)
    {
        startHex = hexCenter;
        hexesInWalkRange = MapManager.Instance.GetHexesListWithinRadius(hexCenter.hexData, range);

        if (hexesInWalkRange.Contains(hexCenter))
        {
            hexesInWalkRange.Remove(hexCenter);
        }

        List<WorldHex> hexesToRemove = new List<WorldHex>();

       
        foreach (WorldHex hex in hexesInWalkRange)
        {
            if (hex.hexData.occupied)
            {
                hexesToRemove.Add(hex);
                continue;
            }

            switch (hex.hexData.type)
            {
                case TileType.DEEPSEA:
                    if (GameManager.Instance.activePlayer.abilities.travelOcean)
                    {
                        hex.ShowHighlight(false);
                    }
                    else
                    {
                        hexesToRemove.Add(hex);
                    }
                    break;
                case TileType.SEA:
                    if (GameManager.Instance.activePlayer.abilities.travelSea)
                    {
                        hex.ShowHighlight(false);
                    }
                    else
                    {
                        hexesToRemove.Add(hex);
                    }
                    break;
                case TileType.MOUNTAIN:
                    if (GameManager.Instance.activePlayer.abilities.travelMountain)
                    {
                        hex.ShowHighlight(false);
                    }
                    else
                    {
                        hexesToRemove.Add(hex);
                    }
                    break;
                case TileType.ICE:
                    hexesToRemove.Add(hex);
                    break;
                case TileType.SAND:
                case TileType.GRASS:
                case TileType.HILL:
                    hex.ShowHighlight(false);
                    break;
            }
        }

        foreach(WorldHex hex in hexesToRemove)
        {
            if (hexesInWalkRange.Contains(hex))
            {
                hexesInWalkRange.Remove(hex);
            }
        }

        if (hexesInWalkRange.Count == 0)
        {
            selectedUnit.noWalkHexInRange = true;
        }
    }

    void FindAttackableHexes(WorldHex hexCenter, int attackRange)
    {
        startHex = hexCenter;
        hexesInAttackRange = MapManager.Instance.GetHexesListWithinRadius(hexCenter.hexData, attackRange);
        List<WorldHex> hexesToRemove = new List<WorldHex>();
        foreach(WorldHex hex in hexesInAttackRange)
        {
            if (!hex.hexData.occupied)
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
            if (hexesInAttackRange.Contains(hex))
            {
                hexesInAttackRange.Remove(hex);
            }
        }

        if (hexesInAttackRange.Count > 0)
        {
            foreach (WorldHex hex in hexesInAttackRange)
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
            unit.AutomoveRandomly();
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
        selectedUnit.Move(hex);
        
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
    public bool defaultLockState;
    public UnitType type;
    public Civilizations civType;
    public int cost;

    [Header("Unit Stats")]
    public int level;
    public int health;
    public int attack;
    public int defense;
    public int walkRange;
    public int attackRange;

    public int attackCharges;
    public int moveCharges;

    [Header("Unit Abilities")]
    public bool canAttackAfterMove;
    public bool canMoveAfterAttack;

    public GameObject[] prefabs; //associatedToUnitLevels if we have the time

    public GameObject GetPrefab()
    {
        if (prefabs.Length > level)
        {
            return prefabs[level];
        }
        else
        {
            return prefabs[0];
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
    Diplomat
}
