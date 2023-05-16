using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;
using SignedInitiative;

public class CityView : MonoBehaviour
{
    WorldHex parentHex;
    [SerializeField] TextMeshProUGUI cityName;
    [SerializeField] TextMeshProUGUI cityOutput;
    [SerializeField] TextMeshProUGUI cityLevel;
    [SerializeField] Image cityNameBackground;
    [SerializeField] Color positiveGainColor;
    [SerializeField] Color negativeGainColor;

    List<GameObject> levelItems = new List<GameObject>();
    int internalLevel = 0;
    int internalProgressLevel = 0;

    [SerializeField] RectTransform levelHolder;
    [SerializeField] GameObject levelPointPrefab;
    List<LevelPoint> generatedLevelPoints = new List<LevelPoint>();

    [SerializeField] GameObject cityCaptureIcon;
    [SerializeField] GameObject captureUnavailable;
    [SerializeField] GameObject capitalIcon;
    [SerializeField] GameObject connectedIcon;

    CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup cityDetailsCanvasGroup;
    [SerializeField] CanvasGroup animatedElementesGroup;
    [SerializeField] Animator cityDetailsAnim;

    public void SetData(WorldHex hex)
    {
        parentHex = hex;
        capitalIcon.SetActive(false);
        cityCaptureIcon.SetActive(false);
        connectedIcon.SetActive(false);
        captureUnavailable.SetActive(false);
        canvasGroup = GetComponent<CanvasGroup>();
        cityDetailsCanvasGroup.alpha = 0;
        OnCreateLevelPoints();
        
    }

    public void SetCapitalStatus(bool status)
    {
        capitalIcon.SetActive(status);
    }

    public void CapitalConnectionStatus(bool connected)
    {
        connectedIcon.SetActive(connected);
    }

    public void SetCanvasGroupAlpha(int alpha)
    {
        canvasGroup.alpha = alpha;
    }

    public void OccupyCity(bool recalculatePoints)
    {
        CityCaptured(recalculatePoints);
    }   

    void CityCaptured(bool recalculatePoints)
    {
        if (parentHex == GameManager.Instance.GetPlayerByIndex(parentHex.hexData.playerOwnerIndex).capitalCity)
        {
            capitalIcon.SetActive(true);
        }
        else
        {
            capitalIcon.SetActive(false);
        }

        cityNameBackground.color = GameManager.Instance.GetCivilizationColor(parentHex.hexData.playerOwnerIndex, CivColorType.uiActiveColor);
        SetCanvasGroupAlpha(1);
        
        RemoveSiegeState();

        UpdateData();
        SetDetailsAlpha(1);

        if (recalculatePoints)
        {
            OnCreateLevelPoints();
        }
        
        cityDetailsAnim.SetTrigger("cityCaptured");

    }

    public void UpdateData()
    {
        cityLevel.text = parentHex.cityData.level.ToString();
        cityName.text = parentHex.cityData.cityName;
        cityOutput.text = parentHex.cityData.output.ToString();

        if (parentHex.hexData.isConnectedToCapital)
        {
            connectedIcon.SetActive(true);
        }
        else
        {
            connectedIcon.SetActive(false);
        }
    }

    public void SetDetailsAlpha(int alpha)
    {
        cityDetailsCanvasGroup.alpha = alpha;
        animatedElementesGroup.alpha = alpha;
    }

    public void AddPopulation()
    {
        levelHolder.transform.GetChild(parentHex.cityData.population-1).GetComponent<LevelPoint>().SetUnitPoint(true);
    }

    public void RemovePopulation()
    {
        levelHolder.transform.GetChild(parentHex.cityData.population-1).GetComponent<LevelPoint>().SetUnitPoint(false);
    }

    public void ResetPopulation()
    {
        for (int i = 0; i < parentHex.cityData.targetLevelPoints; i++)
        {
            generatedLevelPoints[i].SetUnitPoint(false);
        }

        for (int i = 0; i < parentHex.cityData.population; i++)
        {
            generatedLevelPoints[i].SetUnitPoint(true);
        }
    }
    public void EnableSiege(bool siegeCanHappenThisTurn)
    {

        RemoveSiegeState();
        if (parentHex.hexData.playerOwnerIndex == -1)
        {
            SetDetailsAlpha(0);
        }
        if (siegeCanHappenThisTurn)
        {
            cityCaptureIcon.SetActive(true);
            captureUnavailable.SetActive(false);
            cityCaptureIcon.GetComponent<Button>().onClick.AddListener(() => SI_CameraController.Instance.SelectTile(parentHex));
        }
        else
        {
            cityCaptureIcon.GetComponent<Button>().onClick.RemoveAllListeners();
            captureUnavailable.SetActive(true);
            cityCaptureIcon.SetActive(false);
           
        }
    }

    public void RemoveSiegeState()
    {
        captureUnavailable.SetActive(false);
        cityCaptureIcon.SetActive(false);
        cityCaptureIcon.GetComponent<Button>().onClick.RemoveAllListeners();
    }


    public void LevelUp()
    {
        
        cityDetailsAnim.SetTrigger("levelUp");
        OnCreateLevelPoints();
        ResetPopulation();
        SetDetailsAlpha(1);
        UpdateData();
    }

    public void ToggleOnProgressPoint(bool isNegative)
    {
        int currentLevelPointIndex = parentHex.cityData.levelPointsToNext - 1;
        int targetLevelPoints = parentHex.cityData.targetLevelPoints;

        if (isNegative)
        {
             currentLevelPointIndex = parentHex.cityData.negativeLevelPoints - 1;
        }
        

        if (levelHolder.transform.childCount <= currentLevelPointIndex)
        {
            Debug.LogWarning("The level point you tried to access does not exist");
            return;
        }

        levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetLevelPoint(true);
        //levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetPointUnitActive(false);

        if (isNegative)
        {
            levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetLevelPointColor(negativeGainColor);
        }
        else
        {
            levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetLevelPointColor(Color.white);
        }
        //levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).GetComponent<Image>().color = positiveGainColor;

        UpdateData();
        ResetPopulation();
    }

    //call this before remove the level
    public void ToggleOffProgressPoint(bool isNegative)
    {
        //last enabled points starts at -
        int currentLevelPointIndex = parentHex.cityData.levelPointsToNext - 1;
        if (isNegative)
        {
             currentLevelPointIndex = parentHex.cityData.negativeLevelPoints - 1;
        }

        levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetLevelPoint(false);
        UpdateData();
        ResetPopulation();
    }
 

    void OnCreateLevelPoints()
    {
        foreach (RectTransform child in levelHolder)
        {
            Destroy(child.gameObject);
        }

        for(int i  = 0; i < generatedLevelPoints.Count; i++)
        {
            Destroy(generatedLevelPoints[i].gameObject);
        }

        generatedLevelPoints.Clear();

        if (parentHex.hexData.playerOwnerIndex == -1)
        {
            return;
        }

        for (int i = 0; i < parentHex.cityData.targetLevelPoints; i++)
        {
            GameObject obj = Instantiate(levelPointPrefab, levelHolder);
            LevelPoint levelPointCreated = obj.GetComponent<LevelPoint>();
            levelPointCreated.SetLevelPoint(false);
            levelPointCreated.SetUnitPoint(false);
            generatedLevelPoints.Add(levelPointCreated);
            Debug.Log("Created " + i + " level Point for " + parentHex.cityData.cityName);
        }

        for(int i = 0; i < parentHex.cityData.population; i++)
        {
            generatedLevelPoints[i].GetComponent<LevelPoint>().SetUnitPoint(true);
        }
    }
}
