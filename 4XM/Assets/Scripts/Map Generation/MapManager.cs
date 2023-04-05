using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    private WorldHex[,] hexes;

    public int mapRows;
    public int mapColumns;

    public bool ShowCoords;
    public bool useNewMapGeneration;
    [Header("New Simple Map Generation")]
    public int numContinents = 2;
    public int continentSpacing = 20;
    public bool allowWrapEastWest = true;
    public bool allowWrapNorthSouth = false;

    [Header("Old Map Generation")]
    public int octaves;
    public int seed;
    public Vector2 offset;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public float noiseScale = 1;

    public TerrainType[] regions;
    [SerializeField] GameObject tileParent;
    [SerializeField] GameObject cityPrefab;

    [SerializeField] GameObject[] hexPrefabs;
    [SerializeField] Material[] materialPrefabs;

    float[,] noiseMap;

    public List<WorldHex> mapTiles = new List<WorldHex>(); //converted positions from 2D array
    public List<WorldHex> walkableTiles = new List<WorldHex>();

    public bool useFalloff;
    float[,] falloffMap;

    //octaves 7
    //Persistence = 0.391
    //Lacunarity = 2;
    //Noise Scale  = 50;

    //move these out 


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

        falloffMap = FalloffGenerator.GenerateFallofMap(mapRows, mapColumns);
    }

    private void OnValidate()
    {
        if (mapRows < 1)
        {
            mapRows = 1;
        }
        if(mapColumns < 1)
        {
            mapColumns = 1;
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

    public void UpdateHexVisuals()
    {
        for (int column = 0; column < mapColumns; column++)
        {
            for (int row = 0; row < mapRows; row++)
            {
                WorldHex tile = hexes[column, row];
                Hex newHex = tile.hex;
                GameObject tileObject = tile.gameObject;
                MeshRenderer meshRenderer = tileObject.GetComponent<MeshRenderer>();

                if (newHex.Elevation >= 0)
                {

                }
                else
                {
                   // meshRenderer.material = MatOcean;
                }
            }
        }
           
    }
    public void GenerateMap()
    {
        ClearMap();

        mapTiles.Clear();
        walkableTiles.Clear();
        hexes = new WorldHex[mapColumns, mapRows];

        //old noise generaetion 
        noiseMap = Noise.GenerateNoiseMap(mapRows, mapColumns, noiseScale, seed, octaves, persistance, lacunarity, offset);
        falloffMap = FalloffGenerator.GenerateFallofMap(mapRows, mapColumns);
        TileType[] regionMap = new TileType[mapRows * mapColumns];

       

        //someCol * numRows + someRow
        //new map generation spawns all the tiles first, and then creates the land masses. 
        if (useNewMapGeneration)
        {
            for (int column = 0; column < mapColumns; column++)
            {
                for (int row = 0; row < mapRows; row++)
                {
                    GameObject prefab = hexPrefabs[0];
                    GameObject spawnedObject = Instantiate(prefab, tileParent.transform);
                    WorldHex tile = spawnedObject.GetComponent<WorldHex>();

                    if (ShowCoords)
                    {
                        tile.UpdateDebugText(string.Format("{0},{1}", column, row));
                    }
                    else
                    {
                        tile.UpdateDebugText("");
                    }
                   
                    tile.hex.SetData(column, row);

                    spawnedObject.transform.position = tile.hex.Position();
                    spawnedObject.transform.SetParent(tileParent.transform);

                    hexes[column, row] = tile;
                    mapTiles.Add(tile);

                    tile.hex.Elevation = 0f;

                    //absolute ice in the top and bottom
                    if (row == 0 || row == mapRows - 1)
                    {
                        tile.hex.Elevation = -1f;
                    }

                    //chance for ice in the top and bottom
                    if (row < 3 || row >= mapRows - 3)
                    {
                        if (Random.Range(0f, 1f) > 0.5f)
                        {
                            tile.hex.Elevation = -1f;
                        }
                    }

                    tile.RandomizeVisualElevation();
                    tile.UpdateVisuals();
                }
            }

            //seed
            //Random.InitState(0);

            //an alternative map generation with polygon splats
            for (int c = 0; c < numContinents; c++)
            {
                int numSplats = Random.Range(4, 8);
                for (int i = 0; i < numSplats; i++)
                {
                    int range = Random.Range(5, 8);
                    int y = Random.Range(range, mapRows - range);
                    int x = Random.Range(0, 10) - y/2 + (c * continentSpacing);

                    ElevateArea(x, y, range, 1f);
                }
            }

            //Add lumpiness 
            float noiseResolution = 0.1f;
            Vector2 noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            float noiseScale = 2f;
            for(int column = 0; column < mapColumns; column++)
            {
                for (int row = 0; row < mapRows; row++)
                {
                    WorldHex h = GetHexAt(column, row);
                    float noise = Mathf.PerlinNoise( 
                        ((float)column/mapColumns/noiseResolution) + noiseOffset.x, 
                        ((float)row/mapRows/noiseResolution) + noiseOffset.y)  - 0.5f;
                    h.hex.Elevation += noise * noiseScale;
                    h.UpdateVisuals();
                }
            }
        }
        else //old map generation uses data to create the land massses and then creates the appropriate tiles 
        {
            for (int column = 0; column < mapColumns; column++)
            {
                for (int row = 0; row < mapRows; row++)
                {

                    //fetch type of tile based on noiseMap generated
                    if (useFalloff)
                    {
                        noiseMap[row, column] = Mathf.Clamp01(noiseMap[row, column] - falloffMap[row, column]);
                    }

                    float currentHeight = noiseMap[row, column];

                    //match height with regionType 
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            regionMap[row * mapRows + column] = regions[i].type;
                            break;
                        }
                    }


                    //absolute ice in the top and bottom
                    if (row == 0 || row == mapRows - 1)
                    {
                        regionMap[row * mapRows + column] = regions[0].type;
                    }

                    //chance for ice in the top and bottom
                    if (row < 3 || row >= mapRows - 3)
                    {
                        if (Random.Range(0f, 1f) > 0.5f)
                        {
                            regionMap[row * mapRows + column] = regions[0].type;
                        }
                    }

                    //Create the actual world Tile;
                    GameObject prefab = GetTilePrefab(regionMap[row * mapRows + column]);

                    GameObject spawnedObject = Instantiate(prefab, tileParent.transform);

                    WorldHex tile = spawnedObject.GetComponent<WorldHex>();

                    if (ShowCoords)
                    {
                        tile.UpdateDebugText(string.Format("{0},{1}", column, row));
                    }
                    else
                    {
                        tile.UpdateDebugText("");
                    }
                    tile.hex.SetData(column, row);
                    tile.SetElevationFromType();
                    spawnedObject.transform.position = tile.hex.Position();
                    spawnedObject.transform.SetParent(tileParent.transform);

                    hexes[column, row] = tile;

                    mapTiles.Add(tile);

                    switch (tile.hex.type)
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

                    tile.RandomizeVisualElevation();
                }
            }
        }
       

        GenerateCities();

        SI_EventManager.Instance?.OnCameraMoved();
        SI_EventManager.Instance?.OnMapGenerated();
        SI_CameraController.Instance?.UpdateBounds(mapRows, mapColumns);
    }

    public void GenerateCities()
    {
        //TODO: Spawn a set amount of cities based on map size and player count. 

        foreach(Player player in GameManager.Instance.sessionPlayers)
        {
            int randomTileIndex = Random.Range(0, walkableTiles.Count);
           
            walkableTiles[randomTileIndex].SpawnCity(player.index, cityPrefab);
            player.AddCity(walkableTiles[randomTileIndex]);
        }
    }

    public float GetElevationFromType(TileType type)
    {
        switch (type)
        {
            case TileType.ICE:
                return regions[0].height;
            case TileType.DEEPSEA:
                return regions[1].height;
            case TileType.SEA:
                return regions[2].height;
            case TileType.SAND:
                return regions[3].height;
            case TileType.GRASS:
                return regions[4].height;
            case TileType.HILL:
                return regions[5].height;
            case TileType.MOUNTAIN:
                return regions[6].height;
        }

        return 0f;
    }


    void ElevateArea(int q, int r, int radius, float elevation)
    {
        Hex centerHex = GetHexAt(q, r).hex;
        WorldHex[] areaHexes = GetHexesWithinRadiusOf(centerHex, radius);

        foreach(WorldHex worldHex in areaHexes)
        {
            if (worldHex.hex.Elevation < 0)
            {
                worldHex.hex.Elevation = 0;
            }
            //we need a way to find distance from center hex 
            worldHex.hex.Elevation += elevation * Mathf.Lerp(1f, 0.25f, Hex.Distance(centerHex, worldHex.hex) / radius);
            worldHex.UpdateVisuals();
        }
    }

        public WorldHex[] GetHexesWithinRadiusOf(Hex centerHex, int range)
    {
        List<WorldHex> results = new List<WorldHex>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                results.Add(GetHexAt(centerHex.C + dx, centerHex.R + dy));
            }
        }

        return results.ToArray();
    }

    public WorldHex GetHexAt(int x, int y)
    {
        if (hexes == null)
        {
            Debug.LogError("Hexes Array not yet instantiated.");
            return null;
        }

        if ( y >= 0 && y < mapRows)
        {
            x = x % mapColumns;
            if (x < 0)
            {
                x += mapColumns;
            }

            return hexes[x, y];
        }
        else
        {
            Debug.LogWarning("GetHexAt: Value outside of grid bounds");
            return null;
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

    public Material GetTypeMaterial(TileType type)
    {
        switch (type)
        {
            case TileType.ICE:
                return materialPrefabs[0];
            case TileType.DEEPSEA:
                return materialPrefabs[1];
            case TileType.SEA:
                return materialPrefabs[2];
            case TileType.SAND:
                return materialPrefabs[3];
            case TileType.GRASS:
                return materialPrefabs[4];
            case TileType.HILL:
                return materialPrefabs[5];
            case TileType.MOUNTAIN:
                return materialPrefabs[6];
        }

        return materialPrefabs[0];
    }


    public WorldHex GetWorldTile(int x, int y)
    {
        //int index2D = x * mapRows + y;
        //Debug.Log(index2D);
        return GetHexAt(x, y);
       
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
    public Resource[] resources;
}

[System.Serializable]
public struct Resource
{
    public string name;
    public GameObject prefab;
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


