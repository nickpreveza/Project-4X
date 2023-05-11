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

    [SerializeField] Transform levelHolder;
    [SerializeField] GameObject levelPointPrefab;
    List<GameObject> currentLevelPoints = new List<GameObject>();

    [SerializeField] GameObject cityCaptureIcon;
    [SerializeField] GameObject cityCaptureIconInactive;
    [SerializeField] GameObject capitalIcon;
    [SerializeField] GameObject connectedIcon;

    CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup cityDetailsCanvasGroup;
    [SerializeField] Animator cityDetailsAnim;
    public void SetData(WorldHex hex)
    {
        parentHex = hex;
        capitalIcon.SetActive(false);
        cityCaptureIcon.SetActive(false);
        connectedIcon.SetActive(false);
        cityCaptureIconInactive.SetActive(false);
        canvasGroup = GetComponent<CanvasGroup>();
        cityDetailsCanvasGroup.alpha = 0;
        OnCreateLevelPoints();
        
    }

    public void SetCanvasGroupAlpha(bool isHidden)
    {
        if (isHidden)
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            canvasGroup.alpha = 1;
        }
    }

    public void OccupyCity()
    {
        SetCanvasGroupAlpha(false);
        SetDetailsAlpha(true);
        UpdateData();
        CityCaptured();
        RemoveSiegeState();
        OnCreateLevelPoints();
    }   

    void CityCaptured()
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

        SetDetailsAlpha(true);
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

    public void SetDetailsAlpha(bool showDetails)
    {
        if (showDetails)
        {
            cityDetailsCanvasGroup.alpha = 1;
        }
        else
        {
            cityDetailsCanvasGroup.alpha = 0;
        }
    }

    public void SetSiegeState(bool siegeCanHappen)
    {

        RemoveSiegeState();
    
        if (siegeCanHappen)
        {
            cityCaptureIcon.SetActive(true);
            cityCaptureIcon.GetComponent<Button>().onClick.AddListener(() => SI_CameraController.Instance.SelectTile(parentHex));
        }
        else
        {
            cityCaptureIcon.GetComponent<Button>().onClick.RemoveAllListeners();
            cityCaptureIconInactive.SetActive(true);
          
        }
    }

    public void RemoveSiegeState()
    {
        cityCaptureIconInactive.SetActive(false);
        cityCaptureIcon.SetActive(false);
        cityCaptureIcon.GetComponent<Button>().onClick.RemoveAllListeners();
    }


    public void LevelUp()
    {
        OnCreateLevelPoints();
        cityDetailsAnim.SetTrigger("levelUp");
        SetDetailsAlpha(true);
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

        levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetPointActive(true);
        levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetPointUnitActive(false);

        if (isNegative)
        {
            levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().levelActive.GetComponent<Image>().color = negativeGainColor;
        }
        else
        {
            levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().levelActive.GetComponent<Image>().color = Color.white;
        }
        //levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).GetComponent<Image>().color = positiveGainColor;

         UpdateData();
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

        levelHolder.transform.GetChild(currentLevelPointIndex).GetComponent<LevelPoint>().SetPointActive(false);
        UpdateData();
    }
 

    void OnCreateLevelPoints()
    {
        foreach (Transform child in levelHolder)
        {
            Destroy(child.gameObject);
        }
        if (parentHex.hexData.playerOwnerIndex == -1)
        {
            return;
        }

        currentLevelPoints.Clear();

        for (int i = 0; i < parentHex.cityData.targetLevelPoints; i++)
        {
            GameObject obj = Instantiate(levelPointPrefab, levelHolder);
            obj.GetComponent<LevelPoint>().InitState();
            currentLevelPoints.Add(obj);
        }
    }
}
