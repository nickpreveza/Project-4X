using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPoint : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject levelActive;
    [SerializeField] GameObject unitActive;
    public void InitState()
    {
        levelActive.SetActive(false);
        unitActive.SetActive(false);
    }
    public void SetPointActive(bool active)
    {
        if (active)
        {
            anim.SetTrigger("Active");
        }
        else
        {
            anim.SetTrigger("Inactive");
        }
       
    }

    public void SetPointUnitActive(bool active)
    {
        unitActive.SetActive(active);
    }
}
