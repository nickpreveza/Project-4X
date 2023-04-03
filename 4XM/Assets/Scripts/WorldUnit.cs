using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUnit : MonoBehaviour
{
    int x;
    int y;
    public WorldHex parentTile;
    [SerializeField] UnitData data;
    Wiggler wiggler;
    void Start()
    {
        wiggler = GetComponent<Wiggler>();
    }

    public void SetData(int tX, int tY, WorldHex tTile)
    {
        x = tX;
        y = tY;
        parentTile = tTile;
        parentTile.UnitIn(this);
        data.movePossible = true;
    }

    public void Move(int tX, int tY)
    {
        x = tX;
        y = tY;
        parentTile.UnitOut(this);
        parentTile = MapManager.Instance.GetWorldTile(x, y);
        parentTile.UnitIn(this);
        data.movePossible = false;
        wiggler?.AnimatedMove(parentTile.transform.localPosition);
        //check if attack possible
    }

    public void Select()
    {
        wiggler?.Wiggle();
        UnitManager.Instance.SelectUnit(this);
    }

    public void Deselect()
    {

    }
}
