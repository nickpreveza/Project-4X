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
        bool canLoad;

        [SerializeField] Button loadGame;
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

        }

        public void ToggleUIPanel(UIPanel targetPanel, bool state, bool fadeGamePanel = true, float delayAmount = 0.0f)
        {
            StartCoroutine(ToggleUIPanelEnum(targetPanel, state, fadeGamePanel, delayAmount));
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
        }

        public void ShowHexView(WorldHex hex, WorldUnit unit = null)
        {
            gamePanel.GetComponent<GamePanel>().ShowHexView(hex, unit);
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

