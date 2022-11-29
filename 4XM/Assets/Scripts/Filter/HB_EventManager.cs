using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HB_EventManager : MonoBehaviour
{
    public static HB_EventManager Instance;

    public event Action onEnemyKilled;
    public event Action onHempPlanted;
    public event Action onHempHarvested;
    public event Action onHempCollected;

    public event Action onBulletsShot;
    public event Action onWeaponEquipped;

    public event Action<string> onQuestCompleted;
    public event Action<string> onQuestRewarded;


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

    public void OnBulletsShot()
    {
        onBulletsShot?.Invoke();
    }

    public void OnWeaponEquipped()
    {
        onWeaponEquipped?.Invoke();
    }

    public void OnQuestRewarded(string questKey)
    {
        onQuestRewarded?.Invoke(questKey);
    }

    public void OnQuestCompleted(string questKey)
    {
        onQuestCompleted?.Invoke(questKey);
    }

    public void OnHempHarvested()
    {
        onHempHarvested?.Invoke();
    }

    public void OnHempCollected()
    {
        onHempCollected?.Invoke();
    }

    public void OnHempPlanted()
    {
        onHempPlanted?.Invoke();
    }

    public void OnEnemyKilled()
    {
        onEnemyKilled?.Invoke();
    }
}
