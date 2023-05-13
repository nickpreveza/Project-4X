using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SignedInitiative;
public class InitializerPanel : UIPanel
{
    [SerializeField] GameObject settingsPanel;
    [SerializeField] InputField seedField;
    public override void UpdateData()
    {
       
    }

    public override void Setup()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenLinkTreeAction()
    {
        Application.OpenURL(GameManager.Instance.data.linktrURL);
    }

    public void StartGameAction()
    {
        Initializer.Instance.LocalStart();
    }

    public void ToggleSettingsAction()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
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
