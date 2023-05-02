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
            UIManager.Instance.ToggleUIPanel(UIManager.Instance.initializerPanel, false, true, 5f);
            gameReady = true;
            activePlayerIndex = 0;
            SetActivePlayer(sessionPlayers[activePlayerIndex]);
            UIManager.Instance.OpenGamePanel();
        }
        public void SetActivePlayer(Player player)
        {
            activePlayer = player;
            activePlayerIndex = player.index;

            activePlayer.StartTurn();
            UIManager.Instance.UpdateHUD();
            SI_CameraController.Instance.PanToHex(player.playerCities[0]);
            //TODO: Update map and stuff
        }

        public void LocalEndTurn()
        {
            activePlayer.EndTurn();
            foreach(WorldHex city in activePlayer.playerCities)
            {
                AddStars(city.cityData.output);
            }

            SI_EventManager.Instance.OnTurnEnded(activePlayerIndex);

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
        }

        public void AddStars(int amount)
        {
            activePlayer.stars += amount;
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
                DevUpdate();
            }
        }

        void DevUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {

            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {

            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {

            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {

            }
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

    }
}

