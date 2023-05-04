using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using UnityEngine.SceneManagement;

namespace SignedInitiative
{
    [RequireComponent(typeof(CSVReader))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Game Data")]
        public GameData data;

        [Header("Systems Status")]
        public bool gameReady;

        [Header("Game Status")]
        public bool isPaused;
        public bool playerDead = false;
        public bool menuActive;

        [Header("Debug Settings")]
        public bool devMode;
        public bool infiniteCurrency;
        public bool noScenceChanges;
        public bool noSave;
        public bool noLoad;

        [Header("PostProcess")]
        public Volume globalVolume;

        [Header("Saving")]
        bool menuPassed;
        public bool canLoad;
        bool shouldLoad;
        bool setUpInProgress;

        [HideInInspector] public CSVReader csvReader;

        public Player[] sessionPlayers;
        public Player activePlayer;
        public int activePlayerIndex;

        public List<AbilityData> abilities = new List<AbilityData>();
        public Dictionary<Abilities, AbilityData> abilitiesDictionary = new Dictionary<Abilities, AbilityData>();
        //[HideInInspector]
        List<PlayerAbilityData> defaultAbilityDatabase = new List<PlayerAbilityData>();
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            csvReader = GetComponent<CSVReader>();
        }

        public bool SetPause
        {
            get
            {
                return isPaused;
            }
            set
            {
                isPaused = value;
                if (isPaused)
                {

                    Time.timeScale = 0;
                }
                else
                {

                    Time.timeScale = 1;
                }

                UIManager.Instance.PauseChanged();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!menuPassed)
            {
                SetUpPersistentData(false);
            }
            else
            {
                SetUpPersistentData(true);
            }

            Time.timeScale = 1;
        }

        void SetUpPersistentData(bool instantLoad)
        {
            if (PlayerPrefs.HasKey("day"))
            {
                if (instantLoad)
                {
                    Load();
                }
                else
                {
                    if (PlayerPrefs.GetInt("day") >= 1 && PlayerPrefs.GetInt("day") <= 9)
                    {
                        canLoad = true;
                    }
                    else
                    {
                        PlayerPrefs.SetInt("day", 9);
                        canLoad = true;
                    }
                }

            }
            else
            {
                CreatePrefs();
            }
        }
        void CreatePrefs()
        {
            data.day = 1;
            data.hour = 6;
            data.minute = 0;
            data.second = 0;

            data.highscore = 0;
            data.hbscore = 0;
            data.hempSeeds = 10;
            data.hempFibers = 1;

            data.quadrantsUnlocked = 1;
            data.wavesCompleted = 0;

            data.player_weapon = 1;

            data.discordURL = "lmao.com";
            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt("day", data.day);
            PlayerPrefs.SetInt("hour", data.hour);
            PlayerPrefs.SetInt("minute", data.minute);
            PlayerPrefs.SetInt("second", data.second);

            PlayerPrefs.SetInt("highscore", data.highscore);
            PlayerPrefs.SetInt("hbscore", data.hbscore);
            PlayerPrefs.SetInt("hempSeeds", data.hempSeeds);
            PlayerPrefs.SetInt("hempFibers", data.hempFibers);

            PlayerPrefs.SetInt("quadrantsUnlocked", data.quadrantsUnlocked);
            PlayerPrefs.SetInt("wavesCompleted", data.wavesCompleted);

            PlayerPrefs.Save();
        }

        public void Load()
        {
            data.day = PlayerPrefs.GetInt("day", 1);
            data.hour = PlayerPrefs.GetInt("hour");
            data.minute = PlayerPrefs.GetInt("minute");
            data.second = PlayerPrefs.GetInt("second");

            data.highscore = PlayerPrefs.GetInt("highscore");
            data.hbscore = PlayerPrefs.GetInt("hbscore");
            data.hempSeeds = PlayerPrefs.GetInt("hempSeeds");
            data.hempFibers = PlayerPrefs.GetInt("hempFibers");

            data.quadrantsUnlocked = PlayerPrefs.GetInt("quadrantsUnlocked");
            data.wavesCompleted = PlayerPrefs.GetInt("wavesCompleted");

            PlayerData playerData = new PlayerData();
            LoadGame();
        }

        public Player GetPlayerByIndex(int index)
        {
            return sessionPlayers[index];
        }

