using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CSVReader))]
public class SI_GameManager : MonoBehaviour
{
    public static SI_GameManager Instance;

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

            SI_UIManager.Instance.PauseChanged();
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

    public void OnDataReady()
    {
       
    }

    public void RewardScore(int amount)
    {
        data.hbscore += amount;
        SI_UIManager.Instance.UpdateScore();
    }

    public void LoadGame()
    {
        ItemManager.Instance.InitializeItems();
        SI_UIManager.Instance.OpenGamePanel();
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
            if (SI_UIManager.Instance.popupActive)
            {
                SI_UIManager.Instance.latestPopup.Close();
                return;
            }

            if (!SI_UIManager.Instance.menuActive && !SI_UIManager.Instance.popupActive) // && !LevelLoader.Instance.sceneLoadingInProgress
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
                    SI_UIManager.Instance.PauseChanged();
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
        SI_UIManager.Instance.GameOver();
    }

    public void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

}
