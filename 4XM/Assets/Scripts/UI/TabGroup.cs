using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TabGroup : MonoBehaviour
{
    public List<TabGroupButton> tabButtons;

    [SerializeField] Sprite tabIdle;
    [SerializeField] Sprite tabHover;
    [SerializeField] Sprite tabActive;

    TabGroupButton selectedTab;
    [SerializeField] List<GameObject> tabsToSwap;
    [SerializeField] TextMeshProUGUI tabTitle;
    [SerializeField] TabGroupButton defaultTab;
    public void Subscribe(TabGroupButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabGroupButton>();
        }

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabGroupButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            button.SetBackground(tabHover);
        }
    }

    public void OntabExit(TabGroupButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabGroupButton button)
    {
        if (selectedTab != null)
        {
            selectedTab.Deselect();
        }
        selectedTab = button;
        selectedTab.Select();
        tabTitle.text = selectedTab.tabTitle;
        ResetTabs();
        button.SetBackground(tabActive);
        int index = button.transform.GetSiblingIndex();
        for(int i = 0; i < tabsToSwap.Count; i++)
        {
            if (i == index)
            {
                tabsToSwap[i].SetActive(true);
            }
            else
            {
                tabsToSwap[i].SetActive(false);
            }
           
        }
    }

    public void SetDefault()
    {
        OnTabSelected(defaultTab);
    }

    public void ResetTabs()
    {
        foreach(TabGroupButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }
          
            button.SetBackground(tabIdle);
        }
    }
}
