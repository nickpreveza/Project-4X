using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPoint : MonoBehaviour
{
    //[SerializeField] Animator anim;
    [SerializeField] GameObject levelActive;
    [SerializeField] GameObject unitActive;

    public void SetLevelPointColor(Color color)
    {
        levelActive.GetComponent<Image>().color = color;
    }
    public void SetLevelPoint(bool active)
    {
        levelActive.SetActive(active);
        /*
        if (active)
        {
            anim.SetTrigger("Active");
        }
        else
        {
            anim.SetTrigger("Inactive");
        }*/
       
    }

    public void SetUnitPoint(bool active)
    {
        unitActive.SetActive(active);
    }
}
