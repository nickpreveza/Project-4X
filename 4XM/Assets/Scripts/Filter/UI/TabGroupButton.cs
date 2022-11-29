using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabGroupButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] TabGroup tabGroup;
    public string tabTitle;
    Image background;
    void Awake()
    {
        background = GetComponent<Image>();
        tabGroup.Subscribe(this);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OntabExit(this);
    }

    public void SetBackground(Sprite sprite)
    {
        background.sprite = sprite;
    }
    
    public void Select()
    {

    }

    public void Deselect()
    {

    }
}
