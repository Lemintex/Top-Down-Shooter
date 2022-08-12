using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent = 1;
    public int obstacles = 5;


    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;

    public int mapSeed = 0;
    void Start()
    {
        GenerateMap();    
    }
    public void GenerateMap()
    {
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), mapSeed));

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);// this has to be used instead of Destroy() due to editor
        }

        Transform holder = new GameObject(holderName).transform;
        holder.parent = transform;
        for(int x = 0; x < mapSize.x; x++)
        {
            for(int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * outlinePercent;
                newTile.parent = holder;
            }
        }
        for (int i = 0; i < obstacles; i++)
        {
            Coord randomCoord = GetRandomCoord();
            Vector3 obstaclePosition = CoordToPosition(randomCoord);
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity) as Transform;
            newObstacle.parent = holder;
        }    
    }


    Vector3 CoordToPosition(Coord coord)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + coord.x, 0, -mapSize.y / 2 + 0.5f + coord.y);
    }


    public Coord GetRandomCoord()
    {
        Coord coord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(coord);
        return coord;
    }

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
}