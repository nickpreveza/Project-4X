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
    [SerializeField] GameObject[] hexPrefabs;
    
    float[,] noiseMap;

    public List<GameObject> mapTiles = new List<GameObject>(); //converted positions from 2D array
    public List<WorldTile> walkableTiles = new List<WorldTile>();

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

        mapTiles.Clear();
        walkableTiles.Clear();

        noiseMap = Noise.GenerateNoiseMap(width, height, noiseScale, seed, octaves, persistance, lacunarity, offset);
        falloffMap = FalloffGenerator.GenerateFallofMap(width, height);
        TileType[] regionMap = new TileType[width * height];
        gridArray = new TileData[width, height];
        
        for (int column = 0; column < height; column++)
        {
            for (int row = 0; row < width; row++)
            {
                if (useFalloff)
                {
                    noiseMap[row, column] = Mathf.Clamp01(noiseMap[row, column] - falloffMap[row, column]);
                }

                float currentHeight = noiseMap[row, column];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        regionMap[row * width + column] = regions[i].type;
                        break;
                    }
                }

                //Create the actual world Tile;
                GameObject prefab = GetTilePrefab(regionMap[row * width + column]); //short this based on region.
                GameObject obj = Instantiate(prefab, tileParent.transform);

                WorldTile tile = obj.transform.GetChild(0).GetComponent<WorldTile>();
                tile.hex = new Hex(column, row);

                obj.transform.position = tile.hex.Position();
                obj.transform.SetParent(tileParent.transform);

                mapTiles.Add(obj);

                switch (tile.data.type)
                {
                    case TileType.DEEPSEA:
                        break;
                    case TileType.SEA:
                        break;
                    case TileType.SAND:
                        walkableTiles.Add(tile);
                        break;
                    case TileType.GRASS:
                        walkableTiles.Add(tile);
                        break;
                    case TileType.HILL:
                        walkableTiles.Add(tile);
                        break;
                    case TileType.MOUNTAIN:
                        break;
                    case TileType.ICE:
                        break;

                }
            }
        }

        /*
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
                WorldTile tile = obj.transform.GetChild(0).GetComponent<WorldTile>();

                tile.SetData(x, y);
                RegisterTile(x, y, tile);
                obj.transform.SetParent(tileParent.transform);

                float xPos = x * 1.71f;
                float yPos = y * 1.71f;
                Vector3 position = new Vector3(xPos, 0, yPos);
                obj.transform.localPosition = position;

                mapTiles.Add(obj);

                switch (tile.data.type)
                {
                    case TileType.DEEPSEA:
                        break;
                    case TileType.SEA:
                        break;
                    case TileType.SAND:
                        walkableTiles.Add(tile);
                        break;
                    case TileType.GRASS:
                        walkableTiles.Add(tile);
                        break;
                    case TileType.HILL:
                        walkableTiles.Add(tile);
                        break;
                    case TileType.MOUNTAIN:
                        break;
                    case TileType.ICE:
                        break;

                }
               
            }
        }*/

        //AllocateStartingCities();
        SI_EventManager.Instance?.OnMapGenerated();
        //SI_CameraController.Instance?.UpdateBounds(width, height);
    }

    public void AllocateStartingCities()
    {
        int randomTileIndex = Random.Range(0, walkableTiles.Count);
        walkableTiles[randomTileIndex].SpawnCity(cityPrefab);
        UnitManager.Instance?.SetStartingCity(walkableTiles[randomTileIndex].gameObject);
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
                return hexPrefabs[0];
            case TileType.DEEPSEA:
                return hexPrefabs[1];
            case TileType.SEA:
                return hexPrefabs[2];
            case TileType.SAND:
                return hexPrefabs[3];
            case TileType.GRASS:
                return hexPrefabs[4];
            case TileType.HILL:
                return hexPrefabs[5];
            case TileType.MOUNTAIN:
                return hexPrefabs[6];
        }

        return hexPrefabs[0];
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
        if (index2D >= 0 && index2D < mapTiles.Count)
        {
            return mapTiles[index2D].transform.GetChild(0).GetComponent<WorldTile>();
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


