using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : Interactable
{
    [TextArea(15, 20)]
    [SerializeField] List<string> dialogContent = new List<string>();
    [SerializeField] Sprite characterPortrait;
    [SerializeField] string characterName;
    [SerializeField] ConversationType conversationType;
    [SerializeField] QuestSequence questSequence; //just keep the string key here
    [SerializeField] Interactable affectedInteractable;
    public bool questExists;
    public bool questGiven;
    public bool questCompleted;
    public bool rewardsGiven;
    public GameObject questIcon;
    //maybe custom background?
    //maybe custom sound?
    int questIndex = 0;
    private void Start()
    { 
        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (conversationType == ConversationType.Quest)
        {
            questIcon = transform.GetChild(0).gameObject;
            if (questSequence.quests[questIndex] != null)
            {
                if (HB_GameManager.Instance.questManager.DoesQuestExist(questSequence.quests[questIndex].quest.key))
                {
                    questExists = true;
                }
                else
                {
                    questExists = false;
                }
               
            }
            if (questExists && !questGiven)
            {
                if (affectedInteractable != null)
                {
                    affectedInteractable.isInteractable = false;
                }
                questIcon.SetActive(true);
            }
            else
            {
                if (affectedInteractable != null)
                {
                    affectedInteractable.isInteractable = true;
                }
                questIcon.SetActive(false);
            }
        }
    }

    public override void Interact()
    {
        if (!HB_UIManager.Instance.menuActive)
        {
            if (conversationType == ConversationType.Quest)
            {
                if (!questExists)
                {
                    return;
                }

                if (!questGiven)
                {
                    List<string> currentDialogueContent = new List<string>(questSequence.quests[questIndex].quest.questGiverDialogue);
                    HB_UIManager.Instance.StartTextbox(characterName, currentDialogueContent, conversationType, questSequence.quests[questIndex].quest);
                    HB_EventManager.Instance.onQuestCompleted += OnQuestCompleted;
                    questGiven = true;
                    if (affectedInteractable != null)
                    {
                        affectedInteractable.isInteractable = true;
                    }
                }
                else if (questGiven && !questCompleted)
                {
                    List<string> currentDialogueContent = new List<string>(questSequence.quests[questIndex].quest.reminderDialogue);
                    HB_UIManager.Instance.StartTextbox(characterName, currentDialogueContent, ConversationType.Normal);
                }
                else if (questCompleted)
                {
                    List<string> currentDialogueContent = new List<string>(questSequence.quests[questIndex].quest.questCompletedDialogue);
                    HB_UIManager.Instance.StartTextbox(characterName, currentDialogueContent, ConversationType.QuestReward, questSequence.quests[questIndex].quest);
                    HB_EventManager.Instance.onQuestRewarded += OnQuestRewarded;
                }
            }
            else
            {
                HB_UIManager.Instance.StartTextbox(characterName, dialogContent, conversationType);
                isInteractable = true;
            }
        }
        
    }

    public void OnQuestCompleted(string questKey)
    {
        if (questKey == questSequence.quests[questIndex].quest.key)
        {
            questCompleted = true;
            HB_EventManager.Instance.onQuestCompleted -= OnQuestCompleted;
        }
    }

    public void OnQuestRewarded(string questKey)
    {
        if (questKey == questSequence.quests[questIndex].quest.key)
        {
            List<GameObject> itemsToDrop = new List<GameObject>();

            if (questSequence.quests[questIndex].quest.questRewardItem.Length > 0)
            {
                for(int i = 0; i < questSequence.quests[questIndex].quest.questRewardItem.Length; i++)
                {
                    string itemRewardKey = questSequence.quests[questIndex].quest.questRewardItem[0];
                    int amountToDrop = questSequence.quests[questIndex].quest.questRewardAmount[0];
                    GameObject item = ItemManager.Instance.GetWorlditem(itemRewardKey);
                    item.GetComponent<ItemInteractable>().SetAmount(amountToDrop);
                    itemsToDrop.Add(item);
                }
            }

            foreach(GameObject obj in itemsToDrop)
            {
                Instantiate(obj, transform.position, Quaternion.identity);
            }
            
            //for now. This is a test.
            HB_EventManager.Instance.onQuestRewarded -= OnQuestRewarded;
            questIndex++;
            if (questIndex < questSequence.quests.Length)
            {
                questExists = false;
                questGiven = false;
                questCompleted = false;
                rewardsGiven = false;
                UpdateStatus();
                return;
            }

            //else destroy or move away or something
            Destroy(this.gameObject);

        }
    }
}

public enum ConversationType
{
    Normal,
    Choice,
    OpenBlacksmith,
    OpenCrafting,
    OpenCooking,
    OpenFactory,
    OpenQuestBoard,
    Quest,
    QuestReward
}
