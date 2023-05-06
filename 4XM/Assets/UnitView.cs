using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SignedInitiative;

public class UnitView : MonoBehaviour
{
    WorldUnit parentUnit;

    //[SerializeField] TextMeshProUGUI unitName;
    [SerializeField] TextMeshProUGUI unitHealth;
    [SerializeField] Image unitBackground;
    [SerializeField] Image unitIcon;
    [SerializeField] Color backgroundColor;
    [SerializeField] Color inactiveColor;

    public void SetData(WorldUnit unit)
    {
        parentUnit = unit;
        backgroundColor = GameManager.Instance.GetCivilizationColor(parentUnit.playerOwnerIndex, CivColorType.uiActiveColor);
        inactiveColor = GameManager.Instance.GetCivilizationColor(parentUnit.playerOwnerIndex, CivColorType.uiInactiveColor);
        UpdateData();
    }

    public void UpdateData()
    {
        unitHealth.text = parentUnit.currentHealth.ToString();
        if (parentUnit.isInteractable)
        {
            unitBackground.color = backgroundColor;
        }
        else
        {
            unitBackground.color = inactiveColor;
        }
       
    }
}
