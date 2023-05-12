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

        public Civilization[] gameCivilizations;

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
        public bool allAbilitiesUnlocked;
        public bool startWithALotOfMoney;
        public bool noFog;
        public bool allowShipUpgradeEverywhere;
        public bool createStuff;
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

        //ability related, maybe move
        public int roadCost = 2;
        public int destroyCost = 0;

        //quest rewards
        //level 2
        public int visibilityReward = 2;
        public UnitType unitReward = UnitType.Melee;//shouldnotbe
        //level 3
        public int currencyReward = 5;
        public int productionReward = 1;
        //level 4
        public int populationReward = 3;
        public int rangeReward = 2;
        public int startCityOutput = 2;
        public int traderActionReward = 10;
        public int startCurrencyAmount;

        //level 5
        //maybe special unit..uuuuugh

        public bool gameIsNetworked;
        public ulong activePlayerClientID;

        public bool isHost;

        [SerializeField] GameObject waterInteractionParticle;
        [SerializeField] GameObject landInternactionParticle;
        public GameObject explosionParticle;
        public GameObject resourceHarvestParticle;
        public bool abilitiesDicitionariesCreated;

        public GameObject traderActionParticle;

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

        public GameObject GetParticleInteractionByType(TileType type)
        {
            switch (type)
            {
                case TileType.DEEPSEA:
                case TileType.SEA:
                    return waterInteractionParticle;
                case TileType.SAND:
                case TileType.GRASS:
                case TileType.HILL:
                case TileType.MOUNTAIN:
                case TileType.ICE:
                    return landInternactionParticle;
            }

            return null;
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

        public bool CanPlayerDestoryResourceForReward(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.FOREST:
                    return GameManager.Instance.activePlayer.abilities.forestCut;
            }

            //shouldn't really use this honestly.
            return false;

            if (CanPlayerHarvestResource(type))
            {
                return MapManager.Instance.GetResourceByType(type).canBeDestroyedForReward;
            }
            else
            {
                return false;
            }
            
        }

        public Abilities GetAbilityAssociation(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.FarmMaster:
                    return Abilities.Windmill;
                case BuildingType.ForestMaster:
                    return Abilities.ForestMaster;
                case BuildingType.MineMaster:
                    return Abilities.Smithery;
            }

            return Abilities.NONE;
        }
        public Abilities GetAbilityAssociation(ResourceType resource)
        {
            switch (resource)
            {
                case ResourceType.FRUIT:
                    return Abilities.FruitHarvest;
                case ResourceType.FOREST:
                    return Abilities.Forestry;
                case ResourceType.ANIMAL:
                    return Abilities.Husbandry;
                case ResourceType.FARM:
                    return Abilities.Farming;
                case ResourceType.MINE:
                    return Abilities.Mining;
                case ResourceType.FISH:
                    return Abilities.Fishing;
            }

            return Abilities.NONE;
        }

        public bool CanPlayerHarvestResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.FRUIT:
                    return activePlayer.abilities.fruitHarvest;
                case ResourceType.FOREST:
                    return activePlayer.abilities.forestHarvest;
                case ResourceType.ANIMAL:
                    return activePlayer.abilities.animalHarvest;
                case ResourceType.FARM:
                    return activePlayer.abilities.farmHarvest;
                case ResourceType.MINE:
                    return activePlayer.abilities.mineHarvest;
                case ResourceType.FISH:
                    return activePlayer.abilities.fishHarvest;

            }

            Debug.LogWarning("Resource type was not found");
            return false;
        }

        public Player GetPlayerByIndex(int index)
        {
            return sessionPlayers[index];
        }

        public int GetPlayerIndex(Player player)
        {
            return Array.IndexOf(sessionPlayers, player);
        }

        public void StartGame()
        {
            GenerateAbilitiesDictionary();
            CivilizationsSetup();
            if (startWithALotOfMoney)
            {
                foreach(Player player in sessionPlayers)
                {
                    player.AddStars(1000);
                }
            }
            else
            {
                foreach (Player player in sessionPlayers)
                {
                    player.AddStars(startCurrencyAmount);
                }
            }
            if (noFog)
            {
                MapManager.Instance.DebugUnhideHexesForAllPlayers();
            }
            UIManager.Instance.ToggleUIPanel(UIManager.Instance.initializerPanel, false, true, 0f);
            gameReady = true;
            activePlayerIndex = 0;
            
            StartTurn(sessionPlayers[activePlayerIndex]);

            UIManager.Instance.OpenGamePanel();
        }

        public void MonumentReward(int rewardIndex, WorldUnit unit)
        {
            string popupTitle = "Monument Claimed";
            string popupDescr = "You've received a reward";

            switch (rewardIndex)
            {
                case 0:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                     "+" + currencyReward.ToString() + " Stars",
                     () => PopupCustomRewardCurrency()
                     );
                    break;
                case 1:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                     "Extra sight",
                     () => PopupCustomRewardVisibility(unit)
                     );
                    break;
                case 2:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                     "Free Warrior",
                     () => PopupCustomRewardUnit(unit, UnitType.Melee)
                     );
                    break;
                case 3:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                     "Free Archer",
                     () => PopupCustomRewardUnit(unit, UnitType.Ranged)
                     );
                    break;
                case 4:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                   "Free Cavalry",
                     () => PopupCustomRewardUnit(unit, UnitType.Cavalry)
                     );
                    break;
                case 5:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                     "+" + currencyReward.ToString() + " Stars",
                     () => PopupCustomRewardCurrency()
                     );
                    break;
                case 6:
                    UIManager.Instance.waitingForPopupReply = true;
                    UIManager.Instance.OpenPopUpMonument(
                        popupTitle,
                        popupDescr,
                     "+" + currencyReward.ToString() + " Stars",
                     () => PopupCustomRewardCurrency()
                     );
                    break;
            }
        }

        public void PopupCustomRewardCurrency()
        {
            AddStars(activePlayerIndex, currencyReward);
            UIManager.Instance.waitingForPopupReply = false;
        }

        public void PopupCustomRewardVisibility(WorldUnit unit)
        {
            MapManager.Instance.UnhideHexes(activePlayerIndex, unit.parentHex, 2, true);
        }

        public void PopupCustomRewardUnit(WorldUnit unit, UnitType type)
        {
            WorldHex targetHex = unit.parentHex;
            if (unit.TryToMoveRandomly())
            {
                UnitManager.Instance.SpawnUnitAt(activePlayer, type, targetHex, true, false);
            }
            else
            {
                unit.Death(false);
                UnitManager.Instance.SpawnUnitAt(activePlayer, type, targetHex, true, false);
            }

        }

        //TODO reward unit at monument location 


        void CivilizationsSetup()
        {
            foreach(Civilization civ in gameCivilizations)
            {
                foreach (UnitData unit in civ.unitOverrides)
                {
                    civ.unitDictionary.Add(unit.type, unit);
                }
            }
            
        }

        public Color GetCivilizationColor(int index, CivColorType colorType)
        {
            Player player = GetPlayerByIndex(index);
            return GetCivilizationColor(player.civilization, colorType);
        }

        public Color GetCivilizationColor(Civilizations type, CivColorType colorType)
        {
            Civilization civ = GetCivilizationByType(type);
            switch (colorType)
            {
                case CivColorType.unitColor:
                    return civ.unitColor;
                case CivColorType.borderColor:
                    return civ.borderColor;
                case CivColorType.uiActiveColor:
                    return civ.uiColorActive;
                case CivColorType.uiInactiveColor:
                    return civ.uiColorInactive;
            }

           return civ.unitColor;
        }

        public Civilization CivOfActivePlayer()
        {
            return GetCivilizationByType(activePlayer.civilization);
        }
        public Civilization GetCivilizationByType(Civilizations type)
        {
            switch (type)
            {
                case Civilizations.Greeks:
                    return gameCivilizations[0];
                case Civilizations.Romans:
                    return gameCivilizations[1];
            }

            return null;
        }

        public void RecalculatePlayerExpectedStars(int playerIndex)
        {
            Player player = GetPlayerByIndex(playerIndex);
            player.CalculateExpectedStars();
        }
        void GenerateAbilitiesDatabaseForPlayers()
        {
            foreach(Player player in sessionPlayers)
            {
                player.abilityDatabase = new List<PlayerAbilityData>();

                foreach (AbilityData ability in abilities)
                {
                    PlayerAbilityData newAbility = new PlayerAbilityData();

                    newAbility.abilityID = ability.abilityID;
                    newAbility.calculatedAbilityCost = ability.abilityCost;
                    newAbility.hasBeenPurchased = false;
                    newAbility.canBePurchased = ability.isUnlocked;

                    player.abilityDatabase.Add(newAbility);
                    player.abilityDictionary.Add(newAbility.abilityID, newAbility);
                }
            }

            if (allAbilitiesUnlocked)
            {
                foreach (Player player in sessionPlayers)
                {
                    foreach (AbilityData ability in abilities)
                    {
                        UnlockAbility(player.index, ability.abilityID, false, false);
                    }
                  
                }
            }

            abilitiesDicitionariesCreated = true;
        }

        void GenerateAbilitiesDictionary()
        {
            foreach(AbilityData ability in abilities)
            {
                abilitiesDictionary.Add(ability.abilityID, ability);
            }

            GenerateAbilitiesDatabaseForPlayers();
        }

        public void StartTurn(Player player)
        {
            activePlayer = player;
            activePlayerIndex = player.index;
            activePlayerClientID = player.clientID;
            activePlayer.StartTurn();
            activePlayer.CalculateDevelopmentScore(false);
            activePlayer.CalculateExpectedStars();
           
            if (activePlayer.lastMovedUnit != null)
            {
                SI_CameraController.Instance.PanToHex(activePlayer.lastMovedUnit.parentHex);
            }
            else
            {
                SI_CameraController.Instance.PanToHex(player.playerCities[0]);
            }

            MapManager.Instance.UpdateCloudView();
            UIManager.Instance.UpdateHUD();
            UIManager.Instance.UpdateResearchPanel(activePlayerIndex);

            SI_EventManager.Instance.OnTurnStarted(activePlayerIndex);
        }

        public void LocalEndTurn()
        {
            UIManager.Instance.EndTurn();
            activePlayer.EndTurn();
            foreach(WorldHex city in activePlayer.playerCities)
            {
                if (!city.cityData.isUnderSiege)
                {
                    activePlayer.AddStars(city.cityData.output);
                }
            }

            SI_EventManager.Instance.OnTurnEnded(activePlayerIndex);

            SI_CameraController.Instance.DeselectSelection();
            UnitManager.Instance.ClearHexSelectionMode();
            activePlayerIndex++;
            if (activePlayerIndex >= sessionPlayers.Length)
            {
                activePlayerIndex = 0;
            }

            StartTurn(sessionPlayers[activePlayerIndex]);
         
        }

        public void EndTurn(Player player)
        {
            //TODO: Stuff about ending turn and checkign movement;


            activePlayerIndex++;
            if (activePlayerIndex >= sessionPlayers.Length)
            {
                activePlayerIndex = 0;
            }

            StartTurn(sessionPlayers[activePlayerIndex]);
        }

        public void UndoMove()
        {

        }

        public void OnDataReady()
        {

        }

        public void AddStars(int playerIndex, int amount)
        {
            GetPlayerByIndex(playerIndex).AddStars(amount);
        }

        public void RemoveStars(int playerIndex, int amount)
        {
            GetPlayerByIndex(playerIndex).RemoveStars(amount);
        }

        public void AddScore(int playerIndex, int scoreType, int amount)
        {
            GetPlayerByIndex(playerIndex).AddScore(scoreType, amount);
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
            /*
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
            }*/
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
            if (activePlayer.abilityDictionary.ContainsKey(ability))
            {
                return activePlayer.abilityDictionary[ability].canBePurchased;
            }
            else
            {
                return false;
            }
           
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
            if (activePlayer.abilityDictionary.ContainsKey(ability))
            {
                return activePlayer.abilityDictionary[ability].calculatedAbilityCost;
            }
            return 9999;
           
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

        public void ApplicationQuit()
        {
            Application.Quit();
        }

        public void UnlockAbility(int playerIndex, Abilities ability, bool updateUI, bool removeStars)
        {
          
            Player player = GetPlayerByIndex(playerIndex);

            if (player.abilityDictionary[ability].hasBeenPurchased)
            {
                Debug.Log("Ability has already been purchased");
                return;
            }

            Abilities abilityToUnlock = abilitiesDictionary[ability].abilityToUnlock;
            player.AddScore(2, abilitiesDictionary[ability].scoreForPlayer);

            if (ability != Abilities.NONE)
            {
                player.BuyAbility(ability, removeStars);
            }

            if (abilityToUnlock != Abilities.NONE)
            {
                player.UnlockAbility(abilityToUnlock);
            }
           
            switch (ability)
            {
                case Abilities.Climbing:
                    player.abilities.travelMountain = true;
                    break;
                case Abilities.Mining:
                    player.abilities.mineHarvest = true;
                    break;
                case Abilities.Shields:
                    //player.abilities.unitShield = true;
                    player.abilities.unitLance = true;
                    break;
                case Abilities.Smithery:
                    player.abilities.mineMasterBuilding = true;
                    player.abilities.unitLance = true;

                    if (GameManager.Instance.createStuff)
                    {
                        player.abilities.createMine = true;
                    }
                    break;

                case Abilities.FruitHarvest:
                    player.abilities.fruitHarvest = true;
                    break;
                case Abilities.Roads:
                    player.abilities.roads = true;
                    break;
                case Abilities.Creator:
                    player.abilities.destroyAbility = true;
                    //player.abilities.unitDiplomat = true;
                    //player.abilities.Trader = true;
                    break;
                case Abilities.Guild:
                    player.abilities.guildBuilding = true;
                    player.abilities.unitTrader = true;
                    break;
             

                case Abilities.Forestry:
                    player.abilities.forestHarvest = true;
                    break;
                case Abilities.Archery:
                    player.abilities.unitArcher = true;
                    break;
                case Abilities.Engineering:
                    player.abilities.unitTrebucet = true;
                    player.abilities.forestCut = true;
                    break;
                case Abilities.ForestMaster:
                    player.abilities.forestMasterBuilding = true;
                    player.abilities.createForest = true;
                    break;

                case Abilities.Fishing:
                    player.abilities.fishHarvest = true;

                    if (GameManager.Instance.createStuff)
                    {
                        player.abilities.createFish = true;
                    }
                    break;
                case Abilities.Port:
                    player.abilities.portBuilding = true;
                    player.abilities.travelSea = true;
                    break;
                case Abilities.Ocean:
                    player.abilities.travelOcean = true;
                    break;
                case Abilities.Ship:
                    player.abilities.shipUpgrade = true;
                    //player.abilities.fishBuilding = true;
                    //player.abilities.destroyAbility = true;
                    break;

                case Abilities.Husbandry:
                    player.abilities.animalHarvest = true;
                    if (GameManager.Instance.createStuff)
                    {
                        player.abilities.createAnimals = true;
                    }
                    break;
                case Abilities.Horserider:
                    player.abilities.unitHorserider = true;
                    break;
                case Abilities.Farming:
                    player.abilities.farmHarvest = true;
                    break;
                case Abilities.Windmill:
                    player.abilities.farmMasterBuilding = true;
                    player.abilities.createFarm = true;
                    break;
            }

            SI_EventManager.Instance.OnAbilityUnlocked(playerIndex);
            player.UpdateAvailableUnitsFromAbilities();

            if (updateUI)
            {
                UIManager.Instance.UpdateResearchPanel(playerIndex);
                UIManager.Instance.RefreshHexView();
            }
           
        }
    }
}

public enum CivColorType
{
    borderColor,
    unitColor,
    unitInactiveColor,
    uiActiveColor,
    uiInactiveColor
}


