using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

namespace SignedInitiative
{
    [RequireComponent(typeof(CSVReader))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        //the civilization that exist in the game 
        public Civilization[] gameCivilizations;

        //unused really 
        [Header("Game Data")]
        public GameData data;

        [Header("Systems Status")]
        public bool gameReady;

        //unused, overcomplication for game structure
        [Header("Game Status")]
        public bool isPaused;
        public bool menuActive;


        public bool isSinglePlayer;

        [Header("Debug Settings")]
        public bool allowOnlyAIPlayersToPlayer;
        public bool VisualizeAIMoves;
        public bool destroyResourcesToo;
        public bool useRandomSeed;
        public bool allAbilitiesUnlocked;
        public bool startWithALotOfMoney;
        public bool noFog;
        public bool allowShipUpgradeEverywhere;
        public bool createStuff;
        public bool infiniteCurrency;
        //public bool noScenceChanges;
       // public bool noSave;
       // public bool noLoad;

        [Header("PostProcess")]
        public Volume globalVolume;

        /*
        [Header("Saving")]
        bool menuPassed;
        public bool canLoad;
        bool shouldLoad;
        bool setUpInProgress;*/
        public List<Player> setupPlayers = new List<Player>();
        public List<Player> sessionPlayers = new List<Player>();
        [HideInInspector] public List<Player> rankedPlayers = new List<Player>();

        public Player activePlayer;
        public Player singlePlayer;
        public int activePlayerIndex;

        public List<AbilityData> abilities = new List<AbilityData>();
        public Dictionary<Abilities, AbilityData> abilitiesDictionary = new Dictionary<Abilities, AbilityData>();
        //[HideInInspector]
        List<PlayerAbilityData> defaultAbilityDatabase = new List<PlayerAbilityData>();

        [Header("Networking")]
        public bool gameIsNetworked;
        public ulong activePlayerClientID;
        public bool isHost;

        [SerializeField] GameObject waterInteractionParticle;
        [SerializeField] GameObject landInternactionParticle;
        public GameObject cloudInteractionParticle;

        public GameObject explosionParticle;
        public GameObject resourceHarvestParticle;
        public bool abilitiesDicitionariesCreated;

        public GameObject traderActionParticle;

        [SerializeField] GameObject assetTest;
        [SerializeField] GameObject menuObjects;
        public Brain brain;

        public List<Abilities> abilityPath = new List<Abilities>();
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

            assetTest.SetActive(false);
            brain = GetComponent<Brain>();
        }

        

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            /*
            if (!menuPassed)
            {
                SetUpPersistentData(false);
            }
            else
            {
                SetUpPersistentData(true);
            }*/

