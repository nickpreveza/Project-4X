using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePanel : UIPanel
{
    [SerializeField] TextMeshProUGUI scoreValue;
    [SerializeField] TextMeshProUGUI starValue;
    [SerializeField] TextMeshProUGUI turnValue;
    public TextMeshProUGUI devText;
    public Textbox textbox;

    void Start()
    {
      
        if (SI_UIManager.Instance != null)
        {
            SI_UIManager.Instance.gamePanel = this.GetComponent<UIPanel>();
            SI_UIManager.Instance.AddPanel(this);
        }
        //textbox.EndTextbox();
    }

    public void UpdateCurrencies()
    {
        scoreValue.text = ItemManager.Instance.GetItemAmount("hempseeds").ToString();
        starValue.text = ItemManager.Instance.GetItemAmount("hempfibers").ToString();
        scoreValue.text = ItemManager.Instance.GetItemAmount("hemppebbles").ToString();
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
