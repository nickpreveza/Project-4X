using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using System;
public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    [SerializeField] GameObject[] unitPrefabs;

    public bool movementSelectMode;
    public bool attackSelectMode;
    public WorldUnit selectedUnit;

    public GameObject unitTestPrefab;

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
    }

    public void OnUnitMovedCallback(WorldHex oldHex, WorldHex newHex)
    {

    }

    void OnAutopanCompletedCallback(int hexIdentifier)
    {
        waitingForCameraPan = false;
    }



    public void InitializeStartUnits()
    {
        foreach(Player player in GameManager.Instance.sessionPlayers)
        {
            if (player.playerUnits.Count > 0)
            {
                //TODO: place the units of each players
            }
            else
            {
                //spanw a unit at the first city of each player
                SpawnUnitAt(player.index, unitTestPrefab, player.playerCities[0], false);
            }
        }
       
        SI_EventManager.Instance.OnUnitsPlaced();
    }


    public void SpawnUnitAt(int playerIndex, GameObject prefab, WorldHex targetHex, bool exhaustMoves)
    {
        GameObject obj = Instantiate(prefab, targetHex.unitParent.position, Quaternion.identity, targetHex.unitParent);
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
            if (selectedUnit.CanMove && selectedUnit.CanAttack)
            {
                movementSelectMode = true;
                FindActionableHexes(newUnit.parentHex, newUnit.data.range);
            }
            else if (selectedUnit.CanMove && !selectedUnit.CanAttack)
            {

            }
            else if (!selectedUnit.CanMove && selectedUnit.CanAttack)
            {

            }
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

    public void FindWalkableHexes(WorldHex hexCenter, int range)
    {
        startHex = hexCenter;
        hexesInWalkRange = MapManager.Instance.GetHexesListWithinRadius(hexCenter.hexData, range);

        if (hexesInWalkRange.Contains(hexCenter))
        {
            hexesInWalkRange.Remove(hexCenter);
        }

        foreach (WorldHex hex in hexesInWalkRange)
        {
            if (hex.hexData.occupied)
            {
                hexesInWalkRange.Remove(hex);
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
                        hexesInWalkRange.Remove(hex);
                    }
                    break;
                case TileType.SEA:
                    if (GameManager.Instance.activePlayer.abilities.travelSea)
                    {
                        hex.ShowHighlight(false);
                    }
                    else
                    {
                        hexesInWalkRange.Remove(hex);
                    }
                    break;
                case TileType.MOUNTAIN:
                    if (GameManager.Instance.activePlayer.abilities.travelMountain)
                    {
                        hex.ShowHighlight(false);
                    }
                    else
                    {
                        hexesInWalkRange.Remove(hex);
                    }
                    break;
            }
        }
    }

    public void FindAttackableHexes(WorldHex hexCenter, int range)
    {
        startHex = hexCenter;
        hexesInAttackRange = MapManager.Instance.GetHexesListWithinRadius(hexCenter.hexData, range);
        if (hexesInAttackRange.Contains(hexCenter))
        {
            hexesInAttackRange.Remove(hexCenter);
        }

        foreach (WorldHex hex in hexesInAttackRange)
        {
            if (!hex.hexData.occupied)
            {
                hexesInAttackRange.Remove(hex);
                continue;
            }

            if (hex.associatedUnit.BelongsToActivePlayer)
            {
                hexesInAttackRange.Remove(hex);
                continue;
            }

            hex.ShowHighlight(true);
        }

        

    }
    public void FindActionableHexes(WorldHex hexCenter, int range)
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

    

    public void CancelMoveMode()
    {
        ClearFoundHexes();
        movementSelectMode = false;
    }

    public void MoveToTargetHex(WorldHex hex)
    {
        ClearFoundHexes();
        selectedUnit.Move(hex);
        movementSelectMode = false;
    }
}
