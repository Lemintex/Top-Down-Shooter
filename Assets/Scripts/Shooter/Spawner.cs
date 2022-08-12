using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;
    int waveIndex = 0;

    private int enemiesRemainingToSpawn;
    float spawnTime;

    private int enemiesAlive;
    void Start()
    {
        NextWave();
    }
    void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > spawnTime)
        {
            enemiesRemainingToSpawn--;
            spawnTime += currentWave.timeBetweenSpawns;
            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath.AddListener(OnEnemyDeath);
        }
    }

    // called when an enemy in the wave dies
    void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive == 0)
        {
            NextWave();
        }
    }

    // spawns the next wave
    void NextWave()
    {
        if (waveIndex < waves.Length)
        {
            currentWave = waves[waveIndex];
            waveIndex++;
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesAlive = enemiesRemainingToSpawn;
            spawnTime = Time.time;  
        }
    }

    // stores all the info for a wave
    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
    }
}
