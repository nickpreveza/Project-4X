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
    [SerializeField] GameObject cloud;
    [SerializeField] GameObject border;
    [SerializeField] Transform particleParent;
    [SerializeField] GameObject cityGameObject;
    public WorldHex parentCity;
    public CityView cityView;
    CityVisualHelper visualHelper;
    RoadHelper roadHelper;
    public List<WorldHex> adjacentHexes = new List<WorldHex>();
    //0 - Visual Layer 
    //1 - Resource Layer 
    //2 - Unit Layer 
    //3 - Hex Highlight
    //4 - Road Layer
    //5 - Fog Layer
    //6 - Particle Layer
    //Material rimMaterial;


    bool isHidden = true;
    GameObject activeParticle;

    private void Awake()
    {
        hexGameObject = transform.GetChild(0).GetChild(0).gameObject;
        resourceParent = transform.GetChild(1);
        unitParent = transform.GetChild(2);
        hexHighlight = transform.GetChild(3).gameObject;
        roadHelper = transform.GetChild(4).GetChild(0).GetComponent<RoadHelper>();
        cloud = transform.GetChild(5).GetChild(0).gameObject;
        border = transform.GetChild(6).GetChild(0).gameObject;
        particleParent = transform.GetChild(7);
        SI_EventManager.Instance.onCameraMoved += UpdatePositionInMap;
        roadHelper.DisableRoads();
        RandomizeVisualElevation();

        roadHelper.SetParent(this);
    }

    private void OnDestroy()
    {
        SI_EventManager.Instance.onCameraMoved -= UpdatePositionInMap;
    }
    void Start()
    {
        wiggler = GetComponent<Wiggler>();
        //rimMaterial = hexGameObject.GetComponent<MeshRenderer>().materials[0];
        HideHighlight();
    }

    public void SpawnParticle(GameObject particlePrefab)
    {
        if (activeParticle == null && particlePrefab != null)
        {
            activeParticle = Instantiate(particlePrefab, particleParent);
            Invoke("DestroyParticle", 1f);
        }
    }

    void DestroyParticle()
    {
        if (activeParticle != null)
        {
            Destroy(activeParticle);
        }
    }

    public void SetHiddenState(bool isHiddenState, bool fade)
    {
        if (fade)
        {
            StartCoroutine(HiddenStateEnum(isHiddenState));
        }
        else
        {
            ActualHide(isHiddenState);
        }
    }

    IEnumerator HiddenStateEnum(bool hiddenState)
    {
       
        if (cloud != null)
        {

            if (hiddenState)
            {
                ActualHide(hiddenState);
            }
            else
            {
                cloud.GetComponent<Animator>().SetTrigger("FadeOut");
                yield return new WaitForSeconds(.5f);
                ActualHide(hiddenState);
            }

        }
    }


    void ActualHide(bool hiddenState)
    {
        if (cloud != null)
        {
            cloud.SetActive(hiddenState);
            isHidden = hiddenState;
            resourceParent.gameObject.SetActive(!hiddenState);
            unitParent.gameObject.SetActive(!hiddenState);
            if (hexData.type == TileType.MOUNTAIN)
            {
                if (hexGameObject.transform.childCount > 0)
                {
                    hexGameObject.transform.GetChild(0).gameObject.SetActive(!hiddenState);
                }
            }
            if (hexData.hasCity)
            {
                if (cityView != null)
                {
                    cityView.SetCanvasGroupAlpha(hiddenState);
                }
            }

            if (!hiddenState)
            {
                RandomizeVisualElevation();
            }
        }
        
    }

    void HideBorder()
    {
        border?.SetActive(false);
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

        HideBorder();
        SetHiddenState(true, false);
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
        if (newUnit.playerOwnerIndex != hexData.playerOwnerIndex)
        {
            if (hexData.hasCity)
            {
                cityData.isUnderSiege = true;

                //does the same thing for now - debug 
                if (hexData.playerOwnerIndex == -1)
                {
                    if (visualHelper != null)
                    {
                        visualHelper.citySiegeEffect.SetActive(true);
                    }
                    cityView.gameObject.SetActive(true);
                    cityView.SetSiegeState(false);
                    cityView.SetDetailsAlpha(false);
                    MapManager.Instance.SetHexUnderSiege(this);
                }
                else
                {
                    if (visualHelper != null)
                    {
                        visualHelper.citySiegeEffect.SetActive(true);
                    }
                    cityView.gameObject.SetActive(true);
                    cityView.SetSiegeState(false);
                    cityView.SetDetailsAlpha(true);
                    MapManager.Instance.SetHexUnderSiege(this);
                }
              
            }
        }

        if(hexData.type != TileType.MOUNTAIN)
        {
            MapManager.Instance.UnhideHexes(newUnit.playerOwnerIndex, this, 1, false);
        }
        else
        {
            MapManager.Instance.UnhideHexes(newUnit.playerOwnerIndex, this, 2, false);
        }


        associatedUnit = newUnit;

        if (hexData.type == TileType.SEA || hexData.type == TileType.DEEPSEA)
        {
            if (!associatedUnit.isBoat)
            {
                associatedUnit.EnableBoat();
            }
           
        }
        else if (hexData.type != TileType.SEA || hexData.type != TileType.DEEPSEA)
        {
            if (associatedUnit.isBoat || associatedUnit.isShip)
            {
                associatedUnit.DisableBoats();
            }
        }
    }

    public void UnitOut(WorldUnit newUnit, bool unitDied = false)
    {
        hexData.occupied = false;

        if (hexData.hasCity)
        {
            if (cityData.isUnderSiege)
            {
                cityData.isUnderSiege = false;

                cityView.RemoveSiegeState();

                if (hexData.playerOwnerIndex != -1)
                {
                    GameManager.Instance.RecalculatePlayerExpectedStars(hexData.playerOwnerIndex);
                }

                if (visualHelper != null)
                {
                    visualHelper.citySiegeEffect.SetActive(false);
                }

            }
        }
       
        associatedUnit = null;

        if (unitDied)
        {
            // SpawnParticle(UnitManager.Instance.unitDeathParticle);
            SpawnParticle(GameManager.Instance.resourceHarvestParticle);
        }
    }

    public void SpawnCity(string newName)
    {
        RemoveResource(false, false);

        GameObject obj = Instantiate(MapManager.Instance.cityPrefab, transform);
        obj.transform.SetParent(resourceParent);
        cityGameObject = obj;
        hexData.hasCity = true;
        hexData.hasBuilding = true; //maybe this will cause issues? It did. 
        hexData.buildingType = BuildingType.City;
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
        SI_CameraController.Instance.animationsRunning = true;
        for (int i = 0; i < value; i++)
        {
            if (cityData.levelPointsToNext > 0)
            {
                cityView.ToggleOffProgressPoint(false);
                cityData.levelPointsToNext--;
               
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

                cityView.ToggleOnProgressPoint(true);
                yield return new WaitForSeconds(0.3f);
            }

        }

        if (GameManager.Instance.IsIndexOfActivePlayer(hexData.playerOwnerIndex))
        {
            GameManager.Instance.activePlayer.CalculateExpectedStars();
            UIManager.Instance.UpdateHUD();
        }

         SI_CameraController.Instance.animationsRunning = false;
    }

    bool progressPointsAddRunning = false;

    public IEnumerator AddProgressPoint(int value, bool showQuests)
    {
        if (value == 0)
        {
             
            yield break;
        }

        SI_CameraController.Instance.animationsRunning = true;
        progressPointsAddRunning = true;

        for(int i = 0; i < value; i++)
        {          
            wiggler?.Wiggle();
            if (cityData.negativeLevelPoints > 0)
            {
                cityView.ToggleOffProgressPoint(true);
                cityData.negativeLevelPoints--;
                cityData.output++;        
                continue;
            }
            else
            {
                cityData.levelPointsToNext++;

                if (cityData.levelPointsToNext == cityData.targetLevelPoints)
                {
                    cityData.level++;
                    cityData.targetLevelPoints = cityData.level + 1;
                    cityData.levelPointsToNext = 0;
                    cityData.output++;

                    cityView.LevelUp();

                    if (visualHelper != null)
                    {
                        visualHelper.cityLevelEffect.SetActive(true);
                    }

                    yield return new WaitForSeconds(1f);
                    if (showQuests)
                    {
                        string popupTitle = cityData.cityName + " Leved Up";
                        string popupDescr = cityData.cityName + " has grown to level " + cityData.level + "\n\n Choose your reward: ";


                        if (cityData.level == 2)
                        {
                            UIManager.Instance.waitingForPopupReply = true;
                            UIManager.Instance.OpenPopupReward(
                                popupTitle,
                                popupDescr,
                             "+" + GameManager.Instance.unitReward.ToString() + " Unit",
                             () => PopUpCustomRewardUnit(),
                             "+" + GameManager.Instance.visibilityReward + " Visibility",
                             () => PopUpCustomRewardVisibility()
                             );
                        }
                        else if (cityData.level == 3)
                        {
                            UIManager.Instance.waitingForPopupReply = true;
                            UIManager.Instance.OpenPopupReward(
                                popupTitle,
                                popupDescr,
                              "Expand Borders",
                             () => PopupCustomRewardBorders(),
                             "+" + GameManager.Instance.currencyReward + " Stars",
                             () => PopupCustomRewardStars()
                             );
                        }
                        else if (cityData.level == 4)
                        {
                            UIManager.Instance.waitingForPopupReply = true;
                            UIManager.Instance.OpenPopupReward(
                                popupTitle,
                                popupDescr,
                             "+" + GameManager.Instance.populationReward + " Population",
                             () => PopupCustomRewardPopulation(),
                             "+" + GameManager.Instance.productionReward + " Production",
                             () => PopupCustomRewardProduction()
                             );
                        }
                        else if (cityData.level > 4)
                        {
                            if (hexData.occupied)
                            {
                                WorldUnit unit = associatedUnit;
                                if (associatedUnit.TryToMoveRandomly())
                                {
                                    UnitManager.Instance.SpawnUnitAt(GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex), UnitType.Knight, this, true, false);
                                }
                                else
                                {
                                    associatedUnit.Death(false);
                                    UnitManager.Instance.SpawnUnitAt(GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex), UnitType.Knight, this, true, false);
                                }
                            }
                            else
                            {
                                UnitManager.Instance.SpawnUnitAt(GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex), UnitType.Knight, this, true, false);
                            }
                        }
                    }


                    while (UIManager.Instance.waitingForPopupReply)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    yield return new WaitForSeconds(.5f);
                    if (visualHelper != null)
                    {
                        visualHelper.cityLevelEffect.SetActive(false);
                    }
                    else
                    {

                        visualHelper = cityGameObject.GetComponent<CityVisualHelper>();
                        visualHelper.cityLevelEffect.SetActive(false);
                    }


                }
                else
                {
                    cityView.ToggleOnProgressPoint(false);
                }
            }
            
            yield return new WaitForSeconds(.5f);
           
        }

        cityView.UpdateData();

        if (GameManager.Instance.IsIndexOfActivePlayer(hexData.playerOwnerIndex))
        {
            GameManager.Instance.activePlayer.CalculateExpectedStars();
            GameManager.Instance.activePlayer.CalculateDevelopmentScore(false);
            UIManager.Instance.UpdateHUD();
        }

        SI_CameraController.Instance.animationsRunning = false;
        progressPointsAddRunning = false;

    }


    void PopUpCustomRewardVisibility()
    {
        MapManager.Instance.UnhideHexes(hexData.playerOwnerIndex, this, GameManager.Instance.visibilityReward, false);
        UIManager.Instance.waitingForPopupReply = false;
    }
    
    void PopUpCustomRewardUnit()
    {
        if (hexData.occupied)
        {
            WorldUnit unit = associatedUnit;
            if (associatedUnit.TryToMoveRandomly())
            {
                UnitManager.Instance.SpawnUnitAt(GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex), GameManager.Instance.unitReward, this, true, false);
            }
            else
            {
                associatedUnit.Death(false);
                UnitManager.Instance.SpawnUnitAt(GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex), GameManager.Instance.unitReward, this, true, false);
            }
        }
        else
        {
            UnitManager.Instance.SpawnUnitAt(GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex), GameManager.Instance.unitReward, this, true, false);
        }
        UIManager.Instance.waitingForPopupReply = false;
    }

    void PopupCustomRewardStars()
    {
        GameManager.Instance.AddStars(GameManager.Instance.activePlayerIndex, GameManager.Instance.currencyReward);
        UIManager.Instance.waitingForPopupReply = false;
    }

    void PopupCustomRewardProduction()
    {
        cityData.output++;
        UIManager.Instance.waitingForPopupReply = false;
    }

    IEnumerator WaitForProgressPointsToAddProgressPoints(int amount)
    {
        while (progressPointsAddRunning)
        {
            yield return new WaitForFixedUpdate();
        }

        AddLevelPoint(amount);
    }

    IEnumerator WaitForProgressPointsToAddBorders()
    {
        while (progressPointsAddRunning)
        {
            yield return new WaitForFixedUpdate();
        }

        ExpandBorders();
    }

    void ExpandBorders()
    {
        cityData.range = GameManager.Instance.rangeReward;
        List<WorldHex> hexesToAdd = MapManager.Instance.GetHexesListWithinRadius(this.hexData, GameManager.Instance.rangeReward);

        foreach (WorldHex hex in hexesToAdd)
        {
            if (!hex.hexData.isOwnedByCity && !cityData.cityHexes.Contains(hex))
            {
                cityData.cityHexes.Add(hex);
                hex.SetAsOccupiedByCity(this);
            }
        }

        MapManager.Instance.UnhideHexes(hexData.playerOwnerIndex, this, GameManager.Instance.rangeReward + 1, false);


        cityView.UpdateData();
    }

    void PopupCustomRewardPopulation()
    {
        StartCoroutine(WaitForProgressPointsToAddProgressPoints(GameManager.Instance.populationReward));
        UIManager.Instance.waitingForPopupReply = false;
    }

    void PopupCustomRewardBorders()
    {
        StartCoroutine(WaitForProgressPointsToAddBorders());
        UIManager.Instance.waitingForPopupReply = false;
    }

    void AddProductionPoints(int points)
    {
        cityData.output += points;
        cityView.UpdateData();
    }

    void AddLevelPoint(int points)
    {
        StartCoroutine(AddProgressPoint(points, true));
    }

    void RemoveLevelPoint(int points)
    {
        StartCoroutine(RemoveProgressPoint(points));
    }

    public void CreateResource(ResourceType type)
    {
        GenerateResource(type);
        Select(false);
    }

    public void HarvestResource()
    {
        //for some reason this didnt work inside the enum
        GameObject resourceObj = resourceParent.GetChild(0).gameObject;
        Destroy(resourceObj);
        SpawnParticle(GameManager.Instance.resourceHarvestParticle);
        StartCoroutine(HarvestResourceEnum());
    }

    IEnumerator HarvestResourceEnum()
    {
        SI_CameraController.Instance.animationsRunning = true;

        Resource resource = MapManager.Instance.GetResourceByType(hexData.resourceType);

        if(resource.type == ResourceType.MONUMENT)
        {
            associatedUnit.MonumentCapture();
            int randomReward = Random.Range(0, 7);
            GameManager.Instance.MonumentReward(randomReward, associatedUnit);

        }
        else if (resource.transformToBuilding)
        {
           
            hexData.hasBuilding = true;
            hexData.buildingType = MapManager.Instance.GetBuildingByResourceType(hexData.resourceType);

            GameObject obj = Instantiate(MapManager.Instance.GetBuildingByType(hexData.buildingType).levelPrefabs[0], resourceParent);

            if (resource.output > 0)
            {
                parentCity.AddLevelPoint(resource.output);
            }

            yield return new WaitForSeconds(.5f);
            FindMaster();

        }
        else
        {
            if (resource.output > 0)
            {
                parentCity.AddLevelPoint(resource.output);
            }

        }

        hexData.hasResource = false;
        hexData.resourceType = ResourceType.EMPTY;

        Select(false);
        SI_CameraController.Instance.animationsRunning = false;

    }

    

    void FindMaster()
    {
        foreach(WorldHex adjHex in adjacentHexes)
        {
            if (adjHex.hexData.hasCity)
            {
                continue;
            }
            if(adjHex.parentCity != this.parentCity)
            {
                continue;
            }
            if (adjHex.hexData.buildingType == MapManager.Instance.GetBuildingByType(hexData.buildingType).masterBuilding)
            {
                if (adjHex.parentCity == this.parentCity)
                {
                    adjHex.AddLevelToMaster();
                    break;
                }
            }
            
            
        }
    }

    void AddLevelToMaster()
    {
        hexData.buildingLevel++;
        parentCity.AddLevelPoint(MapManager.Instance.GetBuildingByType(hexData.buildingType).output);
    }

    public void SetAsOccupiedByCity(WorldHex parentCityHex)
    {
        parentCity = parentCityHex;
        hexData.isOwnedByCity = true;
        hexData.playerOwnerIndex = parentCity.hexData.playerOwnerIndex;

        Color newColor = GameManager.Instance.GetCivilizationColor(hexData.playerOwnerIndex, CivColorType.borderColor);

        //TODO: Set outline material color only perimiter; 

        border.SetActive(true);
        border.GetComponent<MeshRenderer>().materials[0].color = newColor;

        //set outline material to match player color 
      
        //remove this later on

    }

    public bool DoesCityHaveMasterBuildingOfType(BuildingType type)
    {
        return cityData.masterBuildings.Contains(type);
    }

    public void CreateBuilding(BuildingType type)
    {
        StartCoroutine(CreateMaster(type));
    }

    IEnumerator CreateMaster(BuildingType type)
    {
        SI_CameraController.Instance.animationsRunning = true;
        if (hexData.hasBuilding)
        {
            Debug.LogWarning("Tried to place building on top of building");
            SI_CameraController.Instance.animationsRunning = false;
            yield break;
        }

        if (hexData.hasResource)
        {
            RemoveResource(false, false);
        }

        parentCity.cityData.masterBuildings.Add(type);

        Building building = MapManager.Instance.GetBuildingByType(type);

        hexData.hasBuilding = true;
        hexData.buildingType = type;
        hexData.buildingLevel = 0;
        int buildingLevelPrefab = 0;

        if (type != BuildingType.Guild)
        {
            hexData.buildingLevel++;
            List<WorldHex> hexesToGainLevelFrom = new List<WorldHex>();
            GameObject obj = Instantiate(building.levelPrefabs[buildingLevelPrefab], resourceParent);

            foreach (WorldHex hex in adjacentHexes)
            {
                if (hex.hexData.hasBuilding && hex.parentCity == this.parentCity)
                {
                    if (hex.hexData.buildingType == MapManager.Instance.GetBuildingByType(hexData.buildingType).slaveBuilding)
                    {
                        hexesToGainLevelFrom.Add(hex);
                        
                    }
                }
    
            }

            foreach(WorldHex hex in hexesToGainLevelFrom)
            {
                yield return new WaitForSeconds(0.5f);
                hexData.buildingLevel++;
                hex.wiggler.Wiggle();
                parentCity.AddLevelPoint(MapManager.Instance.GetBuildingByType(hexData.buildingType).output);
            }
            /*
            if (building.levelPrefabs.Length > hexData.buildingLevel)
            {
                buildingLevelPrefab = hexData.buildingLevel-1;
            }*/

        }
        else
        {
            hexData.buildingLevel = 1;
            buildingLevelPrefab = 0;

            parentCity.AddLevelPoint(MapManager.Instance.GetBuildingByType(hexData.buildingType).output);
            GameObject obj = Instantiate(building.levelPrefabs[buildingLevelPrefab], resourceParent);
        }

        SI_CameraController.Instance.animationsRunning = false;
        UIManager.Instance.ShowHexView(this);
    }

    public void DestroyAction(bool isBuilding)
    {
        if (isBuilding)
        {
            SpawnParticle(GameManager.Instance.explosionParticle);
            RemoveBuilding();
        }
        else
        {
            SpawnParticle(GameManager.Instance.explosionParticle);
            RemoveResource(true, true);
        }
    }

    void RemoveBuilding()
    {
        Building building = MapManager.Instance.GetBuildingByType(hexData.buildingType);

        //recalculate master buildings surrounding this;
        //remove level point from city

        if (building.isMaster)
        {
            parentCity.cityData.masterBuildings.Remove(building.type);

            if (building.type == BuildingType.Guild)
            {
                parentCity.RemoveLevelPoint(building.output);
            }
            else
            {
                parentCity.RemoveLevelPoint(hexData.buildingLevel);
            }
           
        }
        else
        {
            int pointsToRemove = MapManager.Instance.GetResourceByType(building.matchingResource).output;
            pointsToRemove += FindPointsToRemoveFromMasters(building);
            parentCity.RemoveLevelPoint(pointsToRemove);
        }
      

        hexData.hasBuilding = false;
        hexData.buildingType = BuildingType.Empty;

        GameObject resourceObj = resourceParent.GetChild(0).gameObject;
        if (resourceObj != null)
        {
            Destroy(resourceObj);
        }
       
        UIManager.Instance.ShowHexView(this);
    }

    int FindPointsToRemoveFromMasters(Building building)
    {
        foreach (WorldHex hex in parentCity.cityData.cityHexes)
        {
            if (hex.hexData.buildingType == building.masterBuilding)
            {
                hex.hexData.buildingLevel--;
                return MapManager.Instance.GetBuildingByType(building.masterBuilding).output;
            }
        }

        return 0;
    }

    public void CreateRoad()
    {
        if (hexData.hasRoad)
        {
            Debug.LogWarning("Tried to create road on an existing road");
            return;
        }

        hexData.hasRoad = true;
        //UpdateVisuals();
        //CalculateRoadConnections
        roadHelper.SetRoads();
        UIManager.Instance.ShowHexView(this);
    }

    public void AdjacentRoadChanged(WorldHex adjHex)
    {
        roadHelper.UpdateByAdjacentHex(adjHex);
    }

    public void GenerateResource(ResourceType resourceType)
    {
        if (hexData.hasCity)
        {
            Debug.LogError("Tried to generate resource for city Hex");
            return;
        }

        if (hexData.hasResource)
        {
            Debug.LogError("Tried to generate resource on top of resource without removing the previous resource");
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

    public void RemoveResource(bool applyReward, bool updateUI)
    {
        if (hexData.hasResource)
        {
            if (applyReward)
            {
                if (MapManager.Instance.GetResourceByType(hexData.resourceType).canBeDestroyedForReward)
                {
                    GameManager.Instance.AddStars(GameManager.Instance.activePlayerIndex, MapManager.Instance.GetResourceByType(hexData.resourceType).destroyReward);
                }
            }
            
            hexData.resourceType = ResourceType.EMPTY;
            hexData.hasResource = false;
            hexData.moveCost = MapManager.Instance.GetMoveCostForType(hexData.type);
            GameObject resourceObj = resourceParent.GetChild(0).gameObject;

            if (resourceObj != null)
            {
                Destroy(resourceObj);
            }
           
        }

        hexData.resourceType = ResourceType.EMPTY;

        if (updateUI)
        UIManager.Instance.ShowHexView(this);
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

        if (cityGameObject != null)
        {
            Destroy(cityGameObject);
        }

        cityGameObject = Instantiate(GameManager.Instance.GetCivilizationByType(GameManager.Instance.activePlayer.civilization).cityObject, resourceParent);
        //cityGameObject.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetCivilizationColor(player.civilization, CivColorType.borderColor);

        visualHelper = cityGameObject.GetComponent<CityVisualHelper>();

        hexData.playerOwnerIndex = GameManager.Instance.GetPlayerIndex(player);
        hexData.cityHasBeenClaimed = true;
        cityData.playerIndex = hexData.playerOwnerIndex;

        List<WorldHex> newCityHexes = new List<WorldHex>(adjacentHexes);

        Color newColor = GameManager.Instance.GetCivilizationColor(hexData.playerOwnerIndex, CivColorType.borderColor);



        //TODO: Set outline material color only perimiter; 
        visualHelper.cityFlag.GetComponent<MeshRenderer>().materials[0].SetColor("_ColorShift", newColor); 
        border.SetActive(true);
        border.GetComponent<MeshRenderer>().materials[0].color = newColor;


        if (!isThisATakeOver)
        {
            foreach (WorldHex hex in adjacentHexes)
            {
                if (hex.hexData.isOwnedByCity)
                {
                    //Remove hexes that already belong to other cities 
                    newCityHexes.Remove(hex);
                }
            }

            cityData.cityHexes = newCityHexes;
            cityData.output = GameManager.Instance.startCityOutput;

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

        
        hexData.hasRoad = true;
        roadHelper.SetRoads();
        MapManager.Instance.UnhideHexes(player.index, this, cityData.range + 1, false);

        cityData.isUnderSiege = false;


        cityView.gameObject.SetActive(true);
        cityView.OccupyCity();

        if (hexData.playerOwnerIndex != -1)
        {
            GameManager.Instance.RecalculatePlayerExpectedStars(hexData.playerOwnerIndex);
        }

        if (visualHelper != null)
        {
            visualHelper.citySiegeEffect.SetActive(false);
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
        if (isHidden)
        {
            UnitManager.Instance.ClearHexSelectionMode();
            SI_CameraController.Instance.DeselectSelection();
            wiggler?.Wiggle();
            return;
        }
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
        SpawnParticle(GameManager.Instance.GetParticleInteractionByType(hexData.type));
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
        hexData.rndVisualElevation = 0f;
        return;

        //skip out of random visual elevation for now to better fit the new style

        if (isHidden)
        {
            hexData.rndVisualElevation = 0f;
            return;
        }

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
