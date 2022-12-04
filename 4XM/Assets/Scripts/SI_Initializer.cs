using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_Initializer : MonoBehaviour
{
    [SerializeField] MapManager mapGenerator;
    [SerializeField] SI_DataHandler dataHandler;
    [SerializeField] UnitManager playerManager;
    [SerializeField] SI_CameraController cameraController;

    bool mapGenerated;
    bool dataLoaded;
    bool unitsPlaced;
    bool processing;
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

        InitializeGame();
    }

    void InitializeGame()
    {
        ResetChecks();
        SI_UIManager.Instance.ToggleUIPanel(SI_UIManager.Instance.initializerPanel, true, false);
        dataHandler.FetchData();
        processing = true;
    }

    private void Update()
    {
        if (processing)
        {
            if (mapGenerated && dataLoaded && unitsPlaced) 
            {
                processing = false;
                SI_UIManager.Instance.ToggleUIPanel(SI_UIManager.Instance.initializerPanel, false, false);
                SI_GameManager.Instance.gameReady = true;
            }
        }
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
        mapGenerator.GenerateMap();
    }

    void OnMapGenerated()
    {
        mapGenerated = true;
        UnitManager.Instance.InitializeUnits();
    }

    void OnUnitsPlaced()
    {
        unitsPlaced = true;
        //give turn order here
    }

}
