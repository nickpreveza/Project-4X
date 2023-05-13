using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SignedInitiative;
public class GamePanel : UIPanel
{
    [SerializeField] TextMeshProUGUI scoreValue;
    [SerializeField] TextMeshProUGUI starValue;
    [SerializeField] TextMeshProUGUI turnValue;
    [SerializeField] TextMeshProUGUI expectedStars;

    public TextMeshProUGUI devText;
    public Textbox textbox;

    [SerializeField] HexView hexView;

    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] GameObject overviewPanel;
    [SerializeField] GameObject researchPanel;
    [SerializeField] GameObject settingsPanel;

    [SerializeField] Image playerAvatarBackground;
    [SerializeField] Image starsImage;
    [SerializeField] Image scoreImage;
    [SerializeField] Image turnImage;



    ResearchViewHandler research;
    OverviewPanel overview;

    [SerializeField] Image civAvatar;

    [SerializeField] Color hasActionsColor;
    [SerializeField] Color noActionsColor;

    [SerializeField] Image endTurnButtonImage;
    [SerializeField] Image researchButtonIamge;

    [SerializeField] GameObject turnChange;
    [SerializeField] TextMeshProUGUI turnName;
    [SerializeField] Image turnIcon;
    [SerializeField] Image turnChangeImage;

    [SerializeField] GameObject playerAvatarParent;
    [SerializeField] ScrollRect scrollRect;

    void Start()
    {
      
        if (UIManager.Instance != null)
        {
            UIManager.Instance.gamePanel = this.GetComponent<UIPanel>();
            UIManager.Instance.AddPanel(this);
        }
        research = researchPanel.GetComponent<ResearchViewHandler>();
        overview = overviewPanel.GetComponent<OverviewPanel>();
        researchPanel.SetActive(false);
        overviewPanel.SetActive(false);
        settingsPanel.SetActive(false);
        HideHexView();
        //textbox.EndTextbox();
    }

    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);

    }

    public void SetupOverview()
    {
        overview.OverviewSetup();
    }

    public void UpdateOverview()
    {
        overview.UpdateEntries();
    }
    public void RestartAction()
    {
        UIManager.Instance.OpenPopup(
                 "Return to title",
                 "Are you sure you want to exit?",
                 true,
                 "exit",
                 "cancel",
                 () => GameManager.Instance.ReloadScene(), true);
       
    }

    public void UpdateGUIButtons()
    {
        if (GameManager.Instance.activePlayer.playerHasActions)
        {
            endTurnButtonImage.color = hasActionsColor;

            if (GameManager.Instance.activePlayer.playerCanBuyAbility)
            {
                researchButtonIamge.color = noActionsColor;
            }
            else
            {
                researchButtonIamge.color = hasActionsColor;
            }
        }
        else
        {
            endTurnButtonImage.color = noActionsColor;
            researchButtonIamge.color = hasActionsColor;
        }
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

    public void SetPlayerAvatar()
    {
        playerAvatarBackground.color = GameManager.Instance.CivOfActivePlayer().uiColorActive;
        turnImage.color = GameManager.Instance.CivOfActivePlayer().uiColorActive;
        scoreImage.color = GameManager.Instance.CivOfActivePlayer().uiColorActive;
        civAvatar.sprite = GameManager.Instance.CivOfActivePlayer().civLogo;
    }

    public void UpdateResearchPanel()
    {
        research.UpdateResearchButtons();
    }

    public void HideResearchPanel()
    {
        researchPanel.SetActive(false);
    }

    public void HideOverviewPanel()
    {
        overviewPanel.SetActive(false);
    }
    public void ToggleOverviewPanel()
    {
        if (overviewPanel.activeSelf)
        {
            overviewPanel.SetActive(false);
        }
        else
        {
            GameManager.Instance.CalculateRanks();
            overviewPanel.SetActive(true);
            UpdateOverview();
            researchPanel.SetActive(false);
        }
    }

    public void ToggleResearchPanel()
    {
        if (researchPanel.activeSelf)
        {
            researchPanel.SetActive(false);
        }
        else
        {
            researchPanel.SetActive(true);
            overviewPanel.SetActive(false);
        }

    }

    public void OpenResearchPanel()
    {
        researchPanel.SetActive(true);
    }

    public void CloseResearchPanel()
    {
        researchPanel.SetActive(false);
    }

    public void OpenResearchPanel(Abilities type)
    {
        researchPanel.SetActive(true);
        ResearchButton button = research.GetButtonByAbility(type);
        StartCoroutine(HighlightButton(button));
    }

    IEnumerator HighlightButton(ResearchButton button)
    {
        yield return new WaitForSeconds(0.5f);
        if (researchPanel.GetComponent<ResearchViewHandler>().researchButtons.IndexOf(button) > 11)
        {
            scrollRect.verticalNormalizedPosition = 0;

        }
        button.OpenHighlight();
    }

    public void UpdateCurrencies()
    {
        playerName.text = GameManager.Instance.GetCivilizationByType(GameManager.Instance.activePlayer.civilization).name;
        scoreValue.text = GameManager.Instance.activePlayer.totalScore.ToString();
        starValue.text = GameManager.Instance.activePlayer.stars.ToString();
        turnValue.text = GameManager.Instance.activePlayer.turnCount.ToString();
        expectedStars.text = " +" + GameManager.Instance.activePlayer.expectedStars.ToString();
    }

    public void ShowHexView(WorldHex hex, WorldUnit unit = null)
    {
        hexView.gameObject.SetActive(true);
        hexView.SetData(hex, unit);
    }

    public void RefreshHexView()
    {
        hexView.Refresh();
    }

    public void EndTurn()
    {
        if (SI_CameraController.Instance.animationsRunning)
        {
            return;
        }

       
        GameManager.Instance.LocalEndTurn();
    }


    public void StartTurnAnim()
    {
        StartCoroutine(TurnChange());
    }

    public void PlayPlayerAvatarAnim()
    {
        playerAvatarParent.GetComponent<Animator>().SetTrigger("TurnChange");
    }

    IEnumerator TurnChange()
    {
        turnChangeImage.color = GameManager.Instance.CivOfActivePlayer().uiColorActive;
        turnName.text = GameManager.Instance.GetCivilizationByType(GameManager.Instance.activePlayer.civilization).name;
        turnIcon.sprite = GameManager.Instance.GetCivilizationByType(GameManager.Instance.activePlayer.civilization).civLogo;
        turnChange.SetActive(true);
        yield return new WaitForSeconds(1f);
        turnChange.SetActive(false);
    }

    public void HideHexView()
    {
        hexView.gameObject.SetActive(false);
    }

    public override void Setup()
    {
        base.Setup();
    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Disable()
    {
        base.Disable();
    }
}
