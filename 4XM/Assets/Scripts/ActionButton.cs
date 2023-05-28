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
    [SerializeField] GameObject researchVisual;
    Image backgroundImage;
    BuildingType masterBuildingType;
    WorldHex targetHex;
    WorldUnit targetUnit;
    int actionCost;
    UnitType unitType;

    public void SetDataForHeal(HexView newHandler, WorldUnit newUnit)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetUnit = newUnit;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        buttonName.text = "Heal";
        actionCost = 0;
        actionCostText.text = actionCost.ToString();
        costVisual.SetActive(false);
        backgroundImage.sprite = parentHandler.actionBackground;

        buttonAction.interactable = true;
        buttonAction.onClick.AddListener(() => HealUnit());

    }

    public void SetDataForPillage(HexView newHandler, WorldHex newHex, bool isBuilding, bool isEnabled)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        buttonName.text = "Pillage";
        actionCost = GameManager.Instance.data.pillageCost;
        actionCostText.text = actionCost.ToString();

        backgroundImage.sprite = parentHandler.destroyBackground;

        if (isEnabled)
        {
            buttonAction.interactable = true;
            buttonAction.onClick.AddListener(() => DestroyAction(isBuilding, true));
        }
        else
        {
            buttonAction.interactable = false;
        }
       
    }
    public void SetDataForDestroy(HexView newHandler, WorldHex newHex, bool isBuilding)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        buttonName.text = "Destroy";
        actionCost = GameManager.Instance.data.destroyCost;

        backgroundImage.sprite = parentHandler.destroyBackground;
        actionCostText.text = actionCost.ToString();
        buttonAction.onClick.AddListener(()=>DestroyAction(isBuilding, false));
        CheckIfAffordable();
    }

    public void SetDataForTrader(HexView newHandler, WorldHex newHex)
    {
        backgroundImage = GetComponent<Image>();
        researchVisual.SetActive(false);
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = "Trade Action";
        actionCost = 0;
        costVisual.SetActive(false);

        buttonAction.onClick.AddListener(TraderAction);
        backgroundImage.sprite = parentHandler.actionBackground;

    }

    public void SetDataForShipButton(HexView newHandler, WorldHex newHex)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        buttonName.text = "Upgrade to Ship";
        actionCost = UnitManager.Instance.GetUnitDataByType(UnitType.Ship, newHex.associatedUnit.civilization).cost;
        actionCostText.text = actionCost.ToString();
       
        backgroundImage.sprite = parentHandler.actionBackground;

        if (targetHex.associatedUnit.buttonActionPossible)
        {
            buttonAction.interactable = true;
            buttonAction.onClick.AddListener(() => ShipCreationButton(newHex.associatedUnit));
            costVisual.SetActive(true);
            CheckIfAffordable();
        }
        else
        {
            costVisual.SetActive(true);
            buttonAction.interactable = false;
        }
       
    }

    public void SetDataForRoad(HexView newHandler, WorldHex newHex)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        buttonName.text = "Road";
        actionCost = GameManager.Instance.data.roadCost;
        actionCostText.text = actionCost.ToString();

        buttonAction.onClick.AddListener(BuildRoadAction);
        backgroundImage.sprite = parentHandler.actionBackground;
        CheckIfAffordable();
    }

    public void SetDataForUnitSpawn(HexView newHandler, WorldHex newHex, UnitType unit)
    {
        backgroundImage = GetComponent<Image>();
        researchVisual.SetActive(false);
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
        if (GameManager.Instance.CanActivePlayerAfford(actionCost) && 
            !targetHex.hexData.occupied && !targetHex.cityData.HasReachedMaxPopulation)
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

    public void SetDataForBuilding(HexView newHandler, WorldHex newHex, BuildingType type, bool shouldBeInteracable, bool shouldOpenResearch)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);
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
            if (shouldOpenResearch)
            {
                buttonAction.onClick.AddListener(() => OpenResearchButton(type));
                researchVisual.SetActive(true);
            }
            
            costVisual.SetActive(false);
            backgroundImage.color = UIManager.Instance.affordableColor;
            buttonAction.interactable = true;
        }

    }
    public void SetDataForResource(HexView newHandler, WorldHex newHex, ResourceType type, bool shouldBeInteracable, bool shouldOpenResearch)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        if (MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).transformToBuilding)
        {
            string buildingName = MapManager.Instance.GetBuildingByType(MapManager.Instance.GetBuildingByResourceType(targetHex.hexData.resourceType)).buildingName;
            buttonName.text = "Build " + buildingName;
        }
        else
        {
            buttonName.text = "Harvest " + MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
            switch (targetHex.hexData.resourceType)
            {
                case ResourceType.ANIMAL:
                    buttonName.text = "Tame " + MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
                    break;
                case ResourceType.FISH:
                    buttonName.text = "Catch " + MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
                    break;
                case ResourceType.MONUMENT:
                    buttonName.text = "Claim " + MapManager.Instance.GetResourceByType(targetHex.hexData.resourceType).resourceName;
                    break;
            }
           
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
            if (shouldOpenResearch)
            {
                buttonAction.onClick.AddListener(() =>OpenResearchButton(type));
                buttonAction.interactable = true;
                researchVisual.SetActive(true);
                backgroundImage.color = UIManager.Instance.affordableColor;
            }
            else
            {
                buttonAction.interactable = false;
                backgroundImage.color = UIManager.Instance.unaffordableColor;
            }
            
            costVisual.SetActive(false);
           
        }
       
    }

    public void SetDataForResourceCreation(HexView newHandler, WorldHex newHex, ResourceType type, bool isOccupied)
    {
        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();
        researchVisual.SetActive(false);

        buttonName.text = "Create " + MapManager.Instance.GetResourceByType(type).resourceName;
        actionCost = MapManager.Instance.GetResourceByType(type).creationCost;
        actionCostText.text = actionCost.ToString();
        backgroundImage.sprite = parentHandler.actionBackground;
        //image also here 
        if (isOccupied)
        {
            buttonAction.interactable = false;
            backgroundImage.color = UIManager.Instance.unaffordableColor;
        }
        else
        {
            buttonAction.onClick.AddListener(() => CreateResourceButton(type));
            CheckIfAffordable();
        }
       

    }

    public void SetDataForCityCapture(HexView newHandler, WorldHex newHex, bool isInteractable)
    {

        backgroundImage = GetComponent<Image>();
        parentHandler = newHandler;
        targetHex = newHex;
        researchVisual.SetActive(false);
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

    public void OpenResearchButton(ResourceType type)
    {
        SI_AudioManager.Instance.PlayClick();
        UIManager.Instance.OpenResearchPanelWithHighlight(type);
    }

    public void OpenResearchButton(BuildingType type)
    {
        SI_AudioManager.Instance.PlayClick();
        UIManager.Instance.OpenResearchPanelWithHighlight(type);
    }

    public void ShipCreationButton(WorldUnit unit)
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        SI_AudioManager.Instance.Play(SI_AudioManager.Instance.unitSpawn);
        unit.EnableShip();
    }

    public void CaptureCityButton()
    {
        SI_AudioManager.Instance.PlayClick();
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
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
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
        if (SI_CameraController.Instance.animationsRunning) { return; }
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        targetHex.CreateResource(type);
    }

    public void TraderAction()
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.Play(SI_AudioManager.Instance.traderAction);
        targetHex.associatedUnit.TraderAction();
    }

    public void HealUnit()
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.Play(SI_AudioManager.Instance.unitHeal);
        targetUnit.HealAction();
        UIManager.Instance.RefreshHexView();
    }

    public void HarvestResource()
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
        //do a confirm pop up to avoid misclicks
        // UIManager.Instance.ShowConfirmationPopup();
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        targetHex.HarvestResource();
    }

    public void CreateMasterBuilding()
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        targetHex.CreateBuilding(masterBuildingType);
    }

    public void SpawnUnit()
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
;       //GameManager.Instance.activePlayer.RemoveStars(actionCost);
       UnitManager.Instance.SpawnUnitAt(GameManager.Instance.activePlayer, unitType, targetHex, true, true, true);

        UIManager.Instance.RefreshHexView();
        UIManager.Instance.UpdateHUD();
    }

    public void DestroyAction(bool isBuilding, bool fromUnit)
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        if (fromUnit)
        {
            targetHex.associatedUnit.ExhaustActions();
        }
        targetHex.DestroyAction(isBuilding);
    }
    
    public void BuildRoadAction()
    {
        if (SI_CameraController.Instance.animationsRunning) { return; }
        SI_AudioManager.Instance.PlayClick();
        GameManager.Instance.activePlayer.RemoveStars(actionCost);
        targetHex.CreateRoad();
    }


}
