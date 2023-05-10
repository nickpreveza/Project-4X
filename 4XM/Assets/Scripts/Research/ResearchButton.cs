using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SignedInitiative;

public class ResearchButton: MonoBehaviour
{
    public Abilities abilityID;
    [SerializeField] Button button;
    [SerializeField] Image backgroundImage;
    [SerializeField] GameObject costIcon;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] TextMeshProUGUI buttonCost;
    int fetchedAbilityCost;
    public void FetchData()
    {
        buttonText.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityName;
        fetchedAbilityCost = GameManager.Instance.GetCurrentPlayerAbilityCost(abilityID);

        if (GameManager.Instance.IsAbilityUnlocked(abilityID))
        {
            if (GameManager.Instance.isAbilityPurchased(abilityID))
            {
                SetAsPurchased();
                return;
            }

            //mmove this to somethingi like CanActivePlayerAfford(value)
            if (GameManager.Instance.CanActivePlayerAffordAbility(abilityID))
            {
                SetAsAvailable();
            }
            else
            {
                SetAsUnavailble();
            }
            
        }
        else
        {
            SetAsLocked();
        }
    }

    public void SetAsPurchased()
    {
        costIcon.SetActive(false);
        backgroundImage.color = UIManager.Instance.researchPurchased;
        buttonCost.text = "";
        buttonText.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupAlreadyPurchased);
    }

    public void SetAsAvailable()
    {
        costIcon.SetActive(true);
        backgroundImage.color = UIManager.Instance.researchAvailable;
        buttonCost.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityCost.ToString();
        buttonText.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OpenPopup(true));
    }

    public void SetAsUnavailble()
    {
        costIcon.SetActive(true);
        backgroundImage.color = UIManager.Instance.researchUnavailable;
        buttonCost.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityCost.ToString();
        buttonCost.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>OpenPopup(false));
    }

    public void SetAsLocked()
    {
        backgroundImage.color = UIManager.Instance.researchLocked;
        buttonCost.text = "";
        buttonText.color = Color.white;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupLocked);
    }

    public void OpenPopupAlreadyPurchased()
    {
        UIManager.Instance.OpenPopup(
           GameManager.Instance.abilitiesDictionary[abilityID].abilityName,
           GameManager.Instance.abilitiesDictionary[abilityID].abilityDescription,
           false,
           "Ok",
           "Ok",
            ()=> EmptyStatement(), false);
    }
    public void OpenPopup(bool available)
    {
        UIManager.Instance.OpenPopup(
            GameManager.Instance.abilitiesDictionary[abilityID].abilityName,
            GameManager.Instance.abilitiesDictionary[abilityID].abilityDescription,
            available,
            "Research",
            "Cancel",
            () => GameManager.Instance.UnlockAbility(GameManager.Instance.activePlayerIndex, abilityID, true, true), true);
    }

    void EmptyStatement()
    {

    }
    public void OpenPopupLocked()
    {
        UIManager.Instance.OpenPopup(
           GameManager.Instance.abilitiesDictionary[abilityID].abilityName,
           GameManager.Instance.abilitiesDictionary[abilityID].abilityDescription,
           false,
           "Ok",
           "Ok",
            () => EmptyStatement(), false);
    }

}
