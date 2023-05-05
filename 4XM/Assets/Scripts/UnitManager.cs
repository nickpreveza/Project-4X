using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using System;
public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    public UnitStruct[] gameUnits; 

    public bool hexSelectMode;

    public WorldUnit selectedUnit;
    public Material unitActive;
    public Material unitUsed;
    public Material highlightHex;

    List<WorldHex> hexesInWalkRange = new List<WorldHex>();
    List<WorldHex> hexesInAttackRange = new List<WorldHex>();

    public WorldHex startHex;
    bool waitingForCameraPan;
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

    public UnitStruct GetUnitByType(UnitType type)
    {
        switch (type)
        {
            case UnitType.Swordsman:
                return gameUnits[0];
            case UnitType.Archer:
                return gameUnits[1];
            case UnitType.Horseman:
                return gameUnits[2];
            case UnitType.Trebuchet:
                return gameUnits[3];
            case UnitType.Shields:
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

            player.UpdateAvailableUnits();

            if (player.playerUnits.Count > 0)
            {
                //TODO: place the units of each players
            }
            else
            {
                //spanw a unit at the first city of each player
                SpawnUnitAt(player.index, UnitType.Swordsman, player.playerCities[0], false, false);
            }
        }
       
        SI_EventManager.Instance.OnUnitsPlaced();
    }


    public void SpawnUnitAt(int playerIndex, UnitType newUnit, WorldHex targetHex, bool exhaustMoves, bool applyCost)
    {
        UnitStruct unitData = GetUnitByType(newUnit);

        if (applyCost)
        {
            GameManager.Instance.RemoveStars(playerIndex, unitData.cost);
        }

        GameObject obj = Instantiate(unitData.prefab, targetHex.unitParent.position, Quaternion.identity, targetHex.unitParent);
        obj.transform.localPosition = Vector3.zero;
        WorldUnit unit = obj.GetComponent<WorldUnit>();
        unit.SpawnSetup(targetHex, playerIndex, exhaustMoves);

        GameManager.Instance.GetPlayerByIndex(playerIndex).AddUnit(unit);
    }

   
    public void SelectUnit(WorldUnit newUnit)
    {
        ClearFoundHexes();

        if (selectedUnit != null)
        {
            selectedUnit.Deselect();
        }

        selectedUnit = newUnit;

        if (GameManager.Instance.activePlayer.index == newUnit.data.associatedPlayerIndex)
        {
            FindActionableHexes(newUnit.parentHex, newUnit.data.range);
            selectedUnit.ValidateRemainigActions();

            if (selectedUnit.CanMove || selectedUnit.CanAttack)
            {
                hexSelectMode = true;
            }

            if (!selectedUnit.CanMove && !selectedUnit.CanAttack)
            {
                hexSelectMode = false;
                ClearFoundHexes();
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
            selectedUnit.data.hasNoValidHexesInRange = true;
        }


    }

    void FindAttackableHexes(WorldHex hexCenter, int range)
    {
        startHex = hexCenter;
        hexesInAttackRange = MapManager.Instance.GetEnemiesInRange(hexCenter.hexData, range);

        selectedUnit.data.enemiesInRange = hexesInAttackRange.Count;
        selectedUnit.ValidateRemainigActions();

        if (hexesInAttackRange.Count > 0)
        {
            foreach (WorldHex hex in hexesInAttackRange)
            {
                hex.ShowHighlight(true);
            }
        }

        /*
        if (hexesInAttackRange.Contains(hexCenter))
        {
            hexesInAttackRange.Remove(hexCenter);
        }

        List<WorldHex> hexesToRemove = new List<WorldHex>();

        foreach (WorldHex hex in hexesInAttackRange)
        {
            if (!hex.hexData.occupied)
            {
                hexesToRemove.Add(hex);
                continue;
            }

            if (hex.associatedUnit.BelongsToActivePlayer)
            {
                hexesToRemove.Add(hex);
                continue;
            }

            hex.ShowHighlight(true);
        }

        foreach (WorldHex hex in hexesToRemove)
        {
            if (hexesInAttackRange.Contains(hex))
            {
                hexesInAttackRange.Remove(hex);
            }
        }*/

    }
    void FindActionableHexes(WorldHex hexCenter, int range)
    {
        FindWalkableHexes(hexCenter, range);
        FindAttackableHexes(hexCenter, range);
       
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
public struct UnitStruct
{
    public string unitName;
    public bool defaultLockState;
    public UnitType type;
    public int cost;
    public GameObject prefab;
}

public enum UnitType
{
    Swordsman,
    Archer,
    Horseman,
    Trebuchet,
    Shields,
    Trader,
    Diplomat
}
