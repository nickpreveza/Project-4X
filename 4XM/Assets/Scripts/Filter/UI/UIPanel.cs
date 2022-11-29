using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [HideInInspector] public bool isActive;
    public bool startEnabled;
    [HideInInspector] public GameObject panelObject;
    [HideInInspector] public CanvasGroup canvasGroup;

    private void Awake()
    {
        panelObject = transform.GetChild(0).gameObject;
        canvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        if (!startEnabled)
        Disable();
    }
    public virtual void Activate()
    {
        isActive = true;
        panelObject.SetActive(true);
    }

    public virtual void Setup()
    {

    }

    public virtual void Disable()
    {
        isActive = false;
        panelObject.SetActive(false);
    }
}
