using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    private TileData[,] gridArray;
    public int width;
    public int height;
    public int octaves;
    public int seed;
    public Vector2 offset;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public float noiseScale = 1;

    [SerializeField] TerrainType[] regions;
    [SerializeField] GameObject tileParent;
    [SerializeField] GameObject cityPrefab;
    [SerializeField] GameObject[] tilePrefabs;
    public List<GameObject> worldTiles = new List<GameObject>(); //converted positions from 2D array
    float[,] noiseMap;
  
    public List<GameObject> landTiles = new List<GameObject>();

    public bool useFalloff;
    float[,] falloffMap;
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

        falloffMap = FalloffGenerator.GenerateFallofMap(width, height);
    }

    private void OnValidate()
    {
        if (width < 1)
        {
            width = 1;
        }
        if(height < 1)
        {
            height = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }

    public void GenerateMap()
    {
        ClearMap();
        worldTiles.Clear();
        landTiles.Clear();
        noiseMap = Noise.GenerateNoiseMap(width, height, noiseScale, seed, octaves, persistance, lacunarity, offset);
        falloffMap = FalloffGenerator.GenerateFallofMap(width, height);
        TileType[] regionMap = new TileType[width * height];
        gridArray = new TileData[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x,y]);
                }
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        regionMap[x * width + y] = regions[i].type;
                        break;
                    }
                }

                //Create the actual world Tile;
                GameObject prefab = GetTilePrefab(regionMap[x * width + y]); //short this based on region.
                GameObject obj = Instantiate(prefab, tileParent.transform);
                WorldTile tile = obj.GetComponent<WorldTile>();
                if (tile.data.type == TileType.GRASS)
                {
                    landTiles.Add(tile.gameObject);
                }
                tile.SetData(x, y);
                RegisterTile(x, y, tile);
                obj.transform.SetParent(tileParent.transform);
                Vector3 position = new Vector3(x, 0, y);
                obj.transform.localPosition = position;
                worldTiles.Add(obj);
            }
        }
        AllocateStartingCities();
        SI_EventManager.Instance?.OnMapGenerated();
        SI_CameraController.Instance?.UpdateBounds(width, height);
    }

    public void AllocateStartingCities()
    {
        int randomTileIndex = Random.Range(0, landTiles.Count);
        landTiles[randomTileIndex].GetComponent<WorldTile>().SpawnCity(cityPrefab);
        UnitManager.Instance?.SetStartingCity(landTiles[randomTileIndex]);

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

    public GameObject GetTilePrefab(TileType type)
    {
        switch (type)
        {
            case TileType.ICE:
                return tilePrefabs[0];
            case TileType.DEEPSEA:
                return tilePrefabs[1];
            case TileType.SEA:
                return tilePrefabs[2];
            case TileType.SAND:
                return tilePrefabs[3];
            case TileType.GRASS:
                return tilePrefabs[4];
            case TileType.HILL:
                return tilePrefabs[5];
            case TileType.MOUNTAIN:
                return tilePrefabs[6];
        }

        return tilePrefabs[0];
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
        if (index2D >= 0 && index2D < worldTiles.Count)
        {
            return worldTiles[index2D].GetComponent<WorldTile>();
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

    public void SetTileAtCoords(int x, int y, TileType type)
    {
        SetTileAtCoords(x, y, (int)type);
    }
    public void SetTileAtCoords(int x, int y, int tileType)
    {

    }
}

[System.Serializable]
public struct TerrainType
{
    public TileType type;
    public float height;
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


