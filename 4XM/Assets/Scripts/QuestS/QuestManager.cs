using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public bool hasActiveQuest;
    public Quest activeQuest;
    public QuestScriptable[] scriptableQuests;
    public Dictionary<string, Quest> questDatabase = new Dictionary<string, Quest>();
    public Dictionary<string, Quest> completedQuestsDatabse = new Dictionary<string, Quest>();

    public int questCurrentAmount;
    public bool activeQuestCompleted;
    private void Awake()
    {
        foreach(QuestScriptable scriptable in scriptableQuests)
        {
            questDatabase.Add(scriptable.quest.key, scriptable.quest);
        }
    }

    public void RewardsGiven()
    {
        RemoveQuestListeners();
        hasActiveQuest = false;
        //SI_UIManager.Instance.UpdateQuest();
    }

    public void SetActiveQuest(Quest newQuest)
    {
        if (hasActiveQuest)
        {
            return;
        }

        activeQuest = newQuest;
        activeQuestCompleted = false;
        hasActiveQuest = true;
        questCurrentAmount = 0;
        RemoveQuestListeners();

        switch (activeQuest.listenerType)
        {
            case QuestListener.EnemiesKilled:
                SI_EventManager.Instance.onUnitKilled += OnEnemyKilled;
                break;
            case QuestListener.HempCollected:
                //SI_EventManager.Instance.onHempCollected += OnHempCollected;
                break;
            case QuestListener.HempHarvested:
                //SI_EventManager.Instance.onHempHarvested += OnHempHarvested;
                break;
            case QuestListener.HempPlanted:
                //SI_EventManager.Instance.onHempPlanted += OnHempPlanted;
                break;
            case QuestListener.BulletsShot:
               // SI_EventManager.Instance.onBulletsShot += OnBulletsShot;
                break;
            case QuestListener.WeaponEquipped:
               // SI_EventManager.Instance.onWeaponEquipped += OnWeaponEquipped;
                break;
        }


        //SI_UIManager.Instance.UpdateQuest();
    }

    void RemoveQuestListeners()
    {
        /*
        SI_EventManager.Instance.onHempHarvested -= OnHempHarvested;
        SI_EventManager.Instance.onHempPlanted -= OnHempPlanted;
        SI_EventManager.Instance.onHempCollected -= OnHempCollected;
        SI_EventManager.Instance.onEnemyKilled -= OnEnemyKilled;
        SI_EventManager.Instance.onBulletsShot -= OnBulletsShot;
        SI_EventManager.Instance.onWeaponEquipped -= OnWeaponEquipped;
        */
    }
    void CheckIfQuestCompleted()
       {
        if (questCurrentAmount >= activeQuest.targetAmount)
        {
            activeQuestCompleted = true;
            SI_EventManager.Instance.OnQuestCompleted(activeQuest.key);
        }

        //SI_UIManager.Instance.UpdateQuest();
    }

    void OnBulletsShot()
    {
        questCurrentAmount++;
        CheckIfQuestCompleted();
    }

    void OnWeaponEquipped()
    {
        questCurrentAmount++;
        CheckIfQuestCompleted();
    }
    void OnHempHarvested()
    {
        questCurrentAmount++;
        CheckIfQuestCompleted();
    }

    void OnHempCollected()
    {
        questCurrentAmount++;
        CheckIfQuestCompleted();
    }

    void OnHempPlanted()
    {
        questCurrentAmount++;
        CheckIfQuestCompleted();
    }

    void OnEnemyKilled()
    {
        questCurrentAmount++;
        CheckIfQuestCompleted();
    }

    public bool DoesQuestExist(string questKey)
    {
        if (questDatabase.ContainsKey(questKey))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasQuestBeenCompleted(string questKey)
    {
        if (completedQuestsDatabse.ContainsKey(questKey))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
