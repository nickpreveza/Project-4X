using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SignedInitiative;
using LeTai.TrueShadow.Demo;

public class CivSelectButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI civName;
    [SerializeField] TextMeshProUGUI civDescription;
    [SerializeField] Image civLogo;
    [SerializeField] Image civBackground;
    Button select;
    [SerializeField] Toggle isLocal;
    [SerializeField] GameObject removeButton;
    int playerIndex;
    InitializerPanel handler;
    Civilizations civType;
    bool overrideCivSettings;

    public static string SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    public void SetUpAsSelection(InitializerPanel master, Civilization civ, int _playerIndex, bool _overrideCivSettings)
    {
        overrideCivSettings = _overrideCivSettings;
        select = GetComponent<Button>();
        handler = master;
        playerIndex = _playerIndex;
        civLogo.sprite= civ.civLogo;
        civName.text = civ.name;
        civType = civ.civType;
        civDescription.text = civ.civilizationIntro + "\n\nStarts with the " + SplitCamelCase(civ.startingAbility.ToString()) + " ability. ";
        civBackground.color = GameManager.Instance.GetCivilizationColor(civ.civType, CivColorType.uiActiveColor);
        //select.onClick.AddListener(SelectWhenSelection);
    }

    public void SelectWhenSelection()
    {
        SI_AudioManager.Instance.Play(SI_AudioManager.Instance.characterSelected);
        handler.SelectCiv(playerIndex, civType, overrideCivSettings);
    }

    public void SetupAsOverview(InitializerPanel master, Player player, int _playerIndex)
    {
        select = GetComponent<Button>();
        handler = master;
        Civilization civ = GameManager.Instance.GetCivilizationByType(player.civilization);
        playerIndex = _playerIndex;
        civLogo.sprite = civ.civLogo;
        civName.text = civ.name;
        civType = civ.civType;
        civBackground.color = GameManager.Instance.GetCivilizationColor(civ.civType, CivColorType.uiActiveColor);
        select.onClick.AddListener(() => handler.SetupSelectYourCiv(playerIndex, true));
        if (Initializer.Instance.setupPlayers[playerIndex].type == PlayerType.AI)
        {
            isLocal.isOn = false;

           // isLocal.isOn = false;
           ToggleSwitchAnimation anim = isLocal.GetComponent<ToggleSwitchAnimation>();
            anim.Setup();

            isLocal.onValueChanged.AddListener(ToggleChanged);
            isLocal.onValueChanged.AddListener(anim.Toggle);
        }
        else
        {
          isLocal.isOn = true;
          
           ToggleSwitchAnimation anim = isLocal.GetComponent<ToggleSwitchAnimation>();
            anim.Setup();

            isLocal.onValueChanged.AddListener(ToggleChanged);
            isLocal.onValueChanged.AddListener(anim.Toggle);
            
        }
        if (playerIndex > 1)
        {
            removeButton.SetActive(true);
        }
        else
        {
            removeButton.SetActive(false);
        }

        if (playerIndex == 0)
        {
            isLocal.gameObject.SetActive(false);
        }
    }

    public void ToggleChanged(bool isOn)
    {
        
        handler?.ToggleChanged(playerIndex, isOn);
    }

    public void RemoveEntry()
    {
        handler.RemoveSetupPlayer(playerIndex);
    }
}
