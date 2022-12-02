using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUpgradableUI : MonoBehaviour
{
    GameObject handler;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI currentStat;
    [SerializeField] TextMeshProUGUI upgradedStat;
    [SerializeField] int statLevel;
    [SerializeField] GameObject levelBarParent;
    [SerializeField] GameObject barPrefab;
    [SerializeField] Button upgradeButton;
    [SerializeField] Image currentyImage;
    [SerializeField] TextMeshProUGUI currencyText;

    Stat statData;
    WeaponScriptable targetWeapon;
    int upgradePrice;
    int[] weaponLevels;

    bool isBlacksmith;
    int associatedStatIndex; //until I think of something I'm blocked sorry 
    public void UpdateSkillTreeData(GameObject newHandler, Stat statToEdit, bool showAsPercentage, int _associatedStatIndex)
    {
        isBlacksmith = false;
        upgradeButton.onClick.RemoveAllListeners();
        handler = newHandler;
        statData = statToEdit;
        associatedStatIndex = _associatedStatIndex;
        title.text = statToEdit.statName;

        if (statData.CurrentLevelValue() < 1 && showAsPercentage)
        {
            currentStat.text = ((statData.CurrentLevelValue() * 100).ToString() + "%");
        }
        else
        {
            currentStat.text = statData.CurrentLevelValue().ToString();
        }

        if (statData.NextLevelValue() == -1)
        {
            upgradedStat.text = "";
            upgradeButton.interactable = false;
            currencyText.text = "MAX LVL";
            currentyImage.gameObject.SetActive(false);
        }
        else
        {
            currentyImage.gameObject.SetActive(true);
            if (statData.NextLevelValue() < 1 && showAsPercentage)
            {
                upgradedStat.text = "+" + ((statData.NextLevelValue() * 100).ToString() + "%");
            }
            else
            {
                upgradedStat.text = "+" + (statData.NextLevelValue().ToString());
            }

            upgradePrice = (int)statData.NextLevelPrice();

            currencyText.text = upgradePrice.ToString();

            if (ItemManager.Instance.DoesPlayerHaveItem("hempfibers", upgradePrice))
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
        }

        for (int i = 0; i < levelBarParent.transform.childCount; i++)
        {
            if (i <= statData.statLevel)
            {
                levelBarParent.transform.GetChild(i).GetComponent<Image>().color = SI_UIManager.Instance.skillbarEnabled;
            }
            else
            {
                levelBarParent.transform.GetChild(i).GetComponent<Image>().color = SI_UIManager.Instance.skillbarDisabled;
            }
        }

        upgradeButton.onClick.AddListener(() => UpgradeAction());

    }
    public void UpdateBlackmsithData(GameObject newHandler, WeaponScriptable newWeapon, Stat newStat, bool showAsPercentage)
    {
        isBlacksmith = true;
        upgradeButton.onClick.RemoveAllListeners();
        handler = newHandler;
        weaponLevels = null; // HB_PlayerController.Instance.weaponObject.weaponLevels;
        targetWeapon = newWeapon;
        statData = newStat;

        if (statData.LevelValue(weaponLevels[statData.statLevel]) < 1 && showAsPercentage)
        {
            currentStat.text = ((statData.LevelValue(weaponLevels[statData.statLevel]) * 100).ToString() + "%");
        }
        else
        {
            currentStat.text = statData.LevelValue(weaponLevels[statData.statLevel]).ToString();
        }

        if (statData.LevelValue(weaponLevels[statData.statLevel] + 1) == -1)
        {
            upgradedStat.text = "";
            upgradeButton.interactable = false;
            currencyText.text = "MAX LVL";
            currentyImage.gameObject.SetActive(false);
        }
        else
        {
            currentyImage.gameObject.SetActive(true);
            if (statData.LevelValue(weaponLevels[statData.statLevel] + 1) < 1 && showAsPercentage)
            {
                upgradedStat.text = "+" + ((statData.LevelValue(weaponLevels[statData.statLevel] + 1) * 100).ToString() + "%");
            }
            else
            {
                upgradedStat.text = "+" + (statData.LevelValue(weaponLevels[statData.statLevel] + 1).ToString());
            }

            upgradePrice = (int)statData.LevelPrice(weaponLevels[statData.statLevel] + 1);

            currencyText.text = upgradePrice.ToString();
            if (ItemManager.Instance.DoesPlayerHaveItem("hempseeds", upgradePrice))
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
        }

        for(int i = 0; i < levelBarParent.transform.childCount; i++)
        {
            if (i <= weaponLevels[statData.statLevel])
            {
                levelBarParent.transform.GetChild(i).GetComponent<Image>().color = SI_UIManager.Instance.skillbarEnabled;
            }
            else
            {
                levelBarParent.transform.GetChild(i).GetComponent<Image>().color = SI_UIManager.Instance.skillbarDisabled;
            }
        }

        upgradeButton.onClick.AddListener(() => UpgradeAction());

    }

    public void UpgradeAction()
    {
        if (isBlacksmith)
        {
            if (ItemManager.Instance.DoesPlayerHaveItem("hempseeds", upgradePrice))
            {
                ItemManager.Instance.UseItem("hempseeds", upgradePrice, false);
                weaponLevels[statData.statLevel]++;
               // HB_PlayerController.Instance.UpdateWeaponLevels(weaponLevels);
                //handler.GetComponent<BlacksmithUI>().UpdateWeaponUpgrades();
            }
            else
            {
                //Fail, so show something in the UI
                Debug.LogWarning("Not enough hemp for Weapon Upgrade");
            }
        }
        else
        {
            if (ItemManager.Instance.DoesPlayerHaveItem("hempfibers", upgradePrice))
            {
                ItemManager.Instance.UseItem("hempfibers", upgradePrice, false);
                //apply effects
               // HB_PlayerController.Instance.UpgradeStatLevel(associatedStatIndex);
               // handler.GetComponent<CookingUI>().UpdateSkills();
            }
            else
            {
                //Fail, so show something in the UI
                Debug.LogWarning("Not enough hempFibers for Skill Upgrade");
            }
           
        }

    }
}

