using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public string key;
    public string questName;
    public string questDescription;
    
    [TextArea(5,10)]
    public List<string> questGiverDialogue;
    [TextArea(5, 10)]
    public List<string> questCompletedDialogue;
    [TextArea(5, 10)]
    public List<string> reminderDialogue;

    public QuestListener listenerType;
    public bool targetAmountVisible;
    public int targetAmount;
    public string[] questRewardItem;
    public int[] questRewardAmount;
}

public enum QuestListener
{
    EnemiesKilled,
    HempPlanted,
    HempHarvested,
    HempCollected,
    WeaponEquipped,
    BulletsShot,

}
