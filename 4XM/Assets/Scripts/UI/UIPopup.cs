using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public abstract class UIPopup : MonoBehaviour
{
    void OnEnable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.latestPopup = this;
        }
        Setup();
    }
    public virtual void Close()
    {
        if (UIManager.Instance != null) 
        {
            UIManager.Instance.popupActive = false;
        }
        this.gameObject.SetActive(false);
    }

    public virtual void Setup()
    {

    }

}
