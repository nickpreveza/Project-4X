using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    public enum SpawnState { INACTIVE, SPAWNING, ACTIVE, COUNTING };

    [SerializeField] bool isArcade; //test

    Wave currentWave;
    public WaveSequence[] waveSequences;
    Wave[] waves;
    public List<HB_EnemySpawner> enemySpawners;
    public List<HB_EnemySpawner> selectedSpawners;
    public int waveIndex = 0;
    public int totalWavesInDay = 0;
    private int nextWaveIndex = 1;

    public float waveCountdown;
    public float waveDuration;
    public float spawnCountdown;

    public SpawnState state = SpawnState.INACTIVE;

    public int totalEnemies;
    public int enemiesRemaining;
    public int enemiesToSpawn;
    public int spawnIndex;
    bool hasPlayedWaveSound;
    public int arcadeWaveIndex =1;
    private void Awake()
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
    public void SetWave(int index)
    {
        spawnIndex = 0;
        state = SpawnState.INACTIVE;

        if (waveSequences.Length <= HB_GameManager.Instance.data.day - 1)
        {
            Debug.LogError("Current Day does not correspond to a sequence");
            //Alt to endless here
        }

        if (HB_GameManager.Instance.devMode && HB_GameManager.Instance.overrideWaveDay)
        {
            waves = waveSequences[HB_GameManager.Instance.overide_sequence].waves;
            index = HB_GameManager.Instance.overide_wave;
        }
        else
        {
            waves = waveSequences[HB_GameManager.Instance.data.day - 1].waves;
        }

        HB_PlayerController.Instance.hasTakenDamageThisWave = false;
        
        if (waves.Length <= index)
        {
            Debug.LogError("WaveHandler, SetWave: Invalid index");
            waveIndex = 0;
            nextWaveIndex = waveIndex + 1;
        }
        else
        {
            waveIndex = index;
            nextWaveIndex = index + 1;
        }

        currentWave = waves[waveIndex];
        totalWavesInDay = waves.Length;
        waveCountdown = currentWave.waveCountdown;
        waveDuration = currentWave.waveDuration;
        selectedSpawners = new List<HB_EnemySpawner>();
        List<HB_EnemySpawner> spawnersList = new List<HB_EnemySpawner>(enemySpawners);
        int foundSpawners = 0;
        for (int i = 0; i < 2; i++)
        {
            HB_EnemySpawner randomSpawner = spawnersList[(int)Random.Range(0, spawnersList.Count-1)];
            
            if (!selectedSpawners.Contains(randomSpawner))
            {
                selectedSpawners.Add(randomSpawner);
                spawnersList.Remove(randomSpawner);
                foundSpawners++;
                if (foundSpawners >= 2)
                {
                    break;
                }
            }
        }

        totalEnemies = 0;
        enemiesToSpawn = 0;

        for (int i = 0; i < selectedSpawners.Count; i++)
        {
            totalEnemies += EnemiesOnSpawner(i);
        }

        enemiesRemaining += totalEnemies;
        enemiesToSpawn = totalEnemies;
     
        state = SpawnState.COUNTING;
    }

    public int EnemiesOnSpawner(int index)
    {
        switch (index)
        {
            case 0:
                return currentWave.spawner1Enemies.Count;
            case 1:
                return currentWave.spawner2Enemies.Count;
        }

        Debug.LogError("Invalid Index - Spawners cases are handled from 0 to 3");
        return 0;
    }

    public EnemyType GetSpawnerItem(int spanwerIndex, int enemyIndex)
    {
        switch (spanwerIndex)
        {
            case 0:
                return currentWave.spawner1Enemies[enemyIndex];
            case 1:
                return currentWave.spawner2Enemies[enemyIndex];
        }

        return EnemyType.LOCUST;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case SpawnState.INACTIVE:
                return;
            case SpawnState.COUNTING:
                waveCountdown -= Time.deltaTime;
                if (waveCountdown <= 0)
                {
                    if (waveIndex != 0)
                    {
                        HB_AudioManager.Instance.Play("spawnerOpen");
                    }
                    else
                    {
                        HB_AudioManager.Instance.Play("waveStart");
                    }
                  
                    state = SpawnState.SPAWNING;
                    SetSpawnersActive();
                    spawnCountdown = currentWave.spawnCountdown;
                }
                break;
            case SpawnState.SPAWNING:
                spawnCountdown -= Time.deltaTime;
                if (spawnCountdown <= 0)
                {
                   
                    for (int i = 0; i < selectedSpawners.Count; i++)
                    {
                        selectedSpawners[i].Spawn(GetSpawnerItem(i, spawnIndex), this);
                        enemiesToSpawn--;
                    }
                    if (enemiesToSpawn <= 0)
                    {
                        SetSpawnersDisabled();
                        state = SpawnState.ACTIVE;
                        return;
                    }
                    else
                    {
                        spawnIndex++;
                        spawnCountdown = currentWave.spawnCountdown;
                    }
                   
                }
                break;
            case SpawnState.ACTIVE:
                waveDuration -= Time.deltaTime;

                if (enemiesRemaining <= 0)
                {
                    WaveCompleted();
                }

                break;
            
        }
    }

    void SetSpawnersActive()
    {
        foreach(HB_EnemySpawner spawner in selectedSpawners)
        {
            spawner.SetAsActive();
        }
    }

    void SetSpawnersDisabled()
    {
        foreach (HB_EnemySpawner spawner in selectedSpawners)
        {
            spawner.SetAsDisabled();
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Wave completed");
        if (waves.Length <= nextWaveIndex)
        {
            if (isArcade)
            {
                arcadeWaveIndex++;
                nextWaveIndex = 0;
                SetWave(waveIndex);
                return;
            }
            SequeneceCompleted();
        }
        else
        {
            waveIndex = nextWaveIndex;
            nextWaveIndex++;
            SetWave(waveIndex);
        }

       
    }

    void SequeneceCompleted()
    {
        state = SpawnState.INACTIVE;
        HB_AudioManager.Instance.Play("win");
        HB_GameManager.Instance.GameOver();
        //GameObject newObj = Instantiate(HB_GameManager.Instance.photoPrefab, photoSpawnPosition.transform.position, Quaternion.identity);
        //photoSpawned = newObj.GetComponent<PhotoInteractable>();
    }
}
