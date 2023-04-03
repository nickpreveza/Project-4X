using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hex 
{
    public void SetData(int c, int r)
    {
        C = c;
        R = r;
        S = -(c + r);
    }

    public int C; //Column
    public int R; //Row
    public int S;

    public int moveCost;
    public TileType type;
    public TraverseRequirment requirment;
    public bool occupied;
    public bool hasCity;

    //Data for map generation and in-game effects
    public float Elevation;
    public float Moisture;

    public float rndVisualElevation;

    float radius = 1f;



    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;
    public Vector3 Position()
    {
        return new Vector3(HexHorizontalSpacing() * (this.C + this.R / 2f), 0, HexVerticalSpacing() * this.R);
    }

    public float HexHeight()
    {
        return radius * 2;
    }

    public float HexWidth()
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }

    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }

    public static float Distance(Hex a, Hex b)
    {
        int NumColumns = MapManager.Instance.mapColumns;
        int NumRows = MapManager.Instance.mapRows;

        int dC = Mathf.Abs(a.C - b.C);
        if (MapManager.Instance.allowWrapEastWest)
        {
            if (dC > NumColumns / 2)
            {
                dC = NumColumns - dC;
            }
        }
       

        int dR = Mathf.Abs(a.R - b.R);
        if (MapManager.Instance.allowWrapNorthSouth)
        {
            if (dR > NumRows / 2)
            {
                dR = NumRows - dR;
            }
        }
          

        return Mathf.Max(dC, dR, Mathf.Abs(a.S - b.S));
    }

  public Vector3 PositionFromCamera(Vector3 cameraPosition, float numRows, float numColumns)
    {
        float mapHeight = numRows * HexVerticalSpacing();
        float mapWidth = numColumns * HexHorizontalSpacing();

        Vector3 position = Position();

        if (MapManager.Instance.allowWrapEastWest)
        {
            float howManyWidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;
            if (howManyWidthsFromCamera > 0)
            {
                howManyWidthsFromCamera += 0.5f;
            }
            else
            {
                howManyWidthsFromCamera -= 0.5f;
            }

            int howManyWidthsToFix = (int)howManyWidthsFromCamera;

            position.x -= howManyWidthsToFix * mapWidth;
        }
        if (MapManager.Instance.allowWrapNorthSouth)
        {
            float howManyWidthsFromCamera = (position.z - cameraPosition.z) / mapWidth;
            if (howManyWidthsFromCamera > 0)
            {
                howManyWidthsFromCamera += 0.5f;
            }
            else
            {
                howManyWidthsFromCamera -= 0.5f;
            }

            int howManyWidthsToFix = (int)howManyWidthsFromCamera;

            position.x -= howManyWidthsToFix * mapWidth;
        }

        position.y = rndVisualElevation;

        return position;
    }
}

public enum TraverseRequirment
{
    NONE,
    CLIMBING,
    SEA1,
    SEA2
}
