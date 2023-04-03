using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldHex : MonoBehaviour
{
    public Hex hex;
    public GameObject go;
    TextMesh debugText;
    Wiggler wiggler;
    [SerializeField] WorldUnit associatedUnit;

    //0 - Visual Layer 
    //1 - Resource Layer 
    //2 - Unit Layer 
    //3 - Text mesh (Debug Only)

    private void Awake()
    {
        go = transform.GetChild(0).GetChild(0).gameObject;
        debugText = transform.GetChild(3).GetComponent<TextMesh>();
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
    }

    public void SetElevationFromType()
    {
        hex.Elevation = MapManager.Instance.GetElevationFromType(hex.type);
    }
    public void UpdateVisuals() //better name, this updates a lot more
    {
        for (int i = 0; i < MapManager.Instance.regions.Length; i++)
        {
            if (hex.Elevation <= MapManager.Instance.regions[i].height)
            {
                hex.type = MapManager.Instance.regions[i].type;
                break;
            }
        }

        go.GetComponent<MeshRenderer>().material = MapManager.Instance.GetTypeMaterial(hex.type);
       
    }
    public void UpdateDebugText(string newText)
    {
        debugText = transform.GetChild(3).GetComponent<TextMesh>();
        debugText.text = newText;
    }
    public void UpdatePositionInMap()
    {
           this.transform.position = hex. PositionFromCamera(
            Camera.main.transform.position,
            MapManager.Instance.mapRows,
            MapManager.Instance.mapColumns);
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

    public void SpawnCity(GameObject cityPrefab)
    {
        GameObject obj = Instantiate(cityPrefab, transform);
        hex.hasCity = true;
    }
    public void Tap(int layer)
    {
        if (UnitManager.Instance.movementSelectMode)
        {
            //UnitManager.Instance.MoveTargetTile(x, y);
            wiggler?.Wiggle();
            return;
        }
        switch (layer)
        {
            case 1:
                Debug.Log("This is the Unit layer");
                if (hex.occupied && associatedUnit != null)
                {
                    associatedUnit.Select();
                    wiggler?.Wiggle();
                    return;
                }
                else
                {
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
                hex.rndVisualElevation = Random.Range(0f, 0.0f);
                break;
            case TileType.GRASS:
                hex.rndVisualElevation = Random.Range(0.2f, 0.5f);
                break;
            case TileType.HILL:
                hex.rndVisualElevation = Random.Range(0.5f, 1f);
                break;
            case TileType.MOUNTAIN:
                hex.rndVisualElevation = Random.Range(1f, 1.2f);
                break;
            case TileType.ICE:
                hex.rndVisualElevation = Random.Range(-.5f, .5f);
                break;

        }
    }

}
