using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SignedInitiative;

public class PlayerOverviewEntry : MonoBehaviour
{
    Player associatedPlayer;
    [SerializeField] Image playerAvatar;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] TextMeshProUGUI developmentScore;
    [SerializeField] TextMeshProUGUI researchScore;
    [SerializeField] TextMeshProUGUI militaryScore;

    public void SetPlayer(Player player)
    {
        associatedPlayer = player;
        playerName.text = associatedPlayer.name;
        playerAvatar.color = GameManager.Instance.GetCivilizationColor(player.civilization, CivColorType.uiActiveColor);
        UpdateData();
    }
    public void UpdateData()
    {
        if (associatedPlayer == null)
        {
            Debug.Log("Player entry does not have a player yet");
            return;
        }

        developmentScore.text = associatedPlayer.developmentScore.ToString(); 
        researchScore.text = associatedPlayer.researchScore.ToString();
        militaryScore.text = associatedPlayer.militaryScore.ToString();
    }
}
