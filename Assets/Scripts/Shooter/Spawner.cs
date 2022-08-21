using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;
    int waveIndex = 0;

    private int enemiesRemainingToSpawn;
    float spawnTime;

    private int enemiesAlive;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        NextWave();
    }


    void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > spawnTime)
        {
            enemiesRemainingToSpawn--;
            spawnTime += currentWave.timeBetweenSpawns;
            StartCoroutine(SpawnEnemy());
        }
    }

    // flash a tile and spawn an enemy
    IEnumerator SpawnEnemy()
    {
        float flashTimeBeforeSpawn = 1;
        float timesToFlash = 2;
        timesToFlash *= 2;// double times to flash as Mathf.PingPong goes from 0 to length in t and length to 0 in 1 in t, so 2t is one full flash
        Transform tile = mapGenerator.GetRandomSpawnableTile();
        Material tileMaterial = tile.GetComponent<Renderer>().material;

        Color originalTileColour = tileMaterial.color;
        Color tileFlashColor = Color.blue;
        float timer = 0;
        while (timer < flashTimeBeforeSpawn)
        {
            timer += Time.deltaTime;

            tileMaterial.color = Color.Lerp(originalTileColour, tileFlashColor, Mathf.PingPong(timer * timesToFlash, 1));
            yield return null;
        }
        tileMaterial.color = originalTileColour;

        Enemy spawnedEnemy = Instantiate(enemy, tile.position, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath.AddListener(OnEnemyDeath);
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
