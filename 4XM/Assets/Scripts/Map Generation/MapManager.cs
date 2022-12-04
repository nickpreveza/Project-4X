using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    private TileData[,] gridArray;
    public int width;
    public int height;
    public float noiseScale;

    [SerializeField] GameObject tileParent;
    [SerializeField] GameObject[] tilePrefabs;

    public List<GameObject> tileIdentifier = new List<GameObject>(); //converted positions from 2D array

    //float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
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

    public void GenerateMap()
    {
        ClearMap();
        gridArray = new TileData[width, height];
        //tileIdentifier = new List<WorldTile>(gridArray.GetLength(0) * gridArray.GetLength(1));
        //tileIdentifier = new WorldTile[];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject chosenTile = TileSelect(x, y);
                WorldTile tile = chosenTile.GetComponent<WorldTile>();
                tile.SetData(x, y);
                RegisterTile(x, y, tile);
                GameObject obj = Instantiate(chosenTile, tileParent.transform);
                obj.transform.SetParent(tileParent.transform);
                Vector3 position = new Vector3(x, 0, y);
                obj.transform.localPosition = position;
                tileIdentifier.Add(obj);
            }
        }
        SI_EventManager.Instance.OnMapGenerated();
        SI_CameraController.Instance.UpdateBounds(width, height);
    }

    public void RegisterTile(int x, int y, WorldTile value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value.data;
        }
        else
        {
            Debug.LogWarning("RegisterTile: Value outside of grid bounds");
        }
    }

    public TileData GetTileData(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            Debug.LogWarning("GetTile: Value outside of grid bounds");
            return null;
        }
    }

    public WorldTile GetWorldTile(int x, int y)
    {
        int index2D = x * width + y;
        Debug.Log(index2D);
        if (index2D >= 0 && index2D < tileIdentifier.Count)
        {
            return tileIdentifier[index2D].GetComponent<WorldTile>();
        }
        else
        {
            Debug.LogWarning("GetWorldTile: Value outside of 2D Array");
            return null;
        }
    }



    public void ClearMap()
    {
#if UNITY_EDITOR
        foreach (Transform child in tileParent.transform)
        {
            DestroyImmediate(child.gameObject);
        }
#endif
        foreach (Transform child in tileParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    GameObject TileSelect(int x, int y)
    {
        if (x == 0 || y == 0 || x == width -1 || y == height -1)
        {
            return tilePrefabs[(int)TileType.ICE];
        }
        else
        {
            return tilePrefabs[(int)TileType.DEEPSEA];
        }
    }
    public void SetTileAtCoords(int x, int y, TileType type)
    {
        SetTileAtCoords(x, y, (int)type);
    }
    public void SetTileAtCoords(int x, int y, int tileType)
    {

    }
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


