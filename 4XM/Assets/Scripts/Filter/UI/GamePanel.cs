using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePanel : UIPanel
{
    [SerializeField] HealthBar healthBar;
    [SerializeField] HealthBar armorBar;
    [SerializeField] TextMeshProUGUI hempValue;
    [SerializeField] TextMeshProUGUI fibersValue;
    [SerializeField] TextMeshProUGUI ammoValue;
    [SerializeField] TextMeshProUGUI scoreText;
    public TextMeshProUGUI devText;
    [SerializeField] TextMeshProUGUI waveCount;
    [SerializeField] TextMeshProUGUI waveRemaining;

    public Image waveGraphic;
    public Image fillMeter;
    public Image fakeFill;

    public Color normalColor;
    public Color waveColor;

    [Range(0,1)]
    [SerializeField] float fillAmount;
    public Gradient gradient;

    public Toggle inventoryPrompt;
    public Textbox textbox;

    [Header("Quest")]
    [SerializeField] GameObject questObject;
    [SerializeField] TextMeshProUGUI questTitle;
    [SerializeField] TextMeshProUGUI questDescription;
    [SerializeField] int questCurrentAmount;
    [SerializeField] int questTargetAmount;
    [SerializeField] GameObject questCompletedSprite;

    [Header("Quick Slots")]
    [SerializeField] GameObject equipableParent;

    Color ColorFromGradient(float value)  // float between 0-1
    {
        return gradient.Evaluate(value);
    }

    public float SetFillAmount
    {
        get
        {
            return fillAmount;
        }
        set
        {
            fillMeter.color = ColorFromGradient(fillAmount);
            fakeFill.color = fillMeter.color;
            fillAmount = Mathf.Clamp(value, 0, 1);
        }

        

    }
    void Start()
    {
      
        if (HB_UIManager.Instance != null)
        {
            HB_UIManager.Instance.gamePanel = this.GetComponent<UIPanel>();
            HB_UIManager.Instance.AddPanel(this);
        }
        textbox.EndTextbox();
        questObject.SetActive(false);
        ClearQuickSlots();
    }

    public void EquipItemFromPeakInventory(int index)
    {
        ItemUI item = equipableParent.transform.GetChild(index).GetComponent<ItemUI>();
        if (item != null)
        {
            if (item.data.isUnlocked)
            {
                item.Interact();
            }
        }
    }

    public void ClearQuickSlots()    
    {
        foreach (Transform child in equipableParent.transform)
        {
            child.GetComponent<ItemUI>().UpdateData(this.gameObject, false);
        }
    }

    public void UpdateScore()
    {
        scoreText.text = "Score \n" + HB_PlayerController.Instance.data.hbScore.ToString();
    }

    public void UpdateQuest()
    {
        if (HB_GameManager.Instance.questManager.hasActiveQuest)
        {
            questObject.SetActive(true);
            questTitle.text = HB_GameManager.Instance.questManager.activeQuest.questName;
            questDescription.text = HB_GameManager.Instance.questManager.activeQuest.questDescription;

            questCurrentAmount = HB_GameManager.Instance.questManager.questCurrentAmount;
            questTargetAmount = HB_GameManager.Instance.questManager.activeQuest.targetAmount;

            if (HB_GameManager.Instance.questManager.activeQuest.targetAmountVisible)
            {
                questDescription.text += "(" + questCurrentAmount + "/" + questTargetAmount + ") ";
            }
           
            if (HB_GameManager.Instance.questManager.activeQuestCompleted)
            {
                questCompletedSprite.SetActive(true);
            }
            else
            {
                questCompletedSprite.SetActive(false);
            }
        }
        else
        {
            questObject.SetActive(false);
        }
    }
    public void UpdateCurrencies()
    {
        hempValue.text = ItemManager.Instance.GetItemAmount("hempseeds").ToString();
        fibersValue.text = ItemManager.Instance.GetItemAmount("hempfibers").ToString();
        ammoValue.text = ItemManager.Instance.GetItemAmount("hemppebbles").ToString();
    }

    private void Update()
    {
        if (WaveManager.Instance != null)
        {
            waveCount.text = "Wave: " + WaveManager.Instance.arcadeWaveIndex.ToString();

            switch (WaveManager.Instance.state)
            {
                case WaveManager.SpawnState.INACTIVE:
                    //waveRemaining.transform.parent.gameObject.SetActive(false);
                    break;
                case WaveManager.SpawnState.ACTIVE:
                    waveRemaining.text = "Enemies Remaining : " + WaveManager.Instance.enemiesRemaining.ToString();
                    break;
                case WaveManager.SpawnState.COUNTING:
                    waveRemaining.text = "Next Wave : " + WaveManager.Instance.waveCountdown.ToString("N0");
                    break;
                case WaveManager.SpawnState.SPAWNING:
                    waveRemaining.text = "Enemies Remaining : " + WaveManager.Instance.enemiesRemaining.ToString();
                    break;
            }
        }
       
        if (HB_GameManager.Instance.devMode)
        {
            devText.text = "Health: " + HB_PlayerController.Instance.Health;
            devText.text += "\n DevMode Enabled";
            // devText.text += "enemies remaining: " + HB_GameManager.Instance.spawner.enemiesRemaining;
            if (HB_GameManager.Instance.infiniteHealth)
            {
                devText.text += "\n infiniteHealth = True";
            }
            else
            {
                devText.text += "\n infiniteHealth = False";
            }

            if (WaveManager.Instance != null)
            {
                devText.text += "\n Wave Duration : " + WaveManager.Instance.waveDuration;
                devText.text += "\n Wave Countdown : " + WaveManager.Instance.waveCountdown;
                devText.text += "\n Total Enemies : " + WaveManager.Instance.totalEnemies;
                devText.text += "\n Enemies Remaining : " + WaveManager.Instance.enemiesRemaining;
                devText.text += "\n Enemies To Spawn : " + WaveManager.Instance.enemiesToSpawn;
            }
        }
        else
        {
            devText.text = "";
        }


        /*

         
         /*
         if (HB_GameManager.Instance.spawner != null)
         {
             switch (HB_GameManager.Instance.spawner.state)
             {
                 case WaveHandler.SpawnState.INACTIVE:
                     waveHeader.text = "WAVE";
                     waveCounter.text = (HB_GameManager.Instance.spawner.waveIndex + 1).ToString() + "/" + (HB_GameManager.Instance.spawner.totalWavesInDay).ToString();
                     waveGraphic.color = normalColor;
                     break;
                 case WaveHandler.SpawnState.COUNTING:
                     waveHeader.text = "NEXT:";
                     waveCounter.text = ((int)HB_GameManager.Instance.spawner.waveCountdown).ToString();
                     waveGraphic.color = waveColor;
                     break;
                 case WaveHandler.SpawnState.ACTIVE:
                     waveHeader.text = "WAVE";
                     waveCounter.text = (HB_GameManager.Instance.spawner.waveIndex + 1).ToString() + "/" + (HB_GameManager.Instance.spawner.totalWavesInDay).ToString();
                     waveGraphic.color = normalColor;
                     break;
                 case WaveHandler.SpawnState.SPAWNING:
                     waveHeader.text = "WAVE";
                     waveCounter.text = (HB_GameManager.Instance.spawner.waveIndex + 1).ToString() + "/" + (HB_GameManager.Instance.spawner.totalWavesInDay).ToString();
                     waveGraphic.color = normalColor;
                     break;
             }


         }
         else
         {
             waveHeader.text = "";
             waveCounter.text = "";
         } */

    }

    public void UpdateHealth(bool skipAnimation)
    {
        healthBar.UpdateHealthbar(HB_PlayerController.Instance.Health, skipAnimation);
    }

    public override void Setup()
    {
        base.Setup();

        /*
        for (int i = 1; i < healthHolder.transform.childCount; i++)
        {
            Destroy(healthHolder.transform.GetChild(i).gameObject);
        }

        healthVessel = new List<GameObject>();
        halfHearts = new List<GameObject>();

        for (int i = 0; i < (int)(HB_PlayerController.Instance.startingHealth / 2); i++)
        {
            GameObject obj = Instantiate(healthPrefab, healthHolder.transform);
            healthVessel.Add(obj);
            halfHearts.Add(obj.transform.GetChild(0).gameObject);
            halfHearts.Add(obj.transform.GetChild(1).gameObject);
        } */
        
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
