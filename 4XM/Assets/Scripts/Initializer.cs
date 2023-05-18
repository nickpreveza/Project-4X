using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

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
        bool openForConnections;
        bool waitingForPlayers;

        public int userSeed;
        public bool isSinglePlayer;

        public int playerCount;

        public List<Player> setupPlayers = new List<Player>();
        public List<Civilizations> selectedCivs = new List<Civilizations>();
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

        private void OnDestroy()
        {
            SI_EventManager.Instance.onDataLoaded -= OnDataLoaded;
            SI_EventManager.Instance.onMapGenerated -= OnMapGenerated;
            SI_EventManager.Instance.onUnitsPlaced -= OnUnitsPlaced;
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
                    openForConnections = false;
                    waitingForPlayers = false;
                }
            }
        }


        void InitializeGame()
        {
            GameManager.Instance.useRandomSeed = true;
            GameManager.Instance.isSinglePlayer = isSinglePlayer;

            ResetChecks();
            UIManager.Instance.ClosePanels();
            //we will use that to do a main menu, ok?
            UIManager.Instance.ToggleUIPanel(UIManager.Instance.initializerPanel, true, false); 

            if (skipMenuForNetworking)
            {
                networkMenu.SetActive(false);
                networkLoading.SetActive(false);
                LocalStart(false) ;
            }
            else
            {
                networkMenu.SetActive(true);
                //networkLoading.SetActive(true);
            }
        }

        public void StartLocalgame()
        {
            networkMenu.SetActive(false);
            networkLoading.SetActive(false);
            LocalStart(false);
        }

        public void OnSeedChanged(string value)
        {
            int number; 
            int.TryParse(value, out int result); number = result;
            userSeed = number;
            GameManager.Instance.useRandomSeed = false;
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            //waitingForPlayers = true;
            networkMenu.SetActive(false);
            networkLoading.SetActive(true);
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            GameManager.Instance.isHost = true;
            openForConnections = true;
            waitingForPlayers = true;
            networkMenu.SetActive(false);
        }

        public void LocalStart(bool pushInitializerPlayers)
        {
            if (!pushInitializerPlayers)
            {
                dataHandler.FetchData();
                processing = true;
                return;
            }

            List<Player> playersPassSetup = new List<Player>();

            foreach(Player player in setupPlayers)
            {
                if (player.activatedOnSetup)
                {
                    playersPassSetup.Add(player);
                }
            }

            GameManager.Instance.sessionPlayers = new List<Player>(playersPassSetup);
            GameManager.Instance.sessionPlayers = GameManager.Instance.sessionPlayers.OrderBy(x => x.type).ToList();

            foreach (Player player in GameManager.Instance.sessionPlayers)
            {
                player.index = GameManager.Instance.sessionPlayers.IndexOf(player);

                player.isAlive = true;
                player.abilities.unitSwordsman = true;
            }

            dataHandler.FetchData();
            processing = true;
        }

        public void OpenLinkTree()
        {
            Application.OpenURL(GameManager.Instance.data.linktrURL);
        }

        public void NetworkStart()
        {
            if (GameManager.Instance.isHost)
            {
                SetSeedServerRpc();
               // BroadcastSeedAndStartRpc(tempSeed);
            }
        }

        [ServerRpc]
        void SetSeedServerRpc()
        {
            //Random.InitState(Random.Range(1000, 9999));
           // tempSeed = Random.Range(1000, 9999);
        }

        [ClientRpc]
        void BroadcastSeedAndStartRpc(int seed)
        {
            MapManager.Instance.seed = seed;
            Debug.LogError("Host broadcasted seed: " + seed);

            networkLoading.SetActive(false);
            GameManager.Instance.sessionPlayers[0].SetupForNetworkPlay(networkedPlayers[0]);
            GameManager.Instance.sessionPlayers[1].SetupForNetworkPlay(networkedPlayers[1]);
            GameManager.Instance.gameIsNetworked = true;
            dataHandler.FetchData();
            processing = true;
        }



        public void Subscribe(NetworkedPlayer player)
        {
            if (GameManager.Instance.isHost && openForConnections && networkedPlayersCount < 2)
            {
                if (!networkedPlayers.Contains(player))
                {
                    networkedPlayers.Add(player);
                    networkedPlayersCount++;
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

