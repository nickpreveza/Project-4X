using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Textbox : MonoBehaviour
{
    bool textboxActive;
    string characterName;
    [SerializeField] GameObject textbox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI promptText;
    bool nextTextAvailable;
    List<string> dialogContent = new List<string>();
    ConversationType convoType;
    //
    //faces, temporary solution
    [SerializeField] GameObject Blacksmith;
    [SerializeField] GameObject Fortune;
    [SerializeField] GameObject Rex;

    Quest currentTextboxQuest;
    bool hasOpenedMenu;
    private void Update()
    {
        if (textboxActive && !HB_GameManager.Instance.isPaused)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F)))
            {
                if (nextTextAvailable)
                {
                    NextTextboxDialog();
                }
                else
                {
                    EndTextbox();
                }
            }
        }
    }
    public void StartTextbox(string _characterName, List<string> _dialogContent, ConversationType _convoType, Quest quest =null)
    {
        if (textboxActive) { return; }
        HB_UIManager.Instance.menuActive = true;
        if (characterName != _characterName)
        {
            characterName = _characterName;
            SwitchFace();
        }
        convoType = _convoType;
        nameText.text = characterName;

        dialogContent = new List<string>(_dialogContent);

        if (dialogContent == null && dialogContent.Count < 1)
        {
            Debug.LogWarning("Textbox cannot be displayed as there is no dialog");
            return;
        }

        promptText.text = dialogContent[0];

        if (dialogContent.Count > 1)
        {
            nextTextAvailable = true;
        }
        hasOpenedMenu = false;
        textboxActive = true;
        if (quest != null)
        {
            currentTextboxQuest = quest;
        }
        else
        {
            currentTextboxQuest = null;
        }
        textbox.SetActive(true);
    }

    public void SwitchFace()
    {
        Fortune.SetActive(false);
        Rex.SetActive(false);
        Blacksmith.SetActive(false);

        switch (characterName)
        {
            case "Blacksmith":
                Blacksmith.SetActive(true);
                break;
            case "FortuneTeller":
                Fortune.SetActive(true);
                break;
            case "Rex":
                Rex.SetActive(true);
                break;
        }
    }

    public void EndTextbox()
     {
        HB_UIManager.Instance.menuActive = false;
        textboxActive = false;
        textbox.SetActive(false);
        if (!hasOpenedMenu) //why is this here wtf
        {
            switch (convoType)
            {
                case ConversationType.Normal:
                case ConversationType.Choice:
                    HB_PlayerController.Instance.controlsActive = true;
                    break;
                case ConversationType.OpenCrafting:
                case ConversationType.OpenFactory:
                    HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.factory, true);
                    break;
                case ConversationType.OpenBlacksmith:
                    HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.blacksmith, true);
                    hasOpenedMenu = true;
                    break;
                case ConversationType.OpenCooking:
                    HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.cooking, true);
                    hasOpenedMenu = true;
                    break;
                case ConversationType.OpenQuestBoard:
                    HB_UIManager.Instance.ToggleUIPanel(HB_UIManager.Instance.questBoard, true);
                    break;
                case ConversationType.Quest:
                    if (currentTextboxQuest != null)
                    {
                        HB_GameManager.Instance.questManager.SetActiveQuest(currentTextboxQuest);
                    }
                    HB_PlayerController.Instance.controlsActive = true;
                    break;
                case ConversationType.QuestReward:

                    //callback to the NPC to drop stuff based on quest rewards
                    HB_GameManager.Instance.questManager.RewardsGiven();
                    if (currentTextboxQuest != null)
                    {
                        HB_EventManager.Instance.OnQuestRewarded(currentTextboxQuest.key);
                    }
                    HB_PlayerController.Instance.controlsActive = true;
                    break;

            }
        }
    }

    void NextTextboxDialog()
    {
        dialogContent.RemoveAt(0);

        promptText.text = dialogContent[0];
        if (dialogContent.Count > 1)
        {
            nextTextAvailable = true;
        }
        else
        {
            nextTextAvailable = false;
        }
        textbox.SetActive(true);
    }
}
