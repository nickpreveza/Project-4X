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
    public void OverviewSetup()
    {
        foreach(Transform child in entriesParent)
        {
            Destroy(child.gameObject);
        }

        foreach(Player player in GameManager.Instance.sessionPlayers)
        {
            GameObject obj = Instantiate(prefab, entriesParent);
            PlayerOverviewEntry entry = obj.GetComponent<PlayerOverviewEntry>();
            entry.SetPlayer(player);
            playerEntries.Add(entry);
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
