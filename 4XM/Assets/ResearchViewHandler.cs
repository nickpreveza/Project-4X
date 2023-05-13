using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public class ResearchViewHandler : MonoBehaviour
{
    public List<ResearchButton> researchButtons = new List<ResearchButton>();
    public Dictionary<Abilities, ResearchButton> buttonsDictionary = new Dictionary<Abilities, ResearchButton>();
    bool hasAbilitiesDictionaryBeenCreated;

    void GenerateAbilitiesDictionary()
    {
        foreach (ResearchButton button in researchButtons)
        {
            buttonsDictionary.Add(button.abilityID, button);
            button.FetchData();
        }
    }

    public ResearchButton GetButtonByAbility(Abilities ability)
    {
        if (buttonsDictionary.ContainsKey(ability))
        {
            return buttonsDictionary[ability];
        }
        else
        {
            return null;
        }
    }

    public void UpdateResearchButtons()
    {
        if (!GameManager.Instance.abilitiesDicitionariesCreated)
        {
            return;
        }

        if (!hasAbilitiesDictionaryBeenCreated)
        {
            foreach (ResearchButton button in researchButtons)
            {
                buttonsDictionary.Add(button.abilityID, button);
            }
            hasAbilitiesDictionaryBeenCreated = true;
        }

        foreach (ResearchButton button in researchButtons)
        {
            button.FetchData();
        }
    }
}
