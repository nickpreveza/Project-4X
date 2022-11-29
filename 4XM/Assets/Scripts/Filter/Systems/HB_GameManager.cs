using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using UnityEngine.SceneManagement;
public class HB_GameManager : MonoBehaviour
{
    public static HB_GameManager Instance;
    public GameData data;

    public bool devMode;
    public bool isPaused;
    public bool playerDead = false;

    [Header("Cheats")]
    public bool infiniteHealth;
    public bool noScenceChanges;
    public bool overrideWaveDay;
    public int overide_sequence;
    public int overide_wave;
    public bool noWaveSpawn;
    public bool noSave;
    public bool noLoad;

    [Header("Visual Prefabs")]
    public GameObject playerClone;

    [Header("System Prefabs")]
    public GameObject playerPrefab;
    public GameObject weaponPrefab;

    [Header("Enemy Prefabs")]
    public GameObject locustPrefab;

    [Header("SubSystems")]
    //public WaveHandler spawner;

    [Header("PostProcess")]
    public Volume globalVolume;

    [Header("Materials")]
    public Material targetMaterial;

    [SerializeField] GameObject lastSpawnPosition;

    [Header("Cameras")]
    CinemachineBrain cameraBrain;
    [SerializeField] CinemachineVirtualCamera menuCamera;
    [SerializeField] CinemachineVirtualCamera playerCamera;

    [Header("Saving")]
    bool menuPassed;
    public bool canLoad;
    bool shouldLoad;
    bool setUpInProgress;

    [SerializeField] MovingDoorInteractable harborDoor;
    [HideInInspector] public QuestManager questManager;
    [HideInInspector] public CSVReader csvReader;
    public event Action onDataReady;

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

        cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
        questManager = GetComponent<QuestManager>();
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
            HB_UIManager.Instance.PauseChanged();
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
        //set the values from GameData to PlayerData here. 
        HB_PlayerController.Instance.DataLoaded(playerData);

        LoadGame();
    }

    private void Start()
    {
        HB_UIManager.Instance.OpenMainMenu();
    }

    public void UnlockHarborDoor(bool open)
    {
        harborDoor.Unlock(open);
    }

    public void LockHarborDoor(bool close)
    {
        harborDoor.Lock(close);
    }

    public void OnDataReady()
    {
        if (onDataReady != null)
        {
            onDataReady();
            HB_UIManager.Instance.UpdateBackpack(true);
            ItemManager.Instance.EquipItem("weaponPistol");
        }
    }

    public void RewardScore(int amount)
    {
        data.hbscore += amount;
        HB_PlayerController.Instance.data.hbScore += amount;
        HB_UIManager.Instance.UpdateScore();
    }

    public void LoadGame()
    {
        ItemManager.Instance.InitializeItems();
        HB_UIManager.Instance.OpenGamePanel();
        HB_AudioManager.Instance.PlayTheme("theme");
        SpawnPlayer(lastSpawnPosition.transform, true);
    }

    public void NewGame()
    {
        ItemManager.Instance.InitializeItems();
        HB_AudioManager.Instance.PlayTheme("theme");
        SpawnPlayer(lastSpawnPosition.transform, true);
        LockHarborDoor(true);
    }

    public void StartWaveManager()
    {
        WaveManager.Instance.SetWave(0);
    }

    public void SetGameView()
    {
        menuCamera.gameObject.SetActive(false);
    }

    public void SetMenuView()
    {
        menuCamera.gameObject.SetActive(true);
    }

    public void SpawnPlayer(Transform spawnPoint, bool setDefaultValues = false)
    {
        menuCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        if (HB_PlayerController.Instance == null)
        {
            return;
        }
        HB_PlayerController.Instance.gameObject.transform.position = spawnPoint.transform.position;
        HB_PlayerController.Instance.ShowPlayer();
        HB_PlayerController.Instance.controlsActive = true;
        
    }
   
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.I) || (Input.GetKeyDown(KeyCode.Tab)))
        {
            if (!HB_UIManager.Instance.menuActive && !HB_UIManager.Instance.popupActive) // && !LevelLoader.Instance.sceneLoadingInProgress
            {
                HB_UIManager.Instance.ToggleInventory();
            }
              
        }

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (HB_UIManager.Instance.popupActive)
            {
                HB_UIManager.Instance.latestPopup.Close();
                return;
            }

            if (!HB_UIManager.Instance.menuActive && !HB_UIManager.Instance.popupActive) // && !LevelLoader.Instance.sceneLoadingInProgress
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
                    HB_UIManager.Instance.PauseChanged();
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

        if (!isPaused)
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                HB_UIManager.Instance.GetItemFromPeakInventory(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                HB_UIManager.Instance.GetItemFromPeakInventory(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                HB_UIManager.Instance.GetItemFromPeakInventory(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                HB_UIManager.Instance.GetItemFromPeakInventory(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                HB_UIManager.Instance.GetItemFromPeakInventory(4);
            }
        }
    }

    void DevUpdate()
    {
        if (Input.GetKey(KeyCode.Alpha0))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UnlockHarborDoor(true);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LockHarborDoor(true);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {

            }

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                HB_PlayerController.Instance.OnDamage(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                HB_PlayerController.Instance.Heal(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ItemManager.Instance.AddToInventory("hempseeds", 10, false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ItemManager.Instance.AddToInventory("hempfibers", 10, false);
            }
        }
    }

    public void GameOver()
    {
        playerDead = true;
        HB_UIManager.Instance.GameOver();
    }

    public void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void ApplyItemEffects(Item item)
    {
        //find a cool way to its effects, apply them
    }
}
