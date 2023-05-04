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
    WorldHex targetHex;
    int actionCost;

    public void SetDataForUnitSpawn(HexView newHandler, WorldHex newHex, WorldUnit unit)
    {
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = unit.data.unitName;
        actionCost = unit.data.cost;
        actionCostText.text = actionCost.ToString();

        buttonAction.onClick.AddListener(SpawnUnit);
        CheckIfSpawnPossible();
    }

    void CheckIfSpawnPossible()
    {
        if (GameManager.Instance.activePlayer.stars > actionCost && !targetHex.hexData.occupied)
        {
            buttonAction.interactable = true;
        }
        else
        {
            buttonAction.interactable = false;
        }
    }
    public void SetDataForResource(HexView newHandler, WorldHex newHex)
    {
        parentHandler = newHandler;
        targetHex = newHex;
        buttonAction.onClick.RemoveAllListeners();

        buttonName.text = MapManager.Instance.hexResources[targetHex.hexData.resourceIndex].resourceName;
        actionCost = MapManager.Instance.hexResources[targetHex.hexData.resourceIndex].cost;

        actionCostText.text = actionCost.ToString();
        //image also here 
        buttonAction.onClick.AddListener(HarvestResource);
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
        buttonAction.onClick.AddListener(CaptureCity);

        buttonAction.interactable = isInteractable;

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
        if (GameManager.Instance.CanActivePlayerAfford(MapManager.Instance.hexResources[targetHex.hexData.resourceIndex].cost))
        {
            buttonImage.color = UIManager.Instance.affordableColor;
            buttonAction.interactable = true;
        }
        else
        {
            buttonImage.color = UIManager.Instance.unaffordableColor;
            
            buttonAction.interactable = false;
        }
    }

    public void CaptureCity()
    {
        GameManager.Instance.activePlayer.AddCity(targetHex);
        targetHex.associatedUnit.CityCaptureAction();
    }

    public void HarvestResource()
    {
        //do a confirm pop up to avoid misclicks
        // UIManager.Instance.ShowConfirmationPopup();
        GameManager.Instance.RemoveStars(actionCost);
        targetHex.HarvestResource();
    }

    public void SpawnUnit()
    {
        GameManager.Instance.RemoveStars(actionCost);
        UnitManager.Instance.SpawnUnitAt(GameManager.Instance.activePlayerIndex, UnitManager.Instance.unitTestPrefab, targetHex, true);
    }


}
