using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HB_UIManager : MonoBehaviour
{
    public static HB_UIManager Instance;

    public UIPanel pausePanel;
    public UIPanel gamePanel;
    public UIPanel gameOverPanel;
    public UIPanel scorePanel;
    public UIPanel mainMenuPanel;
    public UIPanel onboardingPanel; //todo for beta

    public OverlayPanel overlayPanel;
    public UIPopup latestPopup;

    [SerializeField] UIPopup settingsPopup;

    List<UIPanel> allPanelsList;
    public bool menuActive;
    public bool popupActive;
    public bool subPanelActive;
    public Color affordableColor;
    public Color unaffordableColor;
    public Color itemUIselected;
    public Color itemUIDeselected;
    public Color skillbarEnabled;
    public Color skillbarDisabled;
    bool canLoad;
    [SerializeField] Button loadGame;

    public BlacksmithUI blacksmith;
    public FactoryUI factory;
    public CookingUI cooking;
    public QuestboardUI questBoard;
    public InventoryUI inventory;
  
    [SerializeField] UIPanel currentSubpanel;
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

        settingsPopup.gameObject.SetActive(false);
    }

    public void UpdateBackpack(bool reconstruct)
    {
        if (reconstruct)
        {
            inventory.UpdateInventory();
            inventory.UpdateRecipies();
            WeaponsChanged();
        }
        else
        {
            inventory.UpdateValues();
            WeaponsChanged();
        }
    }

    public void GetItemFromPeakInventory(int index)
    {
        gamePanel.GetComponent<GamePanel>().EquipItemFromPeakInventory(index);
    }

    public void UpdateQuest()
    {
        gamePanel.GetComponent<GamePanel>().UpdateQuest();
    }

    public void WeaponsChanged()
    {
        inventory.UpdateWeapons();
    }
    public void ToggleInventory()
    {
        bool state = !inventory.panelObject.activeSelf;
        if (state)
        {
            UpdateBackpack(true);
        }
        ToggleUIPanel(inventory, state, false);
    }

    public void ToggleSettings(bool state)
    {
        if (state)
        {
            popupActive = true;
            settingsPopup.gameObject.SetActive(true);
        }
        else
        {
            popupActive = false;
            settingsPopup.gameObject.SetActive(false);
        }
    }

    public void ToggleUIPanel(UIPanel targetPanel, bool state, bool fadeGamePanel = true)
    { 
        if (state)
        {
            if (subPanelActive)
            {
                return;
            }
            HB_PlayerController.Instance.controlsActive = false;
            currentSubpanel = targetPanel;
            if (currentSubpanel == inventory)
            {
                gamePanel.GetComponent<GamePanel>().inventoryPrompt.isOn = true;
            }
            if (fadeGamePanel)
            {
                gamePanel.canvasGroup.alpha = 0;
            }
            
            overlayPanel.DisablePrompt();
            subPanelActive = true;
            targetPanel.Activate();
        }
        else
        {
            if (currentSubpanel == inventory)
            {
                gamePanel.GetComponent<GamePanel>().inventoryPrompt.isOn = false;
            }
            gamePanel.canvasGroup.alpha = 1;
            subPanelActive = false;
            targetPanel.Disable();
            HB_PlayerController.Instance.controlsActive = true;
            HideTooltip();
        }
    }

    public void CloseCurrentSubpanel()
    {
        if (currentSubpanel == null)
        {
            return;
        }
        currentSubpanel.Disable();
        subPanelActive = false;
        gamePanel.canvasGroup.alpha = 1;
        HB_PlayerController.Instance.controlsActive = true;
        currentSubpanel = null;
    }

    public void AddPanel(UIPanel newPanel)
    {
        if (allPanelsList == null)
        {
            allPanelsList = new List<UIPanel>();
        }

        if (!allPanelsList.Contains(newPanel))
        {
            allPanelsList.Add(newPanel);
        }
    }

    public void StartTextbox(string characterName, List<string> dialogContent, ConversationType convoType, Quest quest = null)
    {
        HB_PlayerController.Instance.controlsActive = false;
        gamePanel.GetComponent<GamePanel>().textbox.StartTextbox(characterName, dialogContent, convoType, quest);
    }


    public void EndTextbox()
    {
        HB_PlayerController.Instance.controlsActive = true;
        gamePanel.GetComponent<GamePanel>().textbox.EndTextbox();
    }

    public void ClosePanels()
    {
        onboardingPanel?.Disable();
        mainMenuPanel.Disable();
        pausePanel.Disable();
        gamePanel.Disable();
    }

    public void OpenGamePanel()
    {
        ClosePanels();
        gamePanel.GetComponent<UIPanel>().Setup();
        gamePanel.Activate();
       
        menuActive = false;
        HB_PlayerController.Instance.controlsActive = true;
        HB_GameManager.Instance.SetGameView();
        UpdatePlayerHealth(true, false);
    }

    public void OpenMainMenu()
    {
        HB_GameManager.Instance.SetPause = false;
        ClosePanels();
        menuActive = true;
        mainMenuPanel.Setup();
        mainMenuPanel.Activate();
        if (canLoad)
        {
            loadGame.interactable = true;
        }
        else
        {
            loadGame.interactable = false;
        }
        HB_PlayerController.Instance.controlsActive = false;
        HB_AudioManager.Instance.PlayTheme("menuTheme");
        HB_GameManager.Instance.SetMenuView();
    }

    public void ActionLoadGame()
    {
        HB_GameManager.Instance.Load();
    }

    public void GameOver()
    {
        gamePanel.Disable();
        pausePanel.Disable();

        gameOverPanel.Setup();
        gameOverPanel.Activate();
    }

    public void UpdatePlayerHealth(bool skipAnimation, bool maxHealthChanged)
    {
        if (maxHealthChanged)
        {
            //gamePanel.GetComponent<GamePanel>().MaxHealthChanged();
            Debug.LogError("Current version does not support maxHealth changes for visual reasons");
        }
        else
        {
            gamePanel.GetComponent<GamePanel>().UpdateHealth(skipAnimation);
        }
       
    }

    public void UpdateScore()
    {
        gamePanel.GetComponent<GamePanel>().UpdateScore();
    }
    public void UpdatePlayerCurrencies()
    {
        gamePanel.GetComponent<GamePanel>().UpdateCurrencies();
    }

    public void ShowOverlay(GameObject target, float offset, bool placementOverlay)
    {
        overlayPanel.EnableOverlayPrompt(target,offset, placementOverlay);
    }

    public void HideOverlay() 
    {
        overlayPanel.DisablePrompt();
    }

    public void ShowTooltip(string body, string header = "")
    {
        overlayPanel.tooltip.SetData(body, header);
        overlayPanel.tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        overlayPanel.tooltip.gameObject.SetActive(false);
    }

    /// <summary>
    /// Used to close all the panels registered to the list
    /// </summary>
    public void CloseAllPanels()
    {
        if (allPanelsList == null)
        {
            Debug.LogWarning("CloseAllPanels failed. No panels registered");
            return;
        }
        foreach(UIPanel panel in allPanelsList)
        {
            panel.Disable();
        }
    }

    /// <summary>
    /// Called from GameManager when the Paused state is changed. Could be using an event here
    /// </summary>
    public void PauseChanged()
    {
        if (HB_GameManager.Instance.isPaused)
        {
            gamePanel.canvasGroup.alpha = 0;
            pausePanel.Activate();
        }
        else
        {
            gamePanel.canvasGroup.alpha = 1;
            pausePanel.Disable();
        }
    }

    public void ActionOpenItchPage()
    {
        Application.OpenURL(HB_GameManager.Instance.data.itchURL);
    }

    public void ActionOpenWebsite()
    {
        Application.OpenURL(HB_GameManager.Instance.data.websiteURL);
    }

    public void ActionOpenDiscord()
    {
        Application.OpenURL(HB_GameManager.Instance.data.discordURL);
    }

}
