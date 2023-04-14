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

    void Start()
    {
      
        if (UIManager.Instance != null)
        {
            UIManager.Instance.gamePanel = this.GetComponent<UIPanel>();
            UIManager.Instance.AddPanel(this);
        }

        HideHexView();
        //textbox.EndTextbox();
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
