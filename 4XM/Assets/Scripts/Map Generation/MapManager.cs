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
    public Building[] hexBuildings;

    public string[] cityNames = new string[] {
    "Bamery", 
    "Ochepsa",
    "Edosgend",
    "Pihsea",
    "Vleuver",
    "Osrery",
    "Yhok",
    "Hurg",
    "Acomond",
    "Ocksas",
    "Crietsa",
    "Yreford",
    "Krehledo",
    "Vruelwell",
    "Keburn",
    "Oprey",
    "Grose",
    "Sheley",
    "Odonsea",
    "Ingate"};

    List<string> availableCityNames = new List<string>();
    public GameObject worldUIprefab;
    //octaves 7
    //Persistence = 0.391
    //Lacunarity = 2;
    //Noise Scale  = 50;

    //move these out 
    public List<Resource> oceanResources = new List<Resource>();
    public List<Resource> grassResources = new List<Resource>();
    public List<Resource> hillResources = new List<Resource>();
    public List<Resource> mountainResources = new List<Resource>();



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
        availableCityNames = new List<string>(cityNames);
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

        public ResourceType GetBuildingMatchingResourceType(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.ForestWorked:
            case BuildingType.FarmWorked:
            case BuildingType.MineWorked:
                Debug.LogError("This building type should not ask for matching resource type");
                return ResourceType.EMPTY;
            case BuildingType.ForestMaster:
                return hexBuildings[1].matchingResource;
            case BuildingType.FarmMaster:
                return hexBuildings[3].matchingResource;
            case BuildingType.MineMaster:
                return hexBuildings[5].matchingResource;
            case BuildingType.Guild:
                return hexBuildings[6].matchingResource;
        }

        Debug.LogError("Building for Building Type was not found");
        return ResourceType.EMPTY;
    }

    public Building GetBuildingByType(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.ForestWorked:
                return hexBuildings[0];
            case BuildingType.ForestMaster:
                return hexBuildings[1];

            case BuildingType.FarmWorked:
                return hexBuildings[2];
            case BuildingType.FarmMaster:
                return hexBuildings[3];
    
            case BuildingType.MineWorked:
                return hexBuildings[4];
            case BuildingType.MineMaster:
                return hexBuildings[5];

            case BuildingType.Guild:
                return hexBuildings[6];
        }

        Debug.LogError("Building for Building Type was not found");
        return hexBuildings[0];
    }


    public Resource GetResourceByType(ResourceType type)
    {

        //resources cheat sheet 
        // 0 - Fruit
        // 1 - Forest 
        // 2 - Animal
        // 3 - Farm
        // 4 - Mine
        // 5- Fish

        switch (type)
        {
            case ResourceType.FRUIT:
                return hexResources[0];
            case ResourceType.FOREST:
                return hexResources[1];
            case ResourceType.ANIMAL:
                return hexResources[2];
            case ResourceType.FARM:
                return hexResources[3];
            case ResourceType.MINE:
                return hexResources[4];
            case ResourceType.FISH:
                return hexResources[5];

        }

        Debug.LogWarning("Resource type was not found");
        return hexResources[0];
    }

    public BuildingType GetBuildingByResourceType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.FRUIT:
            case ResourceType.ANIMAL:
            case ResourceType.FISH:
            case ResourceType.EMPTY:
                Debug.LogWarning("Resource type does not have a matching Building Type");
                return BuildingType.Empty;
            case ResourceType.FOREST:
                return BuildingType.ForestWorked;
            case ResourceType.FARM:
                return BuildingType.FarmWorked;
            case ResourceType.MINE:
                return BuildingType.MineWorked;
        }


        Debug.LogWarning("Resource type does not have a matching Building Type");
        return BuildingType.Empty;

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
                            if (Random.Range(0f,1f) < hexResources[5].spawnChanceRate)
                            {
                                tile.GenerateResource(ResourceType.FISH);
                            }
                            break;
                        case TileType.SAND:
                            if (Random.Range(0f, 1f) < hexResources[0].spawnChanceRate)
                            {
                                tile.GenerateResource(ResourceType.FRUIT);
                            }
                            hexesWhereCityCanSpawn.Add(tile);
                            walkableTiles.Add(tile);
                            break;
                        case TileType.GRASS:
                            if (Random.Range(0f, 1f) < hexResources[0].spawnChanceRate)
                            {
                                if (Random.Range(0f, 1f) < 0.4)
                                {
                                    tile.GenerateResource(ResourceType.FRUIT);
                                }
                                else
                                {
                                    tile.GenerateResource(ResourceType.FOREST);
                                }
                            }
                            walkableTiles.Add(tile);
                            hexesWhereCityCanSpawn.Add(tile);
                            break;
                        case TileType.HILL:
                            if (Random.Range(0f, 1f) < hexResources[3].spawnChanceRate)
                            {
                                if (Random.Range(0f, 1f) < 0.5)
                                {
                                    tile.GenerateResource(ResourceType.FOREST);
                                }
                                else
                                {
                                    tile.GenerateResource(ResourceType.FARM);
                                }
                               
                            }
                            walkableTiles.Add(tile);
                            hexesWhereCityCanSpawn.Add(tile);
                            break;
                        case TileType.MOUNTAIN:
                            if (Random.Range(0f, 1f) < hexResources[4].spawnChanceRate)
                            {
                                tile.GenerateResource(ResourceType.MINE);
                            }
                            break;
                        case TileType.ICE:
                            break;

                    }

                    //update each tile with the correct visual prefab
                    tile.UpdateVisuals(true);
                }
            }
        }

        FindAdjacentTiles();
        GenerateCities();

        SI_EventManager.Instance?.OnCameraMoved();
        SI_EventManager.Instance?.OnMapGenerated();
        SI_CameraController.Instance?.UpdateBounds(mapRows, mapColumns);
    }

    void FindAdjacentTiles()
    {
        for (int column = 0; column < mapColumns; column++)
        {
            for (int row = 0; row < mapRows; row++)
            {
                WorldHex hex = hexes[column, row];
                if (hex.hexData.type != TileType.ICE)
                {
                    List<WorldHex> hexesToAdd = GetHexesListWithinRadius(hex.hexData, 1);
                    if (hexesToAdd.Contains(hex))
                    {
                        hexesToAdd.Remove(hex);
                    }
                    hex.adjacentHexes = hexesToAdd;
                }
               
            }
        }
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
            if (hexesWhereCityCanSpawn.Count <= 0)
            {
                Debug.LogWarning("No more availabel spaces where found for citis");
                break;
            }
            int randomTileIndex = Random.Range(0, hexesWhereCityCanSpawn.Count);
            WorldHex newCity = hexesWhereCityCanSpawn[randomTileIndex];
           // GenerateResources(newCity);
            int cityNameIndex = (Random.Range(0, availableCityNames.Count));
            string cityName = availableCityNames[cityNameIndex];
            availableCityNames.RemoveAt(cityNameIndex);
            newCity.SpawnCity(cityName);
            worldCities.Add(newCity);
            citiesSpawned++;
            //newCity.OccypyCityTiles();
            OccupyHexesForCityGeneration(newCity, 3);
        }

        foreach(WorldHex generatedCity in worldCities)
        {
           // CalculateDistanceFromOtherCities(generatedCity);
        }

        List<WorldHex> worldCitiesToAssign = new List<WorldHex>(worldCities);

        foreach (Player player in GameManager.Instance.sessionPlayers)
        {
            //TODO: Change this to a distance based algorithm
            int randomCity = Random.Range(0, worldCitiesToAssign.Count);
            
            //walkableTiles[randomTileIndex].SpawnCity(player.index, cityPrefab);
           
            ClaimCityByPlayer(player, worldCitiesToAssign[randomCity]);
            worldCitiesToAssign.RemoveAt(randomCity);
        }
    }

    void ClaimCityByPlayer(Player player, WorldHex city)
    {
        player.AddCity(city);
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
            //hex.GenerateResources();
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
        WorldHex centerHex = GetHexAt(q, r);
        WorldHex[] areaHexes = GetHexesWithinRadiusOf(centerHex, radius);

        foreach(WorldHex worldHex in areaHexes)
        {
            if (worldHex.hexData.Elevation < 0)
            {
                worldHex.hexData.Elevation = 0;
            }
            //we need a way to find distance from center hex 
            worldHex.hexData.Elevation += elevation * Mathf.Lerp(1f, 0.25f, Hex.Distance(mapColumns, mapRows, centerHex.hexData, worldHex.hexData) / radius);
            worldHex.UpdateVisuals();
        }
    }

    public WorldHex[] GetHexesWithinRadiusOf(WorldHex centerHex, int range)
    {
        List<WorldHex> results = new List<WorldHex>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                results.Add(GetHexAt(centerHex.hexData.C + dx, centerHex.hexData.R + dy));
            }
        }

        if (results.Contains(centerHex))
        {
            results.Remove(centerHex);
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

    public List<WorldHex> GetEnemiesInRange (Hex centerHex, int range)
    {
        List<WorldHex> results = new List<WorldHex>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                WorldHex hex = GetHexAt(centerHex.C + dx, centerHex.R + dy);
                if (hex.hexData.occupied)
                {
                    if (!hex.associatedUnit.BelongsToActivePlayer)
                    {
                        results.Add(hex);
                    }
                }
               
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
    public string resourceName;
    public ResourceType type;

    public bool canBeBuild;

    public int cost;
    public int output;

    public float spawnChanceRate;

    public GameObject prefab;
}

[System.Serializable]
public struct Building
{
    public string buildingName;
    public BuildingType type;
    public BuildingType slaveBuilding;
    public BuildingType masterBuilding;
    public ResourceType matchingResource;

    public bool hasLevels;

    public int cost;
    public int output;

    public GameObject[] levelPrefabs;
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

public enum ResourceType
{
    FRUIT = 0,
    FOREST = 1,
    ANIMAL = 2,
    MINE = 3,
    FISH = 4,
    FARM = 5,
    EMPTY = 6
}

public enum BuildingType
{
    ForestWorked,
    ForestMaster,
    FarmWorked,
    FarmMaster,
    MineWorked,
    MineMaster,
    Guild,
    Empty,
}


