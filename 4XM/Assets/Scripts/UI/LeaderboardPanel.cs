using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardPanel : UIPanel
{
    public override void Activate()
    {

        base.Activate();
    }

    public override void Disable()
    {

        base.Disable();
    }

    public void BackToMenu()
    {
        SI_UIManager.Instance.OpenMainMenu();
    }
}
