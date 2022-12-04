using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    public int x;
    public int y;
    public TileData data;
    Wiggler wiggler;
    [SerializeField] WorldUnit associatedUnit;
    void Start()
    {
        wiggler = GetComponent<Wiggler>();
    }

    public void SetData(int tX, int tY)
    {
        x = tX;
        y = tY;
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
    public void Tap(int layer)
    {
        if (UnitManager.Instance.movementSelectMode)
        {
            UnitManager.Instance.MoveTargetTile(x, y);
            return;
        }
        switch (layer)
        {
            case 1:
                Debug.Log("This is the Unit layer");
                if (data.occupied && associatedUnit != null)
                {
                    associatedUnit.Select();
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
