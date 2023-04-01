using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    public GameObject startingCity;
    public List<WorldTile> playerCities;
    public List<WorldUnit> worldUnits = new List<WorldUnit>();
    [SerializeField] GameObject unitParent;
    [SerializeField] GameObject[] unitPrefabs;

    public bool movementSelectMode;
    public bool attackSelectMode;
    public WorldUnit selectedUnit;
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
        playerCities.Add(obj.GetComponent<WorldTile>());
    }
    public void InitializeUnits()
    {
        ClearUnits();
        //SpawnUnit(playerCities[0].x, playerCities[0].y);
        SI_EventManager.Instance.OnUnitsPlaced();
    }

    public void ClearUnits()
    {
        foreach (Transform child in unitParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SpawnUnit(int x, int y)
    {
        WorldTile target = MapManager.Instance.GetWorldTile(x, y);
        GameObject obj = Instantiate(unitPrefabs[0], unitParent.transform);
        obj.transform.SetParent(unitParent.transform);
        Vector3 position = new Vector3(x, 1, y);
        obj.transform.localPosition = position;
        obj.GetComponent<WorldUnit>().SetData(x, y, target);
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

    public void MoveTargetTile(int newX, int newY)
    {
        selectedUnit.Move(newX, newY);
        movementSelectMode = false;
    }
}
