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
        backgroundImage.color = UIManager.Instance.researchPurchased;
        buttonCost.text = "";
        buttonText.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupAlreadyPurchased);
    }

    public void SetAsAvailable()
    {
        backgroundImage.color = UIManager.Instance.researchAvailable;
        buttonCost.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityCost.ToString();
        buttonText.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopup);
    }

    public void SetAsUnavailble()
    {
        backgroundImage.color = UIManager.Instance.researchUnavailable;
        buttonCost.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityCost.ToString();
        buttonCost.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopup);
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

    }
    public void OpenPopup()
    {
        //remove this
        GameManager.Instance.RemoveStars(fetchedAbilityCost);
        GameManager.Instance.UnlockAbility(abilityID);
 
    }

    public void OpenPopupLocked()
    {

    }

    public void AttemptPurchase()
    {

    }
}