        public int GetPlayerIndex(Player player)
        {
            return Array.IndexOf(sessionPlayers, player);
        }

        public Color GetPlayerColor(int index)
        {
            return sessionPlayers[index].playerColor;
        }

        public void StartGame()
        {
            GenerateAbilitiesDictionary();
            UIManager.Instance.ToggleUIPanel(UIManager.Instance.initializerPanel, false, true, 5f);
            gameReady = true;
            activePlayerIndex = 0;
            GenerateAbilitiesDatabaseForPlayers();
            SetActivePlayer(sessionPlayers[activePlayerIndex]);
            UIManager.Instance.OpenGamePanel();
        }

        void GenerateAbilitiesDatabaseForPlayers()
        {
            defaultAbilityDatabase.Clear();

            foreach (AbilityData ability in abilities)
            {
                PlayerAbilityData newAbility = new PlayerAbilityData();

                newAbility.abilityID = ability.abilityID;
                newAbility.calculatedAbilityCost = ability.abilityCost;
                newAbility.hasBeenPurchased = false;
                newAbility.canBePurchased = ability.isUnlocked;

                defaultAbilityDatabase.Add(newAbility);
            }

            foreach(Player player in sessionPlayers)
            {
                player.abilityDatabase = new List<PlayerAbilityData>(defaultAbilityDatabase);
                foreach (PlayerAbilityData data in player.abilityDatabase)
                {
                    player.abilityDictionary.Add(data.abilityID, data);
                    player.calculatedAbilityCost.Add(data.abilityID, data.calculatedAbilityCost);
                }
            }
        }

        void GenerateAbilitiesDictionary()
        {
            foreach(AbilityData ability in abilities)
            {
                abilitiesDictionary.Add(ability.abilityID, ability);
            }
        }

        public void SetActivePlayer(Player player)
        {
            activePlayer = player;
            activePlayerIndex = player.index;

            activePlayer.StartTurn();
            UIManager.Instance.UpdateHUD();

            if (activePlayer.lastMovedUnit != null)
            {
                SI_CameraController.Instance.PanToHex(activePlayer.lastMovedUnit.parentHex);
            }
            else
            {
                SI_CameraController.Instance.PanToHex(player.playerCities[0]);
            }
            
            //TODO: Update fog map and stuff
        }

        public void LocalEndTurn()
        {
            activePlayer.EndTurn();
            foreach(WorldHex city in activePlayer.playerCities)
            {
                AddStars(city.cityData.output);
            }

            UIManager.Instance.EndTurn();

            SI_EventManager.Instance.OnTurnEnded(activePlayerIndex);

            SI_CameraController.Instance.selectedTile = null;
            activePlayerIndex++;
            if (activePlayerIndex >= sessionPlayers.Length)
            {
                activePlayerIndex = 0;
            }

           

            SetActivePlayer(sessionPlayers[activePlayerIndex]);
        }
        public void EndTurn(Player player)
        {
            //TODO: Stuff about ending turn and checkign movement;


            activePlayerIndex++;
            if (activePlayerIndex >= sessionPlayers.Length)
            {
                activePlayerIndex = 0;
            }

            SetActivePlayer(sessionPlayers[activePlayerIndex]);
        }

        public void UndoMove()
        {

        }

        public void OnDataReady()
        {

        }

        public void RewardScore(int amount)
        {
            data.hbscore += amount;
            UIManager.Instance.UpdateHUD();
        }

        public void RemoveStars(int amount)
        {
            if (amount > activePlayer.stars)
            {
                Debug.LogError("Not enough stars but nothing stopped it. You broke the economy");
            }

            activePlayer.stars -= amount;

            SI_EventManager.Instance.OnTransactionMade(activePlayer.index);
        }

        public void AddStars(int amount)
        {
            activePlayer.stars += amount;

            SI_EventManager.Instance.OnTransactionMade(activePlayer.index);
        }

        public void LoadGame()
        {
            ItemManager.Instance.InitializeItems();
            UIManager.Instance.OpenGamePanel();
            SI_AudioManager.Instance.PlayTheme("theme");
        }

        public void NewGame()
        {
            ItemManager.Instance.InitializeItems();
            SI_AudioManager.Instance.PlayTheme("theme");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIManager.Instance.popupActive)
                {
                    UIManager.Instance.latestPopup.Close();
                    return;
                }

