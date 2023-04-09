using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public class WorldHex : MonoBehaviour
{
    //calculated as Column * numberOfRows + row 
    public int hexIdentifier;

    public Hex hex;
    public GameObject hexGameObject;
    TextMesh debugText;
    Wiggler wiggler;
    [SerializeField] WorldUnit associatedUnit;
    public Transform unitParent;
    public Transform resourceParent;
    [SerializeField] GameObject hexHighlight;
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
        hex.SetData(column, row);
        hexIdentifier = column * MapManager.Instance.mapRows + row;
        hex.type = newType;
        if (setElevationFromType)
        {
            SetElevationFromType();
        }
    }
    public void SetElevationFromType()
    {
        hex.Elevation = MapManager.Instance.GetElevationFromType(hex.type);
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
            if (hex.Elevation <= HexOrganizerTool.Instance.regions[i].height)
            {
                newType = HexOrganizerTool.Instance.regions[i].type;
                break;
            }
        }

        if (newType != hex.type)
        {
            hex.type = newType;
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
            prefabToSpawn = Instantiate(HexOrganizerTool.Instance.hexVisualPrefabs[(int)hex.type], transform.GetChild(0));
        }
        else
        {
            prefabToSpawn = Instantiate(MapManager.Instance.hexVisualPrefabs[(int)hex.type], transform.GetChild(0));
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
        this.transform.position = hex.PositionFromCamera();
    }

    public void UnitIn(WorldUnit newUnit)
    {
        hex.occupied = true;
        associatedUnit = newUnit;
    }

    public void UnitOut(WorldUnit newUnit)
    {
        hex.occupied = false;
        associatedUnit = null;
    }

    public void SpawnCity(int playerIndex, GameObject cityPrefab)
    {
        GameObject obj = Instantiate(cityPrefab, transform);
        obj.transform.SetParent(resourceParent);
        obj.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerByIndex(playerIndex).playerColor;
        hex.playerOwnerIndex = playerIndex;
        hex.hasCity = true;

        //TODO: Remove resourced that may have been spawned on Hex
        //TODO: Filter out of tiles able to have cities;


    }

    public void Deselect()
    {
        hexGameObject.GetComponent<MeshRenderer>().materials[0] = rimMaterial;
    }
    public void Select(int layer)
    {
        if (UnitManager.Instance == null)
        {
            wiggler?.Wiggle();
            return;
        }

        if (UnitManager.Instance.movementSelectMode)
        {
            if (UnitManager.Instance.IsHexValidMove(this))
            {
                UnitManager.Instance.MoveTargetTile(this);
            }
           
            wiggler?.Wiggle();
            return;
        }

        switch (layer)
        {
            case 1:
                
                if (hex.occupied && associatedUnit != null)
                {
                    Debug.Log("This is the Unit layer");
                    associatedUnit.Select();
                    wiggler?.Wiggle();
                    return;
                }
                else
                {
                    Debug.Log("Auto-moved to the resource layer");
                    Select();
                }
                break;
            case 2:
                Debug.Log("This is the resource layer");
                Select();
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
        switch (hex.type)
        {
            case TileType.DEEPSEA:
                hex.rndVisualElevation = Random.Range(-0.5f, -0.5f);
                break;
            case TileType.SEA:
                hex.rndVisualElevation = Random.Range(-0.5f, -0.5f);
                break;
            case TileType.SAND:
                hex.rndVisualElevation = Random.Range(-0.3f, -0.3f);
                break;
            case TileType.GRASS:
                hex.rndVisualElevation = Random.Range(-0.2f, -0.2f);
                break;
            case TileType.HILL:
                hex.rndVisualElevation = Random.Range(-0.1f, -0.1f);
                break;
            case TileType.MOUNTAIN:
                hex.rndVisualElevation = Random.Range(0f, 0f);
                break;
            case TileType.ICE:
                hex.rndVisualElevation = Random.Range(-.5f, .5f);
                break;

        }
    }

}
