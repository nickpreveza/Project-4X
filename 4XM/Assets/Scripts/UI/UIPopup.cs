using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIPopup : MonoBehaviour
{
    void OnEnable()
    {
        if (SI_UIManager.Instance != null)
        {
            SI_UIManager.Instance.latestPopup = this;
        }
        Setup();
    }
    public virtual void Close()
    {
        if (SI_UIManager.Instance != null) 
        {
            SI_UIManager.Instance.popupActive = false;
        }
        this.gameObject.SetActive(false);
    }

    public virtual void Setup()
    {

    }

}
