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

   

    void ShowHex()
    {
        if (hex.hexData.hasCity)
        {
            hexName.text = hex.cityData.cityName;
            hexDescription.text = "Move a unit here to capture this city";

            if (hex.hexData.playerOwnerIndex == -1) //city is unclaimed
            {
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;

            }
            else
            {
                hexDescriptionBackground.color = GameManager.Instance.GetCivilizationColor(hex.hexData.playerOwnerIndex, CivColorType.uiActiveColor);
            }

            if (hex.hexData.occupied)
            {
                if (hex.associatedUnit.BelongsToActivePlayer)
                {
                    if (!hex.associatedUnit.buttonActionPossible)
                    {
                        hexDescription.text = "City capture will be available on the next turn";
                    }

                    GenerateCityCaptureButton(hex.associatedUnit.buttonActionPossible);
                }
            }

        }
        else
        {
            if (hex.hexData.hasResource)
            {
                if (hex.hexData.resourceType == ResourceType.MONUMENT)
                {
                    hexName.text = MapManager.Instance.GetResourceByType(hex.hexData.resourceType).resourceName;
                    hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                    hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);

                  
                    if (hex.hexData.occupied)
                    {
                        if (!hex.associatedUnit.hasMoved && !hex.associatedUnit.hasAttacked)
                        {
                            hexDescription.text = "Claim the " + hexName.text.ToLower() + " for a reward";
                            GenerateResourceButton(true);
                        }
                        else
                        {
                            hexDescription.text = "The " + hexName.text.ToLower() + " can be claimed on the next turn";
                            GenerateResourceButton(false);
                        }
                    }
                    else
                    {
                        hexDescription.text = "Bring a unit to the  " + hexName.text.ToLower() + " to claim it for a reward";
                        GenerateResourceButton(false);
                      
                    }                    
                }
                else
                {
                    hexName.text = MapManager.Instance.GetResourceByType(hex.hexData.resourceType).resourceName;
                    hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                    hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
                    hexDescription.text = "This  " + hexName.text.ToLower() + " resource is outside of your empire's borders";
                    GenerateResourceButton(false);
                }
               
            }
            else if (hex.hexData.hasBuilding)
            {
                hexName.text = MapManager.Instance.GetBuildingByType(hex.hexData.buildingType).buildingName;
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);

                hexDescription.text = "This hex has a " + hexName.text + " building";
            }
            else
            {
                hexName.text = SetName(hex.hexData.type);
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
                hexDescription.text = "This is an empty " + hexName.text.ToLower() + " hex.";
            }

            if (GameManager.Instance.activePlayer.abilities.roads && !hex.hexData.hasRoad && hex.hexData.playerOwnerIndex == -1)
            {
                if (hex.hexData.type == TileType.GRASS || hex.hexData.type == TileType.SAND || hex.hexData.type == TileType.HILL)
                {
                    GenerateRoadButton();
                }
            }
           
        }
    }

    void ShowPlayerHex()
    {
        if (hex.hexData.hasCity)
        {
            hexName.text = hex.cityData.cityName;

            hexDescriptionBackground.color = GameManager.Instance.GetCivilizationColor(hex.hexData.playerOwnerIndex, CivColorType.uiActiveColor);
            
            GenerateUnitButtons();
            
            if (hex.hexData.occupied)
            {
                hexDescription.text = "You cannot create a unit while the city is occupied";
            }
            else
            {
                hexDescription.text = "Buy more units to expand your empire";
            }

        }
        else
        {
            if (hex.hexData.hasBuilding)
            {
                hexName.text = MapManager.Instance.GetBuildingByType(hex.hexData.buildingType).buildingName;
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);

                hexDescription.text = "This hex has a " + hexName.text + " building";

                if (GameManager.Instance.activePlayer.abilities.destroyAbility)
                {
                    GenerateDestroyButton(true);
                }
            }
            else if (hex.hexData.hasResource)
            {
                if (hex.hexData.resourceType == ResourceType.MONUMENT)
                {
                    hexName.text = MapManager.Instance.GetResourceByType(hex.hexData.resourceType).resourceName;
                    hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                    hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);


                    if (hex.hexData.occupied)
                    {
                        if (hex.associatedUnit.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
                        {
                            if (!hex.associatedUnit.hasMoved && !hex.associatedUnit.hasAttacked)
                            {
                                hexDescription.text = "Claim the " + hexName.text.ToLower() + " for a reward";
                                GenerateResourceButton(true);
                            }
                            else
                            {
                                hexDescription.text = "The" + hexName.text.ToLower() + " can be claimed on the next turn";
                                GenerateResourceButton(false);
                            }
                        }
                        else
                        {
                            hexDescription.text = "The" + hexName.text.ToLower() + " will be claimed by the enemy on the next turn";
                            GenerateResourceButton(false);
                        }
                        
                    }
                    else
                    {
                        hexDescription.text = "Bring a unit to the  " + hexName.text.ToLower() + " to claim it for a reward";
                        GenerateResourceButton(false);

                    }
                }
                else
                {
                    hexName.text = MapManager.Instance.GetResourceByType(hex.hexData.resourceType).resourceName;
                    hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                    hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
                    bool resourceButtonState = false;

                    if (hex.hexData.occupied && hex.hexData.occupiedByEnemyUnit)
                    {
                        hexDescription.text = "You cannot harvest a resource while an enemy is occupying the hex";
                        resourceButtonState = false;
                    }
                    else if (GameManager.Instance.CanPlayerHarvestResource(hex.hexData.resourceType))
                    {
                        hexDescription.text = "Harvest this  " + hexName.text.ToLower() + " resource to upgrade your city";
                        resourceButtonState = true;
                    }
                    else
                    {
                        hexDescription.text = "Research more technologies to harvest  " + hexName.text.ToLower() + "  resources";
                        resourceButtonState = false;
                    }

                    GenerateResourceButton(resourceButtonState);

                    if (GameManager.Instance.CanPlayerDestoryResourceForReward(hex.hexData.resourceType))
                    {
                        GenerateDestroyButton(false);
                    }

                    if (MapManager.Instance.GetResourceByType(hex.hexData.resourceType).canMasterBeCreateOnTop)
                    {
                        CheckMasterBuildingButtons();
                    }

                }


            }
            else
            {
                hexName.text = SetName(hex.hexData.type);
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
                hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
                hexDescription.text = "This is an empty " + hexName.text.ToLower() + " hex in your empire";

               


                switch (hex.hexData.type)
                {
                    case TileType.GRASS:
                    case TileType.SAND:
                    case TileType.HILL:
                        CheckMasterBuildingButtons();

                        if (GameManager.Instance.activePlayer.abilities.guildBuilding)
                        {
                            if (!hex.parentCity.cityData.masterBuildings.Contains(BuildingType.Guild))
                            {
                                GenerateGuildButton();
                            }
                        }

                        if (GameManager.Instance.activePlayer.abilities.createAnimals)
                        {
                            GenerateCreationButton(ResourceType.ANIMAL);
                        }
                        if (GameManager.Instance.activePlayer.abilities.createForest)
                        {
                            GenerateCreationButton(ResourceType.FOREST);
                        }
                        if (GameManager.Instance.activePlayer.abilities.createFarm)
                        {
                            GenerateCreationButton(ResourceType.FARM);
                        }
                        if (GameManager.Instance.activePlayer.abilities.createFruit)
                        {
                            GenerateCreationButton(ResourceType.FRUIT);
                        }
                        break;
                    case TileType.SEA:
                        if (GameManager.Instance.activePlayer.abilities.createFish)
                        {
                            GenerateCreationButton(ResourceType.FISH);
                        }
                        break;
                    case TileType.MOUNTAIN:
                        if (GameManager.Instance.activePlayer.abilities.createMine)
                        {
                            GenerateCreationButton(ResourceType.MINE);
                        }
                        break;
                }
               
            }

            if (GameManager.Instance.activePlayer.abilities.roads && !hex.hexData.hasRoad)
            {
                if (hex.hexData.type == TileType.GRASS || hex.hexData.type == TileType.SAND || hex.hexData.type == TileType.HILL)
                {
                    GenerateRoadButton();
                }
            }
        }
    }

    void CheckMasterBuildingButtons()
    {
        List<ResourceType> adjacentResources = new List<ResourceType>();

        bool forestMaster = false;
        bool farmMaster = false;
        bool mineMaster = false;

        foreach (WorldHex adjacentHex in hex.adjacentHexes)
        {
            if (adjacentHex.hexData.hasBuilding && adjacentHex.hexData.playerOwnerIndex == GameManager.Instance.activePlayer.index)
            {
                if (adjacentHex.hexData.buildingType == BuildingType.ForestWorked)
                {
                    if (!hex.parentCity.cityData.masterBuildings.Contains(BuildingType.ForestMaster))
                    {
                        forestMaster = true;
                    }
                }
                else if (adjacentHex.hexData.buildingType == BuildingType.FarmWorked)
                {
                    if (!hex.parentCity.cityData.masterBuildings.Contains(BuildingType.FarmMaster))
                    {
                        farmMaster = true;
                    }
                }
                else if (adjacentHex.hexData.buildingType == BuildingType.MineWorked)
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
            GenerateBuildingButton(BuildingType.ForestMaster, GameManager.Instance.activePlayer.abilities.forestMasterBuilding);
        }

        if (farmMaster)
        {
            GenerateBuildingButton(BuildingType.FarmMaster, GameManager.Instance.activePlayer.abilities.farmMasterBuilding);
        }

        if (mineMaster)
        {
            GenerateBuildingButton(BuildingType.MineMaster, GameManager.Instance.activePlayer.abilities.mineMasterBuilding);
        }
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
            ShowUnit();
        }
        else
        {
            if (hex.hexData.playerOwnerIndex != GameManager.Instance.activePlayerIndex)
            {
                ShowHex();
            }
            else
            {
                ShowPlayerHex();
            }
           
        }        
    }

    void ShowUnit()
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

            if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
            {
                hexDescription.text = "Claim the monument for a reward";
                GenerateResourceButton(unit.buttonActionPossible);
            }

        }
        else
        {
            hexDescription.text = "This unit belongs to a different player";
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionUnavailable;
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

    void GenerateDestroyButton(bool isBuilding)
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForDestory(this, hex, isBuilding);
    }

    void GenerateGuildButton()
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForBuilding(this, hex, BuildingType.Guild, true);
    }

    void GenerateRoadButton()
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForRoad(this, hex);
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

    void GenerateCreationButton(ResourceType type)
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForResourceCreation(this, hex, type);
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
