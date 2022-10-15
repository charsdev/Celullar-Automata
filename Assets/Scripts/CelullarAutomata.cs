using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WorldGenerator
{
    public class CelullarAutomata
    {
        private class Room : System.IComparable<Room>
        {
            public List<Vector2Int> tiles;
            public List<Vector2Int> edgeTiles;
            public List<Room> connectedRooms;
            public int roomSize;
            public bool isAccesibleFromMainRoom;

            public Room()
            {
                tiles = new List<Vector2Int>();
                edgeTiles = new List<Vector2Int>();
                connectedRooms = new List<Room>();
                roomSize = 0;
                isAccesibleFromMainRoom = false;
            }

            public Room(List<Vector2Int> roomTiles, TileType[,] map)
            {
                tiles = roomTiles;
                roomSize = tiles.Count;
                connectedRooms = new List<Room>();
                edgeTiles = new List<Vector2Int>();
                isAccesibleFromMainRoom = false;

                foreach (var tile in tiles)
                {
                    for (int x = tile.x - 1; x <= tile.y + 1; x++)
                    {
                        for (int y = tile.x - 1; y <= tile.y + 1; y++)
                        {
                            if ((x == tile.x || y == tile.y)
                                && map[x, y] == TileType.Wall)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }

            public void SetAccesibleFromMainRoom()
            {
                if (!isAccesibleFromMainRoom)
                {
                    isAccesibleFromMainRoom = true;
                    foreach (var connectedRoom in connectedRooms)
                    {
                        connectedRoom.SetAccesibleFromMainRoom();
                    }
                }
            }

            public bool IsConnected(Room otherRoom)
            {
                return connectedRooms.Contains(otherRoom);
            }

            public static void ConnectRooms(Room roomA, Room roomB)
            {
                if (roomA.isAccesibleFromMainRoom)
                    roomB.SetAccesibleFromMainRoom();

                else if (roomB.isAccesibleFromMainRoom)
                    roomA.SetAccesibleFromMainRoom();

                roomA.connectedRooms.Add(roomB);
                roomB.connectedRooms.Add(roomA);
            }

            public int CompareTo(Room otherRoom)
            {
                return otherRoom.roomSize.CompareTo(roomSize);
            }
        }

        public enum TileType
        {
            Floor = 0,
            Wall,
            Border,
        }

        private System.Random _rnd;
        private int randomFillPercent = 50;

        private static CelullarAutomata _instance;

        private CelullarAutomata()
        {
            Width = 0;
            Height = 0;
            Map = null;
            Seed = System.DateTime.Now.GetHashCode();
            _rnd = new System.Random(Seed);
        }

        ~CelullarAutomata()
        {
            _instance = null;
        }

        public static CelullarAutomata Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CelullarAutomata();

                return _instance;
            }
        }

        public int Width { set; get; }
        public int Height { set; get; }
        public TileType[,] Map { set; get; }

        public System.Random AutomataRandom
        {
            get
            {
                return _rnd;
            }
            set
            {
                if (_rnd == null)
                {
                    _rnd = value;
                }
            }
        }

        public int Seed { get; set; }

        public void MakeMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (IsOnBorders(x, y))
                    {
                        Map[x, y] = TileType.Border;
                    }
                    else
                    {
                        Map[x, y] = _rnd.Next(0, 100) > randomFillPercent ? TileType.Floor : TileType.Wall;
                    }
                }
            }

            for (byte i = byte.MinValue; i < byte.MaxValue; i++)
            {
                SmoothMap();
            }

            ProcessMap();
        }


        public bool IsWall(int x, int y) => Map[x, y] == TileType.Wall;

        public bool IsFloor(int x, int y) => Map[x, y] == TileType.Floor;

        private bool IsOnBorders(int x, int y) => x == 0 || x == Width - 1 || y == 0 || y == Height - 1;

        public bool IsInMapRange(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        private void SmoothMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int neighbourWallTiles = GetAdjacentWalls(x, y);
                    if (neighbourWallTiles < 4 && !IsOnBorders(x, y))
                    {
                        Map[x, y] = TileType.Floor;
                    }
                    if (neighbourWallTiles > 4 && !IsOnBorders(x, y))
                    {
                        Map[x, y] = TileType.Wall;
                    }
                }
            }
        }

        private int GetAdjacentWalls(int x, int y)
        {
            int wallCount = 0;
            for (int iX = x - 1; iX <= x + 1; iX++)
            {
                for (int iY = y - 1; iY <= y + 1; iY++)
                {
                    if (IsInMapRange(iX, iY))
                    {
                        if (iX != x || iY != y)
                        {
                            wallCount += (int)Map[x, y];
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
            return wallCount;
        }

        private List<Vector2Int> GetRegionTiles(int startX, int startY)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            TileType[,] mapFlags = new TileType[Width , Height];
            TileType tileType = Map[startX, startY];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));
            mapFlags[startX, startY] = TileType.Wall;

            while (queue.Count > 0)
            {
                Vector2Int tile = queue.Dequeue();
                tiles.Add(tile);

                for (int x = tile.x - 1; x <= tile.x + 1; x++)
                {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++)
                    {
                        if (IsInMapRange(x, y) && (x == tile.x || y == tile.y)
                            && (mapFlags[x, y] == TileType.Floor
                            && Map[x, y] == tileType)
                            && mapFlags[x, y] != TileType.Border)
                        {
                            mapFlags[x, y] = TileType.Wall; 
                            queue.Enqueue(new Vector2Int(x, y));
                        }
                    }
                }
            }
            return tiles;
        }

        private List<List<Vector2Int>> GetRegions(TileType tileType)
        {
            var regions = new List<List<Vector2Int>>();
            var mapFlags = new TileType[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if ((mapFlags[x, y] == TileType.Floor
                        && Map[x, y] == tileType)
                        && mapFlags[x, y] != TileType.Border)
                    {
                        List<Vector2Int> newRegion = GetRegionTiles(x, y);
                        regions.Add(newRegion);

                        foreach (Vector2Int tile in newRegion)
                        {
                            mapFlags[tile.x, tile.y] = TileType.Wall;
                        }
                    }
                }
            }
            return regions;
        }

        private void ProcessMap()
        {
            int size = 50;

            foreach (var wallRegion in GetRegions(TileType.Wall))
            {
                if (wallRegion.Count < size)
                {
                    foreach (var tile in wallRegion)
                    {
                        Map[tile.x, tile.y] = TileType.Floor;
                    }
                }
            }

            var roomRegions = GetRegions(TileType.Floor);
            var survivingRooms = new List<Room>();

            foreach (var roomRegion in roomRegions)
            {
                if (roomRegion.Count < size)
                {
                    foreach (var tile in roomRegion)
                    {
                        Map[tile.x, tile.y] = TileType.Wall;
                    }
                }
                else
                {
                    survivingRooms.Add(new Room(roomRegion, Map));
                }
            }

            survivingRooms.Sort();
            survivingRooms[0].isAccesibleFromMainRoom = true;

            ConnectClosestRooms(survivingRooms);
        }

        private void ConnectClosestRooms(List<Room> allRooms)
        {
            List<Room> roomListA = new List<Room>();
            List<Room> roomListB = new List<Room>();

            foreach (Room room in allRooms)
            {
                if (room.isAccesibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }

            int bestDistance = 0;
            Vector2Int bestTileA = new Vector2Int();
            Vector2Int bestTileB = new Vector2Int();
            Room bestRoomA = new Room();
            Room bestRoomB = new Room();
            bool possibleConnectionFound = false;

            foreach (var roomA in roomListA)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }

                foreach (var roomB in roomListB)
                {
                    if (roomA == roomB || roomA.IsConnected(roomB))
                    {
                        continue;
                    }

                    for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                    {
                        for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                        {
                            Vector2Int tileA = roomA.edgeTiles[tileIndexA];
                            Vector2Int tileB = roomB.edgeTiles[tileIndexB];
                            int distanceBetweenRooms = System.Convert.ToInt32(Mathf.Pow(tileA.x - tileB.y, 2) + Mathf.Pow(tileA.x - tileB.y, 2));
                            if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                            {
                                bestDistance = distanceBetweenRooms;
                                possibleConnectionFound = true;
                                bestTileA = tileA;
                                bestTileB = tileB;
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                            }
                        }
                    }
                }
                if (possibleConnectionFound)
                {
                    CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
                }
            }

            if (possibleConnectionFound)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
                ConnectClosestRooms(allRooms);
            }
        }

        private void DrawCircle(Vector2Int c, int r)
        {
            int squareRadius = r * r;
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if ((x * x) + (y * y) <= squareRadius)
                    {
                        int drawX = c.x + x;
                        int drawY = c.y + y;

                        if (IsInMapRange(drawX, drawY))
                        {
                            Map[drawX, drawY] = TileType.Floor;
                        }
                    }
                }
            }
        }

        private void CreatePassage(Room roomA, Room roomB, Vector2Int tileA, Vector2Int tileB)
        {
            Room.ConnectRooms(roomA, roomB);
            foreach (var c in GetLine(tileA, tileB))
            {
                DrawCircle(c, 7);
            }
        }

        private List<Vector2Int> GetLine(Vector2Int from, Vector2Int to)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            bool inverted = false;

            int x = from.x;
            int y = from.y;

            int dx = to.x - from.x;
            int dy = to.y - from.y;

            int step = System.Math.Sign(dx);
            int gradientStep = System.Math.Sign(dy);

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);

            if (longest < shortest)
            {
                inverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);

                step = System.Math.Sign(dy);
                gradientStep = System.Math.Sign(dx);
            }

            int gradientAccumulation = longest / 2;

            for (int i = 0; i < longest; i++)
            {
                line.Add(new Vector2Int(x, y));

                if (inverted)
                {
                    y += step;
                }
                else
                {
                    x += step;
                }

                gradientAccumulation += shortest;
                if (gradientAccumulation >= longest)
                {
                    if (inverted)
                    {
                        x += gradientStep;
                    }
                    else
                    {
                        y += gradientStep;
                    }

                    gradientAccumulation -= longest;
                }
            }
            return line;
        }
    }
}
