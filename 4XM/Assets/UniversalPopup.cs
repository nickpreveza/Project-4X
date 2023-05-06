using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SignedInitiative;

public class UniversalPopup : UIPopup
{
    //probably update this to support images 
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;

    [SerializeField] Button confirm;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button cancel;
    [SerializeField] TextMeshProUGUI cancelText;

    
    public void SetData(string newTitle, string newDescription, bool available)
    {
        title.text = newTitle;
        description.text = newDescription;
        confirm.onClick.RemoveAllListeners();
        confirm.interactable = available;

        if (available)
        {
            confirm.onClick.AddListener(() => UIManager.Instance.confirmAction());
        }
        confirm.onClick.AddListener(()=> CloseWithDelay());
        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(Close);
    }

    public void CloseWithDelay()
    {
        StartCoroutine(CloseEnum());
    }

    IEnumerator CloseEnum()
    {
        yield return new WaitForSeconds(0.1f);
        Close();
    }

    public override void Close()
    {
        base.Close();
    }


}
