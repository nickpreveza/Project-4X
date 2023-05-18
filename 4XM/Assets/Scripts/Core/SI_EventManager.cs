using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SI_EventManager : MonoBehaviour
{
    public static SI_EventManager Instance;

    public event Action onUnitKilled;
    public event Action onDataLoaded;
    public event Action onMapGenerated;
    public event Action onUnitsPlaced;
    public event Action onSignInCompleted;

    public event Action<string> onQuestCompleted;
    public event Action<string> onQuestRewarded;

    public event Action onCameraMoved;


    public event Action<int> onTurnEnded;
    public event Action<int> onTurnStarted;

    public event Action<int> onCityCaptured;
    public event Action<int> onTransactionMade;
    //Camera
    public event Action<int> onAutopanCompleted; //int = WorldHex.hexIdentifier 
    public event Action<WorldUnit> onUnitMoved;
    public event Action<WorldUnit> onUnitAction;

    public event Action<int> onAbilityUnlocked;

    public event Action<WorldHex> cityGiveOuput;

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

    public void OnAbilityUnlocked(int playerIndex)
    {
        onAbilityUnlocked?.Invoke(playerIndex);
    }

    public void OnTransactionMade(int playerIndex)
    {
        onTransactionMade?.Invoke(playerIndex);
    }

    public void OnAutoPanCompleted(int hexIdentifier)
    {
        onAutopanCompleted?.Invoke(hexIdentifier);
    }

    public void OnDataLoaded()
    {
        onDataLoaded?.Invoke();
    }

    public void OnMapGenerated()
    {
        onMapGenerated?.Invoke();
    }

    public void OnCityCaptured(int playerIndex)
    {
        onCityCaptured?.Invoke(playerIndex);
    }
    public void OnUnitsPlaced()
    {
        onUnitsPlaced?.Invoke();
    }

    public void OnCameraMoved()
    {
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

    public void OnUnitKilled()
    {
        onUnitKilled?.Invoke();
    }

    public void OnTurnEnded(int playerIndex)
    {
        onTurnEnded?.Invoke(playerIndex);
    }

    public void OnTurnStarted(int playerIndex)
    {
        onTurnStarted?.Invoke(playerIndex);
    }
}
