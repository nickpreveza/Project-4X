using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    public GameObject startingCity;
    public List<WorldHex> playerCities;
    public List<GameObject> worldUnits = new List<GameObject>();
    [SerializeField] GameObject unitParent;
    [SerializeField] GameObject[] unitPrefabs;

    public bool movementSelectMode;
    public bool attackSelectMode;
    public WorldUnit selectedUnit;

    public GameObject unitTestPrefab;
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

    public void SetStartingCity(GameObject obj)
    {
        startingCity = obj;
        playerCities.Clear();
        playerCities.Add(obj.GetComponent<WorldHex>());
    }
    public void InitializeUnits()
    {
        ClearUnits();
        SpawnUnitAt(unitTestPrefab, playerCities[0]);
        SI_EventManager.Instance.OnUnitsPlaced();
    }

    public void ClearUnits()
    {
        foreach (Transform child in unitParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SpawnUnitAt(GameObject prefab, WorldHex targetHex)
    {
        int c = targetHex.hex.C;
        int r = targetHex.hex.R;

        GameObject obj = Instantiate(prefab, targetHex.unitParent.position, Quaternion.identity, targetHex.unitParent);
        targetHex.UnitIn(obj);
        obj.transform.localPosition = Vector3.zero;
        obj.GetComponent<WorldUnit>().SetData(c, r, targetHex);

        worldUnits.Add(obj);
    }

    public void OnUnitMoved(WorldHex oldHex, WorldHex newHex)
    {

    }
     
    public void SelectUnit(WorldUnit newUnit)
    {
        if (selectedUnit != null)
        {
            selectedUnit.Deselect();
        }

        selectedUnit = newUnit;
        movementSelectMode = true;
    }

    public void MoveTargetTile(WorldHex hex)
    {
        selectedUnit.Move(hex);
        movementSelectMode = false;
    }
}
