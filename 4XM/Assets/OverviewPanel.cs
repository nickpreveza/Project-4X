using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SignedInitiative;

public class OverviewPanel : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] Transform entriesParent;
    List<PlayerOverviewEntry> playerEntries = new List<PlayerOverviewEntry>();
    Dictionary<Player, int> rankedPlayers = new Dictionary<Player, int>();

    public void OverviewSetup(bool rankInScoreOrder = false)
    {
        foreach(Transform child in entriesParent)
        {
            Destroy(child.gameObject);
        }

        if (rankInScoreOrder)
        {
            foreach (Player player in GameManager.Instance.rankedPlayers)
            {
                GameObject obj = Instantiate(prefab, entriesParent);
                PlayerOverviewEntry entry = obj.GetComponent<PlayerOverviewEntry>();
                entry.SetPlayer(player);
                playerEntries.Add(entry);
            }
        }
        else
        {
            foreach (Player player in GameManager.Instance.sessionPlayers)
            {
                GameObject obj = Instantiate(prefab, entriesParent);
                PlayerOverviewEntry entry = obj.GetComponent<PlayerOverviewEntry>();
                entry.SetPlayer(player);
                playerEntries.Add(entry);
            }
        }
    }

    public void UpdateEntries()
    {
        foreach(PlayerOverviewEntry entry in playerEntries)
        {
            entry.UpdateData();
        }
    }

}
