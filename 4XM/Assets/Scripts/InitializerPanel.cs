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
        Initializer.Instance.selectedCivs.Clear();

        foreach(Player player in Initializer.Instance.setupPlayers)
        {
            player.activatedOnSetup = false;
        }
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
        SI_AudioManager.Instance.PlayClick();
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
        Civilizations previousType = Initializer.Instance.setupPlayers[playerIndex].civilization;
        Initializer.Instance.setupPlayers[playerIndex].civilization = type;

        bool otherPlayerFound = false;
        foreach (Player player in Initializer.Instance.setupPlayers)
        {
            if (player == Initializer.Instance.setupPlayers[playerIndex])
            {
                continue;
            }
            if (player.civilization == type && player.activatedOnSetup)
            {
                player.civilization = previousType;
                otherPlayerFound = true;
            }
            if (player.civilization == previousType && player.activatedOnSetup)
            {
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

        if (playerIndex != 0)
        {
            Initializer.Instance.setupPlayers[playerIndex].type = PlayerType.AI;
        }
        else
        {
            Initializer.Instance.setupPlayers[playerIndex].isFirstPlayer = true;

        }

        Initializer.Instance.setupPlayers[playerIndex].activatedOnSetup = true;


        CloseSelectCivPanel(true);
    }

    public void CloseSelectCivPanel(bool openOverview)
    {
        SI_AudioManager.Instance.PlayClick();
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
        List<Civilizations> typesFound = new List<Civilizations>();
        Initializer.Instance.setupPlayers[0].type = PlayerType.LOCAL;
        for (int i = 0; i < Initializer.Instance.setupPlayers.Count; i++)
        {
            if (Initializer.Instance.setupPlayers[i].activatedOnSetup)
            {
                totalPlayers++;
                GameObject obj = Instantiate(setupGameEntryPrefab, setupGameEntryParent);
                typesFound.Add(Initializer.Instance.setupPlayers[i].civilization);
                obj.GetComponent<CivSelectButton>().SetupAsOverview(this, Initializer.Instance.setupPlayers[i], i);
            }
        }

        //work around for weird bug I can't solve that essentiall doesnt remove on from selected civs on remove
        Initializer.Instance.selectedCivs = new List<Civilizations>(typesFound);

        if (totalPlayers < 4)
        {
            GameObject obj = Instantiate(plusPrefab, setupGameEntryParent);
            obj.GetComponent<Button>().onClick.AddListener(()=>AddPlayer());
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

    void AddPlayer()
    {
        SI_AudioManager.Instance.PlayClick();
        int foundIndex = 0;
        for (int i = 0; i < Initializer.Instance.setupPlayers.Count; i++)
        {
            if (!Initializer.Instance.setupPlayers[i].activatedOnSetup)
            {
                foundIndex = i;
                break;
            }
        }

        SetupSelectYourCiv(foundIndex, false);
    }

    public void ToggleChanged(int playerIndex, bool togglevalue)
    {
        SI_AudioManager.Instance.PlayClick();
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
        SI_AudioManager.Instance.PlayClick();
        Initializer.Instance.setupPlayers[playerIndex].activatedOnSetup = false;
        if (Initializer.Instance.selectedCivs.Contains(Initializer.Instance.setupPlayers[playerIndex].civilization))
        {
            Initializer.Instance.selectedCivs.Remove(Initializer.Instance.setupPlayers[playerIndex].civilization);
        }
        SetupOverviewSubPanel();
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
        SI_AudioManager.Instance.PlayClick();
        Application.OpenURL(GameManager.Instance.data.linktrURL);
    }

    public void StartGameAction()
    {
        SI_AudioManager.Instance.Play(SI_AudioManager.Instance.startGame);
        Initializer.Instance.LocalStart(true);
    }

    public void ToggleSettingsAction()
    {
        SI_AudioManager.Instance.PlayClick();
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ExitAction()
    {
        SI_AudioManager.Instance.PlayClick();
        UIManager.Instance.OpenPopup(
            "QUIT GAME", 
            "Are you sure you want to exit?", 
            true,
            "exit", 
            "cancel", 
            () => GameManager.Instance.ApplicationQuit(), true);
    }




}
