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
    [SerializeField] Image hexDescriptionBackground;
    [SerializeField] Image hexAvatar;

    [SerializeField] Transform horizontalScrollParent;

    [SerializeField] GameObject actionItemPrefab;
    WorldHex hex;
    WorldUnit unit;
    bool isUnitView;

    public void Refresh()
    {
        SetData(hex, unit);
    }


    public void SetData(WorldHex newHex, WorldUnit newUnit = null)
    {
        unit = null;

        foreach(Transform child in horizontalScrollParent)
        {
            Destroy(child.gameObject);
        }

        hex = newHex;
        unit = newUnit;

        if (unit != null)
        {
            isUnitView = true;
        }
        else
        {
            isUnitView = false;
        }

        if (isUnitView)
        {
            hexName.text = unit.data.unitName;
            hexAvatar.color = Color.white; //TODO: change avatar with related icon

            if (unit.data.associatedPlayerIndex == GameManager.Instance.activePlayer.index)
            {
                if (unit.IsInteractable)
                {
                    hexDescription.text = "This unit has available actions";
                    hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                }
                else
                {
                    hexDescription.text = "This unit does not have any actions left";
                    hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionUnavailable;
                }

                if (hex.hexData.hasCity && hex.hexData.playerOwnerIndex != GameManager.Instance.activePlayer.index)
                {
                    GenerateCityCaptureButton(unit.HasActionsLeft);
                }

            }
            else
            {
                hexDescription.text = "This unit belongs to a different player";
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionUnavailable;
            }

            
        }
        else
        {
            //if there's a city, and it belongs to the player, create buttons to spawn units
            if (hex.hexData.hasCity)
            {
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                hexName.text = hex.cityData.cityName;

                if (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
                {
                    GenerateUnitButtons();
                    hexDescription.text = "Create more units! ";
                }
                else if (hex.hexData.playerOwnerIndex == -1)
                {
                    hexDescription.text = "Move a unit here to capture this city";

                    if (hex.hexData.occupied)
                    {
                        if (hex.associatedUnit.BelongsToActivePlayer)
                        {
                            if (hex.hexData.playerOwnerIndex != GameManager.Instance.activePlayer.index)
                            {
                                GenerateCityCaptureButton(hex.associatedUnit.HasActionsLeft);
                            }
                        }
                    }
                }
                else
                {
                    hexDescription.text = "This city belongs to a different player";

                    if (hex.hexData.occupied)
                    {
                        if (hex.associatedUnit.BelongsToActivePlayer)
                        {
                            if (hex.hexData.playerOwnerIndex != GameManager.Instance.activePlayer.index)
                            {
                                GenerateCityCaptureButton(hex.associatedUnit.HasActionsLeft);
                            }
                        }
                    }
                }
                
               
              

               
            }
            else
            {
                string newHexName = SetName(hex.hexData.type);
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
                if (hex.hexData.hasResource)
                {
                    //TODO: move this to someplace else
                   // newHexName += " ," + MapManager.Instance.hexResources[hex.hexData.resourceIndex].resourceName;
                }

                hexName.text = newHexName;
                hexDescription.text = "This tile really has nothing on top";

                if (hex.hexData.isOwnedByCity && (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex))
                {
                    if (hex.hexData.hasResource)
                    {
                        if (GameManager.Instance.CanPlayerHarvestResource(hex.hexData.resourceType))
                        {
                            hexDescription.text = "Harvest this resource to upgrade your city";
                            GenerateResourceButtons(true);
                        }
                        else
                        {
                            hexDescription.text = "Research required.";
                            GenerateResourceButtons(false);
                        }
                       
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
        foreach(UnitType unitType in GameManager.Instance.activePlayer.gameUnitsDictionary.Keys)
        {
            if (GameManager.Instance.activePlayer.gameUnitsDictionary[unitType])
            {
                GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
                obj.GetComponent<ActionButton>().SetDataForUnitSpawn(this, hex, unitType);
            }
           
        }
    }

    void GenerateResourceButtons(bool shouldBeInteractable)
    {
        //update this to support multiple resources on the same hex
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForResource(this, hex, shouldBeInteractable);
    }

    public void GenerateCityCaptureButton(bool doesUnitHaveActions)
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForCityCapture(this, hex, doesUnitHaveActions);
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
