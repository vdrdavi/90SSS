using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomNode
{
    public RectInt Bounds;
    public RoomNode Left;
    public RoomNode Right;

    public RoomNode(RectInt bounds)
    {
        Bounds = bounds;
    }
}

public class BSPMapGenerator : MonoBehaviour
{
    public GameObject player;

    public int mapWidth = 30;
    public int mapHeight = 20;
    public int minRoomSize = 5;
    public int maxRooms = 6;

    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase entranceTile;

    public List<RoomNode> allRooms = new List<RoomNode>();
    public Vector3Int playerSpawnPoint;

    private RoomNode rootNode;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        rootNode = new RoomNode(new RectInt(0, 0, mapWidth, mapHeight));
        Queue<RoomNode> nodesToSplit = new Queue<RoomNode>();
        nodesToSplit.Enqueue(rootNode);

        while (nodesToSplit.Count > 0 && allRooms.Count < maxRooms)
        {
            RoomNode current = nodesToSplit.Dequeue();

            if (allRooms.Count + nodesToSplit.Count >= maxRooms)
            {
                allRooms.Add(current);
                continue;
            }

            if (SplitNode(current))
            {
                nodesToSplit.Enqueue(current.Left);
                nodesToSplit.Enqueue(current.Right);
            }
            else
            {
                allRooms.Add(current);
            }
        }

        while (nodesToSplit.Count > 0)
        {
            allRooms.Add(nodesToSplit.Dequeue());
        }

        DrawMap();
        ConnectRooms(rootNode);
        SpawnPlayer(GenerateEntrance());
    }

    private void SpawnPlayer(Vector3Int spawnpoint)
    {
        if (player != null)
        {
            Instantiate(player, spawnpoint, Quaternion.identity);
        }
    }

    private bool SplitNode(RoomNode node)
    {
        bool splitHorizontal = Random.value > 0.5f;

        if (node.Bounds.width > node.Bounds.height && (float)node.Bounds.width / node.Bounds.height >= 1.25f)
        {
            splitHorizontal = false;
        }
        else if (node.Bounds.height > node.Bounds.width && (float)node.Bounds.height / node.Bounds.width >= 1.25f)
        {
            splitHorizontal = true;
        }

        int max;
        if (splitHorizontal)
        {
            max = node.Bounds.height - minRoomSize;
            if (max <= minRoomSize) return false;

            int splitY = Random.Range(minRoomSize, max);
            node.Left = new RoomNode(new RectInt(node.Bounds.x, node.Bounds.y, node.Bounds.width, splitY));
            node.Right = new RoomNode(new RectInt(node.Bounds.x, node.Bounds.y + splitY, node.Bounds.width, node.Bounds.height - splitY));
        }
        else
        {
            max = node.Bounds.width - minRoomSize;
            if (max <= minRoomSize) return false;

            int splitX = Random.Range(minRoomSize, max);
            node.Left = new RoomNode(new RectInt(node.Bounds.x, node.Bounds.y, splitX, node.Bounds.height));
            node.Right = new RoomNode(new RectInt(node.Bounds.x + splitX, node.Bounds.y, node.Bounds.width - splitX, node.Bounds.height));
        }

        return true;
    }

    private void DrawMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
        }

        foreach (RoomNode room in allRooms)
        {
            int startX = room.Bounds.x + 1;
            int endX = room.Bounds.xMax;
            if (endX >= mapWidth) endX = mapWidth - 1;

            int startY = room.Bounds.y + 1;
            int endY = room.Bounds.yMax;
            if (endY >= mapHeight) endY = mapHeight - 1;

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }

    private void ConnectRooms(RoomNode node)
    {
        if (node.Left == null || node.Right == null) return;

        ConnectRooms(node.Left);
        ConnectRooms(node.Right);

        Vector2Int centerLeft = new Vector2Int(
            (int)node.Left.Bounds.center.x,
            (int)node.Left.Bounds.center.y
        );

        Vector2Int centerRight = new Vector2Int(
            (int)node.Right.Bounds.center.x,
            (int)node.Right.Bounds.center.y
        );

        CreateCorridor(centerLeft, centerRight);
    }

    private void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            CarveDoor(x, y);
            x += (end.x > x) ? 1 : -1;
        }
        while (y != end.y)
        {
            CarveDoor(x, y);
            y += (end.y > y) ? 1 : -1;
        }
    }

    private void CarveDoor(int x, int y)
    {
        wallTilemap.SetTile(new Vector3Int(x, y, 0), null);
        floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
    }

    private Vector3Int GenerateEntrance()
    {
        RoomNode borderRoom = allRooms[0];

        foreach (RoomNode room in allRooms)
        {
            if (room.Bounds.y == 0)
            {
                borderRoom = room;
                break;
            }
        }

        int doorX = (int)borderRoom.Bounds.center.x;
        int doorY = 0;

        Vector3Int doorPos = new Vector3Int(doorX, doorY, 0);

        wallTilemap.SetTile(doorPos, null);
        floorTilemap.SetTile(doorPos, entranceTile);

        playerSpawnPoint = new Vector3Int(doorX, doorY + 2, 0);
        return playerSpawnPoint;
    }
}