using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace SignedInitiative
{
    public class Initializer : MonoBehaviour
    {
        public static Initializer Instance;
        public List<NetworkedPlayer> networkedPlayers;

        [SerializeField] MapManager mapGenerator;
        [SerializeField] SI_DataHandler dataHandler;
        [SerializeField] UnitManager playerManager;
        [SerializeField] SI_CameraController cameraController;


        public int networkedPlayersCount = 0;

        bool mapGenerated;
        bool dataLoaded;
        bool unitsPlaced;
        bool processing;

        [SerializeField] GameObject networkMenu;
        [SerializeField] GameObject networkLoading;

        public bool skipMenuForNetworking;
        bool waitingForPlayers;
        private void Awake()
        {
            Instance = this;
        }
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
            UIManager.Instance.ClosePanels();
            UIManager.Instance.ToggleUIPanel(UIManager.Instance.initializerPanel, true, false);

            if (skipMenuForNetworking)
            {
                networkMenu.SetActive(false);
                networkLoading.SetActive(false);
                LocalStart();
            }
            else
            {
                networkMenu.SetActive(true);
                networkLoading.SetActive(true);
            }
        }

        public void LocalStart()
        {
            dataHandler.FetchData();
            processing = true;
        }

        public void NetworkStart()
        {
            networkLoading.SetActive(false);
            GameManager.Instance.sessionPlayers[0].SetupForNetworkPlay(networkedPlayers[0]);
            GameManager.Instance.sessionPlayers[1].SetupForNetworkPlay(networkedPlayers[1]);
            dataHandler.FetchData();
            processing = true;
        }

        public void StartAsHost()
        {
            StartHost();
        }

        public void StartAsClient()
        {
            StartClient();
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            waitingForPlayers = true;
            networkMenu.SetActive(false);
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            waitingForPlayers = true;
            networkMenu.SetActive(false);
        }

        public void Subscribe(NetworkedPlayer player)
        {
            if (networkedPlayersCount < 2)
            {
                if (!networkedPlayers.Contains(player))
                {
                    networkedPlayers.Add(player);
                    networkedPlayersCount++;
                }
            }
        }

        private void Update()
        {
            if (processing)
            {
                if (mapGenerated && dataLoaded && unitsPlaced)
                {
                    processing = false;
                    GameManager.Instance.StartGame();
                }
            }

            if (waitingForPlayers)
            {
                if (networkedPlayersCount == 2)
                {
                    NetworkStart();
                    waitingForPlayers = false;
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
            UnitManager.Instance.InitializeStartUnits();
        }

        void OnUnitsPlaced()
        {
            unitsPlaced = true;
            //give turn order here
        }

    }
}

