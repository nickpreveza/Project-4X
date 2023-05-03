using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchViewHandler : MonoBehaviour
{
    public List<ResearchButton> researchButtons = new List<ResearchButton>();
    public Dictionary<Abilities, ResearchButton> buttonsDictionary = new Dictionary<Abilities, ResearchButton>();
    void Start()
    {
        GenerateAbilitiesDictionary();
    }

    void GenerateAbilitiesDictionary()
    {
        foreach (ResearchButton button in researchButtons)
        {
            buttonsDictionary.Add(button.abilityID, button);
            button.FetchData();
        }
    }

    public void UpdateResearchButtons()
    {
        foreach (ResearchButton button in researchButtons)
        {
            button.FetchData();
        }
    }
}
