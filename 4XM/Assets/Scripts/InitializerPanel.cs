using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SignedInitiative;
public class InitializerPanel : UIPanel
{
    [SerializeField] GameObject settingsPanel;
    [SerializeField] InputField seedField;
    [SerializeField] GameObject newGameSetupPanel;
    [SerializeField] GameObject playerEntryPrefab;

    int numberOfPlayers;
    int minNumberOfPlayers = 2;
    [SerializeField] Transform entriesParent;

    private void Start()
    {
        settingsPanel.SetActive(false);
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
        SetupNewGamePanel();
    }
    void SetupNewGamePanel()
    {
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
        }
    }

    public override void Setup()
    {
        settingsPanel.SetActive(false);
        newGameSetupPanel.SetActive(false);
    }

    public void OpenLinkTreeAction()
    {
        Application.OpenURL(GameManager.Instance.data.linktrURL);
    }

    public void StartGameAction()
    {
        Initializer.Instance.LocalStart();
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