            Time.timeScale = 1;
        }

        

        public void StartGame()
        {
            menuObjects.SetActive(false);
           
            rankedPlayers = new List<Player>(sessionPlayers);

            GenerateAbilitiesDictionary();
            CivilizationsSetup();

            SI_CameraController.Instance.GameStarted();

            //reorder the players so humans go first
            

            if (noFog)
            {
                MapManager.Instance.DebugUnhideHexesForAllPlayers();
            }

            foreach (Player player in sessionPlayers)
            {
                if (startWithALotOfMoney && !player.isAI())
                {
                    player.AddStars(1000);
                }

                player.AddStars(data.startCurrencyAmount);
                player.CalculateDevelopmentScore(false);

                UnlockAbility(player.index, GetCivilizationByType(player.civilization).startingAbility, false, false);
            }

            UIManager.Instance.ToggleUIPanel(UIManager.Instance.initializerPanel, false, true, 0f);
            gameReady = true;
            activePlayerIndex = 0;
            
            StartTurn(sessionPlayers[activePlayerIndex]);
            SI_AudioManager.Instance.PlayTheme(sessionPlayers[activePlayerIndex].civilization);
            SI_AudioManager.Instance.Play("ambience");
            UIManager.Instance.OpenGamePanel();
        }

        public void CalculateRanks()
        {
            List<Player> playersToSort = new List<Player>(rankedPlayers);
            rankedPlayers = playersToSort.OrderByDescending(x => x.totalScore).ToList();
            string debug = "Current ranks: ";
            foreach(Player player in rankedPlayers)
            {
                debug += "\n" + rankedPlayers.IndexOf(player) + ": " + player.civilization + " with " + player.totalScore + ".";
            }
        }

        public void MonumentReward(int rewardIndex, WorldUnit unit)
        {
            string popupTitle = "Monument Claimed";
            string popupDescr = "You received a reward";
            if (!activePlayer.isAI())
            {
                switch (rewardIndex)
                {
                    case 0:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                         "+" + data.currencyReward.ToString() + " Stars",
                         () => PopupCustomRewardCurrency()
                         );
                        break;
                    case 1:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                         "Extra Sight",
                         () => PopupCustomRewardVisibility(unit)
                         );
                        break;
                    case 2:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                         "Warrior Recruited",
                         () => PopupCustomRewardUnit(unit, UnitType.Melee)
                         );
                        break;
                    case 3:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                         "Archer Recruited",
                         () => PopupCustomRewardUnit(unit, UnitType.Ranged)
                         );
                        break;
                    case 4:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                       "Cavalry Recruited",
                         () => PopupCustomRewardUnit(unit, UnitType.Cavalry)
                         );
                        break;
                    case 5:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                         "+" + data.currencyReward.ToString() + " Stars",
                         () => PopupCustomRewardCurrency()
                         );
                        break;
                    case 6:
                        UIManager.Instance.waitingForPopupReply = true;
                        UIManager.Instance.OpenPopUpMonument(
                            popupTitle,
                            popupDescr,
                         "+" + data.currencyReward.ToString() + " Stars",
                         () => PopupCustomRewardCurrency()
                         );
                        break;
                }
            }
            else
            {
                switch (rewardIndex)
                {
                    case 0:
                        PopupCustomRewardCurrency();
                        break;
                    case 1:
                        PopupCustomRewardVisibility(unit);
                        break;
                    case 2:
                        PopupCustomRewardUnit(unit, UnitType.Melee);
                        break;
                    case 3:
                        PopupCustomRewardUnit(unit, UnitType.Ranged);
                        break;
                    case 4:
                        PopupCustomRewardUnit(unit, UnitType.Cavalry);
                        break;
                    case 5:
                        PopupCustomRewardCurrency();
                        break;
                    case 6:
                        PopupCustomRewardCurrency();
                        break;
                }
            }
          
        }

        public void PopupCustomRewardCurrency()
        {
            AddStars(activePlayerIndex, data.currencyReward);
            UIManager.Instance.waitingForPopupReply = false;
        }

        public void PopupCustomRewardVisibility(WorldUnit unit)
        {
            MapManager.Instance.UnhideHexes(activePlayerIndex, unit.parentHex, 2, true);
        }

        public void PopupCustomRewardUnit(WorldUnit unit, UnitType type)
        {
            WorldHex targetHex = unit.parentHex;
            if (!unit.TryToMoveRandomly())
            {
                unit.InstantDeath(false);
            }

            UnitManager.Instance.SpawnUnitAt(activePlayer, type, targetHex, true, false, false, true);

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
                case CivColorType.unitInactiveColor:
                    return civ.unitInactive;
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
                case Civilizations.Egyptians:
                    return gameCivilizations[1];
                case Civilizations.Romans:
                    return gameCivilizations[2];
                case Civilizations.Celts:
                    return gameCivilizations[3];
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
                    if (!player.isAI())
                    {
                        foreach (AbilityData ability in abilities)
                        {
                            UnlockAbility(player.index, ability.abilityID, false, false);
                        }
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
            SI_CameraController.Instance.GameStarted();
          
            activePlayer = player;
            activePlayerIndex = player.index;
            activePlayerClientID = player.clientID;
            activePlayer.StartTurn();
            activePlayer.CalculateDevelopmentScore(false);
            activePlayer.CalculateExpectedStars();
            if (activePlayer.showAction())
            {
                SI_CameraController.Instance.PanToHex(player.playerCities[0]);

                MapManager.Instance.UpdateCloudView();
                UIManager.Instance.UpdateHUD();
                UIManager.Instance.UpdateResearchPanel(activePlayerIndex);
            }
            else
            {
                UIManager.Instance.UpdateOnlyPlayerAvatar();
            }

            UIManager.Instance.ToggleEndTurn(!activePlayer.isAI());

            UIManager.Instance.StartTurnAnim();
            SI_EventManager.Instance.OnTurnStarted(activePlayerIndex);

            if (activePlayer.isAI())
            {
                brain.StartEvaluation(activePlayer);
            }
            else
            {
                //SI_AudioManager.Instance.PlayTheme(sessionPlayers[activePlayerIndex].civilization);
            }
          
        }

        public void SpawnUnitAction(int actionCost, UnitType unitType, WorldHex cityHex)
        {
            UnitManager.Instance.SpawnUnitAt(GameManager.Instance.activePlayer, unitType, cityHex, true, true, true);

            if (activePlayer.showAction())
            {
                if (!activePlayer.isAI())
                {
                    UIManager.Instance.RefreshHexView();
                }
                UIManager.Instance.UpdateHUD();
            }
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

            int previousPlayerIndex = activePlayerIndex;
           
            bool foundPlayer = false;

            while (!foundPlayer)
            {
                activePlayerIndex++;
                if (activePlayerIndex >= sessionPlayers.Count)
                {
                    activePlayerIndex = 0;
                }
                if (GetPlayerByIndex(activePlayerIndex).isAlive && GetPlayerByIndex(activePlayerIndex).playerCities.Count > 0)
                {
                    foundPlayer = true;
                    break;
                }
            }

            if (!allowOnlyAIPlayersToPlayer)
            {
                bool foundHumanPlayer = false;
                foreach(Player player in sessionPlayers)
                {
                    if (!player.isAI() && player.isAlive)
                    {
                        foundHumanPlayer = true;
                        break;
                    }
                }

                if (!foundHumanPlayer)
                {
                    GameOver(activePlayer);
                    return;
                }
            }


            if (previousPlayerIndex == activePlayerIndex)
            {
                GameOver(activePlayer);
                return;
            }
            else
            {
                        
                SI_CameraController.Instance.animationsRunning = false;
                StartTurn(sessionPlayers[activePlayerIndex]);
            }

            
        }

        public void EndTurn(Player player)
        {
            activePlayerIndex++;
            if (activePlayerIndex >= sessionPlayers.Count)
            {
                activePlayerIndex = 0;
            }

            StartTurn(sessionPlayers[activePlayerIndex]);
        }

        public void GameOver(Player player)
        {
            SI_CameraController.Instance.animationsRunning = true;
            SI_AudioManager.Instance.PlayTheme(Civilizations.None);
            UIManager.Instance.GameOver(player);
        }

        public void RemovePlayerFromGame(Player player, bool showPopup)
        {
            List<WorldUnit> playerUnits = new List<WorldUnit>(player.playerUnits);
            foreach(WorldUnit unit in playerUnits)
            {
                if (unit != null)
                {
                    unit.InstantDeath(false);
                }
              
            }

            player.isAlive = false;

            if (showPopup)
            {
                UIManager.Instance.OpenPopupInformative(
              GetCivilizationByType(player.civilization) + " Defeated",
              "All their cities have been captured",
              "OK");
            }
          
        }

        public void OnDataReady()
        {

        }

        public void AddStartsToActivePlayer(int amount)
        {
            activePlayer.AddStars(amount);
            UIManager.Instance.AnimateMoney();
        }
        public void AddStars(int playerIndex, int amount)
        {
            GetPlayerByIndex(playerIndex).AddStars(amount);
            if (playerIndex == activePlayerIndex)
            {
                UIManager.Instance.AnimateMoney();
            }
        }

        public void RemoveStars(int playerIndex, int amount)
        {
            GetPlayerByIndex(playerIndex).RemoveStars(amount);
            if (playerIndex == activePlayerIndex)
            {
                UIManager.Instance.AnimateMoney();
            }
        }

        public void AddScore(int playerIndex, int scoreType, int amount)
        {
            GetPlayerByIndex(playerIndex).AddScore(scoreType, amount);
        }


        public void LoadGame()
        {
            //ItemManager.Instance.InitializeItems();
            UIManager.Instance.OpenGamePanel();
        }

        public void NewGame()
        {
            //ItemManager.Instance.InitializeItems();
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
                    player.abilities.createMine = true;
                    break;

                case Abilities.FruitHarvest:
                    player.abilities.fruitHarvest = true;
                    break;
                case Abilities.Scout:
                    player.abilities.unitTrader = true;
                    break;
                case Abilities.Roads:
                    player.abilities.roads = true;
                    break;
                case Abilities.Guild:
                    player.abilities.guildBuilding = true;
                    player.abilities.destroyAbility = true;
                    break;
      
                case Abilities.Forestry:
                    player.abilities.forestHarvest = true;
                    break;
                case Abilities.Archery:
                    player.abilities.unitArcher = true;
                    break;
                case Abilities.Engineering:
                    player.abilities.unitTrebucet = true;
                    //player.abilities.forestCut = true;
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
            player.CheckForAvailableResearch();

            if (updateUI)
            {
                UIManager.Instance.UpdateResearchPanel(playerIndex);

                if (!activePlayer.isAI())
                {
                    UIManager.Instance.RefreshHexView();
                    SI_AudioManager.Instance.Play(SI_AudioManager.Instance.researchUnlocked);
                }
                
            }
           
        }

        #region Helpers 

        public int GetCurrentPlayerStars()
        {
            return activePlayer.stars;
        }

        public bool CanPlayerAfford(int playerIndex, int value)
        {
            if (value <= sessionPlayers[playerIndex].stars)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        public bool IsAbilityUnlocked(int playerIndex, Abilities ability)
        {
            if (GetPlayerByIndex(playerIndex).abilityDictionary.ContainsKey(ability))
            {
                return GetPlayerByIndex(playerIndex).abilityDictionary[ability].canBePurchased;
            }
            else
            {
                return false;
            }

        }

        public bool IsAbilityPurchased(int playerIndex, Abilities ability)
        {
            return GetPlayerByIndex(playerIndex).abilityDictionary[ability].hasBeenPurchased;
        }


        public bool CanPlayerAffordAbility(int playerIndex, Abilities ability)
        {
            if (GetAbilityCost(playerIndex, ability) <= GetPlayerByIndex(playerIndex).stars)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public int GetAbilityCost(int playerIndex, Abilities ability)
        {
            if (GetPlayerByIndex(playerIndex).abilityDictionary.ContainsKey(ability))
            {
                return GetPlayerByIndex(playerIndex).abilityDictionary[ability].calculatedAbilityCost;
            }
            return 9999;

        }


        public int GetBaseAbilityCost(Abilities ability)
        {
            return abilitiesDictionary[ability].abilityCost;
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
                    return landInternactionParticle;
                case TileType.ICE:
                    return cloudInteractionParticle;
            }

            return null;
        }

        //find which ability to buy in order to get the ability you want 
        //hard coded unfortunatelly, but alas, time is our master
        public Abilities GetAbilityPath(int playerIndex, Abilities abilityID)
        {
            if (abilityID == Abilities.NONE){
                return abilityID;
            }
            if (IsAbilityPurchased(playerIndex, abilityID))
            {
                return abilityID;
            }
            
            if (IsAbilityUnlocked(playerIndex, abilityID))
            {
                return abilityID;
            }
            else
            {
                switch (abilityID)
                {
                    case Abilities.Mining:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Climbing))
                        {
                            return Abilities.Climbing;
                        }
                        break;
                    case Abilities.Shields:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Mining))
                        {
                            return Abilities.Mining;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Climbing))
                        {
                            return Abilities.Climbing;
                        }
                        break;
                    case Abilities.Smithery:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Shields))
                        {
                            return Abilities.Shields;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Mining))
                        {
                            return Abilities.Mining;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Climbing))
                        {
                            return Abilities.Climbing;
                        }
                        break;
                    case Abilities.Scout:
                        if (IsAbilityUnlocked(playerIndex, Abilities.FruitHarvest))
                        {
                            return Abilities.FruitHarvest;
                        }
                        break;
                    case Abilities.Roads:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Scout))
                        {
                            return Abilities.Scout;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.FruitHarvest))
                        {
                            return Abilities.FruitHarvest;
                        }
                        break;
                    case Abilities.Guild:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Roads))
                        {
                            return Abilities.Roads;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Scout))
                        {
                            return Abilities.Scout;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.FruitHarvest))
                        {
                            return Abilities.FruitHarvest;
                        }
                        break;
                    case Abilities.Archery:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Forestry))
                        {
                            return Abilities.Forestry;
                        }
                        break;
                    case Abilities.Engineering:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Archery))
                        {
                            return Abilities.Archery;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Forestry))
                        {
                            return Abilities.Forestry;
                        }
                        break;
                    case Abilities.ForestMaster:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Engineering))
                        {
                            return Abilities.Engineering;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Archery))
                        {
                            return Abilities.Archery;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Forestry))
                        {
                            return Abilities.Forestry;
                        }
                        break;
                    case Abilities.Port:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Fishing))
                        {
                            return Abilities.Fishing;
                        }
                        break;
                    case Abilities.Ocean:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Port))
                        {
                            return Abilities.Port;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Fishing))
                        {
                            return Abilities.Fishing;
                        }
                        break;
                    case Abilities.Ship:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Ocean))
                        {
                            return Abilities.Ocean;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Port))
                        {
                            return Abilities.Port;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Fishing))
                        {
                            return Abilities.Fishing;
                        }
                        break;
                    case Abilities.Horserider:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Husbandry))
                        {
                            return Abilities.Husbandry;
                        }
                        break;
                    case Abilities.Farming:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Horserider))
                        {
                            return Abilities.Horserider;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Husbandry))
                        {
                            return Abilities.Husbandry;
                        }
                        break;
                    case Abilities.Windmill:
                        if (IsAbilityUnlocked(playerIndex, Abilities.Farming))
                        {
                            return Abilities.Farming;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Horserider))
                        {
                            return Abilities.Horserider;
                        }
                        else if (IsAbilityUnlocked(playerIndex, Abilities.Husbandry))
                        {
                            return Abilities.Husbandry;
                        }
                        break;
                }
            }

            Debug.LogError("Catastrophic error: AbilityID was not matched to a path");
            return abilityID;

        }

        public Abilities GetAbilityOrPreviousTarget(int playerIndex, BuildingType buildingType)
        {
            Abilities ability = GetAbilityPath(playerIndex, GetAbilityAssociation(buildingType));
            return ability;
        }

        public Abilities GetAbilityOrPreviousTarget(int playerIndex, ResourceType resourceType)
        {
            Abilities ability = GetAbilityPath(playerIndex, GetAbilityAssociation(resourceType));
            return ability;
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
                case BuildingType.Guild:
                    return Abilities.Guild;
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

        //does not apply to resources that transform to buildilngs 
        public bool CanPlayerCreateBuilding(int playerIndex, BuildingType type)
        {
            switch (type)
            {
                case BuildingType.FarmMaster:
                    return IsAbilityUnlocked(playerIndex, Abilities.Windmill);
                case BuildingType.ForestMaster:
                    return IsAbilityUnlocked(playerIndex, Abilities.ForestMaster);
                case BuildingType.MineMaster:
                    return IsAbilityUnlocked(playerIndex, Abilities.Smithery);
                case BuildingType.Guild:
                    return IsAbilityPurchased(playerIndex, Abilities.Guild);
                case BuildingType.Port:
                    return IsAbilityPurchased(playerIndex, Abilities.Port);
            }

            return false;
        }

        public bool CanPlayerHarvestResource(int playerIndex, ResourceType type)
        {
            switch (type)
            {
                case ResourceType.FRUIT:
                    return sessionPlayers[playerIndex].abilities.fruitHarvest;
                case ResourceType.FOREST:
                    return sessionPlayers[playerIndex].abilities.forestHarvest;
                case ResourceType.ANIMAL:
                    return sessionPlayers[playerIndex].abilities.animalHarvest;
                case ResourceType.FARM:
                    return sessionPlayers[playerIndex].abilities.farmHarvest;
                case ResourceType.MINE:
                    return sessionPlayers[playerIndex].abilities.mineHarvest;
                case ResourceType.FISH:
                    return sessionPlayers[playerIndex].abilities.fishHarvest;
                case ResourceType.EMPTY:
                    return false;
                case ResourceType.MONUMENT:
                    return false;

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
            return sessionPlayers.IndexOf(player);
        }
        #endregion //helper functions 

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


