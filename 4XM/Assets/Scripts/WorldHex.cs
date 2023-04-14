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
    TextMesh debugText;
    Wiggler wiggler;
    [SerializeField] WorldUnit associatedUnit;
    public Transform unitParent;
    public Transform resourceParent;
    [SerializeField] GameObject hexHighlight;
    [SerializeField] GameObject cityGameObject;
    public WorldHex parentCity;
    //0 - Visual Layer 
    //1 - Resource Layer 
    //2 - Unit Layer 
    //3 - Text mesh (Debug Only)
    //4 - Hex Highlight

    Material rimMaterial;

    private void Awake()
    {
        hexGameObject = transform.GetChild(0).GetChild(0).gameObject;
        resourceParent = transform.GetChild(1);
        unitParent = transform.GetChild(2);
        debugText = transform.GetChild(3).GetComponent<TextMesh>();
        hexHighlight = transform.GetChild(4).gameObject;
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

    public void ShowHighlight()
    {
        if (hexHighlight != null)
        hexHighlight?.SetActive(true);
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

    public void UpdateDebugText(string newText)
    {
        debugText = transform.GetChild(3).GetComponent<TextMesh>();
        debugText.text = newText;
    }
    public void UpdatePositionInMap()
    {
        this.transform.position = hexData.PositionFromCamera();
    }

    public void UnitIn(WorldUnit newUnit)
    {
        hexData.occupied = true;
        associatedUnit = newUnit;
    }

    public void UnitOut(WorldUnit newUnit)
    {
        hexData.occupied = false;
        associatedUnit = null;
    }

    List<WorldHex> OccupyCityHexes()
    {
        return MapManager.Instance.GetHexesListWithinRadius(this.hexData, cityData.range);
    }

    public void SpawnCity()
    {
        GameObject obj = Instantiate(MapManager.Instance.cityPrefab, transform);
        obj.transform.SetParent(resourceParent);
        cityGameObject = obj;
        hexData.hasCity = true;
        cityData = new CityData();

        cityData.level = 1;
        cityData.output = 1;
        cityData.range = 1;
        cityData.playerIndex = -1;
        cityData.cityHexes = OccupyCityHexes();

      

    }

    public void SetAsOccupiedByCity(WorldHex parentCityHex)
    {
        parentCity = parentCityHex;
        hexData.isOwnedByCity = true;
        hexData.playerOwnerIndex = parentCity.hexData.playerOwnerIndex;
        this.hexGameObject.GetComponent<MeshRenderer>().materials[0].color =
            GameManager.Instance.GetPlayerByIndex(hexData.playerOwnerIndex).playerColor;
        //remove this later on

    }

    public void GenerateResources()
    {
        if (hexData.hasCity)
        {
            Debug.LogError("Tried to generate resource for city Hex");
            return;
        }
        
        //allocate a resource base on mapmanager chances and surrounded resources
        //spawn resource
        //set correct data
        //GameObject resourceObj = Instantiate(MapManager.Instance.GetResourcePrefab)
    }

    public void OccupyCityByPlayer(Player player, bool spawnUnit = false)
    {
        if (!hexData.hasCity)
        {

            Debug.LogError("Hed does not have a city");
            return;
        }

        cityGameObject.GetComponent<MeshRenderer>().material.color = player.playerColor;
        hexData.playerOwnerIndex = GameManager.Instance.GetPlayerIndex(player);
        cityData.playerIndex = hexData.playerOwnerIndex;

        foreach (WorldHex newHex in cityData.cityHexes)
        {
            newHex.SetAsOccupiedByCity(this);
        }

        cityData.availableUnits = player.playerUnitsThatCanBeSpawned;

        if (spawnUnit)
        {

        }

    }

    public void Deselect()
    {
        hexGameObject.GetComponent<MeshRenderer>().materials[0] = rimMaterial;
    }

    public void Select(int layer)
    {
        if (UnitManager.Instance == null)
        {
            //This is just for visual prototype feedback testing
            wiggler?.Wiggle();
            return;
        }

        //if a unit is moving 
        if (UnitManager.Instance.movementSelectMode)
        {
            if (UnitManager.Instance.IsHexValidMove(this))
            {
                UnitManager.Instance.MoveTargetTile(this);
            }
           
            wiggler?.Wiggle();
            return;
        }

        //layer sets if the unit or the hex is going to be shown
        switch (layer)
        {
            case 1:
                
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
                    Debug.Log("Auto-moved to the resource layer");
                    UIManager.Instance.ShowHexView(this);
                    Select();
                }
                break;
            case 2:
                Debug.Log("This is the resource layer");
                Select();
                UIManager.Instance.ShowHexView(this);
                break;
        }
    }

    public void Select()
    {
        /*
        var newMaterials = hexGameObject.GetComponent<MeshRenderer>().materials;
        newMaterials[0] = UnitManager.Instance.highlightHex;
        hexGameObject.GetComponent<MeshRenderer>().materials = newMaterials; */
        wiggler?.Wiggle();
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
