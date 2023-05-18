using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using UnityEngine.UI;
using TMPro;

public class EndGamePanel : UIPanel
{
    
    [SerializeField] OverviewPanel overviewEnd;
    Player player;
    Color civColor;
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] Image logoImageToColor;
    public override void Activate()
    {
        base.Activate();
    }

    public override void Disable()
    {
        base.Disable();
    }

    public override void Setup()
    {
        base.Setup();
    }

    public override void UpdateData()
    {
        base.UpdateData();
    }

    public void ShowWinnerScore(Player player, bool isWinner = true)
    {
        SI_CameraController.Instance.GameEnded();
        civColor = GameManager.Instance.GetCivilizationByType(player.civilization).uiColorActive;
        logoImageToColor.color = civColor;
        overviewEnd.gameObject.SetActive(true);
        overviewEnd.OverviewSetup(true);

        winnerText.text = GameManager.Instance.GetCivilizationByType(player.civilization).name + " WIN";
        
    }

    public void OneMoreTurnButton()
    {
        GameManager.Instance.ReloadScene();
    }

    public void MainMenuButton()
    {
        GameManager.Instance.ReloadScene();
    }

    public void ItchButton()
    {
        Application.OpenURL(GameManager.Instance.data.itchURL);
    }

    public void FollowButton()
    {
        Application.OpenURL(GameManager.Instance.data.linktrURL);
    }

    public void ExitAction()
    {
        UIManager.Instance.OpenPopup(
            "QUIT GAME",
            "Are you sure you want to exit?",
            true,
            "exit",
            "cancel",
            () => GameManager.Instance.ApplicationQuit(), true);
    }
}
