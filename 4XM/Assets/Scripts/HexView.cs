using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SignedInitiative;

public class HexView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI hexName;
    [SerializeField] TextMeshProUGUI hexDescription;
    [SerializeField] Image hexAvatar;

    [SerializeField] Transform horizontalScrollParent;

    [SerializeField] GameObject actionItemPrefab;
    WorldHex hex;
    bool isUnitView;

    public void SetData(WorldHex newHex, WorldUnit newUnit = null)
    {
        foreach(Transform child in horizontalScrollParent)
        {
            Destroy(child.gameObject);
        }

        hex = newHex;

        if (newUnit != null)
        {
            isUnitView = true;
        }
        else
        {
            isUnitView = false;
        }

        if (isUnitView)
        {
            hexName.text = newUnit.data.unitName;

            if (newUnit.data.associatedPlayerIndex == GameManager.Instance.activePlayer.index)
            {
                if (newUnit.data.hasMoved)
                {
                    hexDescription.text = "This unit does not have any actions left";
                }
                else
                {
                    hexDescription.text = "This unit has available actions";
                }
            }
           
            if (hex.hexData.hasCity && hex.hexData.playerOwnerIndex != GameManager.Instance.activePlayer.index)
            {
                GenerateCityCaptureButton();
            }
        }
        else
        {
            if (hex.hexData.hasCity)
            {
                hexName.text = hex.cityData.cityName;

                if (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
                {
                    GenerateUnitButtons();
                }
            }
            else
            {
                string newHexName = SetName(hex.hexData.type);
                if (hex.hexData.hasResource)
                {
                    newHexName += " ," + MapManager.Instance.hexResources[hex.hexData.resourceIndex].resourceName;
                }

                hexName.text = newHexName;
                hexDescription.text = "This tile really has nothing on top";

                if (hex.hexData.isOwnedByCity && (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex))
                {
                    if (hex.hexData.hasResource)
                    {
                        hexDescription.text = "Harvest this resource to upgrade your city";
                        GenerateResourceButtons();
                    }
                  
                }
                else
                {
                    if (hex.hexData.hasResource)
                    {
                        hexDescription.text = "This resource is outside of your empire's borders";
                    }
                    else
                    {
                        hexDescription.text = "This tile really has nothing on top";
                    }
                   
                }
               

            }
        }
        
    }

    void GenerateUnitButtons()
    {
       // List<Units>
        foreach(WorldUnit unit in GameManager.Instance.activePlayer.playerUnitsThatCanBeSpawned)
        {
            GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
            obj.GetComponent<ActionButton>().SetDataForUnitSpawn(this, hex, unit);
        }
    }

    void GenerateResourceButtons()
    {
        //update this to support multiple resources on the same hex
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForResource(this, hex);
    }

    public void GenerateCityCaptureButton()
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForCityCapture(this, hex);
    }


    public string SetName(TileType type)
    {
        switch (type)
        {
            case TileType.DEEPSEA:
                return "Ocean";
            case TileType.SEA:
                return "Sea";
            case TileType.SAND:
                return "Sand";
            case TileType.GRASS:
                return "Grasslands";
            case TileType.HILL:
                return "Hills";
            case TileType.MOUNTAIN:
                return "Mountains";
            case TileType.ICE:
                return "Icecaps";
        }

        return null;
    }

}
