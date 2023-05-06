using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public class WorldHex : MonoBehaviour
{
    //calculated as Column * numberOfRows + row 
    public int hexIdentifier;

    public Hex hexData;
    public CityData cityData;
    public GameObject hexGameObject;
    Wiggler wiggler;
    public WorldUnit associatedUnit;
    public Transform unitParent;
    public Transform resourceParent;
    [SerializeField] GameObject hexHighlight;
    [SerializeField] GameObject cityGameObject;
    public WorldHex parentCity;
    public CityView cityView;

    public List<WorldHex> adjacentHexes = new List<WorldHex>();
    //0 - Visual Layer 
    //1 - Resource Layer 
    //2 - Unit Layer 
    //3 - Hex Highlight

    Material rimMaterial;

    private void Awake()
    {
        hexGameObject = transform.GetChild(0).GetChild(0).gameObject;
        resourceParent = transform.GetChild(1);
        unitParent = transform.GetChild(2);
        hexHighlight = transform.GetChild(3).gameObject;
        SI_EventManager.Instance.onCameraMoved += UpdatePositionInMap;
        RandomizeVisualElevation();
    }

    private void OnDestroy()
    {
        SI_EventManager.Instance.onCameraMoved -= UpdatePositionInMap;
    }
    void Start()
    {
        wiggler = GetComponent<Wiggler>();
        rimMaterial = hexGameObject.GetComponent<MeshRenderer>().materials[0];
        HideHighlight();
    }

    public void ShowHighlight(bool combat)
    {
        if (hexHighlight != null)
        hexHighlight?.SetActive(true);

        //TODO: Visualize combat or blocked tiles.
        if (combat)
        {
            hexHighlight.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            hexHighlight.GetComponent<MeshRenderer>().material.color = Color.white;
        }


    }

    public void HideHighlight()
    {
        if (hexHighlight != null)
            hexHighlight?.SetActive(false);
    }

    public void SetData(int column, int row, TileType newType, bool setElevationFromType)
    {
        hexData.SetData(column, row);
        hexIdentifier = column * MapManager.Instance.mapRows + row;
        hexData.type = newType;
        hexData.moveCost = MapManager.Instance.GetMoveCostForType(newType);
        if (setElevationFromType)
        {
            SetElevationFromType();
        }
    }
    public void SetElevationFromType()
    {
        hexData.Elevation = MapManager.Instance.GetElevationFromType(hexData.type);
    }
    public void UpdateVisuals(bool isForced = false) //better name, this updates a lot more
    {
        if (MapManager.Instance == null)
        {
            UpdateVisualsTool();
            return;
        }

        UpdateVisualObject();
    }

    void UpdateVisualsTool()
    {
        TileType newType = TileType.ICE;

        for (int i = 0; i < HexOrganizerTool.Instance.regions.Length; i++)
        {
            if (hexData.Elevation <= HexOrganizerTool.Instance.regions[i].height)
            {
                newType = HexOrganizerTool.Instance.regions[i].type;
                break;
            }
        }

        if (newType != hexData.type)
        {
            hexData.type = newType;
            UpdateVisualObject();
        }
        else
        {
            RandomizeVisualElevation();
        }

       
    }

    public void UpdateVisualObject()
    {
        foreach(Transform child in transform.GetChild(0))
        {
            Destroy(child.gameObject);
        }

        GameObject prefabToSpawn = null;

        if (MapManager.Instance == null)
        {
            prefabToSpawn = Instantiate(HexOrganizerTool.Instance.hexVisualPrefabs[(int)hexData.type], transform.GetChild(0));
        }
        else
        {
            prefabToSpawn = Instantiate(MapManager.Instance.hexVisualPrefabs[(int)hexData.type], transform.GetChild(0));
        }

        hexGameObject = prefabToSpawn;
        RandomizeVisualElevation();
    }

    public void UpdatePositionInMap()
    {
        this.transform.position = hexData.PositionFromCamera();
    }

    public void UnitIn(WorldUnit newUnit)
    {
        hexData.occupied = true;
        if (newUnit.playerOwnerIndex != hexData.playerOwnerIndex && hexData.playerOwnerIndex != -1)
        {
            hexData.occupiedByEnemyUnit = true;
        }
        associatedUnit = newUnit;
    }

    public void UnitOut(WorldUnit newUnit)
    {
        hexData.occupied = false;
       
        associatedUnit = null;

        if (hexData.occupiedByEnemyUnit)
        {
            hexData.occupiedByEnemyUnit = false;
            if (hexData.hasCity)
            {
                GameManager.Instance.RecalculatePlayerExpectedStars(hexData.playerOwnerIndex);
            }
        }
    }

    public void SpawnCity(string newName)
    {
        RemoveResource();

        GameObject obj = Instantiate(MapManager.Instance.cityPrefab, transform);
        obj.transform.SetParent(resourceParent);
        cityGameObject = obj;
        hexData.hasCity = true;
        hexData.hasBuilding = true; //maybe this will cause issues?
        cityData = new CityData();

        cityData.cityName = newName;
        cityData.level = 1;
        cityData.output = 1;
        cityData.range = 1;
        cityData.playerIndex = -1;

        GameObject worldUI = Instantiate(MapManager.Instance.worldUIprefab, transform);
        cityView = worldUI.GetComponent<CityView>();
        cityView.SetData(this);
        cityView.gameObject.SetActive(false);
    }
    public IEnumerator RemoveProgressPoint(int value)
    {
        for (int i = 0; i < value; i++)
        {
            if (cityData.levelPointsToNext > 0)
            {
                cityData.levelPointsToNext--;
                cityView.RemoveProgressUIPoint();
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                if (cityData.negativeLevelPoints >= cityData.targetLevelPoints  || cityData.output == 0)
                {
                    continue;
                }

                cityData.negativeLevelPoints++;
                cityData.output--;

                cityView.AddNegativeProgressUIPoint();
                cityView.UpdateData();
                yield return new WaitForSeconds(0.3f);
            }

            cityView.UpdateData();

        }

        if (GameManager.Instance.IsIndexOfActivePlayer(hexData.playerOwnerIndex))
        {
            GameManager.Instance.activePlayer.CalculateExpectedStars();
            UIManager.Instance.UpdateHUD();
        }
    }
    public IEnumerator AddProgressPoint(int value)
    {
        if (value == 0)
        {
            yield break;
        }
        for(int i = 0; i < value; i++)
        {
            if (cityData.negativeLevelPoints > 0)
            {
                cityData.negativeLevelPoints--;
                cityView.RemoveProgressUIPoint();
                cityData.output++;
                cityView.UpdateData();
                yield return new WaitForSeconds(0.3f);
                
                continue;
            }
    
            cityData.levelPointsToNext++;

            if (cityData.levelPointsToNext >= cityData.targetLevelPoints)
            {
                cityView.AddProgressUIPoint();
                yield return new WaitForSeconds(0.3f);

                cityData.level++;
                cityData.targetLevelPoints = cityData.level + 1;
                cityData.levelPointsToNext = 0;
                cityData.output++;
                cityView.AddLevelUIPoint();
                cityView.RemoveAllProgressPoints();
                cityView.UpdateData();
               
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                cityView.AddProgressUIPoint();
                yield return new WaitForSeconds(0.3f);
            }
        }

        cityView.UpdateData();

        if (GameManager.Instance.IsIndexOfActivePlayer(hexData.playerOwnerIndex))
        {
            GameManager.Instance.activePlayer.CalculateExpectedStars();
            GameManager.Instance.activePlayer.CalculateDevelopmentScore(false);
            UIManager.Instance.UpdateHUD();
        }
        
    }

    void AddLevelPoint(int points)
    {
        StartCoroutine(AddProgressPoint(points));
    }

    void RemoveLevelPoint(int points)
    {
        StartCoroutine(RemoveProgressPoint(points));
    }

    public void CreateMasterBuilding(BuildingType type)
    {
        if (hexData.hasBuilding)
        {
            Debug.Log("Tried to place building on top of building");
            return;
        }

        if (hexData.hasResource)
        {
            Debug.Log("Tried to place building on top of a resource");
            return;
        }

        Building building = MapManager.Instance.GetBuildingByType(type);

        hexData.hasBuilding = true;
        hexData.buildingType = type;
        hexData.buildingLevel = 0;

        CalculateBuildingLevel();

        int buildingLevelPrefab = 0;

        if (building.levelPrefabs.Length > hexData.buildingLevel)
        {
            buildingLevelPrefab = hexData.buildingLevel;
        }

        GameObject obj = Instantiate(building.levelPrefabs[buildingLevelPrefab], resourceParent);

        CalculatePointsForMasterBuilding();

        UIManager.Instance.ShowHexView(this);
    }

    void CalculatePointsForMasterBuilding()
    {
        for(int i = 0; i < hexData.buildingLevel; i++)
        {
            parentCity.AddLevelPoint(MapManager.Instance.GetBuildingByType(hexData.buildingType).output);
        }
    }

    void CalculateBuildingLevel()
    {
        foreach(WorldHex hex in adjacentHexes)
        {
            if (hex.hexData.hasBuilding)
            {
                if (hex.hexData.buildingType == MapManager.Instance.GetBuildingByType(hexData.buildingType).slaveBuilding)
                {
                    hexData.buildingLevel++;
                }

            }
            else
            {
                continue;
            }
        }
    }

    public void HarvestResource()
    {
        GameObject resourceObj = resourceParent.GetChild(0).gameObject;
        Destroy(resourceObj);

        if (MapManager.Instance.GetResourceByType(hexData.resourceType).canBeBuild)
        {
            hexData.hasBuilding = true;
            hexData.buildingType = MapManager.Instance.GetBuildingByResourceType(hexData.resourceType);

            GameObject obj = Instantiate(MapManager.Instance.GetBuildingByType(hexData.buildingType).levelPrefabs[0], resourceParent);

            CheckForMasterBuilding();
        }
        else
        {
           //I thought somewthing else could happen here
        }

        parentCity.AddLevelPoint(MapManager.Instance.GetResourceByType(hexData.resourceType).output);

        hexData.hasResource = false;
        hexData.resourceType = ResourceType.EMPTY;

       
       
        UIManager.Instance.ShowHexView(this);
    }

    void CheckForMasterBuilding()
    {
        bool masterExists = false;

        foreach(WorldHex hex in adjacentHexes)
        {
            if (hex.hexData.buildingType == MapManager.Instance.GetBuildingByType(hexData.buildingType).masterBuilding)
            {
                hex.AddLevelForMaster();
                break;
            }
        }
    }

    void AddLevelForMaster()
    {
        hexData.buildingLevel++;
        parentCity.AddLevelPoint(MapManager.Instance.GetBuildingByType(hexData.buildingType).output);
    }

    public void SetAsOccupiedByCity(WorldHex parentCityHex)
    {
        parentCity = parentCityHex;
        hexData.isOwnedByCity = true;
        hexData.playerOwnerIndex = parentCity.hexData.playerOwnerIndex;


        //set outline material to match player color 
        this.hexGameObject.GetComponent<MeshRenderer>().materials[0].color = GameManager.Instance.GetCivilizationColor(hexData.playerOwnerIndex, CivColorType.borderColor);
        //remove this later on

    }

    public void GenerateResource(ResourceType resourceType)
    {
        if (hexData.hasCity)
        {
            Debug.LogError("Tried to generate resource for city Hex");
            return;
        }

        Resource selectedResource = MapManager.Instance.GetResourceByType(resourceType);

        GameObject obj = Instantiate(selectedResource.prefab, resourceParent);
        hexData.resourceType = resourceType;
        hexData.hasResource = true;
        //allocate a resource base on mapmanager chances and surrounded resources
        //spawn resource
        //set correct data
        //GameObject resourceObj = Instantiate(MapManager.Instance.GetResourcePrefab)
    }

    public void RemoveResource()
    {
        if (hexData.hasResource)
        {
            hexData.resourceType = ResourceType.EMPTY;
            hexData.hasResource = false;
            hexData.moveCost = MapManager.Instance.GetMoveCostForType(hexData.type);
            GameObject resourceObj = resourceParent.GetChild(0).gameObject;

            if (resourceObj != null)
            {
                Destroy(resourceObj);
            }
           
        }
    }
    public void OccupyCityByPlayer(Player player, bool spawnUnit = false)
    {
        if (!hexData.hasCity)
        {

            Debug.LogError("Hed does not have a city");
            return;
        }

        bool isThisATakeOver = false;

        if (hexData.playerOwnerIndex > -1 && hexData.playerOwnerIndex != player.index)
        {
            GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex).RemoveCity(this);
            isThisATakeOver = true;
        }
       

        cityGameObject.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetCivilizationColor(player.civilization, CivColorType.borderColor);
        hexData.playerOwnerIndex = GameManager.Instance.GetPlayerIndex(player);
        hexData.cityHasBeenClaimed = true;
        cityData.playerIndex = hexData.playerOwnerIndex;

        List<WorldHex> cityHexes = new List<WorldHex>(adjacentHexes);

        if (!isThisATakeOver)
        {
            foreach (WorldHex hex in adjacentHexes)
            {
                if (hex.hexData.isOwnedByCity)
                {
                    //Remove hexes that already belong to other cities 
                    cityHexes.Remove(hex);
                }
            }

            cityData.cityHexes = cityHexes;

            foreach (WorldHex newHex in cityData.cityHexes)
            {
                newHex.SetAsOccupiedByCity(this);
            }
        }
        else
        {
            foreach (WorldHex newHex in cityData.cityHexes)
            {
                newHex.SetAsOccupiedByCity(this);
            }
        }
       

        cityView.gameObject.SetActive(true);


        cityView.UpdateData();
        cityView.UpdateForCityCapture();
        cityView.InitialLevelSetup();
        
        if (spawnUnit)
        {

        }

  

    }

    public void Deselect()
    {
        // hexGameObject.GetComponent<MeshRenderer>().materials[0] = rimMaterial;
        HideHighlight();
        UIManager.Instance.HideHexView();
    }

    public void Select(bool isRepeat)
    {
        //if a unit is moving 
        if (UnitManager.Instance.hexSelectMode)
        {
            if (UnitManager.Instance.startHex == this)
            {
                UnitManager.Instance.ClearHexSelectionMode();
            }
            else if (UnitManager.Instance.IsHexValidAttack(this))
            {
                UnitManager.Instance.AttackTargetUnitInHex(this);

                wiggler?.Wiggle();
                return;
            }
            else if (UnitManager.Instance.IsHexValidMove(this))
            {
                UnitManager.Instance.MoveToTargetHex(this);

                wiggler?.Wiggle();
                return;
            }
            else
            {
                UnitManager.Instance.ClearHexSelectionMode();
            }
        }

        if (!isRepeat)
        {
            if (hexData.occupied && associatedUnit != null)
            {
                Debug.Log("This is the Unit layer");
                associatedUnit.Select();
                wiggler?.Wiggle();
                UIManager.Instance.ShowHexView(this, associatedUnit);
                return;
            }
            else
            {
                //TODO: Change the bheaviour to select the unit normally, but show that it's currently inactive
                // ShowHighlight(false);
                Debug.Log("Auto-moved to the resource layer");
                SI_CameraController.Instance.repeatSelection = true;
                UnitManager.Instance.ClearHexSelectionMode();
                UIManager.Instance.ShowHexView(this);
                Select();
            }
        }
        else
        {
            SI_CameraController.Instance.repeatSelection = true;
            Debug.Log("This is the resource layer");
            Select();
            UIManager.Instance.ShowHexView(this);
        }

    }

    public void Select()
    {
        /*
        var newMaterials = hexGameObject.GetComponent<MeshRenderer>().materials;
        newMaterials[0] = UnitManager.Instance.highlightHex;
        hexGameObject.GetComponent<MeshRenderer>().materials = newMaterials; */
        ShowHighlight(false);
        wiggler?.Wiggle();

        if (hexData.isOwnedByCity)
        {
            parentCity.wiggler?.Wiggle();
        }
    }

    public void Hold()
    {
        wiggler?.Wiggle();
        Debug.Log("This item was long pressed");
    }

    public void RandomizeVisualElevation()
    {
        switch (hexData.type)
        {
            case TileType.DEEPSEA:
                hexData.rndVisualElevation = Random.Range(-0.5f, -0.5f);
                break;
            case TileType.SEA:
                hexData.rndVisualElevation = Random.Range(-0.5f, -0.5f);
                break;
            case TileType.SAND:
                hexData.rndVisualElevation = Random.Range(-0.3f, -0.3f);
                break;
            case TileType.GRASS:
                hexData.rndVisualElevation = Random.Range(-0.2f, -0.2f);
                break;
            case TileType.HILL:
                hexData.rndVisualElevation = Random.Range(-0.1f, -0.1f);
                break;
            case TileType.MOUNTAIN:
                hexData.rndVisualElevation = Random.Range(0f, 0f);
                break;
            case TileType.ICE:
                hexData.rndVisualElevation = Random.Range(-.5f, .5f);
                break;

        }
    }

}
