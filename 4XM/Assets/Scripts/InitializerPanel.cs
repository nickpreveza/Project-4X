using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SignedInitiative;

public class InitializerPanel : UIPanel
{
    [SerializeField] GameObject settingsPanel;
    [SerializeField] InputField seedField;

    [SerializeField] GameObject newGamePanel; //the panel where we can see all civs in the match
    [SerializeField] GameObject selectCivsubPanel;
    [SerializeField] GameObject overviewGamesubPanel;

    [SerializeField] Transform setupGameEntryParent;
    [SerializeField] GameObject setupGameEntryPrefab;
    [SerializeField] Transform selectCivEntryParent;
    [SerializeField] GameObject seleCivEntryPrefab;
    [SerializeField] GameObject plusPrefab;

   [SerializeField] Button startButton;
    int selectedPlayerIndex;
    int selectedCivIndex;
    private void Start()
    {
        Setup();
    }

    public void CloseSetupGamePanel()
    {
        newGamePanel.SetActive(false);
        overviewGamesubPanel.SetActive(false);
        selectCivsubPanel.SetActive(false);
        Initializer.Instance.setupPlayers.Clear();
    }
    public override void UpdateData()
    {

    }

    public void ValueChangedOnHorizontalScroll(int index)
    {
        Debug.Log(index);
    }

    public void SetCiv(int value)
    {
        Debug.Log(value);
        switch (value)
        {
            case 1:
                break;
        }
    }

    public void SetTypeOfPlayer(int value)
    {
        Debug.Log(value);
        switch (value)
        {
            case 1:
                break;
        }
    }

    public void OpenNewGamePanel()
    {
        newGamePanel.SetActive(true);
      
       
        selectedPlayerIndex = 0;
        selectedCivIndex = 0;
        Initializer.Instance.selectedCivs.Clear();
        SetupSelectYourCiv(0, false);
        startButton.gameObject.SetActive(false); // interactable = false;
    }

    public void SetupSelectYourCiv(int playerIndex, bool existsAlready)
    {
        overviewGamesubPanel.SetActive(false);
        selectCivsubPanel.SetActive(true);

        foreach (Transform child in selectCivEntryParent)
        {
            Destroy(child.gameObject);
        }

        foreach(Civilization civ in GameManager.Instance.gameCivilizations)
        {
            if (!existsAlready)
            {
                if (!Initializer.Instance.selectedCivs.Contains(civ.civType))
                {
                    GameObject obj = Instantiate(seleCivEntryPrefab, selectCivEntryParent);
                    obj.GetComponent<CivSelectButton>().SetUpAsSelection(this, civ, playerIndex, existsAlready);
                }
            }
            else
            {
                GameObject obj = Instantiate(seleCivEntryPrefab, selectCivEntryParent);
                obj.GetComponent<CivSelectButton>().SetUpAsSelection(this, civ, playerIndex, existsAlready);
            }
           
        }
    }

    public void SelectCiv(int playerIndex, Civilizations type, bool overrideCivSettings)
    {
        if (overrideCivSettings)
        {
            Civilizations previousType = Initializer.Instance.setupPlayers[playerIndex].civilization;
            Initializer.Instance.setupPlayers[playerIndex].civilization = type;

            bool otherPlayerFound = false;
            foreach(Player player in Initializer.Instance.setupPlayers)
            {
                if (player == Initializer.Instance.setupPlayers[playerIndex])
                {
                    continue;
                }
                if (player.civilization == type)
                {
                    player.civilization = previousType;
                    otherPlayerFound = true;
                }
            }

            if (!otherPlayerFound)
            {
                if (Initializer.Instance.selectedCivs.Contains(previousType))
                {
                    Initializer.Instance.selectedCivs.Remove(previousType);
                }
            }
            if (!Initializer.Instance.selectedCivs.Contains(type))
            {
                Initializer.Instance.selectedCivs.Add(type);
            }
        }
        else
        {
            Initializer.Instance.selectedCivs.Add(type);
            Initializer.Instance.setupPlayers[playerIndex].civilization = type;
            if (playerIndex != 0)
            {
                Initializer.Instance.setupPlayers[playerIndex].type = PlayerType.AI;
            }
            
        }

        Initializer.Instance.setupPlayers[playerIndex].activatedOnSetup = true;
        if (playerIndex == 0)
        {
            Initializer.Instance.setupPlayers[playerIndex].isFirstPlayer = true;
        }

        CloseSelectCivPanel(true);
    }

