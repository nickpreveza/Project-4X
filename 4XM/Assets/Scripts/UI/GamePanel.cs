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
    public TextMeshProUGUI devText;
    public Textbox textbox;

    [SerializeField] HexView hexView;

    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] GameObject overviewPanel;
    [SerializeField] GameObject researchPanel;
    [SerializeField] GameObject settingsPanel;

    [SerializeField] Image playerAvatarBackground;

    ResearchViewHandler research;
    void Start()
    {
      
        if (UIManager.Instance != null)
        {
            UIManager.Instance.gamePanel = this.GetComponent<UIPanel>();
            UIManager.Instance.AddPanel(this);
        }
        research = researchPanel.GetComponent<ResearchViewHandler>();
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

    public void RestartAction()
    {
        GameManager.Instance.ReloadScene();
    }

    public void ExitAction()
    {
        GameManager.Instance.ApplicationQuit();
    }

    public void SetPlayerAvatar()
    {
        playerAvatarBackground.color = GameManager.Instance.CivOfActivePlayer().uiColorActive;
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
            overviewPanel.SetActive(true);
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
    public void UpdateCurrencies()
    {
        playerName.text = GameManager.Instance.activePlayer.name;
        scoreValue.text = "SCORE: " + GameManager.Instance.activePlayer.score;
        starValue.text = "STARS: " + GameManager.Instance.activePlayer.stars;
        turnValue.text = "TURN: " + GameManager.Instance.activePlayer.turnCount;
        /*
        scoreValue.text = ItemManager.Instance.GetItemAmount("hempseeds").ToString();
        starValue.text = ItemManager.Instance.GetItemAmount("hempfibers").ToString();
        scoreValue.text = ItemManager.Instance.GetItemAmount("hemppebbles").ToString(); */
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
