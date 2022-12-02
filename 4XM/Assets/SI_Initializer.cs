using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_Initializer : MonoBehaviour
{
    MapGenerator mapGenerator;
    //UnitGenerator unitGenerator;
    SI_CameraController cameraController;

    bool mapGenerated;
    bool dataLoaded;
    bool unitsPlaced;

    void Start()
    {
#if UNTIY_ANDROID || UNITY_IOS
    QualitySettings.vSyncCount = 0;
    Application.targetFramerate = 60;
    QualitySettings.antiAliasing = 0;
    Screen.sleepTimeout = SleepTimeout.Never;
#endif

        SI_EventManager.Instance.onDataLoaded += OnDataLoaded;
        SI_EventManager.Instance.onMapGenerated += OnMapGenerated;
        SI_EventManager.Instance.onUnitsPlaced += OnUnitsPlaced;
        StartCoroutine(Initilization());
    }

    IEnumerator Initilization()
    {
        ResetChecks();
        SI_UIManager.Instance.ToggleUIPanel(SI_UIManager.Instance.initializerPanel, true);
        mapGenerator.GenerateMap();
        while (!mapGenerated || !dataLoaded || !unitsPlaced)
        {
            yield return new WaitForSeconds(0.1f);
        }

        SI_UIManager.Instance.ToggleUIPanel(SI_UIManager.Instance.initializerPanel, true);
        //SI_GameManager.Instance.gameReady = true;
    }

    void ResetChecks()
    {
        dataLoaded = false;
        mapGenerated = false;
        unitsPlaced = false;
    }

    void OnDataLoaded()
    {
        dataLoaded = true;
    }

    void OnMapGenerated()
    {
        mapGenerated = true;
    }

    void OnUnitsPlaced()
    {
        unitsPlaced = true;
    }

}
