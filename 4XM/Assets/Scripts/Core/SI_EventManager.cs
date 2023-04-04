using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_EventManager : MonoBehaviour
{
    public static SI_EventManager Instance;

    public event Action onEnemyKilled;
    public event Action onDataLoaded;
    public event Action onMapGenerated;
    public event Action onUnitsPlaced;
    public event Action onSignInCompleted;

    public event Action<string> onQuestCompleted;
    public event Action<string> onQuestRewarded;

    public event Action onCameraMoved;


    public event Action onTurnEnded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void OnDataLoaded()
    {
        onDataLoaded?.Invoke();
    }

    public void OnMapGenerated()
    {
        onMapGenerated?.Invoke();
    }

    public void OnUnitsPlaced()
    {
        onUnitsPlaced?.Invoke();
    }

    public void OnCameraMoved()
    {
        Debug.Log("Camera Moved");
        onCameraMoved?.Invoke();
    }

    public void OnQuestRewarded(string questKey)
    {
        onQuestRewarded?.Invoke(questKey);
    }

    public void OnQuestCompleted(string questKey)
    {
        onQuestCompleted?.Invoke(questKey);
    }

    public void OnEnemyKilled()
    {
        onEnemyKilled?.Invoke();
    }
}
