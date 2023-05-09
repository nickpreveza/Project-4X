using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityVisualHelper : MonoBehaviour
{
   public GameObject cityFlag;
   public GameObject citySiegeEffect;
   public GameObject cityLevelEffect;


    private void Start()
    {
        citySiegeEffect.SetActive(false);
        cityLevelEffect.SetActive(false);
    }
}
