using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using Unity.VisualScripting;
using System;

public class Generator2D : MonoBehaviour
{
    public enum CellType
    {
        None,
        Room,
        Hallway
    }

    class Room
    {
        public RectInt bounds;

        public Room(Vector2Int location, Vector2Int size)
        {
            bounds = new RectInt(location, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }
    }

    [SerializeField] private Vector2 cellSize = new Vector2(8, 8);
    [SerializeField] private Transform cellsRoot;
    [SerializeField] private Vector3 cellsOffset;
    [SerializeField] private List<Transform> objectsToPlace;
    private List<Transform> defaultList;

    private Random random;
    public Grid2D<CellType> grid { get; private set; }
    private List<Room> rooms;

    private void Awake()
    {
        defaultList = new List<Transform>(objectsToPlace);
    }

    /// <summary>
    /// Adds an object to the list of objects to add on generation
    /// </summary>
    /// <param name="obj">The object</param>
    public void AddEntityToPlace(Transform obj)
    {
        objectsToPlace.Add(obj);
    }

    /// <summary>
    /// Resets the entities to place
    /// </summary>
    public void ResetEntitiesToPlace()
    {
        objectsToPlace = new List<Transform>(defaultList);
    }

    public void Generate(DungeonData data)
    {

        random = new Random(UnityEngine.Random.Range(0, int.MaxValue));
        grid = new Grid2D<CellType>(data.size, Vector2Int.zero);
        rooms = new List<Room>();

        PlaceRooms(data.size, data.roomCount, data.roomMaxSize);
        CreateHallways();

        InstantiateDungeon(data.size, data.cellPrefab);
        PlaceObjects();
    }

    private void PlaceObjects()
    {
        List<Vector2Int> taken = new List<Vector2Int>();
        Room room;
        int selectedIdx;
        int xWithin = 0;
        int yWithin = 0;

        foreach (Transform obj in objectsToPlace)
        {
            bool good = false;

            while (!good)
            {
                good = true;
                selectedIdx = random.Next(0, rooms.Count);
                room = rooms[selectedIdx];
                xWithin = room.bounds.xMin + random.Next(0, room.bounds.xMax - room.bounds.xMin);
                yWithin = room.bounds.yMin + random.Next(0, room.bounds.yMax - room.bounds.yMin);

                foreach (Vector2Int vec in taken)
                {
                    if (vec.x == xWithin && yWithin == vec.y)
                    {
                        good = false;
                        break;
                    }
                }
            }


            taken.Add(new Vector2Int(xWithin, yWithin));

            print(obj.name + " -> " + xWithin + "/" + yWithin);
            obj.position = new Vector3(
                xWithin * cellSize.x + cellsOffset.x + cellSize.x / 2f,
                cellsOffset.y,
                yWithin * cellSize.y + cellsOffset.z + cellSize.y / 2f);


        }
    }

    void PlaceRooms(Vector2Int size, int roomCount, Vector2Int roomMaxSize)
    {

        for (int i = 0; i < roomCount; i++)
        {

            Vector2Int roomSize = new Vector2Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1)
            );

            Vector2Int location = new Vector2Int(
                random.Next(0, size.x - roomSize.x),
                random.Next(0, size.y - roomSize.y)
            );

            Room newRoom = new Room(location, roomSize);
            rooms.Add(newRoom);

            foreach (var pos in newRoom.bounds.allPositionsWithin)
            {
                grid[pos] = CellType.Room;
            }
        }
    }

    void PathFind(Vector2Int start, Vector2Int end)
    {
        int sideX = start.x < end.x ? 1 : -1;
        int sideY = start.y < end.y ? 1 : -1;

        while (start.x != end.x || start.y != end.y)
        {
            if (start.x != end.x)
            {
                start.x += sideX;
            }
            else
            {
                start.y += sideY;
            }
            if (grid[start] == CellType.None)
            {
                grid[start] = CellType.Hallway;
            }
        }
    }

    void CreateHallways()
    {
        int amountOfRandomHallways = rooms.Count / 2;

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            PathFind(rooms[i].bounds.min, rooms[i + 1].bounds.min);
        }
        int selectionOne;
        int selectionTwo;

        for (int i = 0; i < amountOfRandomHallways; i++)
        {
            selectionOne = random.Next(0, rooms.Count);
            selectionTwo = random.Next(0, rooms.Count);


            if (selectionTwo != selectionOne) PathFind(rooms[selectionOne].bounds.min, rooms[selectionTwo].bounds.min);
        }
    }

    private void InstantiateDungeon(Vector2Int size, DungeonCell cellPrefab)
    {
        foreach (Transform child in cellsRoot)
        {
            Destroy(child.gameObject);
        }

        Vector2Int pos = Vector2Int.zero;
        bool leftOpen;
        bool rightOpen;
        bool topOpen;
        bool bottomOpen;
        for (int x = 0; x < size.x; x++)
        {
            pos.x = x;
            for (int y = 0; y < size.y; y++)
            {
                pos.y = y;
                if (grid[pos] != CellType.None)
                {
                    DungeonCell cell = Instantiate(cellPrefab, new Vector3(x * cellSize.x + cellsOffset.x, cellsOffset.y, y * cellSize.y + cellsOffset.z), Quaternion.identity, cellsRoot);
                    pos.x = x - 1;
                    leftOpen = x > 0 ? (grid[pos] == CellType.None ? false : true) : false;
                    pos.x = x + 1;
                    rightOpen = x < size.x - 1 ? (grid[pos] == CellType.None ? false : true) : false;
                    pos.x = x;
                    pos.y = y - 1;
                    bottomOpen = y > 0 ? (grid[pos] == CellType.None ? false : true) : false;
                    pos.y = y + 1;
                    topOpen = y < size.y - 1 ? (grid[pos] == CellType.None ? false : true) : false;

                    cell.Init(leftOpen, rightOpen, topOpen, bottomOpen);
                    cell.gameObject.name = Enum.GetNames(typeof(CellType))[(int)grid[pos]];
                }
            }
        }
    }
}
