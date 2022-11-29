using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public List<EnemyType> spawner1Enemies;
    public List<EnemyType> spawner2Enemies;
    public Dictionary<GameObject, EnemyType> spawnSequence;
    public float waveCountdown = 10;
    public float waveDuration = 60;
    public float spawnCountdown = 2;
}

[System.Serializable]
public class WaveSequence
{
    public Wave[] waves;
}