                if (!UIManager.Instance.menuActive && !UIManager.Instance.popupActive) // && !LevelLoader.Instance.sceneLoadingInProgress
                {
                    if (!playerDead)
                    {
                        isPaused = !isPaused;
                        if (isPaused)
                        {
                            Time.timeScale = 0;
                        }
                        else
                        {
                            Time.timeScale = 1;
                        }
                        UIManager.Instance.PauseChanged();
                    }

                }
            }

            if (Input.GetKeyDown(KeyCode.BackQuote) && Debug.isDebugBuild)
            {
                devMode = !devMode;
            }

            if (devMode && Debug.isDebugBuild)
            {
                //DevUpdate();
            }
        }



        public int GetCurrentPlayerStars()
        {
            return activePlayer.stars;
        }

        public bool CanActivePlayerAfford(int value)
        {
            if (value <= activePlayer.stars)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsIndexOfActivePlayer(int index)
        {
            if (activePlayer.index == index)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsAbilityUnlocked(Abilities ability)
        {
            return activePlayer.abilityDictionary[ability].canBePurchased;
        }

        public bool isAbilityPurchased(Abilities ability)
        {
            return activePlayer.abilityDictionary[ability].hasBeenPurchased;
        }

        public bool CanActivePlayerAffordAbility(Abilities ability)
        {
            if (GetCurrentPlayerAbilityCost(ability) <= activePlayer.stars)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public int GetCurrentPlayerAbilityCost(Abilities ability)
        {
            return activePlayer.calculatedAbilityCost[ability];
        }


        public int GetBaseAbilityCost(Abilities ability)
        {
            return abilitiesDictionary[ability].abilityCost;
        }
        public void GameOver()
        {
            playerDead = true;
            UIManager.Instance.GameOver();
        }

        public void ReloadScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        public void UnlockAbility(Abilities ability)
        {
            Abilities abilityToUnlock = abilitiesDictionary[ability].abilityToUnlock;

            if (ability != Abilities.NONE)
            {
                activePlayer.BuyAbility(ability);
            }

            if (abilityToUnlock != Abilities.NONE)
            {
                activePlayer.UnlockAbility(abilityToUnlock);
            }
           
            switch (ability)
            {
                case Abilities.Climbing:
                    activePlayer.abilities.travelMountain = true;
                    break;
                case Abilities.Mining:
                    activePlayer.abilities.mineHarvest = true;
                    break;
                case Abilities.Shields:
                    activePlayer.abilities.unitShield = true;
                    break;
                case Abilities.Smithery:
                    activePlayer.abilities.smitheryBuilding = true;
                    break;

                case Abilities.Roads:
                    activePlayer.abilities.roads = true;
                    break;
                case Abilities.Trader:
                    activePlayer.abilities.unitTrader = true;
                    break;
                case Abilities.Diplomat:
                    activePlayer.abilities.unitDiplomat = true;
                    break;
                case Abilities.Guild:
                    activePlayer.abilities.merchantBuilding = true;
                    break;

                case Abilities.Forestry:
                    activePlayer.abilities.forestHarvest = true;
                    break;
                case Abilities.Husbandry:
                    activePlayer.abilities.animalHarvest = true;
                    break;
                case Abilities.Engineering:
                    activePlayer.abilities.unitTrebucet = true;
                    break;
                case Abilities.Papermill:
                    activePlayer.abilities.forestBuilding = true;
                    break;

                case Abilities.Fishing:
                    activePlayer.abilities.fishHarvest = true;
                    break;
                case Abilities.Port:
                    activePlayer.abilities.portBuilding = true;
                    break;
                case Abilities.OpenSea:
                    activePlayer.abilities.travelOcean = true;
                    break;
                case Abilities.FishFarm:
                    activePlayer.abilities.fishBuilding = true;
                    break;

                case Abilities.Harvest:
                    activePlayer.abilities.fruitHarvest = true;
                    break;
                case Abilities.Horserider:
                    activePlayer.abilities.unitHorserider = true;
                    break;
                case Abilities.Farming:
                    activePlayer.abilities.farmHarvest = true;
                    break;
                case Abilities.Windmill:
                    activePlayer.abilities.farmBuilding = true;
                    break;
            }

            UIManager.Instance.UpdateResourcePanel();
            
        }
    }
}

