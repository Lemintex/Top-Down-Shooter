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

    Map currentMap;
    void Start()
    {
        GenerateMap();    
    }

    // regenerates the entire map structure
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];

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

        // these offsets ensure the map is centred regardless of dimensions
        if (currentMap.mapSize.x % 2 == 0) currentMap.offsetX = 0.5f;
        if (currentMap.mapSize.y % 2 == 0) currentMap.offsetY = 0.5f;

        Transform holder = new GameObject(holderName).transform;
        holder.parent = transform;

        // generates tiles
        for(int x = 0; x < currentMap.mapSize.x; x++)
        {
            for(int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(-currentMap.mapSize.x / 2 + currentMap.offsetX + x, 0, -currentMap.mapSize.y / 2 + currentMap.offsetY + y) * tileSize;
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = holder;
            }
        }

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
                Color obstacleColour = Color.Lerp(currentMap.backgroundColour, currentMap.foregroundColour, (float)rnd.NextDouble());
                obstacleMaterial.color = obstacleColour;

                obstacleRenderer.sharedMaterial = obstacleMaterial;
                // all that for a drop of colour?
            }
            else
            {
                obstacleBoolMap[randomCoord.x, randomCoord.y] = false;
            }
        }
        SetNavmeshBoundaries(holder);
    }

    // initialise invisible boundaries to the Navmesh so the dumb AI doesn't walk off the edge
    void SetNavmeshBoundaries(Transform holder)
    {
        // holy shit this is disgusting but I can't figure out a better alternative
        // the only reason this hack is used is we can't bake at runtime

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 0) * tileSize;// quad is rotated so z axis is now y axis

        Transform boundaryLeft = Instantiate(navmeshBoundaryPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        boundaryLeft.parent = holder;
        boundaryLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1, maxMapSize.y) * tileSize;

        Transform boundaryRight = Instantiate(navmeshBoundaryPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        boundaryRight.parent = holder;
        boundaryRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1, maxMapSize.y) * tileSize;

        Transform boundaryTop = Instantiate(navmeshBoundaryPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
        boundaryTop.parent = holder;
        boundaryTop.localScale = new Vector3(currentMap.mapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;

        Transform boundaryBottom = Instantiate(navmeshBoundaryPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
        boundaryBottom.parent = holder;
        boundaryBottom.localScale = new Vector3(currentMap.mapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;
    }

    // returns a Vector3 with the Coords world position
    Vector3 CoordToPosition(Coord coord)
    {
        return new Vector3(-currentMap.mapSize.x / 2 + currentMap.offsetX + coord.x, 0, -currentMap.mapSize.y / 2 + currentMap.offsetY + coord.y) * tileSize;
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
    public Coord GetRandomCoord()
    {
        Coord coord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(coord);
        return coord;
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

    // this isn't in a seperate file because... I said so
    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;

        public int mapSeed = 0;

        public float offsetX, offsetY;
        [Range(0.01f, 1)]
        public float minObstacleHeight = 02f;
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