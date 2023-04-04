using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUnit : MonoBehaviour
{
    int x;
    int y;
    public WorldHex parentTile;
    [SerializeField] Unit data;
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
        data.movePossible = true;
    }

    public void Move(WorldHex hex)
    {
        x = hex.hex.C;
        y = hex.hex.R;
        parentTile.UnitOut(this);
        parentTile = hex;
        parentTile.UnitIn(this.gameObject);
        data.movePossible = false;
        //wiggler?.AnimatedMove(parentTile.transform.localPosition);
        //check if attack possibled
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
