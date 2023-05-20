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
    [SerializeField] TextMeshProUGUI totalScore;
    [SerializeField] TextMeshProUGUI developmentScore;
    [SerializeField] TextMeshProUGUI researchScore;
    [SerializeField] TextMeshProUGUI militaryScore;
    [SerializeField] TextMeshProUGUI rank;
    [SerializeField] Image rankImage;
    [SerializeField] GameObject deathImage;
    public void SetPlayer(Player player)
    {
        associatedPlayer = player;
        playerName.text = GameManager.Instance.GetCivilizationByType(associatedPlayer.civilization).name;
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
        int rankNum = GameManager.Instance.rankedPlayers.IndexOf(associatedPlayer)  + 1;
        rank.text = rankNum.ToString();

        if (rankNum == 1)
        {
            rankImage.sprite = UIManager.Instance.rankedTopImage;
        }
        else
        {
            rankImage.sprite = UIManager.Instance.rankedSecondImage;
        }

        if (associatedPlayer.isAlive)
        {
            deathImage.SetActive(false);
        }
        else
        {
            deathImage.SetActive(true);
        }
        totalScore.text = associatedPlayer.totalScore.ToString();
        developmentScore.text = associatedPlayer.developmentScore.ToString(); 
        researchScore.text = associatedPlayer.researchScore.ToString();
        militaryScore.text = associatedPlayer.militaryScore.ToString();
    }
}
