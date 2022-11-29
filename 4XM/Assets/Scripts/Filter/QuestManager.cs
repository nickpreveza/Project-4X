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
        HB_UIManager.Instance.UpdateQuest();
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
                HB_EventManager.Instance.onEnemyKilled += OnEnemyKilled;
                break;
            case QuestListener.HempCollected:
                HB_EventManager.Instance.onHempCollected += OnHempCollected;
                break;
            case QuestListener.HempHarvested:
                HB_EventManager.Instance.onHempHarvested += OnHempHarvested;
                break;
            case QuestListener.HempPlanted:
                HB_EventManager.Instance.onHempPlanted += OnHempPlanted;
                break;
            case QuestListener.BulletsShot:
                HB_EventManager.Instance.onBulletsShot += OnBulletsShot;
                break;
            case QuestListener.WeaponEquipped:
                HB_EventManager.Instance.onWeaponEquipped += OnWeaponEquipped;
                break;
        }


        HB_UIManager.Instance.UpdateQuest();
    }

    void RemoveQuestListeners()
    {
        HB_EventManager.Instance.onHempHarvested -= OnHempHarvested;
        HB_EventManager.Instance.onHempPlanted -= OnHempPlanted;
        HB_EventManager.Instance.onHempCollected -= OnHempCollected;
        HB_EventManager.Instance.onEnemyKilled -= OnEnemyKilled;
        HB_EventManager.Instance.onBulletsShot -= OnBulletsShot;
        HB_EventManager.Instance.onWeaponEquipped -= OnWeaponEquipped;
    }
    void CheckIfQuestCompleted()
       {
        if (questCurrentAmount >= activeQuest.targetAmount)
        {
            activeQuestCompleted = true;
            HB_EventManager.Instance.OnQuestCompleted(activeQuest.key);
        }

        HB_UIManager.Instance.UpdateQuest();
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
