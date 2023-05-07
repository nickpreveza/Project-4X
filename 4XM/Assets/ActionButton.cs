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
    BuildingType masterBuildingType;
    WorldHex targetHex;
    int actionCost;
    UnitType unitType;

    public void SetDataForDestory(HexView newHandler, WorldHex newHex, bool isBuilding)
    {
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

        actionCostText.text = actionCost.ToString();
        buttonAction.onClick.AddListener(()=>DestroyAction(isBuilding));
        CheckIfAffordable();
    }

    public void SetDataForRoad(HexView newHandler, WorldHex newHex)
    {
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = "Road";
        actionCost = GameManager.Instance.roadCost;
        actionCostText.text = actionCost.ToString();

        buttonAction.onClick.AddListener(BuildRoadAction);
        CheckIfAffordable();
    }

    public void SetDataForUnitSpawn(HexView newHandler, WorldHex newHex, UnitType unit)
    {
        unitType = unit;
        UnitData unitData = UnitManager.Instance.GetUnitDataByType(unit, GameManager.Instance.activePlayer.civilization);

        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = unitData.name;
        actionCost = unitData.cost;
        actionCostText.text = actionCost.ToString();

        buttonAction.onClick.AddListener(SpawnUnit);
        CheckIfSpawnPossible();
    }

    void CheckIfSpawnPossible()
    {
        if (GameManager.Instance.CanActivePlayerAfford(actionCost) && !targetHex.hexData.occupied)
        {
            costVisual.SetActive(true);
            buttonAction.interactable = true;
        }
        else
        {
            costVisual.SetActive(true);
            buttonAction.interactable = false;
        }
    }

    public void SetDataForBuilding(HexView newHandler, WorldHex newHex, BuildingType type, bool shouldBeInteracable)
    {
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
            buttonImage.color = UIManager.Instance.unaffordableColor;
            buttonAction.interactable = true;
        }

    }
    public void SetDataForResource(HexView newHandler, WorldHex newHex, bool shouldBeInteracable)
    {
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        if (MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).transformToBuilding)
        {
            buttonName.text = "Build";
        }
        else
        {
            buttonName.text = "Harvest";
        }
        //MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
        actionCost = MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).harvestCost;

        actionCostText.text = actionCost.ToString();
        //image also here 

         if (shouldBeInteracable)
         {
            buttonAction.onClick.AddListener(HarvestResource);
            CheckIfAffordable();
         }
        else
        {
            buttonAction.onClick.AddListener(UIManager.Instance.OpenResearchPanel);
            buttonImage.color = UIManager.Instance.unaffordableColor;
            buttonAction.interactable = true;
        }
       
    }

    public void SetDataForResourceCreation(HexView newHandler, WorldHex newHex, ResourceType type)
    {
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
        actionCost = MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).creationCost;
        actionCostText.text = actionCost.ToString();
        //image also here 
        buttonAction.onClick.AddListener(()=>CreateResourceButton(type));
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
        if (isInteractable)
        {
            buttonImage.color = UIManager.Instance.affordableColor;
        }
        else
        {
            buttonImage.color = UIManager.Instance.unaffordableColor;
        }
        //CheckIfAffordable();
    }

    void CheckIfAffordable()
    {
        if (GameManager.Instance.CanActivePlayerAfford(actionCost))
        {
            buttonImage.color = UIManager.Instance.affordableColor;
            costVisual.SetActive(true);
            buttonAction.interactable = true;
        }
        else
        {
            buttonImage.color = UIManager.Instance.unaffordableColor;
            costVisual.SetActive(true);
            buttonAction.interactable = false;
        }
    }

    public void CaptureCityButton()
    {
        UIManager.Instance.OpenPopup(
            "Capture city", 
            "Add this city to your empire", 
            true, 
            () => targetHex.associatedUnit.CityCaptureAction());
    }

    public void CreateResourceButton(ResourceType type)
    {
        UIManager.Instance.OpenPopup(
            "Create",
            "Create a " + MapManager.Instance.GetResourceByType(type).resourceName + " resource",
            true,
            () => CreateResource(type));
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
