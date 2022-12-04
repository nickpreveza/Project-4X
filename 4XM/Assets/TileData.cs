using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    public int x, y;
    public int moveCost;
    public TileType type;
    public TraverseRequirment requirment;
    public bool occupied;
    public bool hasCity;
    public TileData(int tX, int tY, int tmC, TileType tType, TraverseRequirment tRequirment, bool tOccupied)
    {
        x = tX;
        y = tY;
        moveCost = tmC;
        type = tType;
        requirment = tRequirment;
        occupied = tOccupied;
    }
}

public enum TraverseRequirment
{
    NONE,
    CLIMBING,
    SEA1,
    SEA2
}
