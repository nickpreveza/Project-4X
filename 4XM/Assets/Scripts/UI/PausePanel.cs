using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using SignedInitiative;

public class PausePanel : UIPanel
{
    [Header("Subpanels")]
    [SerializeField] GameObject settings;
    [SerializeField] GameObject credits;
    [SerializeField] GameObject exit;


    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.pausePanel = this;
            UIManager.Instance.AddPanel(this);
        }

    }

    public override void Activate()
    {
        DepthOfField dof;
        if (GameManager.Instance.globalVolume.profile.TryGet<DepthOfField>(out dof))
        {
            dof.active = true;
        }
      
        base.Activate();
    }

    public override void Disable()
    {
        /*
        DepthOfField dof;
        if (globalVolume.profile.TryGet<DepthOfField>(out dof))
        {
            if (dof != null)
            dof.active = false;
        }*/
        base.Disable();
    }

    public void ActionResume()
    {
        GameManager.Instance.SetPause = false;
    }

    public void ActionExit()
    {
        UIManager.Instance.OpenMainMenu();
    }


    public void CreditsLink(int index)
    {
        switch (index)
        {
            case 1:
                Application.OpenURL("https://nickpreveza.itch.io/");
                break;
            case 2:
                
                break;
            case 3:
               
                break;
            case 4:
              
                break;
            case 5:
           
                break;
        }
    }
}
