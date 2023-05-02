using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;

public class CityView : MonoBehaviour
{
    WorldHex parentHex;
    [SerializeField] TextMeshProUGUI cityName;
    [SerializeField] TextMeshProUGUI cityOutput;
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
    public void SetData(WorldHex hex)
    {
        parentHex = hex;
    }

    public void UpdateData()
    {
        cityName.text = parentHex.cityData.cityName;
        cityOutput.text = parentHex.cityData.output.ToString();
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


    public void UpdateLevelProgressPoint()
    {/*
        int currentLevelPoints = parentHex.cityData.targetLevelPoints;
        int progressLevelPoints = parentHex.cityData.levelPointsToNext;

        if (internalProgressLevel != progressLevelPoints)
        {
            if (internalProgressLevel < progressLevelPoints)
            {
                if (progressLevelPoints != 0)
                {
                    int pointsToGenerate = progressLevelPoints - internalProgressLevel;
                    for (int i = 0; i < pointsToGenerate; i++)
                    {
                        AddLevelProgressPoint();
                    }
                }
            }
            else
            {
                int pointsToRemove = internalProgressLevel - progressLevelPoints;
                for (int i = 0; i < pointsToRemove; i++)
                {
                    RemoveLevelProgressPoint();
                }

            }

            internalProgressLevel = progressLevelPoints;
        }*/
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
