using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    enum State
    {
        PLAYING,
        CAMPING,
        DEAD
    };
    State playerState = State.PLAYING;

    public MapGenerator mapGenerator;
    public UnityEvent<int> OnNewWave;
    public Wave[] waves;
    public Enemy enemy;

    DamageableEntity playerEntity;
    Transform playerTransform;

    Wave currentWave;
    int waveIndex = 0;

    private int enemiesRemainingToSpawn;
    float spawnTime;

    private int enemiesAlive;
    Vector3 campPosition;
    float campcheckTime;
    const float campTresholdDistance = 1;// TODO: make sensible and calculated value
    const float timeBetweenCampingChecks = 1;

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();

        playerEntity.OnDeath.AddListener(OnPlayerDeath);
        playerTransform = playerEntity.transform;
        campPosition = playerTransform.position;
        campcheckTime = timeBetweenCampingChecks + Time.time;
        mapGenerator = FindObjectOfType<MapGenerator>();
        NextWave();
    }


    void Update()
    {
        if (playerState != State.DEAD)
        {
            CheckCamping();
            if (enemiesRemainingToSpawn > 0 && Time.time > spawnTime)
            {
                enemiesRemainingToSpawn--;
                spawnTime += currentWave.timeBetweenSpawns;
                StartCoroutine(SpawnEnemy());
            }
        }
    }

    // called when the player dies
    void OnPlayerDeath()
    {
        playerState = State.DEAD;
    }

    // checks if the player is moving around
    void CheckCamping()
    {
        if (campcheckTime < Time.time)
        {
            campcheckTime = Time.time + timeBetweenCampingChecks;
            bool isCamping = (Vector3.Distance(campPosition, playerTransform.position) < campTresholdDistance);
            if (isCamping)
            {
                playerState = State.CAMPING;
            }
            else// if the player is dead this function won't be called
            {
                playerState = State.PLAYING;
            }
            campPosition = playerTransform.position;
        }
    }

    // flash a tile and spawn an enemy
    IEnumerator SpawnEnemy()
    {
        float flashTimeBeforeSpawn = 1;
        float timesToFlash = 2;
        timesToFlash *= 2;// double times to flash as Mathf.PingPong goes from 0 to length in t and length to 0 in t, so 2t is one full flash
        Transform tile;
        if (playerState == State.CAMPING)
        {
            tile = mapGenerator.PositionToTile(playerTransform.position);
        }
        else
        {
            tile = mapGenerator.GetRandomSpawnableTile();
        }
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
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesAlive = enemiesRemainingToSpawn;
            spawnTime = Time.time;
            if (OnNewWave != null)
            {
                OnNewWave.Invoke(waveIndex);
            }
            waveIndex++;
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
