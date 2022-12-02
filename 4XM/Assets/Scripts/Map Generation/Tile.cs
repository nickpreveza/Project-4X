using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] TileData data;
}

[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public int movePoints;
    public TileType type;
    public bool occupied;
}

public enum TileType
{
    ICE = 0,
    DEEPSEA = 1,
    SEA = 2,
    SAND = 3,
    GRASS = 4,
    HILL = 5,
    MOUNTAIN = 6
}
