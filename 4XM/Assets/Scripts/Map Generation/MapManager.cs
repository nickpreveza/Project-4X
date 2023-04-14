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
    public int citiesNum; //maybe this should be a variable based on mapsize 

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
   

    public GameObject hexBasePrefab;
    public GameObject[] hexVisualPrefabs;
    public GameObject cityPrefab;

    float[,] noiseMap;

    public List<WorldHex> mapTiles = new List<WorldHex>(); //converted positions from 2D array
    public List<WorldHex> walkableTiles = new List<WorldHex>();

    public bool useFalloff;
    float[,] falloffMap;


    public List<WorldHex> hexesWhereCityCanSpawn = new List<WorldHex>();
    public List<WorldHex> worldCities = new List<WorldHex>();

    public Resource[] hexResources; 
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
                Hex newHex = tile.hexData;
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
                    GameObject prefab = hexBasePrefab;
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
                   
                    tile.hexData.SetData(column, row);

                    spawnedObject.transform.position = tile.hexData.Position();
                    spawnedObject.transform.SetParent(tileParent.transform);

                    hexes[column, row] = tile;
                    mapTiles.Add(tile);

                    tile.hexData.Elevation = 0f;

                    //absolute ice in the top and bottom
                    if (row == 0 || row == mapRows - 1)
                    {
                        tile.hexData.Elevation = -1f;
                    }

                    //chance for ice in the top and bottom
                    if (row < 3 || row >= mapRows - 3)
                    {
                        if (Random.Range(0f, 1f) > 0.5f)
                        {
                            tile.hexData.Elevation = -1f;
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
                    h.hexData.Elevation += noise * noiseScale;
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
                    GameObject prefab = hexBasePrefab; // GetTilePrefab(regionMap[row * mapRows + column]);

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
                    tile.SetData(column, row, regionMap[row * mapRows + column], true);
                    spawnedObject.transform.position = tile.hexData.Position();
                    spawnedObject.transform.SetParent(tileParent.transform);

                    hexes[column, row] = tile;

                    mapTiles.Add(tile);

                    //filter the hexes into lists for other uses 
                    switch (tile.hexData.type)
                    {
                        case TileType.DEEPSEA:
                            break;
                        case TileType.SEA:
                            break;
                        case TileType.SAND:
                            hexesWhereCityCanSpawn.Add(tile);
                            walkableTiles.Add(tile);
                            break;
                        case TileType.GRASS:
                            walkableTiles.Add(tile);
                            hexesWhereCityCanSpawn.Add(tile);
                            break;
                        case TileType.HILL:
                            walkableTiles.Add(tile);
                            hexesWhereCityCanSpawn.Add(tile);
                            break;
                        case TileType.MOUNTAIN:
                            break;
                        case TileType.ICE:
                            break;

                    }

                    //update each tile with the correct visual prefab
                    tile.UpdateVisuals(true);
                }
            }
        }
       

        GenerateCities();

        SI_EventManager.Instance?.OnCameraMoved();
        SI_EventManager.Instance?.OnMapGenerated();
        SI_CameraController.Instance?.UpdateBounds(mapRows, mapColumns);
    }

    void OccupyHexesForCityGeneration(WorldHex cityCenter, int range)
    {
        List<WorldHex> hexesToRemove = GetHexesListWithinRadius(cityCenter.hexData, range);

        foreach(WorldHex hex in hexesToRemove)
        {
            if (hexesWhereCityCanSpawn.Contains(hex))
            {
                hexesWhereCityCanSpawn.Remove(hex);
            }
        }

    }

    void CalculateDistanceFromOtherCities(WorldHex city)
    {
        for(int i = 0; i < worldCities.Count; i++)
        {
            //city.cityData.
        }
    }

    public void GenerateCities()
    {
        //define the number of cities to spawn based on map size: made this a public var
        int citiesSpawned = 0;

        for (int i = 0; i < citiesNum; i++)
        {
            if (hexesWhereCityCanSpawn.Count < 0)
            {
                Debug.LogWarning("No more availabel spaces where found for citis");
                break;
            }
            int randomTileIndex = Random.Range(0, hexesWhereCityCanSpawn.Count);
            WorldHex newCity = hexesWhereCityCanSpawn[randomTileIndex];
            newCity.SpawnCity();
            worldCities.Add(newCity);
            citiesSpawned++;
            //newCity.OccypyCityTiles();
            
        }

        foreach(WorldHex generatedCity in worldCities)
        {
            CalculateDistanceFromOtherCities(generatedCity);
        }

        List<WorldHex> worldCitiesToAssign = new List<WorldHex>(worldCities);

        foreach (Player player in GameManager.Instance.sessionPlayers)
        {
            //TODO: Change this to a distance based algorithm
            int randomCity = Random.Range(0, worldCitiesToAssign.Count);
            
            //walkableTiles[randomTileIndex].SpawnCity(player.index, cityPrefab);
            player.AddCity(worldCitiesToAssign[randomCity]);
            OccupyHexesForCityGeneration(worldCitiesToAssign[randomCity], 2);
            worldCitiesToAssign.RemoveAt(randomCity);
        }
    }

    public void GenerateResources(WorldHex cityCenter)
    {
        List<WorldHex> cityHexes = GetHexesListWithinRadius(cityCenter.hexData, cityCenter.cityData.range);

        //move this to a more complex system where a number of resource points is guaranteed.
        if (cityHexes.Contains(cityCenter))
        {
            cityHexes.Remove(cityCenter);
        }

        foreach(WorldHex hex in cityHexes)
        {
            hex.GenerateResources();
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
        Hex centerHex = GetHexAt(q, r).hexData;
        WorldHex[] areaHexes = GetHexesWithinRadiusOf(centerHex, radius);

        foreach(WorldHex worldHex in areaHexes)
        {
            if (worldHex.hexData.Elevation < 0)
            {
                worldHex.hexData.Elevation = 0;
            }
            //we need a way to find distance from center hex 
            worldHex.hexData.Elevation += elevation * Mathf.Lerp(1f, 0.25f, Hex.Distance(mapColumns, mapRows, centerHex, worldHex.hexData) / radius);
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

    public List<WorldHex> GetHexesListWithinRadius(Hex centerHex, int range)
    {
        List<WorldHex> results = new List<WorldHex>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                results.Add(GetHexAt(centerHex.C + dx, centerHex.R + dy));
            }
        }

        return results;
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
}

[System.Serializable]
public struct Resource
{
    public string name;
    public TileType compatibleTerrain;

    public int cost;
    public int output;

    public float spawnChanceRate;

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


