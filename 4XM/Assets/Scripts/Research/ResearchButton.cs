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
    [SerializeField] GameObject purchasedIcon;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] TextMeshProUGUI buttonCost;
    [SerializeField] GameObject lockImage;
    [SerializeField] GameObject highlightImage;

    int fetchedAbilityCost;

    private void Start()
    {
        highlightImage.SetActive(false);
    }
    public void FetchData()
    {
        buttonText.text = GameManager.Instance.abilitiesDictionary[abilityID].abilityName;
        fetchedAbilityCost = GameManager.Instance.GetAbilityCost(GameManager.Instance.activePlayer.index, abilityID);
        buttonCost.text = fetchedAbilityCost.ToString();

        if (GameManager.Instance.IsAbilityUnlocked(GameManager.Instance.activePlayer.index, abilityID))
        {
            if (GameManager.Instance.IsAbilityPurchased(GameManager.Instance.activePlayer.index, abilityID))
            {
                SetAsPurchased();
                return;
            }

            //mmove this to somethingi like CanActivePlayerAfford(value)
            if (GameManager.Instance.CanPlayerAffordAbility(GameManager.Instance.activePlayer.index, abilityID))
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

    public void OpenHighlight()
    {
        highlightImage.SetActive(true);
        Invoke("CloseHighlight", 1f);
    }

    void CloseHighlight()
    {
        highlightImage.SetActive(false);
    }

    public void SetAsPurchased()
    {
        costIcon.SetActive(false);
        lockImage.SetActive(false);
        purchasedIcon.SetActive(true);
        backgroundImage.color = UIManager.Instance.researchPurchased;
        //buttonCost.text = "";
        //buttonText.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupAlreadyPurchased);
    }

    public void SetAsAvailable()
    {
        purchasedIcon.SetActive(false);
        costIcon.SetActive(true);
        lockImage.SetActive(false);
        backgroundImage.color = UIManager.Instance.researchAvailable;
        buttonCost.color = UIManager.Instance.researchAvailable;
        costIcon.GetComponent<Image>().color = UIManager.Instance.researchAvailable;
        buttonCost.text = fetchedAbilityCost.ToString();
        //buttonText.color = Color.white;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OpenPopup(true));
    }

    public void SetAsUnavailble()
    {
        purchasedIcon.SetActive(false);
        costIcon.SetActive(true);
        lockImage.SetActive(false);
        backgroundImage.color = UIManager.Instance.researchUnavailable;
        buttonCost.color = UIManager.Instance.researchUnavailable;
        buttonCost.text = fetchedAbilityCost.ToString();
        //buttonCost.color = Color.black;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>OpenPopup(false));
    }

    public void SetAsLocked()
    {
        purchasedIcon.SetActive(false);
        costIcon.SetActive(false);
        lockImage.SetActive(true);
        backgroundImage.color = UIManager.Instance.researchLocked;
        buttonCost.text = "";
        buttonText.color = Color.white;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopupLocked);
    }

    public void OpenPopupAlreadyPurchased()
    {
        SI_AudioManager.Instance.PlayClick();
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
        SI_AudioManager.Instance.PlayClick();
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
        SI_AudioManager.Instance.PlayClick();
        UIManager.Instance.OpenPopup(
           GameManager.Instance.abilitiesDictionary[abilityID].abilityName,
           GameManager.Instance.abilitiesDictionary[abilityID].abilityDescription,
           false,
           "Ok",
           "Ok",
            () => EmptyStatement(), false);
    }

}
