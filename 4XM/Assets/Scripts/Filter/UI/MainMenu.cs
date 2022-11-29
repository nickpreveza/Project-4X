using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
public class MainMenu : UIPanel
{
    [SerializeField] GameObject settingsPanel;
    public override void Activate()
    {
       
        base.Activate();
        settingsPanel.SetActive(false);
    }

    public override void Disable()
    {
        
        base.Disable();
    }

    public void ActionOpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void ActionCloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ActionPlay()
    {
        HB_GameManager.Instance.NewGame();
        HB_UIManager.Instance.OpenGamePanel();
    }
}
