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

    [SerializeField] TextMeshProUGUI playerName;

    void Start()
    {
      
        if (UIManager.Instance != null)
        {
            UIManager.Instance.gamePanel = this.GetComponent<UIPanel>();
            UIManager.Instance.AddPanel(this);
        }
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

    private void Update()
    {/*
        if (SI_GameManager.Instance.devMode)
        {
            devText.text = "devMode";
           
        }
        else
        {
            devText.text = "";
        }*/
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
