using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    public Hex hex;
    public TileData data;
    Wiggler wiggler;
    [SerializeField] WorldUnit associatedUnit;
    void Start()
    {
        wiggler = GetComponent<Wiggler>();
    }

    public void UnitIn(WorldUnit newUnit)
    {
        data.occupied = true;
        associatedUnit = newUnit;
    }

    public void UnitOut(WorldUnit newUnit)
    {
        data.occupied = false;
        associatedUnit = null;
    }

    public void SpawnCity(GameObject cityPrefab)
    {
        GameObject obj = Instantiate(cityPrefab, transform);
        data.hasCity = true;
    }
    public void Tap(int layer)
    {
        if (UnitManager.Instance.movementSelectMode)
        {
            //UnitManager.Instance.MoveTargetTile(x, y);
            wiggler?.Wiggle();
            return;
        }
        switch (layer)
        {
            case 1:
                Debug.Log("This is the Unit layer");
                if (data.occupied && associatedUnit != null)
                {
                    associatedUnit.Select();
                    wiggler?.Wiggle();
                    return;
                }
                else
                {
                    Select();
                }
                break;
            case 2:
                Debug.Log("This is the resource layer");
                Select();
                break;
        }
    }

    public void Select()
    {
        wiggler?.Wiggle();
    }

    public void Hold()
    {
        wiggler?.Wiggle();
        Debug.Log("This item was long pressed");
    }

}
