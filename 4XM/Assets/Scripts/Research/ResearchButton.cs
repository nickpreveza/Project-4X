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
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] TextMeshProUGUI buttonCost;
    public void FetchData()
    {
        buttonText.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityName;

        if (GameManager.Instance.activePlayer.abilityDictionary[abilityID].canBePurchased)
        {
            if (GameManager.Instance.activePlayer.abilityDictionary[abilityID].hasBeenPurchased)
            {
                SetAsPurchased();
                return;
            }

            //mmove this to somethingi like CanActivePlayerAfford(value)
            if (GameManager.Instance.GetCurrentPlayerStars() >= GameManager.Instance.GetAbilityCost(abilityID))
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
        backgroundImage.color = UIManager.Instance.researchPurchased;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupAlreadyPurchased);
    }

    public void SetAsAvailable()
    {
        backgroundImage.color = UIManager.Instance.researchAvailable;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopup);
    }

    public void SetAsUnavailble()
    {
        backgroundImage.color = UIManager.Instance.researchUnavailable;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopup);
    }

    public void SetAsLocked()
    {
        backgroundImage.color = UIManager.Instance.researchLocked;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupLocked);
    }

    public void OpenPopupAlreadyPurchased()
    {

    }
    public void OpenPopup()
    {
        //remove this
        GameManager.Instance.UnlockAbility(abilityID);
    }

    public void OpenPopupLocked()
    {

    }

    public void AttemptPurchase()
    {

    }
}
