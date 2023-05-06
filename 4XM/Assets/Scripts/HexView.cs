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
        if (hex!=null)
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
            hexName.text = unit.unitReference.name;
            hexAvatar.color = Color.white; //TODO: change avatar with related icon

            if (unit.playerOwnerIndex == GameManager.Instance.activePlayer.index)
            {
                if (unit.isInteractable || unit.buttonActionPossible)
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
                    GenerateCityCaptureButton(unit.buttonActionPossible);
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
                                GenerateCityCaptureButton(hex.associatedUnit.buttonActionPossible);
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
                                GenerateCityCaptureButton(hex.associatedUnit.buttonActionPossible);
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

                hexName.text = newHexName;
                hexDescription.text = "This tile really has nothing on top";

                if (hex.hexData.isOwnedByCity && (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex))
                {
                    if (hex.hexData.hasResource)
                    {
                        if (hex.hexData.occupiedByEnemyUnit)
                        {
                            hexDescription.text = "Hex is occupied by Enemy.";
                            GenerateResourceButton(false);
                        }
                        else if (GameManager.Instance.CanPlayerHarvestResource(hex.hexData.resourceType))
                        {
                            
                            hexDescription.text = "Harvest this resource to upgrade your city";
                            GenerateResourceButton(true);
                        }
                        else
                        {
                            hexDescription.text = "Research more technologies to harvest this resource.";
                            GenerateResourceButton(false);
                        }
                    }
                    else
                    {
                        //Move this as permanent data to optimize;
                        List<ResourceType> adjacentResources = new List<ResourceType>();

                        bool forestMaster = false;
                        bool farmMaster = false;
                        bool mineMaster = false;

                        foreach (WorldHex adjacentHex in hex.adjacentHexes)
                        {
                            if (adjacentHex.hexData.hasBuilding)
                            {
                                if (adjacentHex.hexData.buildingType == BuildingType.ForestWorked && GameManager.Instance.activePlayer.abilities.forestBuilding)
                                {
                                    if (!hex.parentCity.cityData.masterBuildings.Contains(BuildingType.ForestMaster))
                                    {
                                        forestMaster = true;
                                     
                                    }
                                }
                                else if (adjacentHex.hexData.buildingType == BuildingType.FarmWorked && GameManager.Instance.activePlayer.abilities.farmBuilding)
                                {
                                    if (!hex.parentCity.cityData.masterBuildings.Contains(BuildingType.FarmWorked))
                                    {
                                        farmMaster = true;

                                    }
                                }
                                else if (adjacentHex.hexData.buildingType == BuildingType.MineWorked && GameManager.Instance.activePlayer.abilities.smitheryBuilding)
                                {
                                    if (!hex.parentCity.cityData.masterBuildings.Contains(BuildingType.MineMaster))
                                    {
                                        mineMaster = true;

                                    }
                                }
                            }
                        }

                        if (forestMaster)
                        {
                            GenerateBuildingButton(BuildingType.ForestMaster, true);
                        }

                        if (farmMaster)
                        {
                            GenerateBuildingButton(BuildingType.FarmMaster, true);
                        }

                        if (mineMaster)
                        {
                            GenerateBuildingButton(BuildingType.MineMaster, true);
                        }

                    }
                  
                }
                else
                {
                    if (hex.hexData.hasResource)
                    {
                        hexDescription.text = "This resource is outside of your empire's borders";
                        GenerateResourceButton(false);
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

    void GenerateBuildingButton(BuildingType type, bool shouldBeInteractable)
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForBuilding(this, hex, type, shouldBeInteractable);
    }

    void GenerateResourceButton(bool shouldBeInteractable)
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
