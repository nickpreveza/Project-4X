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

    int lastEnabledProgressPointIndex = -1;
    [SerializeField] GameObject cityCaptureIcon;
    [SerializeField] GameObject cityCaptureIconInactive;
    [SerializeField] GameObject capitalIcon;
    [SerializeField] GameObject connectedIcon;

    CanvasGroup canvasGroup;
    public void SetData(WorldHex hex)
    {
        parentHex = hex;
        capitalIcon.SetActive(false);
        cityCaptureIcon.SetActive(false);
        connectedIcon.SetActive(false);
        cityCaptureIconInactive.SetActive(false);
        canvasGroup = GetComponent<CanvasGroup>();
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

    public void UpdateForCityCapture()
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
    }

    public void UpdateData()
    {
        cityLevel.text = parentHex.cityData.level.ToString();
        cityName.text = parentHex.cityData.cityName;
        cityOutput.text = parentHex.cityData.output.ToString();
    }

    public void UpdateSiegeState(bool activeIcon, bool inactiveIcon)
    {
        cityCaptureIconInactive.SetActive(inactiveIcon);
        cityCaptureIcon.SetActive(activeIcon);
    }
    public void AddLevelUIPoint()
    {
        int pointsThatShouldExist = parentHex.cityData.targetLevelPoints;

        if (levelHolder.childCount > pointsThatShouldExist)
        {
            Debug.LogError("You done fucked up");
        }

        GameObject obj = Instantiate(levelPointPrefab, levelHolder);
        obj.transform.GetChild(0).gameObject.SetActive(false);
        currentLevelPoints.Add(obj);
    }

    public void AddProgressUIPoint()
    {
        ToggleOnProgressPoint();
    }

    public void RemoveProgressUIPoint()
    {
        levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).gameObject.SetActive(false);
        lastEnabledProgressPointIndex--;
    }

    public void RemoveAllProgressPoints()
    {
        foreach(Transform child in levelHolder)
        {
            child.GetChild(0).gameObject.SetActive(false);
        }

        lastEnabledProgressPointIndex = -1;
    }
    void ToggleOnProgressPoint()
    {
        lastEnabledProgressPointIndex++;
        levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).gameObject.SetActive(true);
        levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).GetComponent<Image>().color = positiveGainColor;
    }

    void ToggleOffProgressPoint()
    {
        levelHolder.transform.GetChild(levelHolder.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
    }

    public void AddNegativeProgressUIPoint()
    {
        lastEnabledProgressPointIndex++;
        levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).gameObject.SetActive(true);
        levelHolder.transform.GetChild(lastEnabledProgressPointIndex).GetChild(0).GetComponent<Image>().color = negativeGainColor;
    }
 

    public void InitialLevelSetup()
    {
        foreach(Transform child in levelHolder)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < parentHex.cityData.targetLevelPoints; i++)
        {
            AddLevelUIPoint();
        }
    }
}
