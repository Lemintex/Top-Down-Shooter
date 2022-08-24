using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshBoundaryPrefab;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent = 1;
    [Range(0.1f, 10)]
    public float tileSize = 1;
    public int obstacles = 5;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledSpawnableCoords;

    Transform[,] tileArray;

    Map currentMap;
    void Start()
    {
        FindObjectOfType<Spawner>().OnNewWave.AddListener(OnNewWave);
        GenerateMap();
    }

    void OnNewWave(int waveIndex)
    {
        mapIndex = waveIndex;
        GenerateMap();    
    }
    // regenerates the entire map structure
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];

        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.1f, currentMap.mapSize.y * tileSize);// make the floor collider the correct size

        tileArray = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random rnd = new System.Random(currentMap.mapSeed);
        allTileCoords = new List<Coord>();

        // generates all tile coords
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.mapSeed));

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);// this has to be used instead of Destroy() due to editor
        }

        Transform holder = new GameObject(holderName).transform;
        holder.parent = transform;

        // generates tiles
        for(int x = 0; x < currentMap.mapSize.x; x++)
        {
            for(int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3((-currentMap.mapSize.x + 1) / 2f + x, 0, (-currentMap.mapSize.y + 1) / 2f + y) * tileSize;
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = holder;
                tileArray[x, y] = newTile;
            }
        }
        List<Coord> spawnable = allTileCoords;

        // generates obstacles
        obstacles = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); 
        bool[,] obstacleBoolMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int currentObstacles = 0;
        for (int i = 0; i < obstacles; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleBoolMap[randomCoord.x, randomCoord.y] = true;
            if (!randomCoord.Equals(currentMap.mapCentre) && MapIsAccessible(obstacleBoolMap, currentObstacles + 1))
            {
                spawnable.Remove(randomCoord);
                currentObstacles++;
                Vector3 obstaclePosition = CoordToPosition(randomCoord);
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)rnd.NextDouble());
                obstaclePosition.y += obstacleHeight / 2;  
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity) as Transform;
                newObstacle.parent = holder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                // random colour for obstacle
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                Color obstacleColour = Color.Lerp(currentMap.backgroundColour, currentMap.foregroundColour, randomCoord.y / (float)currentMap.mapSize.y);
                obstacleMaterial.color = obstacleColour;

                obstacleRenderer.sharedMaterial = obstacleMaterial;
                // all that for a drop of colour?
            }
            else
            {
                obstacleBoolMap[randomCoord.x, randomCoord.y] = false;
            }
        }
        shuffledSpawnableCoords = new Queue<Coord>(Utility.ShuffleArray(spawnable.ToArray(), currentMap.mapSeed));
        SetNavmeshBoundaries(holder);
    }

    // initialise invisible boundaries to the Navmesh so the dumb AI doesn't walk off the edge
    void SetNavmeshBoundaries(Transform holder)
    {
        // holy shit this is disgusting but I can't figure out a better alternative
        // the only reason this hack is used is we can't bake at runtime

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 0) * tileSize;// quad is rotated so z axis is now y axis

        Transform boundaryLeft = Instantiate(navmeshBoundaryPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        boundaryLeft.parent = holder;
        boundaryLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1, maxMapSize.y) * tileSize;

        Transform boundaryRight = Instantiate(navmeshBoundaryPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        boundaryRight.parent = holder;
        boundaryRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1, maxMapSize.y) * tileSize;

        Transform boundaryTop = Instantiate(navmeshBoundaryPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        boundaryTop.parent = holder;
        boundaryTop.localScale = new Vector3(currentMap.mapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;

        Transform boundaryBottom = Instantiate(navmeshBoundaryPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        boundaryBottom.parent = holder;
        boundaryBottom.localScale = new Vector3(currentMap.mapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;
    }

    // returns a Vector3 with the Coords world position
    public Vector3 CoordToPosition(Coord coord)
    {
        return new Vector3((-currentMap.mapSize.x + 1) / 2f + coord.x, 0, (-currentMap.mapSize.y + 1) / 2f + coord.y) * tileSize;
    }

    // convers a given Vector3 position to the tile it's on
    public Transform PositionToTile(Vector3 position)
    {
        int x = Mathf.RoundToInt((position.x / tileSize) + ((currentMap.mapSize.x - 1) / 2f));
        int y = Mathf.RoundToInt((position.z / tileSize) + ((currentMap.mapSize.y - 1) / 2f));
        return tileArray[x, y];
    }

    // flood fill returns true if all empty tiles are adjacent
    bool MapIsAccessible(bool[,] obstacleMap, int obstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;
        int accessibleTiles = 1;
        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for(int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 ^ y == 0)// XOR operator ensures we don't consider the tile we just dequeued
                    {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTiles++;
                            }
                        }
                    }
                }
                    
            }
        }
        int target = (int)((currentMap.mapSize.x * currentMap.mapSize.y) - obstacleCount);
        return target == accessibleTiles;
    }

    // returns the next random Coord in the random queue
    Coord GetRandomCoord()
    {
        Coord coord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(coord);
        return coord;
    }

    // returns the next spawnable tile in the spawnable queue
    public Transform GetRandomSpawnableTile()
    {
        Coord randomCoord = shuffledSpawnableCoords.Dequeue();
        shuffledSpawnableCoords.Enqueue(randomCoord);
        Transform tile = tileArray[randomCoord.x, randomCoord.y];
        return tile;
    }


    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // this isn't in a seperate file because I said so (it doesn't need to be)
    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;

        public int mapSeed = 0;


        [Range(0.01f, 1)]
        public float minObstacleHeight = 0.2f;
        [Range(1, 4)]
        public float maxObstacleHeight = 1;
        public Color foregroundColour;
        public Color backgroundColour;

        public Coord mapCentre
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}