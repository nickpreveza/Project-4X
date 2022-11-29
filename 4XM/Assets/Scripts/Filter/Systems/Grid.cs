using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid 
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    private int[,] gridArray;
    Vector3 originPosition;
    bool gridExists;
    bool gridChanged;

    float internalTimer;
    public Grid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];

        Vector3 halvedCellSize = new Vector3(cellSize, cellSize) * 0.5f;

        for (int x = 0; x< gridArray.GetLength(0); x++)
        {
            for(int y = 0; y < gridArray.GetLength(1); y++)
            {
                SetValue(x, y, 0);
                //Gizmos.DrawSphere((GetWorldPosition(x, y) + halvedCellSize),  1f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1 ), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

        gridExists = true;
    }

    void Update()
    {
        if (gridExists && internalTimer <= 0)
        {
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 10f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 10f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 10f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 10f);
            internalTimer = 1f;
        }

        internalTimer -= Time.deltaTime;
    }
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x - originPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y - originPosition.y / cellSize);
    }

    public void SetValue(int x, int y, int value)
    {
        if (!gridExists) { return; }

        if (x>=0 && y>= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
        else
        {
            Debug.LogWarning("SetValue: Value outside of grid bounds");
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public int GetValue(int x, int y)
    {
        if (!gridExists) { return -1; }

        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }

        Debug.LogWarning("SetValue: Value outside of grid bounds");
        return -1;
    }

    public int GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public Vector3 GetTileData(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetTileData(x, y);
    }


    public Vector3 GetTileData(int x, int y)
    {
        Vector3 newTileData = new Vector3();
        int z = GetValue(x, y);
        newTileData.x = x;
        newTileData.y = y;
        newTileData.z = z;

        return newTileData;
    }


    public void Clear()
    {
        if (!gridExists) { return; }

        gridArray = null;
        gridExists = false;
    }

}