    public void CloseSelectCivPanel(bool openOverview)
    {
        selectCivsubPanel.SetActive(false);

        if (openOverview)
        {
            overviewGamesubPanel.SetActive(true);
            SetupOverviewSubPanel();
        }    
    }

    void SetupOverviewSubPanel()
    {
        foreach (Transform child in setupGameEntryParent)
        {
            Destroy(child.gameObject);
        }

        int totalPlayers = 0;

        for (int i = 0; i < Initializer.Instance.setupPlayers.Count; i++)
        {
            if (Initializer.Instance.setupPlayers[i].activatedOnSetup)
            {
                totalPlayers++;
                GameObject obj = Instantiate(setupGameEntryPrefab, setupGameEntryParent);
                obj.GetComponent<CivSelectButton>().SetupAsOverview(this, Initializer.Instance.setupPlayers[i], i);
            }
        }

        if (totalPlayers < 4)
        {
            GameObject obj = Instantiate(plusPrefab, setupGameEntryParent);
            obj.GetComponent<Button>().onClick.AddListener(()=>AddPlayer(totalPlayers));
        }

        if (totalPlayers  > 1)
        {
            startButton.gameObject.SetActive(true);
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }
       
    }

    void AddPlayer(int index)
    {
        SetupSelectYourCiv(index, false);
    }

    public void ToggleChanged(int playerIndex, bool togglevalue)
    {
        if (togglevalue)
        {
            Initializer.Instance.setupPlayers[playerIndex].type = PlayerType.LOCAL;
        }
        else
        {
            Initializer.Instance.setupPlayers[playerIndex].type = PlayerType.AI;
        }
    }

    public void RemoveSetupPlayer(int playerIndex)
    {
        Initializer.Instance.setupPlayers[playerIndex].activatedOnSetup = false;
        if (Initializer.Instance.selectedCivs.Contains(Initializer.Instance.setupPlayers[playerIndex].civilization))
        {
            Initializer.Instance.selectedCivs.Remove(Initializer.Instance.setupPlayers[playerIndex].civilization);
        }
        SetupOverviewSubPanel();
    }

    void SetupNewGamePanel()
    {
        //open panel for player 1. 
        foreach(Civilization civ in GameManager.Instance.gameCivilizations)
        {

        }
        /*
        numberOfPlayers = 2;

        foreach(Transform child in entriesParent)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < numberOfPlayers; i++)
        {
            GameObject obj = Instantiate(playerEntryPrefab, entriesParent);
            
            PlayerOverviewEntry entry = obj.GetComponent<PlayerOverviewEntry>();
            //entry.SetPlayer(player);
            //playerEntries.Add(entry);
        }*/
    }

    public override void Setup()
    {
        settingsPanel.SetActive(false);
        overviewGamesubPanel.SetActive(false);
        selectCivsubPanel.SetActive(false);
        newGamePanel.SetActive(false);
    }

    public void OpenLinkTreeAction()
    {
        Application.OpenURL(GameManager.Instance.data.linktrURL);
    }

    public void StartGameAction()
    {
        Initializer.Instance.LocalStart(true);
    }

    public void ToggleSettingsAction()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ExitAction()
    {
        UIManager.Instance.OpenPopup(
            "QUIT GAME", 
            "Are you sure you want to exit?", 
            true,
            "exit", 
            "cancel", 
            () => GameManager.Instance.ApplicationQuit(), true);
    }




}
