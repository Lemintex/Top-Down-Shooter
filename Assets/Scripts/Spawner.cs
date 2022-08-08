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
            enemiesAlive++;
            spawnTime += currentWave.timeBetweenSpawns;
            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath.AddListener(OnEnemyDeath);
        }
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive == 0)
        {
            NextWave();
        }
    }
    void NextWave()
    {
        currentWave = waves[waveIndex];
        waveIndex++;
        enemiesRemainingToSpawn = currentWave.enemyCount;
        enemiesAlive = enemiesRemainingToSpawn;
    }


    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
    }
}
