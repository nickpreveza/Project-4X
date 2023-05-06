using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SignedInitiative
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public UIPanel pausePanel;
        public UIPanel gamePanel;
        public UIPanel gameOverPanel;
        public UIPanel scorePanel;
        public UIPanel mainMenuPanel;
        public UIPanel onboardingPanel;
        public UIPanel initializerPanel;
        public UIPanel settingsPanel;

        public OverlayPanel overlayPanel;
        public UIPopup latestPopup;

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

        public Color researchPurchased;
        public Color researchLocked;
        public Color researchAvailable;
        public Color researchUnavailable;

        public Color hexViewDescriptionAvailable;
        public Color hexViewDescriptionUnavailable;

        public Color oceanHex;
        public Color seaHex;
        public Color sandHex;
        public Color grassHex;
        public Color hillhex;
        public Color mountainHex;


        bool canLoad;

        [SerializeField] Button loadGame;
        [SerializeField] UIPanel currentSubpanel;

        [SerializeField] UniversalPopup universalPopup;
        public delegate void popupFunction();
        public popupFunction confirmAction;

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

        }

        private void Start()
        {
            SI_EventManager.Instance.onCityCaptured += OnCityCaptureCallback;
            SI_EventManager.Instance.onTransactionMade += OnTransactionMadeCallback;

            ClosePopup();
        }

        public void OpenPopup(string title, string description, bool available, popupFunction  newFunction)
        {
            confirmAction = newFunction;

            universalPopup.gameObject.SetActive(true);
            universalPopup.SetData(title, description, available);
        }

        public void ClosePopup()
        {
            universalPopup.Close();
        }

        void OnCityCaptureCallback(int playerIndex)
        {
            if (GameManager.Instance.IsIndexOfActivePlayer(playerIndex))
            {
                gamePanel.GetComponent<GamePanel>().UpdateResearchPanel();
            }
        }

        public void OnTransactionMadeCallback(int playerIndex)
        {
            if (GameManager.Instance.IsIndexOfActivePlayer(playerIndex))
            {
                UpdateHUD();
            }
        }

        public void ToggleUIPanel(UIPanel targetPanel, bool state, bool fadeGamePanel = true, float delayAmount = 0.0f)
        {
            StartCoroutine(ToggleUIPanelEnum(targetPanel, state, fadeGamePanel, delayAmount));
        }

        public void OpenResearchPanel()
        {
            gamePanel.GetComponent<GamePanel>().OpenResearchPanel();
        }

        IEnumerator ToggleUIPanelEnum(UIPanel targetPanel, bool state, bool fadeGamePanel = true, float delayAmount = 0.0f)
        {
            yield return new WaitForSeconds(delayAmount);

            if (state)
            {
                GameManager.Instance.menuActive = true;
                currentSubpanel = targetPanel;
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
                gamePanel.canvasGroup.alpha = 1;
                subPanelActive = false;
                targetPanel.Disable();
                GameManager.Instance.menuActive = false;
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
            GameManager.Instance.menuActive = true;
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

        public void EndTurn()
        {
            HideHexView();
          
            gamePanel.GetComponent<GamePanel>().HideResearchPanel();
            gamePanel.GetComponent<GamePanel>().HideOverviewPanel();
            //gamePanel.GetComponent<GamePanel>().HideSettingsPanel();
        }
        public void StartTextbox(string characterName, List<string> dialogContent, ConversationType convoType, Quest quest = null)
        {
            GameManager.Instance.menuActive = true;
            gamePanel.GetComponent<GamePanel>().textbox.StartTextbox(characterName, dialogContent, convoType, quest);
        }


        public void EndTextbox()
        {
            GameManager.Instance.menuActive = false;
            gamePanel.GetComponent<GamePanel>().textbox.EndTextbox();
        }

        public void ClosePanels()
        {
            onboardingPanel?.Disable();
            mainMenuPanel?.Disable();
            pausePanel?.Disable();
            gamePanel?.Disable();
        }

        public void OpenGamePanel()
        {
            ClosePanels();
            gamePanel.GetComponent<UIPanel>().Setup();
            gamePanel.Activate();

            menuActive = false;
            SetupOverview();
            GameManager.Instance.menuActive = true;
        }

        public void OpenMainMenu()
        {
            GameManager.Instance.SetPause = false;
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
            GameManager.Instance.menuActive = true;
            SI_AudioManager.Instance.PlayTheme("menuTheme");
        }

        public void ActionLoadGame()
        {
            GameManager.Instance.Load();
        }

        public void GameOver()
        {
            gamePanel.Disable();
            pausePanel.Disable();

            gameOverPanel.Setup();
            gameOverPanel.Activate();
        }

        public void UpdateHUD()
        {
            gamePanel.GetComponent<GamePanel>().UpdateCurrencies();
            gamePanel.GetComponent<GamePanel>().UpdateOverview();
           //TODO: optimize this to only update when turn changes
           gamePanel.GetComponent<GamePanel>().SetPlayerAvatar();
        }

        public void SetupOverview()
        {
            gamePanel.GetComponent<GamePanel>().SetupOverview();
        }

        public void ShowHexView(WorldHex hex, WorldUnit unit = null)
        {
            gamePanel.GetComponent<GamePanel>().ShowHexView(hex, unit);
        }
        
        public void RefreshHexView()
        {
            gamePanel.GetComponent<GamePanel>().RefreshHexView();
        }

        public Color GetHexColorByType(TileType type)
        {
            switch (type)
            {
                case TileType.DEEPSEA:
                    return oceanHex;
                case TileType.SEA:
                    return seaHex;
                case TileType.SAND:
                    return sandHex;
                case TileType.GRASS:
                    return grassHex;
                case TileType.HILL:
                    return hillhex;
                case TileType.MOUNTAIN:
                    return mountainHex;
            }

            return Color.white;
        }

        public void HideHexView()
        {
            gamePanel.GetComponent<GamePanel>().HideHexView();
        }

        public void UpdateResourcePanel(int playerIndex)
        {
            if (playerIndex == GameManager.Instance.activePlayerIndex)
            {
                gamePanel.GetComponent<GamePanel>().UpdateResearchPanel();
            }
            
        }

        public void ShowOverlay(GameObject target, float offset, bool placementOverlay)
        {
            overlayPanel.EnableOverlayPrompt(target, offset, placementOverlay);
        }

        public void HideOverlay()
        {
            overlayPanel.DisablePrompt();
        }

        public void ShowTooltip(string body, string header = "")
        {
            //overlayPanel.tooltip.SetData(body, header);
            // overlayPanel.tooltip.gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            //overlayPanel.tooltip.gameObject.SetActive(false);
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
            foreach (UIPanel panel in allPanelsList)
            {
                panel.Disable();
            }
        }

        /// <summary>
        /// Called from GameManager when the Paused state is changed. Could be using an event here
        /// </summary>
        public void PauseChanged()
        {
            if (GameManager.Instance.isPaused)
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
            Application.OpenURL(GameManager.Instance.data.itchURL);
        }

        public void ActionOpenWebsite()
        {
            Application.OpenURL(GameManager.Instance.data.websiteURL);
        }

        public void ActionOpenDiscord()
        {
            Application.OpenURL(GameManager.Instance.data.discordURL);
        }

   
    }

}

