using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SignedInitiative;

public class ActionButton : MonoBehaviour
{
    HexView parentHandler;

    [SerializeField] TextMeshProUGUI buttonName;
    [SerializeField] TextMeshProUGUI actionCostText;

    [SerializeField] Image buttonImage;

    [SerializeField] Button buttonAction;
    [SerializeField] GameObject costVisual;
    Image backgroundImage;
    BuildingType masterBuildingType;
    WorldHex targetHex;
    int actionCost;
    UnitType unitType;

    public void SetDataForDestory(HexView newHandler, WorldHex newHex, bool isBuilding)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        if (isBuilding)
        {
            buttonName.text = "Destroy";
            actionCost = GameManager.Instance.destroyCost;
        }
        else
        {
            buttonName.text = "Clear Resource";
            actionCost = GameManager.Instance.destroyCost; 
        }
        backgroundImage.sprite = parentHandler.destroyBackground;
        actionCostText.text = actionCost.ToString();
        buttonAction.onClick.AddListener(()=>DestroyAction(isBuilding));
        CheckIfAffordable();
    }

    public void SetDataForShipButton(HexView newHandler, WorldHex newHex)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = "Upgrade to Ship";
        actionCost = newHex.associatedUnit.boatReference.cost; 
        actionCostText.text = actionCost.ToString();
        buttonAction.onClick.AddListener(() => ShipCreationButton(newHex.associatedUnit));
        backgroundImage.sprite = parentHandler.actionBackground;
        CheckIfAffordable();
    }

    public void SetDataForRoad(HexView newHandler, WorldHex newHex)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = "Road";
        actionCost = GameManager.Instance.roadCost;
        actionCostText.text = actionCost.ToString();

        buttonAction.onClick.AddListener(BuildRoadAction);
        backgroundImage.sprite = parentHandler.actionBackground;
        CheckIfAffordable();
    }

    public void SetDataForUnitSpawn(HexView newHandler, WorldHex newHex, UnitType unit)
    {
        backgroundImage = GetComponent<Image>();
       
        unitType = unit;
        UnitData unitData = UnitManager.Instance.GetUnitDataByType(unit, GameManager.Instance.activePlayer.civilization);

        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = unitData.name;
        actionCost = unitData.cost;
        actionCostText.text = actionCost.ToString();

        buttonAction.onClick.AddListener(SpawnUnit);
        backgroundImage.sprite = parentHandler.unitBackground;
        CheckIfSpawnPossible();
    }

    void CheckIfSpawnPossible()
    {
        if (GameManager.Instance.CanActivePlayerAfford(actionCost) && !targetHex.hexData.occupied)
        {
            actionCostText.color = UIManager.Instance.researchAvailable;
            costVisual.SetActive(true);
  
            buttonAction.interactable = true;
        }
        else
        {
            actionCostText.color = UIManager.Instance.researchUnavailable;
            costVisual.SetActive(true);

            buttonAction.interactable = false;
        }
    }

    public void SetDataForBuilding(HexView newHandler, WorldHex newHex, BuildingType type, bool shouldBeInteracable)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        masterBuildingType = type;

        buttonName.text = MapManager.Instance.GetBuildingByType(type).buildingName;
        actionCost = MapManager.Instance.GetBuildingByType(type).cost;

        actionCostText.text = actionCost.ToString();
        
        //image also here 

        if (shouldBeInteracable)
        {
            buttonAction.onClick.AddListener(CreateMasterBuilding);
            CheckIfAffordable();
        }
        else
        {
            buttonAction.onClick.AddListener(UIManager.Instance.OpenResearchPanel);
            costVisual.SetActive(false);
            buttonAction.interactable = true;
        }

    }
    public void SetDataForResource(HexView newHandler, WorldHex newHex, bool shouldBeInteracable)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        if (MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).transformToBuilding)
        {
            buttonName.text = "Build " + MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
        }
        else
        {
            buttonName.text = "Harvest " + MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
        }
        //MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
        actionCost = MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).harvestCost;

        actionCostText.text = actionCost.ToString();
        backgroundImage.sprite = parentHandler.resourceBackground;
        //image also here 

        if (shouldBeInteracable)
         {
            buttonAction.onClick.AddListener(HarvestResource);
            CheckIfAffordable();
         }
        else
        {
            buttonAction.onClick.AddListener(UIManager.Instance.OpenResearchPanel);
            costVisual.SetActive(false);
            buttonAction.interactable = true;
        }
       
    }

    public void SetDataForResourceCreation(HexView newHandler, WorldHex newHex, ResourceType type)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = MapManager.Instance.GetResourceByType(type).resourceName;
        actionCost = MapManager.Instance.GetResourceByType(type).creationCost;
        actionCostText.text = actionCost.ToString();
        //image also here 
        buttonAction.onClick.AddListener(()=>CreateResourceButton(type));
        backgroundImage.sprite = parentHandler.actionBackground;
        CheckIfAffordable();

    }

    public void SetDataForCityCapture(HexView newHandler, WorldHex newHex, bool isInteractable)
    {
        parentHandler = newHandler;
        targetHex = newHex;

        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = "Capture City";
        actionCost = 0;
        actionCostText.text = "";
        //image also here 
        buttonAction.onClick.AddListener(CaptureCityButton);

        buttonAction.interactable = isInteractable;
        costVisual.SetActive(false);
        backgroundImage.sprite = parentHandler.claimCityBackground;
        //CheckIfAffordable();
    }

    void CheckIfAffordable()
    {
        if (actionCost <= 0)
        {
            costVisual.SetActive(false);
            buttonAction.interactable = true;
        }
        else
        {
            if (GameManager.Instance.CanActivePlayerAfford(actionCost))
            {
                actionCostText.color = UIManager.Instance.researchAvailable;
                backgroundImage.color = UIManager.Instance.affordableColor;
                costVisual.SetActive(true);
                buttonAction.interactable = true;
            }
            else
            {
                actionCostText.color = UIManager.Instance.researchUnavailable;
                backgroundImage.color = UIManager.Instance.unaffordableColor;
                costVisual.SetActive(true);
                buttonAction.interactable = false;
            }
        }
    }

    public void ShipCreationButton(WorldUnit unit)
    {
        unit.EnableShip();
    }

    public void CaptureCityButton()
    {
        UIManager.Instance.OpenPopup(
            "Capture city", 
            "Add this city to your empire", 
            true,
            "Capture",
            "Cancel",
            () => targetHex.associatedUnit.CityCaptureAction(), true);
    }

    public void CreateResourceButton(ResourceType type)
    {
        UIManager.Instance.OpenPopup(
            "Create",
            "Create a " + MapManager.Instance.GetResourceByType(type).resourceName + " resource",
            true,
            "Create",
            "Cancel",
            () => CreateResource(type), true);
    }

    public void CreateResource(ResourceType type)
    {
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        targetHex.CreateResource(type);
    }

   

    public void HarvestResource()
    {
        //do a confirm pop up to avoid misclicks
        // UIManager.Instance.ShowConfirmationPopup();
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        targetHex.HarvestResource();
    }

    public void CreateMasterBuilding()
    {
        targetHex.GenerateMasterBuilding(masterBuildingType);
    }

    public void SpawnUnit()
    {
        UnitManager.Instance.SpawnUnitAt(GameManager.Instance.activePlayer, unitType, targetHex, true, true);

        UIManager.Instance.RefreshHexView();
        UIManager.Instance.UpdateHUD();
    }

    public void DestroyAction(bool isBuilding)
    {
        targetHex.DestroyAction(isBuilding);
    }
    
    public void BuildRoadAction()
    {
        targetHex.CreateRoad();
    }


}
