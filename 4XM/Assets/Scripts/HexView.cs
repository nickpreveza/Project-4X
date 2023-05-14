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

    public Sprite resourceBackground;
    public Sprite unitBackground;
    public Sprite actionBackground;
    public Sprite buildingBackground;
    public Sprite claimCityBackground;
    public Sprite destroyBackground;
    public void Refresh()
    {
        if (hex!=null)
        SetData(hex, unit);
    }

    public void SetData(WorldHex newHex, WorldUnit newUnit = null)
    {
        unit = null;

        foreach (Transform child in horizontalScrollParent)
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
            bool isOwner = (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex);

            if (hex.hexData.hasCity)
            {
                ShowCity(isOwner);
            }
            else if (hex.hexData.hasBuilding)
            {
                ShowBuilding(isOwner);
                RoadCheck();
                DestroyCheck();

            }
            else if (hex.hexData.hasResource)
            {
                if (hex.hexData.resourceType == ResourceType.MONUMENT)
                {
                    ShowMonument();
                }
                else
                {
                    ShowResource(isOwner);
                }
                RoadCheck();
                DestroyCheck();
            }
            else
            {
                ShowHex(isOwner);
                RoadCheck();
            }
        }
    }


    void DestroyCheck()
    {
        if (GameManager.Instance.activePlayer.abilities.destroyAbility)
        {
            if (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
            {
                if (hex.hexData.hasBuilding)
                {
                    GenerateDestroyButton(true);
                }
                else if (hex.hexData.hasResource)
                {
                    GenerateDestroyButton(false);
                }
            }
        }
    }
    void RoadCheck()
    {

        if (GameManager.Instance.activePlayer.abilities.roads && !hex.hexData.hasRoad)
        {
            if (hex.hexData.playerOwnerIndex == GameManager.Instance.activePlayerIndex || hex.hexData.playerOwnerIndex == -1)
            {
                if (hex.hexData.type == TileType.GRASS || hex.hexData.type == TileType.SAND || hex.hexData.type == TileType.HILL)
                {
                    GenerateRoadButton();
                }
            }

        }

    }

    void ShowHex(bool isOwner)
    {
        if (isOwner)
        {
            hexName.text = SetName(hex.hexData.type);
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
            hexDescription.text = "This is an empty " + hexName.text.ToLower() + " hex in your empire";


            switch (hex.hexData.type)
            {
                case TileType.SEA:
                    if (GameManager.Instance.activePlayer.abilities.portBuilding)
                    {
                        GeneratePortButton();
                    }
                    if (GameManager.Instance.activePlayer.abilities.createFish)
                    {
                        GenerateCreationButton(ResourceType.FISH);
                    }
                    break;
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
                case TileType.MOUNTAIN:
                    if (GameManager.Instance.activePlayer.abilities.createMine)
                    {
                        GenerateCreationButton(ResourceType.MINE);
                    }
                    break;
            }
        }
        else
        {
            hexName.text = SetName(hex.hexData.type);
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
            hexDescription.text = "This is an empty " + hexName.text.ToLower() + " hex.";
        }
    }

    void ShowCity(bool isOwner)
    {
        if (isOwner)
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

            if (hex.hexData.playerOwnerIndex == -1)
            {
                hexName.text = "Unclaimed City";
                hexDescription.text = "Move a unit here to claim this city";
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;

            }
            else
            {
                hexName.text = hex.cityData.cityName;
                hexDescription.text = "This city belongs to the " +
                    GameManager.Instance.GetCivilizationByType(GameManager.Instance.GetPlayerByIndex(hex.hexData.playerOwnerIndex).civilization);

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
                    else
                    {
                        hexDescription.text = "Capture this city to add it to your empire";
                    }

                    GenerateCityCaptureButton(hex.associatedUnit.buttonActionPossible);
                }
            }
        }
    }


    void ShowResource(bool isOwner) //Not Monument
    {
        if (isOwner)
        {
            hexName.text = MapManager.Instance.GetResourceByType(hex.hexData.resourceType).resourceName;
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
            bool resourceButtonState = false;

            if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex != hex.hexData.playerOwnerIndex)
            {
                hexDescription.text = "You cannot harvest a resource while an enemy is occupying the hex";
                resourceButtonState = false;
            }
            else
            {
                if (GameManager.Instance.CanPlayerHarvestResource(GameManager.Instance.activePlayerIndex, hex.hexData.resourceType))
                {
                    hexDescription.text = "Harvest this  " + hexName.text.ToLower() + " resource to upgrade your city";
                    resourceButtonState = true;
                }
                else
                {
                    hexDescription.text = "Research more technologies to harvest  " + hexName.text.ToLower() + "  resources";
                    resourceButtonState = false;
                }
            }

            GenerateResourceButton(resourceButtonState, hex.hexData.resourceType, true);

            if (MapManager.Instance.GetResourceByType(hex.hexData.resourceType).canMasterBeCreateOnTop)
            {
                CheckMasterBuildingButtons();
            }
        }
        else
        {
            hexName.text = MapManager.Instance.GetResourceByType(hex.hexData.resourceType).resourceName;
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);
            hexDescription.text = "This  " + hexName.text.ToLower() + " resource is outside of your empire's borders";
            GenerateResourceButton(false, ResourceType.EMPTY, false);

            if (GameManager.Instance.activePlayer.abilities.destroyAbility)
            {
                if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
                {
                    GeneratePillageButton(hex.associatedUnit.buttonActionPossible, false);
                }
            }
               
        }
    }

    void ShowMonument()
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
                    GenerateResourceButton(true, ResourceType.EMPTY, false);
                }
                else
                {
                    hexDescription.text = "The" + hexName.text.ToLower() + " can be claimed on the next turn";
                    GenerateResourceButton(false, ResourceType.EMPTY, false);
                }
            }
            else
            {
                hexDescription.text = "The" + hexName.text.ToLower() + " will be claimed by the enemy on the next turn";
                GenerateResourceButton(false, ResourceType.EMPTY, false);
            }

        }
        else
        {
            hexDescription.text = "Bring a unit to the  " + hexName.text.ToLower() + " to claim it for a reward";
            GenerateResourceButton(false, ResourceType.EMPTY, false);
        }
    }

    

    void ShowBuilding(bool isOwner)
    {
        if (isOwner)
        {
            hexName.text = MapManager.Instance.GetBuildingByType(hex.hexData.buildingType).buildingName;
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);

            if (hex.hexData.occupied && hex.hexData.buildingType == BuildingType.Guild)
            {
                if (hex.associatedUnit.type == UnitType.Trader && hex.associatedUnit.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
                {
                    //TODO check if ciy is traders origin city
                    if (hex.parentCity != hex.associatedUnit.originCity)
                    {
                        GenerateTraderButton();
                        hexDescription.text = "Trader with the city of " + hex.cityData.cityName + " for a reward";
                    }
                }
            }

            

            hexDescription.text = "This hex has a " + hexName.text + " building";
        }
        else
        {
            hexName.text = MapManager.Instance.GetBuildingByType(hex.hexData.buildingType).buildingName;
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            hexAvatar.color = UIManager.Instance.GetHexColorByType(hex.hexData.type);

            hexDescription.text = "This hex has a " + hexName.text + " building";

            if (GameManager.Instance.activePlayer.abilities.destroyAbility)
            {
                if (hex.hexData.occupied && hex.associatedUnit.playerOwnerIndex == GameManager.Instance.activePlayerIndex)
                {
                    GeneratePillageButton(hex.associatedUnit.buttonActionPossible, true);
                }
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
                hexDescription.text = "This " + unit.unitReference.name + " unit has available actions";
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionAvailable;
            }
            else
            {
                hexDescription.text = "This " + unit.unitReference.name + " unit does not have any actions left";
                hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionUnavailable;
            }

            if (GameManager.Instance.activePlayer.abilities.destroyAbility)
            {
                if (unit.parentHex.hexData.playerOwnerIndex != -1 && unit.playerOwnerIndex != unit.parentHex.hexData.playerOwnerIndex)
                {
                    if (unit.parentHex.hexData.hasResource)
                    {
                        GeneratePillageButton(unit.buttonActionPossible, false);
                    }

                    if (unit.parentHex.hexData.hasBuilding)
                    {
                        GeneratePillageButton(unit.buttonActionPossible, true);
                    }
                }
            }
           
            if (hex.hexData.hasCity && hex.hexData.playerOwnerIndex != GameManager.Instance.activePlayer.index)
            {
                GenerateCityCaptureButton(unit.buttonActionPossible);
            }

            if (hex.hexData.hasResource && hex.hexData.resourceType == ResourceType.MONUMENT)
            {
                GenerateResourceButton(unit.buttonActionPossible, ResourceType.EMPTY, false);
            }

            if (GameManager.Instance.activePlayer.abilities.shipUpgrade)
            {
                if (hex.hexData.hasBuilding && hex.hexData.buildingType == BuildingType.Port) //change this to port
                {
                    if (hex.associatedUnit.isBoat && !hex.associatedUnit.isShip)
                    {
                        GenerateShipButton();
                    }

                }
            }
           
            if (unit.unitReference.type == UnitType.Trader && hex.hexData.hasBuilding && hex.hexData.buildingType == BuildingType.Guild)
            {
                if (hex.hexData.playerOwnerIndex == unit.playerOwnerIndex && hex.parentCity != unit.originCity)
                {
                    GenerateTraderButton();
                    hexDescription.text = "Trade with the Guild of " + hex.cityData.cityName + " for a reward";
                }
                else
                {
                   
                    hexDescription.text = "Move " + unit.unitReference.name  +" to a Guild in a different city to claim a reward";
                }
            }

        }
        else
        {
            hexDescription.text = "This " + unit.unitReference.name + " unit belongs to a different player";
            hexDescriptionBackground.color = UIManager.Instance.hexViewDescriptionUnavailable;
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

    void GeneratePillageButton(bool isEnabled, bool isBuilding)
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForPillage(this, hex, isBuilding, isEnabled);
    }
    void GenerateTraderButton()
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForTrader(this, hex);
    }

    void GenerateDestroyButton(bool isBuilding)
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForDestory(this, hex, isBuilding);
    }

    void GenerateShipButton()
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForShipButton(this, hex);
    }

    void GeneratePortButton()
    {
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForBuilding(this, hex, BuildingType.Port, true);
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

    void GenerateResourceButton(bool shouldBeInteractable, ResourceType type, bool shouldOpenResearchPanel) 
    {
        //update this to support multiple resources on the same hex
        GameObject obj = Instantiate(actionItemPrefab, horizontalScrollParent);
        obj.GetComponent<ActionButton>().SetDataForResource(this, hex, type, shouldBeInteractable, shouldOpenResearchPanel);
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
