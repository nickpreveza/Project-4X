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

    WorldHex[] highlightedHexes;
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
        SI_EventManager.Instance.onAutopanCompleted += AutoPanCallback;
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
                SpawnUnitAt(player.index, unitTestPrefab, player.playerCities[0]);
            }
        }
       
        SI_EventManager.Instance.OnUnitsPlaced();
    }


    public void SpawnUnitAt(int playerIndex, GameObject prefab, WorldHex targetHex)
    {
        GameObject obj = Instantiate(prefab, targetHex.unitParent.position, Quaternion.identity, targetHex.unitParent);
        obj.transform.localPosition = Vector3.zero;
        WorldUnit unit = obj.GetComponent<WorldUnit>();
        unit.SpawnSetup(targetHex, playerIndex);

        GameManager.Instance.GetPlayerByIndex(playerIndex).AddUnit(unit);
    }

    public void OnUnitMoved(WorldHex oldHex, WorldHex newHex)
    {

    }
     
    public void SelectUnit(WorldUnit newUnit)
    {
        ClearHighlightedHexes();

        if (selectedUnit != null)
        {
            selectedUnit.Deselect();
        }

        selectedUnit = newUnit;
        movementSelectMode = true;

        HighlightHexes(newUnit.parentHex, newUnit.data.range);
    }

    public void ClearHighlightedHexes()
    {
        if (highlightedHexes != null && highlightedHexes.Length > 0)
        {
            foreach (WorldHex hex in highlightedHexes)
            {
                hex.HideHighlight();
            }
        }
    }
    public void HighlightHexes(WorldHex hexCenter, int range)
    {
        startHex = hexCenter;
        highlightedHexes = MapManager.Instance.GetHexesWithinRadiusOf(hexCenter, range);
        foreach (WorldHex hex in highlightedHexes)
        {
            if (hex == hexCenter)
            {
                continue;
            }
            hex.ShowHighlight();
        }
    }

    void AutoPanCallback(int hexIdentifier)
    {
        waitingForCameraPan = false;
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
        ClearHighlightedHexes();
        movementSelectMode = false;
    }

    public bool IsHexValidMove(WorldHex hex)
    {
        return Array.Exists(highlightedHexes, element => element == hex);
    }

    public void MoveTargetTile(WorldHex hex)
    {
        ClearHighlightedHexes();
        selectedUnit.Move(hex);
        movementSelectMode = false;
    }
}
