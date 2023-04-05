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
    public Transform unitParent;
    //0 - Visual Layer 
    //1 - Resource Layer 
    //2 - Unit Layer 
    //3 - Text mesh (Debug Only)

    private void Awake()
    {
        go = transform.GetChild(0).GetChild(0).gameObject;
        unitParent = transform.GetChild(2);
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
        this.transform.position = hex.PositionFromCamera();
    }

    public void UnitIn(GameObject newUnit)
    {
        hex.occupied = true;
        newUnit.transform.SetParent(unitParent);
        //newUnit.transform.localPosition = new Vector3(0,0.5f,0);
        associatedUnit = newUnit.GetComponent<WorldUnit>();
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
            UnitManager.Instance.MoveTargetTile(this);
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
                hex.rndVisualElevation = Random.Range(0.1f, 0.1f);
                break;
            case TileType.HILL:
                hex.rndVisualElevation = Random.Range(0.5f, 0.5f);
                break;
            case TileType.MOUNTAIN:
                hex.rndVisualElevation = Random.Range(.7f, .7f);
                break;
            case TileType.ICE:
                hex.rndVisualElevation = Random.Range(-.5f, .5f);
                break;

        }
    }

}
