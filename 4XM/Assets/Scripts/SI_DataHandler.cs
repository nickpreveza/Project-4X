using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_DataHandler : MonoBehaviour
{
    public void FetchData()
    {
        //cool functions
        SI_EventManager.Instance.OnDataLoaded();
    }
}
