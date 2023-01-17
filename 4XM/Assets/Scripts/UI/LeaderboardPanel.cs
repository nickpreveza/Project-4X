using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

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
        UIManager.Instance.OpenMainMenu();
    }
}
